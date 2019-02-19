// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace Inflo.Services
{
    public interface ISigningCredentialService
    {
        SigningCredentials GetSigningCredentials();

        IEnumerable<SecurityKey> GetPublicSecurityKeys();

        SecurityKey GetPublicSecurityKeyByKeyId(string keyId);
    }
}
