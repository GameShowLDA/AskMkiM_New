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
      [CommandDisplayInfo("КС", "Ом", 1.0, 0.5)]
      /// <summary>
      /// Тип команды KC.
      /// </summary>
      KC,

      [CommandDisplayInfo("ПР", "Ом", 1.0, 0.1)]
      /// <summary>
      /// Тип команды PR.
      /// </summary>
      PR,

      [CommandDisplayInfo("СИ", "МОм", 1.0, 0.1)]
      /// <summary>
      /// Тип команды CI.
      /// </summary>
      CI,

      [CommandDisplayInfo("ИЕ", "нФ", 1.0, 0.1)]
      /// <summary>
      /// Тип команды IE.
      /// </summary>
      IE,
    }
  }
}
