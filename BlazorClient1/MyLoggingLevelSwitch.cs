using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorClient1 {
  public class MyLoggingLevelSwitch :
    LoggingLevelSwitch, IMyLoggingLevelSwitch {
    public new LogEventLevel MinimumLevel {
      get => base.MinimumLevel;
      set => base.MinimumLevel = value;
    }
  }

  public interface IMyLoggingLevelSwitch {
    public LogEventLevel MinimumLevel { get; set; }
  }
}
