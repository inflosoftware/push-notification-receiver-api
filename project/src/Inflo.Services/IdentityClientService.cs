// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using Inflo.Data;
using Inflo.EntityModels;
using Microsoft.AspNetCore.Identity;

namespace Inflo.Services
{
    public class IdentityClientService : ServiceBase, IIdentityClientService
    {
        public IdentityClientService(
            IDataAccess dataAccess,
            IPasswordHasher<IdentityClient> passwordHasher) : base(dataAccess)
        {
            PasswordHasher = passwordHasher;
        }

        private IPasswordHasher<IdentityClient> PasswordHasher { get; }


        public IdentityClient GetById(long id)
        {
            return DataAccess.GetAll<IdentityClient>().FirstOrDefault(p => p.Id.Equals(id));
        }

        public IdentityClient GetByName(string name)
        {
            return DataAccess.GetAll<IdentityClient>().FirstOrDefault(p => p.Name.ToLower() == name.ToLower());
        }


        public (PasswordVerificationResult Result, IdentityClient IdentityClient) ValidateCredentials(string name, string password)
        {
            // Default to failed and try get a success
            var result = PasswordVerificationResult.Failed;

            // Get the user by the name
            var identityClient = GetByName(name);

            // We found the user account so lets try validating the password
            if (identityClient != null)
            {
                // Get a validation result from the password verification provider
                result = PasswordHasher.VerifyHashedPassword(identityClient, identityClient.PasswordHash, password);
            }

            return (result, identityClient);
        }
    }
}
