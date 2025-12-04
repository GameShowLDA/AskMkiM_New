using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCommandAnalyser.Attributes
{
  [AttributeUsage(AttributeTargets.Class)]
  public class ResistanceRangeAttribute : Attribute
  {
    public double Min { get; }
    public double Max { get; }
    public double DefaultLower { get; }

    public ResistanceRangeAttribute(double min, double max, double defaultLower)
    {
      Min = min;
      Max = max;
      DefaultLower = defaultLower;
    }
  }
}
