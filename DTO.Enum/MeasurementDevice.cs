using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Enum
{
  /// <summary>
  /// Тип измерительного прибора, используемого командой.
  /// </summary>
  public enum MeasurementDevice
  {
    /// <summary>
    /// Команда не использует измерительный прибор.
    /// </summary>
    None = 0,

    /// <summary>
    /// Прецизионный мультиметр (режим ПР, измерение сопротивления, напряжения и т.д.).
    /// </summary>
    Multimeter = 1,

    /// <summary>
    /// Пробойная установка (испытание изоляции, пробой, режимы ПИ/ПЭ/ЭТ/и др.).
    /// </summary>
    BreakdownTester = 2
  }
}
