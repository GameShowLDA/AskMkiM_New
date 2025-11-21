using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ControlCommandExecutor.BaseStrategies.NodeFullChecker;

namespace ControlCommandExecutor.BaseStrategies.Data
{
  internal class NodeFullContext : ExecutorContext
  {
    internal PerformMeasurementAsync PerformMeasurementAsync { get; set; }
  }
}
