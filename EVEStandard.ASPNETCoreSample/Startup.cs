﻿using EVEStandard.Enumerations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EVEStandard.ASPNETCoreSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Add cookie authentication and set the login url
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                });

            // Register your application at: https://developers.eveonline.com/applications to obtain client ID and secret key and add them to user secrets
            // by right-clicking the solution and selecting Manage User Secrets

            // URL for the callback
            // IMPORTANT: Replace with correct port
            var callbackUrl = "https://localhost:44314/Auth/Callback";

            // Initialize the client
            var esiClient = new EVEStandardAPI(
                    "EVEStandard",              // User agent
                    DataSource.Tranquility,     // Server [Tranquility/Singularity]
                    TimeSpan.FromSeconds(30),   // Timeout
                    callbackUrl,                // SSO callback URL
                    Configuration["ClientId"],                   // Client ID
                    Configuration["SecretKey"]);                 // Secret Key

            // Register with DI container
            services.AddSingleton<EVEStandardAPI>(esiClient);

            // Session is required 
            services.AddSession();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
