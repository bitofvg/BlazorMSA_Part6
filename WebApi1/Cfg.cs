// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.Extensions.Configuration;

namespace WebApi1 {

  public static class Cfg {

    public static void Init(IConfiguration Configuration) {
      SubEnv = Configuration["SubEnvironment"];

      ServicesUrls.BlazorClient1 = Configuration["ServicesUrls:BlazorClient1"];
      ServicesUrls.IdServer = Configuration["ServicesUrls:IdServer"];
      ServicesUrls.WebApi1 = Configuration["ServicesUrls:WebApi1"];
    }

    public static string SubEnv { get; set; }

    public const string CORSPolicy = "CORSPolicy";
    public static class Policies {
      public const string WeatherList = "Weather.List";
      public const string WeatherGetById = "Weather.GetById";
    }


    public static class ServicesUrls {
      public static string BlazorClient1 { get; set; }
      public static string IdServer { get; set; }
      public static string WebApi1 { get; set; }
    }


  }
}