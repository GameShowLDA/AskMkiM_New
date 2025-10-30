using DTO.Attributes;
using DTO.Attributes.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Enum
{
  public class Measurement
  {
    /// <summary>
    /// Перечисление, представляющее различные типы команд в системе.
    /// </summary>
    public enum MeasurementTypeCommand
    {
      [CommandDisplayInfo("КС", "Ом", 1, 10_000_000)]
      /// <summary>
      /// Тип команды KC.
      /// </summary>
      KC,

      [CommandDisplayInfo("ПР", "Ом", 1, 100_000)]
      /// <summary>
      /// Тип команды PR.
      /// </summary>
      PR,

      [CommandDisplayInfo("СИ", "МОм", 1, 1000)]
      /// <summary>
      /// Тип команды CI.
      /// </summary>
      CI,

      [CommandDisplayInfo("ИЕ", "нФ", 0.2, 100000)]
      /// <summary>
      /// Тип команды IE.
      /// </summary>
      IE,

      [CommandDisplayInfo("КН_ACW", "В", 0.1, 250)]
      /// <summary>
      /// Тип команды KN переменным током.
      /// </summary>
      KN_ACW,

      [CommandDisplayInfo("КН_DCW", "В", 0.1, 250)]

      /// <summary>
      /// Тип команды KN постоянным током.
      /// </summary>
      KN_DCW,

      [CommandDisplayInfo("ПИ_ACW", "В", 50, 700)]
      /// <summary>
      /// Тип команды PI переменным током.
      /// </summary>
      PI_ACW,

      [CommandDisplayInfo("ПИ_DCW", "В", 50, 1000)]
      /// <summary>
      /// Тип команды PI постоянным током.
      /// </summary>
      PI_DCW,

      [CommandDisplayInfo("ПИ", "В", 0, 0)]
      /// <summary>
      /// Тип команды PI постоянным током.
      /// </summary>
      PI,

      [CommandDisplayInfo("ЭТ", "Ом", 0.01, 100)]
      /// <summary>
      /// Тип команды EHT постоянным током.
      /// </summary>
      EHT,
    }

    public enum OrganizationalComands
    {
      [CommandOrganizationalAttribute("СП")]
      /// <summary>
      /// Тип команды CP.
      /// </summary>
      CP,

      [CommandOrganizationalAttribute("ЦУ")]
      /// <summary>
      /// Тип команды CU.
      /// </summary>
      CU,
      
      [CommandOrganizationalAttribute("КЦ")]
      /// <summary>
      /// Тип команды KSC.
      /// </summary>
      KSC,
      
      [CommandOrganizationalAttribute("ОК")]
      /// <summary>
      /// Тип команды OK постоянным током.
      /// </summary>
      OK,
      
      [CommandOrganizationalAttribute("РМ")]
      /// <summary>
      /// Тип команды RM.
      /// </summary>
      RM,
      
      [CommandOrganizationalAttribute("УП")]
      /// <summary>
      /// Тип команды UP.
      /// </summary>
      UP,
    }
  }
}
