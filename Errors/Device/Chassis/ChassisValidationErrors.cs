using Errors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Errors.Device.Chassis
{
  /// <summary>
  /// Содержит стандартные ошибки, возникающие при проверке шасси —
  /// его наличия, адреса, корректности подключения и структуры модулей.
  /// </summary>
  public static class ChassisValidationErrors
  {
    public static SystemExceptionBase NotFound(int number) =>
      new(new ErrorItem
      {
        Code = ErrorCode.Equipment_ChassisNotFound,
        Description = $"Шасси с номером {number} не найдено в конфигурации."
      });
  }
}
