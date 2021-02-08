// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdServer.Data;
using IdServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedLib;
using System.IO;
using System.Security.Cryptography.X509Certificates;


namespace IdServer {
  public class Startup {
    public IWebHostEnvironment Environment { get; }
    public IConfiguration Configuration { get; }

    public Startup(IWebHostEnvironment environment, IConfiguration configuration) {
      Environment = environment;
      Configuration = configuration;
      Cfg.Init(Configuration);
    }

    public void ConfigureServices(IServiceCollection services) {
      services.AddControllersWithViews();

      services.AddDbContext<ApplicationDbContext>(options =>
          options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

      services.AddIdentity<ApplicationUser, IdentityRole>()
          .AddEntityFrameworkStores<ApplicationDbContext>()
          .AddDefaultTokenProviders();

      var builder = services.AddIdentityServer(options => {
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;
        // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
        options.EmitStaticAudienceClaim = true;
        options.Discovery.CustomEntries.Add("users_endpoint", "~/users");
      });
      builder.AddInMemoryIdentityResources(Cfg.IdCfg.IdentityResources)
          .AddInMemoryApiScopes(Cfg.IdCfg.ApiScopes)
          .AddInMemoryApiResources(Cfg.IdCfg.ApiResources)
          .AddInMemoryClients(Cfg.IdCfg.Clients)
          .AddAspNetIdentity<ApplicationUser>()
          .AddProfileService<ProfileService>();

      ManageIdentityServerCertificate(builder);

      services.AddAuthentication()
          .AddGoogle(options => {
            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                  // register your IdentityServer with Google at https://console.developers.google.com
                  // enable the Google+ API
                  // set the redirect URI to https://localhost:5001/signin-google
            options.ClientId = "copy client ID from Google here";
            options.ClientSecret = "copy client secret from Google here";
          })
          .AddLocalApi();


      services.AddAuthorization(options => {
        AppPolicies(options);
      });

      services.AddCors(options => { // this defines a CORS policy called ("CORSPolicy", 
        options.AddPolicy(Cfg.CORSPolicy, builder => {
          builder.WithOrigins(
            Cfg.ServicesUrls.BlazorClient1,
            Cfg.ServicesUrls.WebApi1
          )
          .AllowAnyHeader();
        });
      });

    }

    public void Configure(IApplicationBuilder app) {
      if(Environment.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
        app.UseDatabaseErrorPage();
      }

      app.UseStaticFiles();

      app.UseRouting();
      app.UseCors(Cfg.CORSPolicy); // This MUST be placed after "app.UseRouting();"
      app.UseIdentityServer();
      app.UseAuthorization();
      app.UseEndpoints(endpoints => {
        endpoints.MapDefaultControllerRoute();
      });
    }



    private static void ManageIdentityServerCertificate( IIdentityServerBuilder builder) {
      string certificateFileName = Cfg.IdentityServerCertificate.FileName;
      if (certificateFileName != null)
        if (!File.Exists(certificateFileName))
          Serilog.Log.Error($"Certificate not found: {certificateFileName}");
        else {
          Serilog.Log.Information("Loading certificate....");
          var certificate = new X509Certificate2(certificateFileName, Cfg.IdentityServerCertificate.Password);
          builder.AddSigningCredential(certificate);
          Serilog.Log.Information("Using AddSigningCredential()");
        }
      else {
        Serilog.Log.Warning("Using AddDeveloperSigningCredential() NOT recommended for production!");
        builder.AddDeveloperSigningCredential();
      }
    }


  private static void AppPolicies(AuthorizationOptions options) {

      options.AddPolicy(Cfg.Policies.UsersManagement_List, policy => {
        policy.AddAuthenticationSchemes(IdentityServerConstants.LocalApi.AuthenticationScheme);
        //policy.RequireRole(IdNames.Roles.IdServer_User);
        policy.RequireScope(IdNames.Scopes.IdServerUsersRead); //needs pakage: IdentityServer4.AccessTokenValidation
        policy.RequireClaim(IdNames.Claims.IdServer_Users_List);
        policy.RequireAuthenticatedUser();
      });

      options.AddPolicy(Cfg.Policies.UsersManagement_Add, policy => {
        policy.AddAuthenticationSchemes(IdentityServerConstants.LocalApi.AuthenticationScheme);
        //policy.RequireRole(IdNames.Roles.IdServer_Admin);
        policy.RequireScope(IdNames.Scopes.IdServerUsersWrite); //needs pakage: IdentityServer4.AccessTokenValidation
        policy.RequireClaim(IdNames.Claims.IdServer_Users_Add);
        policy.RequireAuthenticatedUser();
      });


      options.AddPolicy(Cfg.Policies.CertificatesManagement_Create, policy => {
        policy.AddAuthenticationSchemes(IdentityServerConstants.LocalApi.AuthenticationScheme);
        //policy.RequireRole(IdNames.Roles.IdServer_Admin);
        policy.RequireScope(IdNames.Scopes.IdServerCertificates); //needs pakage: IdentityServer4.AccessTokenValidation
        policy.RequireClaim(IdNames.Claims.IdServer_Cerificate_Create);
        policy.RequireAuthenticatedUser();
      });
    }



  }
}