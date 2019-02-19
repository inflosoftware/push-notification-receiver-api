// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Inflo.EntityModels;
using Inflo.Services.Models;
using System.Collections.Generic;

namespace Inflo.Services
{
    public interface ITokenService
    {
        (string AccessToken, int ExpiresIn) CreateAccessToken(
            string issuer,
            string clientId,
            User user = null,
            IList<string> scopes = null);
    }
}