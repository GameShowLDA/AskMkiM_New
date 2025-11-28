using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Errors.Models
{
  [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
  public sealed class WarningCodeTagAttribute : Attribute
  {
    public string Tag { get; }
    public WarningCodeTagAttribute(string tag) => Tag = tag;
  }
}
