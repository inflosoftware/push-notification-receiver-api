// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Inflo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using Inflo.WebApi.Models;
using IdentityModel;

namespace Inflo.WebApi.Controllers
{
    public class AuthController : Controller
    {
        public AuthController(IOptions<InfloOptions> options)
        {
            Options = options.Value;
        }
        private InfloOptions Options { get; }

        /// <summary>
        /// This endpoint will use Client Credentials flow
        /// </summary>
        [BasicAuth, HttpPost("~/connect/token")]
        public IActionResult CreateToken(
            [FromForm]string grant_type,
            [FromForm]string scope,
            [FromServices]ITokenService tokenService,
            [FromServices]IIdentityClientService identityClientService)
        {
            if(grant_type != OidcConstants.GrantTypes.ClientCredentials)
            {
                return BadRequest("invalid grant type");
            }

            if (long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out long clientId))
            {
                var issuer = GetServerIssuerUri();
                var identityClient = identityClientService.GetById(clientId);

                string[] scopes = null;

                if (string.IsNullOrWhiteSpace(scope))
                {
                    if (!string.IsNullOrEmpty(identityClient.Scopes))
                    {
                        scopes = identityClient.Scopes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                }
                else
                {
                    scopes = scope.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }

                var (AccessToken, ExpiresIn) = tokenService.CreateAccessToken(
                                issuer,
                                identityClient.Id.ToString(),
                                user: null,
                                scopes: scopes);

                return Json(new TokenResponse
                {
                    AccessToken = AccessToken,
                    TokenType = OidcConstants.TokenResponse.BearerTokenType,
                    ExpiresIn = ExpiresIn,
                });
            }

            return BadRequest("Could not create access token");
        }


        #region " Helper Methods "


        private string GetServerIssuerUri()
        {
            // If we have explicitly configured a URI then use it,

            if (!string.IsNullOrWhiteSpace(Options.FullyQualifiedDomainName))
            {
                return Options.FullyQualifiedDomainName;
            }

            // Otherwise dynamically calculate it

            var uri = $"{Request.Scheme}://{Request.Host.Value}";

            return uri.EndsWith("/")
                    ? uri.TrimEnd('/').ToLowerInvariant()
                    : uri.ToLowerInvariant();
        }


        #endregion
    }
}
