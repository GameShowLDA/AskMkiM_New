using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO.Attributes;

namespace DTO.Enum
{
  public class Metrology
  {
    /// <summary>
    /// Перечисление, представляющее различные типы команд в системе.
    /// </summary>
    public enum MetrologyTypeCommand
    {
      [CommandDisplayInfo("КС", "Ом")]
      /// <summary>
      /// Тип команды KC.
      /// </summary>
      KC,

      [CommandDisplayInfo("ПР", "Ом")]
      /// <summary>
      /// Тип команды PR.
      /// </summary>
      PR,

      [CommandDisplayInfo("СИ", "МОм")]
      /// <summary>
      /// Тип команды CI.
      /// </summary>
      CI,

      [CommandDisplayInfo("ИЕ", "нФ")]
      /// <summary>
      /// Тип команды IE.
      /// </summary>
      IE,

      [CommandDisplayInfo("КН_ACW", "В")]
      /// <summary>
      /// Тип команды KN переменным током.
      /// </summary>
      KN_ACW,

      [CommandDisplayInfo("КН_DCW", "В")]

      /// <summary>
      /// Тип команды KN постоянным током.
      /// </summary>
      KN_DCW,

      [CommandDisplayInfo("PI_ACW", "В")]
      /// <summary>
      /// Тип команды PI переменным током.
      /// </summary>
      PI_ACW,

      [CommandDisplayInfo("PI_DCW", "В")]
      /// <summary>
      /// Тип команды PI постоянным током.
      /// </summary>
      PI_DCW,
    }
  }
}
