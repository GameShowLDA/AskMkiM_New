using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ControlCommandExecutor.BaseStrategies.ConnectedPointChecker;

namespace ControlCommandExecutor.BaseStrategies.Data
{
  internal class ConnectedPointContext : ExecutorContext
  {
    internal PerformMeasurementAsync PerformMeasurementAsync { get; set; }
  }
}
