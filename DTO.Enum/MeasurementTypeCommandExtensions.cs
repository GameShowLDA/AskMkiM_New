using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Enum
{
  public class MeasurementTypeCommandExtensions
  {
    /// <summary>
    /// Возвращает тип команды ПИ в зависимости от знака тока в теле команды.
    /// </summary>
    public static Measurement.MeasurementTypeCommand ResolvePiBySign(bool isDcw)
    {
      return isDcw
          ? Measurement.MeasurementTypeCommand.PI_DCW
          : Measurement.MeasurementTypeCommand.PI_ACW;
    }
  }
}
