using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi1.Controllers {
  [ApiController]
  [Route("[controller]/[action]")]
  [Authorize]

  public class WeatherForecastController : ControllerBase {

    private readonly WeatherForecast[] _WeatherForecasts;

    public WeatherForecastController() {
      var Summaries = new[]
            { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
      var rng = new Random();
      _WeatherForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = rng.Next(-20, 55),
        Summary = Summaries[rng.Next(Summaries.Length)]
      }).ToArray();
    }

    [HttpGet]
    [Authorize(Policy = Cfg.Policies.WeatherList)]
    public IEnumerable<WeatherForecast> Get() {
      Thread.Sleep(1000);
      return _WeatherForecasts;
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Cfg.Policies.WeatherGetById)]
    public WeatherForecast GetById(int Id) {
      Thread.Sleep(1000);
      try { return _WeatherForecasts[Id];  }
      catch { };
      return null;
    }


  }
}
