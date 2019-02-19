// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Inflo.Services;
using Inflo.EntityModels;
using Microsoft.AspNetCore.Identity;

namespace Inflo.WebApi.Handlers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IIdentityClientService identityClientService)
            : base(options, logger, encoder, clock)
        {
            IdentityClientService = identityClientService;
        }

        private IIdentityClientService IdentityClientService { get; }


        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
            }

            (PasswordVerificationResult Result, IdentityClient Client) loginAttempt;

            try
            {
                // Get the Authorization header from the HTTP request
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);

                // Undo the Base64 encoding
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);

                // Credentials format is "username:password" so split on the ':'
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');

                var username = credentials[0];
                var password = credentials[1];

                loginAttempt = IdentityClientService.ValidateCredentials(username, password);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Invalid Authorization Header");

                // If anything went wrong the cannot authenticate
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }

            switch (loginAttempt.Result)
            {
                // If the username and password was invalid then cannot authenticate
                default:
                case PasswordVerificationResult.Failed:
                    {
                        Logger.LogError("Invalid Username or Password");

                        return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));
                    }
                case PasswordVerificationResult.Success:
                case PasswordVerificationResult.SuccessRehashNeeded:
                    {
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, loginAttempt.Client.Id.ToString()),
                            new Claim(ClaimTypes.Name, loginAttempt.Client.Name),
                        };

                        var identity = new ClaimsIdentity(claims, Scheme.Name);
                        var principal = new ClaimsPrincipal(identity);
                        var ticket = new AuthenticationTicket(principal, Scheme.Name);

                        return Task.FromResult(AuthenticateResult.Success(ticket));
                    }
            }
        }
    }
}
