using DTO.Attributes.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Enum.Unit
{
  /// <summary>
  /// Единицы измерения сопротивления.
  /// </summary>
  public enum ResistanceUnit
  {
    /// <summary>
    /// Ом (Ω).
    /// </summary>
    //[UnitDisplay("Ом, R")]
    Ohm,

    /// <summary>
    /// Килоом (кΩ).
    /// </summary>
    //[UnitDisplay("КОм, R")]
    KiloOhm,

    /// <summary>
    /// Мегаом (МΩ).
    /// </summary>
   // [UnitDisplay("МОм, R")]
    MegaOhm,

    /// <summary>
    /// Гигаом (ГΩ).
    /// </summary>
   // [UnitDisplay("ГОм, R")]
    GigaOhm
  }
}
