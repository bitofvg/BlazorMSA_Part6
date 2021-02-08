

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
      public static readonly string BlazorClient1 = "BlazorClient1_HttpClient";
      public static readonly string IdServer = "IdServer_HttpClient";
      public static readonly string WebApi1 = "WebApi1_HttpClient";
    }

    public static class ServicesUrls {
      public static string BlazorClient1 { get;  set; }
      public static string IdServer { get;  set; }
      public static string WebApi1 { get;  set; }

    }


    public static class Policies {
      public const string WApi1_Weather_List = "WApi1.Weather.List_Policy";
      public const string WApi1_Weather_GetById = "WApi1.Weather.GetById_Policy";
      public const string Users_Add = "IdServer.UsersAdd_Policy";
      public const string Users_List = "IdServer.UsersList_Policy";
      public const string Cerificate_Create = "IdServer.Cerificate.Create_Policy";
    }




  }
}