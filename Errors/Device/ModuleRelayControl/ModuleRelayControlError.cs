using Errors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Errors.Device.ModuleRelayControl
{
  static public class ModuleRelayControlError
  {
    /// <summary>
    /// Ошибка: команда ИЕ не содержит ни одной границы емкости.
    /// </summary>
    public static ErrorItem PointError(int startLineNumber, string point) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = "МКР Самоконтроль",
      Code = ErrorCode.MKR_PointError,
      Description = $"Ошибка точки {point}"
    };
  }
}
