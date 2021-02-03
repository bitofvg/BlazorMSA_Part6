

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;

namespace BlazorClient1 {


  public static class Cfg {
    public static string SubEnv { get; set; }
    public static void Init(IConfiguration Configuration) {
      SubEnv= Configuration["SubEnvironment"];
      ServicesUrls.BlazorClient1 = Configuration["ServicesUrls:BlazorClient1"];
      ServicesUrls.IdServer = Configuration["ServicesUrls:IdServer"];
      ServicesUrls.WebApi1 = Configuration["ServicesUrls:WebApi1"];
    }

    public static class HttpClients {
      public static readonly string BlazorClient1 = "BlazorClient1";
      public static readonly string IdServer = "IdServer";
      public static readonly string WebApi1 = "WebApi1";
    }

    public static class ServicesUrls {
      public static string BlazorClient1 { get;  set; }
      public static string IdServer { get;  set; }
      public static string WebApi1 { get;  set; }

    }




  }
}