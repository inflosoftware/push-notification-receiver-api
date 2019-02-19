// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Inflo.Data;
using Inflo.EntityModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Inflo.Services
{
    public class SigningCredentialService : ServiceBase, ISigningCredentialService
    {
        public SigningCredentialService(
            IDataAccess dataAccess,
            IOptions<InfloOptions> options,
            IMemoryCache cache) : base(dataAccess)
        {
            Options = options.Value;
            Cache = cache;
        }

        private InfloOptions Options { get; }
        private IMemoryCache Cache { get; set; }

        /// <summary>
        /// Gets the SigningCredentials that is valid for signing with a private key
        /// </summary>
        /// <returns></returns>
        public SigningCredentials GetSigningCredentials()
        {
            return Cache.GetOrCreate(
                key: CacheKeys.SigningCredentials,
                factory: cacheEntry =>
                {
                    // Cache for 3 hours as these creds should not be recycling too frequently
                    cacheEntry.Priority = CacheItemPriority.Normal;
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3);

                    var asymmetricKey = DataAccess.GetAll<AsymmetricKey>().FirstOrDefault(p => p.IsSigningCredential);

                    // Signing credential is still valid
                    if (asymmetricKey != null &&
                        asymmetricKey.DateTimeCreated.AddDays(Options.KeyRotationDays) >= DateTime.UtcNow.Date)
                    {
                        return CreateSigningCredential(asymmetricKey);
                    }

                    // There are no valid signing credentials so lets generate one
                    return CreateSigningCredential();
                });
        }

        /// <summary>
        /// Gets all the public SecurityKeys that are still usuable for validating signatures
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SecurityKey> GetPublicSecurityKeys()
        {
            return Cache.GetOrCreate(
                key: CacheKeys.PublicSecurityKeys,
                factory: cacheEntry =>
                {
                    // Cache for 3 hours as these creds should not be recycling too frequently
                    cacheEntry.Priority = CacheItemPriority.Normal;
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3);

                    var keys = getPublicKeys();

                    // If there are not signing keys we need to generate one
                    if (!keys.Any())
                    {
                        CreateSigningCredential();

                        keys = getPublicKeys();
                    }

                    return keys;
                });


            IEnumerable<SecurityKey> getPublicKeys()
            {
                // We dont want any keys that have been out of rotation for longer than 2 days
                var date = DateTime.UtcNow.Date.AddDays(-(Options.KeyRotationDays + 2));

                return DataAccess.GetAll<AsymmetricKey>()
                            .Where(p => p.DateTimeCreated >= date)
                            // Need to bring the values into memory from the DB before performing operations
                            .AsEnumerable()
                            // Create RsaSecurityKeys from the db entries
                            .Select(p => CreateRsaSecurityKey(asymmetricKey: p, includePrivateKey: true))
                            .ToList();
            }
        }

        /// <summary>
        /// Gets a public SecurityKey that is still usuable for validating signatures
        /// using the KeyId which shoud be the GUID of the <see cref="AsymmetricKey.Id"/> without hyphens
        /// </summary>
        public SecurityKey GetPublicSecurityKeyByKeyId(string keyId)
        {
            return GetPublicSecurityKeys().FirstOrDefault(p => p.KeyId == keyId);
        }


        #region " Helpers "

        private static class CacheKeys
        {
            public const string SigningCredentials = "Inflo.Services.SigningCredentialService.SigningCredentials";
            public const string PublicSecurityKeys = "Inflo.Services.SigningCredentialService.PublicSecurityKeys";
        }

        /// <summary>
        /// Creates a new SigningCredentials from the AsymmetricKey if supplied otherwise 
        /// will create a new AsymmetricKey to be stored in the DB and will use that to create
        /// the SigningCredentials and mark the older AsymmetricKeys as non signing keys.
        /// </summary>
        private SigningCredentials CreateSigningCredential(AsymmetricKey asymmetricKey = null)
        {
            RsaSecurityKey rsaSecurityKey;

            if (asymmetricKey != null)
            {
                rsaSecurityKey = CreateRsaSecurityKey(asymmetricKey, includePrivateKey: true);

                if (rsaSecurityKey.PrivateKeyStatus == PrivateKeyStatus.DoesNotExist)
                {
                    throw new InvalidOperationException("RSA key does not have a private key.");
                }

                return new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);
            }

            rsaSecurityKey = CreateRsaSecurityKey();

            RSAParameters parameters;

            if (rsaSecurityKey.Rsa != null)
            {
                parameters = rsaSecurityKey.Rsa.ExportParameters(includePrivateParameters: true);
            }
            else
            {
                parameters = rsaSecurityKey.Parameters;
            }


            if (rsaSecurityKey.PrivateKeyStatus == PrivateKeyStatus.DoesNotExist)
            {
                throw new InvalidOperationException("RSA key does not have a private key.");
            }

            var signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

            // Retire the old AsymmetricKeys
            foreach (var item in DataAccess.GetAll<AsymmetricKey>().Where(p => p.IsSigningCredential).ToList())
            {
                item.IsSigningCredential = false;
                DataAccess.Update(item, saveChanges: false);
            }


            // Store the new AsymmetricKey
            DataAccess.Create(new AsymmetricKey
            {
                Id = Guid.Parse(rsaSecurityKey.KeyId),
                IsSigningCredential = true,
                DateTimeCreated = DateTime.UtcNow,
                JsonMetadata = JsonConvert.SerializeObject(
                                parameters,
                                new JsonSerializerSettings
                                {
                                    ContractResolver = new RsaParametersContractResolver()
                                })
            });

            // lets make sure we wipe out the public key cache so the new key we just saved becomes available
            Cache.Remove(CacheKeys.PublicSecurityKeys);

            return signingCredentials;
        }


        /// <summary>
        /// Creates a new RSA security key.
        /// </summary>
        private static RsaSecurityKey CreateRsaSecurityKey(AsymmetricKey asymmetricKey, bool includePrivateKey)
        {
            var rsaParameters = JsonConvert.DeserializeObject<RSAParameters>(
                                    asymmetricKey.JsonMetadata,
                                    new JsonSerializerSettings
                                    {
                                        ContractResolver = new RsaParametersContractResolver()
                                    });

            if (!includePrivateKey)
            {
                // This is to be a public key only so 
                // need to wipe out the private key parts
                rsaParameters.D = null;
                rsaParameters.DP = null;
                rsaParameters.DQ = null;
                rsaParameters.InverseQ = null;
                rsaParameters.P = null;
                rsaParameters.Q = null;
            }

            var rsaSecurityKey = new RsaSecurityKey(rsaParameters)
            {
                KeyId = asymmetricKey.Id.ToString("N")
            };

            return rsaSecurityKey;
        }

        /// <summary>
        /// Creates a new RSA security key.
        /// </summary>
        private static RsaSecurityKey CreateRsaSecurityKey()
        {
            RsaSecurityKey key;

            var rsa = RSA.Create();

            // Unfortunately, the default for RSA.Create() on .NET Framework 
            // is RSACryptoServiceProvider, which doesn't respect set_KeySize.
            if (rsa is RSACryptoServiceProvider)
            {
                // Dont store the key locally in windows server
                ((RSACryptoServiceProvider)rsa).PersistKeyInCsp = false;

                rsa.Dispose();

                var parameters = new RSACng(2048).ExportParameters(includePrivateParameters: true);
                key = new RsaSecurityKey(parameters);
            }
            else
            {
                rsa.KeySize = 2048;
                key = new RsaSecurityKey(rsa);
            }

            key.KeyId = Guid.NewGuid().ToString("N");
            return key;
        }

        private class RsaParametersContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                property.Ignored = false;

                return property;
            }
        }

        #endregion
    }
}
