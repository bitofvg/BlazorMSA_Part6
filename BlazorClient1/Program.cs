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
using Serilog;
using BlazorDownloadFile;
using SharedLib;


namespace BlazorClient1 {
  public class Program {
    public static async Task Main(string[] args) {
      var builder = WebAssemblyHostBuilder.CreateDefault(args);

      LoggingConfiguration(builder);

      // adds confifuration  appsettings.{Environment}.{SubEnvironment}.json
      await AddSubEnvironmentConfiguration(builder);
      Cfg.Init(builder.Configuration);

      //doc: https://bitofvg.wordpress.com/2021/01/29/identity-server-4-self-signed-certificates/
      builder.Services.AddBlazorDownloadFile();

      builder.RootComponents.Add<App>("#app");

      // Registers a named HttpClient here for the BlazorClient
      AddHttpClient(builder, Cfg.HttpClients.BlazorClient1, Cfg.ServicesUrls.BlazorClient1, null);

      // Register a named HttpClient for the WebApi1
      AddHttpClient(builder, Cfg.HttpClients.WebApi1, Cfg.ServicesUrls.WebApi1, 
        scopes: new[] {
          IdNames.Scopes.WApi1WeatherRead
        });

      // Registers a named HttpClient for the IdentityServer local Apis
      AddHttpClient(builder, Cfg.HttpClients.IdServer, Cfg.ServicesUrls.IdServer,
        scopes: new[] {
          IdNames.Scopes.IdServerUsersRead,
          IdNames.Scopes.IdServerUsersWrite,
          IdNames.Scopes.IdServerCertificates
        });

      builder.Services.AddOidcAuthentication(options => {
        // load Oidc options for the Identity Server authentication from appsettings.json.
        //builder.Configuration.Bind("oidc", options.ProviderOptions); 
        options.ProviderOptions.ClientId = IdNames.Clients.BlazorClient1;
        options.ProviderOptions.ResponseType = "code";
        options.ProviderOptions.DefaultScopes.Add(IdNames.OidcStandardScopes.OpenId);
        options.ProviderOptions.DefaultScopes.Add(IdNames.OidcStandardScopes.Profile);
        options.ProviderOptions.DefaultScopes.Add(IdNames.OidcStandardScopes.Email);
        options.ProviderOptions.DefaultScopes.Add(IdNames.OidcStandardScopes.Phone);
        options.ProviderOptions.DefaultScopes.Add(IdNames.IdResources.Gender);
        options.ProviderOptions.Authority = Cfg.ServicesUrls.IdServer + "/";
        options.ProviderOptions.PostLogoutRedirectUri = Cfg.ServicesUrls.BlazorClient1 + "/";
        // get the roles from the claims named "role"
        options.UserOptions.RoleClaim = "role";
      })
      .AddAccountClaimsPrincipalFactory<CustomUserFactory>();

      builder.Services.AddAuthorizationCore(options => {
        options.AddPolicy(Cfg.Policies.WApi1_Weather_List, policy => 
          policy.RequireClaim(IdNames.Claims.WApi1_Weather_List, "true"));
        options.AddPolicy(Cfg.Policies.WApi1_Weather_GetById, policy =>
          policy.RequireClaim(IdNames.Claims.WApi1_Weather_GetById, "true"));
        options.AddPolicy(Cfg.Policies.Users_Add, policy =>
          policy.RequireClaim(IdNames.Claims.IdServer_Users_Add, "true"));
        options.AddPolicy(Cfg.Policies.Users_List, policy =>
          policy.RequireClaim(IdNames.Claims.IdServer_Users_List, "true"));
        options.AddPolicy(Cfg.Policies.Cerificate_Create, policy =>
          policy.RequireClaim(IdNames.Claims.IdServer_Cerificate_Create, "true"));

      });

      await builder.Build().RunAsync();
    }


    //https://docs.microsoft.com/it-it/aspnet/core/blazor/security/webassembly/additional-scenarios?view=aspnetcore-5.0#custom-authorizationmessagehandler-class
    //https://bitofvg.wordpress.com/2020/12/16/blazormsa-part-3-webapi/
    private static void AddHttpClient(WebAssemblyHostBuilder builder, string Name, string BaseAddress, IEnumerable<string> scopes) {
      var httpCliBuilder = builder.Services.AddHttpClient(Name, hc => hc.BaseAddress = new Uri(BaseAddress));
      if (scopes is not null)
        httpCliBuilder.AddHttpMessageHandler(sp => {
          var handler = sp.GetService<AuthorizationMessageHandler>()
            .ConfigureHandler(
               authorizedUrls: new[] { BaseAddress },
               scopes: scopes);
          return handler;
        });
      builder.Services.AddScoped(sp => sp.GetService<IHttpClientFactory>()
         .CreateClient(Name));
    }



    //Doc: https://bitofvg.wordpress.com/2021/01/22/blazor-wasm-load-appsettings-environment-subenvironment/
    private static async Task AddSubEnvironmentConfiguration(WebAssemblyHostBuilder builder) {
      Log.Information("Loading Sub-Environment....");
      var subenv = builder.Configuration["SubEnvironment"];
      Log.Information("SubEnvironment:"+ subenv);

      var settingsfile = $"appsettings.{builder.HostEnvironment.Environment}.{subenv}.json";

      using (var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }) {
        using (var appsettingsResponse = await http.GetAsync(settingsfile)) {
          using (var stream = await appsettingsResponse.Content.ReadAsStreamAsync()) {
            builder.Configuration.AddJsonStream(stream);
          };
        };
      };
    }

    //Doc: https://bitofvg.wordpress.com/2021/01/27/blazor-wasm-serilog-con-log-level-dinamico/
    private static void LoggingConfiguration(WebAssemblyHostBuilder builder) {
      var levelSwitch = new MyLoggingLevelSwitch();
      Log.Logger = new LoggerConfiguration()
      .MinimumLevel.ControlledBy(levelSwitch)
      .Enrich.FromLogContext()
      .WriteTo.BrowserConsole()
      .CreateLogger();

      builder.Services.AddSingleton<IMyLoggingLevelSwitch>(levelSwitch);

      Log.Information("Logging ready!");
    }

  }
}
