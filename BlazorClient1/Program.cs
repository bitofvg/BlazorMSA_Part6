using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorClient1 {
  public class Program {
    public static async Task Main(string[] args) {
      var builder = WebAssemblyHostBuilder.CreateDefault(args);

      // adds confifuration  appsettings.{Environment}.{SubEnvironment}.json
      await AddSubEnvironmentConfiguration(builder);

      builder.RootComponents.Add<App>("#app");

      //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
      // Registers a named HttpClient here for the WebApi1
      builder.Services.AddHttpClient("LocalHttpClient", hc => hc.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
      builder.Services.AddScoped(sp => sp.GetService<IHttpClientFactory>()
        .CreateClient("LocalHttpClient"));

      // Register a named HttpClient for the WebApi1
      builder.Services.AddHttpClient("WebApi1HttpClient")
                .AddHttpMessageHandler(sp => {
                  var handler = sp.GetService<AuthorizationMessageHandler>()
                                  .ConfigureHandler(
                                    authorizedUrls: new[] { builder.Configuration["ServicesUrls:WebApi1"] }, // WebApi
                                    scopes: new[] { "WApi1.Weather.List", }
                                  );
                  return handler;
                });
      builder.Services.AddScoped(sp => sp.GetService<IHttpClientFactory>()
        .CreateClient("WebApi1Client"));

      // Registers a named HttpClient the IdentityServer UsersController
      builder.Services.AddHttpClient("IdentityServerUsersHttpClient")
        .AddHttpMessageHandler(sp => {
          var handler = sp.GetService<AuthorizationMessageHandler>()
            .ConfigureHandler(
               authorizedUrls:
                 new[] { builder.Configuration["ServicesUrls:IdServer"] },
               scopes:
                 new[] {
                         "IdentityServer.Users.List",
                         "IdentityServer.Users.Add"
                 });
          return handler;
        });
      builder.Services.AddScoped(sp => sp.GetService<IHttpClientFactory>()
         .CreateClient("IdentityServerUsersHttpClient"));



      builder.Services.AddOidcAuthentication(options => {
        // load Oidc options for the Identity Server authentication.
        builder.Configuration.Bind("oidc", options.ProviderOptions);
        options.ProviderOptions.Authority = builder.Configuration["ServicesUrls:IdServer"] + "/";
        options.ProviderOptions.PostLogoutRedirectUri = builder.Configuration["ServicesUrls:BlazorClient1"] + "/";
        // get the roles from the claims named "role"
        options.UserOptions.RoleClaim = "role";
      })
      .AddAccountClaimsPrincipalFactory<CustomUserFactory>();


      builder.Services.AddAuthorizationCore(options => {
        options.AddPolicy("WebApi_List", policy => policy.RequireClaim("WebApi1.List", "true"));
        options.AddPolicy("WebApi_Update", policy => policy.RequireClaim("WebApi1.Update", "true"));
        options.AddPolicy("WebApi_Delete", policy => policy.RequireClaim("WebApi1.Delete", "true"));
      });


      await builder.Build().RunAsync();
    }


    private static async Task AddSubEnvironmentConfiguration(WebAssemblyHostBuilder builder) {
      var subenv = builder.Configuration["SubEnvironment"];
      var settingsfile = $"appsettings.{builder.HostEnvironment.Environment}.{subenv}.json";

      using (var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }) {
        using (var appsettingsResponse = await http.GetAsync(settingsfile)) {
          using (var stream = await appsettingsResponse.Content.ReadAsStreamAsync()) {
            builder.Configuration.AddJsonStream(stream);
          };
        };
      };
    }


  }
}
