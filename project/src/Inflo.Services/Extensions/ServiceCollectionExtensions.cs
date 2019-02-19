// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Inflo.Data;
using Inflo.EntityModels;
using Inflo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfloServices(this IServiceCollection services, Action<InfloOptions> options)
        {
            // Add caching
            services.AddMemoryCache();

            // Add basic identity services in order to validate user password hashes
            //services.AddIdentityCore<User>();

            services.TryAddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.TryAddScoped<IPasswordHasher<IdentityClient>, PasswordHasher<IdentityClient>>();


            // Add DataAccess layer
            services.AddScoped<IDataAccess, DataAccess>();

            // Add all services here that impliment ServiceBase
            services.AutoRegister();


            services.AddOptions();
            services.Configure(options);

            return services;
        }


        private static IServiceCollection AutoRegister(this IServiceCollection services)
        {
            var baseInterfaceType = typeof(IServiceBase);

            var discoveredServices = baseInterfaceType.Assembly.ExportedTypes
                                        .Select(p => new
                                        {
                                            Type = p,
                                            TypeInfo = p.GetTypeInfo()
                                        })
                                        .Where(p =>
                                                p.TypeInfo.IsClass &&
                                                !p.TypeInfo.IsAbstract &&
                                                !p.TypeInfo.IsInterface &&
                                                p.TypeInfo.ImplementedInterfaces.Contains(baseInterfaceType))
                                        .ToList();

            foreach (var discoveredService in discoveredServices)
            {
                // Add the service once as its own key
                services.TryAddScoped(discoveredService.Type);

                // For each interface the service impliments
                foreach (var interfaceType in discoveredService.TypeInfo.ImplementedInterfaces)
                {
                    // Add the interface as a key and get the pre registered service by regsietering a factory func
                    services.TryAddScoped(interfaceType,
                        implementationFactory: p => p.GetService(discoveredService.Type));
                }
            }

            return services;
        }
    }
}
