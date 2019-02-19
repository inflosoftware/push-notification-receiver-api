// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Inflo.Services.Models;
using System.IdentityModel.Tokens.Jwt;
using IdentityModel;
using Inflo.Services.Extensions;
using Inflo.Data;
using Inflo.EntityModels;
using Microsoft.Extensions.Options;

namespace Inflo.Services
{
    public class TokenService : ServiceBase, ITokenService
    {
        public TokenService(
            IDataAccess dataAccess,
            IOptions<InfloOptions> options,
            ISigningCredentialService signingCredentialService) : base(dataAccess)
        {
            Options = options.Value;
            SigningCredentialService = signingCredentialService;
        }

        private InfloOptions Options { get; }
        private ISigningCredentialService SigningCredentialService { get; }




        public (string AccessToken, int ExpiresIn) CreateAccessToken(
            string issuer, 
            string clientId, 
            User user = null, 
            IList<string> scopes = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString())
            };

            claims.AddRange(GetAccessTokenClaims(clientId, user, scopes));

            var dateNow = DateTime.UtcNow;

            var token = new Token
            {
                Issuer = issuer,
                CreationTime = dateNow,
                ExpiryTime = dateNow.AddSeconds(Options.TokenLifetimeSeconds),
                Audiences = { $"{issuer}/resources" },
                ClientId = clientId,
                Claims = claims,
            };

            return (CreateToken(token), token.ExpiresIn);
        }


        /// <summary>
        /// Returns claims for an access token
        /// </summary>
        private IEnumerable<Claim> GetAccessTokenClaims(string clientId, User user = null, IList<string> scopes = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.ClientId, clientId)
            };

            // Add any scopes if needed
            if (scopes != null)
            {
                foreach (var scope in scopes)
                {
                    claims.Add(new Claim(JwtClaimTypes.Scope, scope));
                }
            }

            // There is a user involved
            if (user != null)
            {
                claims.AddRange(new[]
                {
                    new Claim(JwtClaimTypes.Subject, user.Id.ToString()),
                    new Claim(JwtClaimTypes.IdentityProvider, nameof(Inflo)),
                    new Claim(JwtClaimTypes.AuthenticationTime, DateTime.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer),
                });
            }

            return claims;
        }


        #region " Token Generation "

        /// <summary>
        /// Creates the token.
        /// </summary>
        private string CreateToken(Token token)
        {
            var header = CreateHeader(token);
            var payload = CreatePayload(token);

            return CreateJwt(new JwtSecurityToken(header, payload));
        }

        private JwtHeader CreateHeader(Token token)
        {
            var credential = SigningCredentialService.GetSigningCredentials();

            if (credential == null)
            {
                throw new InvalidOperationException("No signing credential is configured. Can't create JWT token");
            }

            var header = new JwtHeader(credential);


            // emit x5t claim for backwards compatibility with v4 of MS JWT library
            if (credential.Key is X509SecurityKey x509key)
            {
                var cert = x509key.Certificate;
                header["x5t"] = Base64Url.Encode(cert.GetCertHash());
            }

            return header;
        }

        private JwtPayload CreatePayload(Token token)
        {
            var payload = new JwtPayload(
                token.Issuer,
                null,
                null,
                token.CreationTime,
                token.ExpiryTime);

            foreach (var aud in token.Audiences)
            {
                payload.AddClaim(new Claim(JwtClaimTypes.Audience, aud));
            }

            var amrClaims = token.Claims.Where(x => x.Type == JwtClaimTypes.AuthenticationMethod);
            var scopeClaims = token.Claims.Where(x => x.Type == JwtClaimTypes.Scope);
            var jsonClaims = token.Claims.Where(x => x.ValueType == "json");

            var normalClaims = token.Claims
                                .Except(amrClaims)
                                .Except(jsonClaims)
                                .Except(scopeClaims);

            payload.AddClaims(normalClaims);

            // scope claims
            if (!scopeClaims.IsNullOrEmpty())
            {
                var scopeValues = scopeClaims.Select(x => x.Value).ToArray();
                payload.Add(JwtClaimTypes.Scope, scopeValues);
            }

            // amr claims
            if (!amrClaims.IsNullOrEmpty())
            {
                var amrValues = amrClaims.Select(x => x.Value).Distinct().ToArray();
                payload.Add(JwtClaimTypes.AuthenticationMethod, amrValues);
            }

            // deal with json types
            // calling ToArray() to trigger JSON parsing once and so later 
            // collection identity comparisons work for the anonymous type
            var jsonTokens = jsonClaims.Select(x => new { x.Type, JsonValue = JRaw.Parse(x.Value) }).ToArray();

            var jsonObjects = jsonTokens.Where(x => x.JsonValue.Type == JTokenType.Object).ToArray();
            var jsonObjectGroups = jsonObjects.GroupBy(x => x.Type).ToArray();

            foreach (var group in jsonObjectGroups)
            {
                if (payload.ContainsKey(group.Key))
                {
                    throw new Exception(String.Format("Can't add two claims where one is a JSON object and the other is not a JSON object ({0})", group.Key));
                }

                if (group.Skip(1).Any())
                {
                    // add as array
                    payload.Add(group.Key, group.Select(x => x.JsonValue).ToArray());
                }
                else
                {
                    // add just one
                    payload.Add(group.Key, group.First().JsonValue);
                }
            }

            var jsonArrays = jsonTokens.Where(x => x.JsonValue.Type == JTokenType.Array).ToArray();
            var jsonArrayGroups = jsonArrays.GroupBy(x => x.Type).ToArray();

            foreach (var group in jsonArrayGroups)
            {
                if (payload.ContainsKey(group.Key))
                {
                    throw new Exception($"Can't add two claims where one is a JSON array and the other is not a JSON array ({group.Key})");
                }

                var newArr = new List<JToken>();
                foreach (var arrays in group)
                {
                    var arr = (JArray)arrays.JsonValue;
                    newArr.AddRange(arr);
                }

                // add just one array for the group/key/claim type
                payload.Add(group.Key, newArr.ToArray());
            }

            var unsupportedJsonTokens = jsonTokens.Except(jsonObjects).Except(jsonArrays);
            var unsupportedJsonClaimTypes = unsupportedJsonTokens.Select(x => x.Type).Distinct();

            if (unsupportedJsonClaimTypes.Any())
            {
                throw new Exception($"Unsupported JSON type for claim types: {unsupportedJsonClaimTypes.Aggregate((x, y) => x + ", " + y)}");
            }

            return payload;
        }

        private string CreateJwt(JwtSecurityToken jwt)
        {
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        #endregion
    }
}
