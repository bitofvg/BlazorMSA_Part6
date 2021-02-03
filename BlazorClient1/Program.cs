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
      AddHttpClient(builder, Cfg.HttpClients.WebApi1, Cfg.ServicesUrls.WebApi1, new[] {"WApi1.Weather.List"});

      // Registers a named HttpClient for the IdentityServer local Apis
      AddHttpClient(builder, Cfg.HttpClients.IdServer, Cfg.ServicesUrls.IdServer,
        new[] {
          "IdentityServer.Users.List",
          "IdentityServer.Users.Add"
        });

      builder.Services.AddOidcAuthentication(options => {
        // load Oidc options for the Identity Server authentication.
        builder.Configuration.Bind("oidc", options.ProviderOptions);
        options.ProviderOptions.Authority = Cfg.ServicesUrls.IdServer + "/";
        options.ProviderOptions.PostLogoutRedirectUri = Cfg.ServicesUrls.BlazorClient1 + "/";
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


    private static void AddHttpClient(WebAssemblyHostBuilder builder, string Name, string BaseAddress, IEnumerable<string> scopes) {
      var httpCliBuilder = builder.Services.AddHttpClient(Name, hc => hc.BaseAddress = new Uri(BaseAddress));
      if (scopes is not null)
        httpCliBuilder.AddHttpMessageHandler(sp => {
          var handler = sp.GetService<AuthorizationMessageHandler>()
            .ConfigureHandler(new[] { BaseAddress }, scopes);
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
