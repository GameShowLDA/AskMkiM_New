using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Enum.Unit
{
  /// <summary>
  /// Обозначение физической величины (R, U, I, C и т.д.).
  /// </summary>
  public enum QuantitySymbol
  {
    /// <summary>
    /// Сопротивление (R).
    /// </summary>
    R,

    /// <summary>
    /// Напряжение (U).
    /// </summary>
    U,

    /// <summary>
    /// Ток (I).
    /// </summary>
    I,

    /// <summary>
    /// Ёмкость (C).
    /// </summary>
    C
  }
}
