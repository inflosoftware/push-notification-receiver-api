// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Inflo.Data;
using Inflo.Services;
using Inflo.WebApi.Handlers;
using Inflo.WebApi.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO.Compression;

namespace Inflo.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            HostingEnvironment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add MVC engines
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Add Routing engine
            services.AddRouting(options =>
            {
                options.AppendTrailingSlash = false;
                options.LowercaseQueryStrings = false;
                options.LowercaseUrls = false;
            });

            // Add HTTP Compression
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();

                options.EnableForHttps = true;
            })
            .Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            })
            .Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });


            // Adds required Inflo services
            services.AddInfloServices(options =>
            {
                // Settings located inside appsettings.json
                Configuration.Bind(nameof(InfloOptions), options);
            });

            // Using custom build OAuth Client Credentials flow to get JWT access tokens
            // Look into using IdentityServer for a fuller solution (https://github.com/IdentityServer)
            services
                .AddAuthentication(/*Do not set a default authentication scheme if using multiple schemes as there is a bug in AspNetCore*/)
                // Add BasicAuth using custom build handler to control OAuth Client Credentials flow login
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(Constants.AuthSchemes.BasicAuth, null)
                // Add JWT Auth for access tokens
                .AddJwtBearer(Constants.AuthSchemes.JwtAuth, options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = !HostingEnvironment.IsDevelopment();
                    options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                });


            // Need DI for the delegates of the JwtBearerOptions to work so
            // created a custom post options bind that needs to be registered
            services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();


            // Configure EF Core to use SQL server and retrive 
            // the connection string from appsettings.json
            services.AddDbContext<DataContext>(
                builder => 
                    builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. 
                // You may want to change this for production scenarios,
                // see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

                app.UseHttpsRedirection();
            }

            // Use compression
            app.UseResponseCompression();

            // returns index.html for example
            app.UseDefaultFiles();

            // Return static files so no need to hit authentication
            app.UseStaticFiles();

            // Authenticate before hitting controllers
            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
