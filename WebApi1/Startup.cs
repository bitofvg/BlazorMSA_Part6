using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi1 {
  public class Startup {
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
      Cfg.Init(Configuration);
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {

      services.AddControllers();
      services.AddSwaggerGen(c => {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApi1", Version = "v1" });
      });

      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => {
          options.Authority = Cfg.ServicesUrls.IdServer;
          // the requested ApiScopes MUST belong to the ApiResource "WApi1"
          options.Audience = IdNames.ApiRes.WApi;
          //To avoid Audience verify:
          //options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false };
        });

      services.AddAuthorization(options => {
        options.AddPolicy(Cfg.Policies.WeatherList, policyAdmin => {
          policyAdmin.RequireScope(IdNames.Scopes.WApi1WeatherRead);
          policyAdmin.RequireClaim(IdNames.Claims.WApi1_Weather_List);
        });
      });
      services.AddAuthorization(options => {
        options.AddPolicy(Cfg.Policies.WeatherGetById, policyAdmin => {
          policyAdmin.RequireScope(IdNames.Scopes.WApi1WeatherRead);
          policyAdmin.RequireClaim(IdNames.Claims.WApi1_Weather_GetById);
        });
      });


      services.AddCors(options => { // this defines a CORS policy called "CORSPolicy"
        options.AddPolicy(Cfg.CORSPolicy, builder => {
          builder.WithOrigins(Cfg.ServicesUrls.BlazorClient1)
                 .AllowAnyHeader();
        });
      });

    }




    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApi1 v1"));
      }

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseCors(Cfg.CORSPolicy); // This MUST be placed after "app.UseRouting();"

      //authentication will be performed automatically on every call
      app.UseAuthentication();
      //endpoint cannot be accessed by anonymous clients.
      app.UseAuthorization();

      app.UseEndpoints(endpoints => {
        endpoints.MapControllers();
      });
    }
  }
}
