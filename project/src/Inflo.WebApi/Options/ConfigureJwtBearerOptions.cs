// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Inflo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace Inflo.WebApi.Options
{
    public class ConfigureJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
    {
        public ConfigureJwtBearerOptions(
            IOptions<InfloOptions> options,
            IServiceScopeFactory serviceScopeFactory)
        {
            _options = options.Value;
            _serviceScopeFactory = serviceScopeFactory;
        }

        private readonly InfloOptions _options;
        private readonly IServiceScopeFactory _serviceScopeFactory;


        public void PostConfigure(string name, JwtBearerOptions options)
        {
            if (!string.IsNullOrWhiteSpace(_options.FullyQualifiedDomainName))
            {
                if (options.TokenValidationParameters.ValidIssuers == null ||
                    options.TokenValidationParameters.ValidIssuers.All(p => p != _options.FullyQualifiedDomainName))
                {
                    var validIssuers = new List<string>(options.TokenValidationParameters.ValidIssuers ?? Enumerable.Empty<string>())
                    {
                        _options.FullyQualifiedDomainName
                    };

                    options.TokenValidationParameters.ValidIssuers = validIssuers;
                    options.TokenValidationParameters.ValidateIssuer = true;
                }

                var audience = $"{_options.FullyQualifiedDomainName}/resources";

                if (options.TokenValidationParameters.ValidAudiences == null || 
                    options.TokenValidationParameters.ValidAudiences.All(p => p != audience))
                {
                    var validAudiences = new List<string>(options.TokenValidationParameters.ValidAudiences ?? Enumerable.Empty<string>())
                    {
                        audience
                    };

                    options.TokenValidationParameters.ValidAudiences = validAudiences;
                    options.TokenValidationParameters.ValidateAudience = true;
                }
            }

            options.TokenValidationParameters.IssuerSigningKeyResolver =
                 (string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters) =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var service = scope.ServiceProvider.GetRequiredService<ISigningCredentialService>();

                            var key = service.GetPublicSecurityKeyByKeyId(kid);

                            return key == null
                                    ? Enumerable.Empty<SecurityKey>()
                                    : new[] { key };
                        }
                    };
        }
    }
}
