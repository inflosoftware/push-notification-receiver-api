// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Inflo.EntityModels
{
    public class IdentityClient : EntityBase<long>
    {
        /// <summary>
        /// Client name (used in the client_id scope)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// hashed password for the client
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Space seperated list of scopes
        /// </summary>
        public string Scopes { get; set; } 
    }
}
