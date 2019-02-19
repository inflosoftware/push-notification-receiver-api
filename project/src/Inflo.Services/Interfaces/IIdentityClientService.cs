// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Inflo.EntityModels;
using Microsoft.AspNetCore.Identity;

namespace Inflo.Services
{
    public interface IIdentityClientService
    {
        IdentityClient GetById(long id);

        IdentityClient GetByName(string name);

        (PasswordVerificationResult Result, IdentityClient IdentityClient) ValidateCredentials(string name, string password);
    }
}
