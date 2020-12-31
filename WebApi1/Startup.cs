using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi1 {
  public class Startup {
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
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
          options.Authority = "https://localhost:5020";
          // the requested ApiScopes MUST belong to the ApiResource "WApi1"
          options.Audience = "WApi1";
          //To avoid Audience verify:
          //options.TokenValidationParameters = new TokenValidationParameters {
          //  ValidateAudience = false
          //};
        });


      services.AddCors(options => { // this defines a CORS policy called "CORSPolicy"
        options.AddPolicy("CORSPolicy", builder => {
          builder.WithOrigins("https://localhost:5001")
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

      app.UseCors("CORSPolicy"); // This MUST be placed after "app.UseRouting();"

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
