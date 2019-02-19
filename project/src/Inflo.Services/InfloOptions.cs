// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Inflo.Services
{
    public class InfloOptions
    {
        /// <summary>
        /// The domain this server runs on publically (useful if running the app behind a proxy or firewall)
        /// <para>E.g. "https://example.com"</para>
        /// </summary>
        public string FullyQualifiedDomainName { get; set; }

        /// <summary>
        /// Number of days before a key is retired from signing new credentials. 
        /// Once a key is retired it can only be used for validating old credentials for upto 5 days before the key is retired permanently.
        /// <para>Defaults to "30 Days" if not specified.</para>
        /// </summary>
        public int KeyRotationDays { get; set; } = 30;


        /// <summary>
        /// The length of time a JWT token should live for once issued.
        /// <para>Defaults to "30 minutes" if not specified.</para>
        /// </summary>
        public int TokenLifetimeSeconds { get; set; } = 1800;
    }
}
