// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Inflo.Services.Models
{
    public class Token
    {
        public ICollection<string> Audiences { get; set; } = new HashSet<string>();

        public string Issuer { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime ExpiryTime { get; set; }


        public int ExpiresIn => (int)(ExpiryTime - DateTime.UtcNow).TotalSeconds;

        public string Type { get; set; } = OidcConstants.TokenTypes.AccessToken;

        public string ClientId { get; set; }

        public ICollection<Claim> Claims { get; set; } = new HashSet<Claim>(new ClaimComparer());

        public int Version { get; set; } = 4;

        public string SubjectId => Claims.Where(x => x.Type == JwtClaimTypes.Subject).Select(x => x.Value).SingleOrDefault();

        public IEnumerable<string> Scopes => Claims.Where(x => x.Type == JwtClaimTypes.Scope).Select(x => x.Value);
    }
}
