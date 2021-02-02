using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi1 {
  public class Program {
    public static void Main(string[] args) {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, builder) => {
              string subenv = context.Configuration["SubEnvironment"];
              if (!string.IsNullOrEmpty(subenv)) {
                var env = context.HostingEnvironment;
                builder.AddJsonFile($"appsettings.{env.EnvironmentName}.{subenv}.json", optional: false, reloadOnChange: true);
              }
            })
            .ConfigureWebHostDefaults(webBuilder => {
              webBuilder.UseStartup<Startup>();
            });
  }
}
