using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Attributes.Attributes
{
  /// <summary>
  /// Человеко-читаемое представление значения enum
  /// (для протоколов, отчётов и логов).
  /// </summary>
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
  public sealed class UnitDisplayAttribute : Attribute
  {
    public string Value { get; }

    /// <summary>
    /// Обозначение величины (например: R, U, I, C).
    /// </summary>
    // public QuantitySymbol Symbol { get; }

    public UnitDisplayAttribute(string value)
    {
      Value = value;
    }
  }
}
