// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authorization;

namespace Inflo.WebApi
{
    public class BasicAuthAttribute : AuthorizeAttribute
    {
        public BasicAuthAttribute(string policy) : base(policy)
        {
            AuthenticationSchemes = Constants.AuthSchemes.BasicAuth;
        }

        public BasicAuthAttribute()
        {
            AuthenticationSchemes = Constants.AuthSchemes.BasicAuth;
        }
    }

    public class JwtAuthAttribute : AuthorizeAttribute
    {
        public JwtAuthAttribute(string policy) : base(policy)
        {
            AuthenticationSchemes = Constants.AuthSchemes.JwtAuth;
        }

        public JwtAuthAttribute()
        {
            AuthenticationSchemes = Constants.AuthSchemes.JwtAuth;
        }
    }
}
