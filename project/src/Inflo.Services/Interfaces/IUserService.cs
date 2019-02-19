// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Inflo.EntityModels;
using Microsoft.AspNetCore.Identity;

namespace Inflo.Services
{
    public interface IUserService
    {
        User GetById(long id);

        (PasswordVerificationResult Result, User User) ValidateCredentials(string username, string password);
    }
}
