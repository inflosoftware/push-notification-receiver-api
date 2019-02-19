// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Inflo.Data;
using Inflo.EntityModels;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace Inflo.Services
{
    public class UserService : ServiceBase, IUserService
    {
        public UserService(
            IDataAccess dataAccess,
            IPasswordHasher<User> passwordHasher) : base(dataAccess)
        {
            PasswordHasher = passwordHasher;
        }

        private IPasswordHasher<User> PasswordHasher { get; }


        public User GetByUsername(string username)
        {
            return DataAccess.GetAll<User>().FirstOrDefault(p => p.Username.ToLower() == username.ToLower());
        }

        public User GetById(long id)
        {
            return DataAccess.GetAll<User>().FirstOrDefault(p => p.Id.Equals(id));
        }


        public (PasswordVerificationResult Result, User User) ValidateCredentials(string username, string password)
        {
            // Default to failed and try get a success
            var result = PasswordVerificationResult.Failed;

            // Get the user by the username
            var user = GetByUsername(username);

            // We found the user account so lets try validating the password
            if (user != null)
            {
                // Get a validation result from the password verification provider
                result = PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            }

            return (result, user);
        }
    }
}
