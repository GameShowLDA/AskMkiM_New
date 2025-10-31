using Errors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Errors
{
  /// <summary>
  /// Содержит стандартные исключения, используемые при возникновении непредвиденных (необработанных) ошибок в системе.
  /// Используется как универсальный обработчик для ситуаций, которых не должно происходить при нормальной работе.
  /// </summary>
  public static class SystemUnexpectedErrors
  {
    /// <summary>
    /// Исключение: произошло неизвестное или непредвиденное исключение.
    /// </summary>
    /// <param name="ex">Оригинальное исключение (если доступно).</param>
    /// <returns><see cref="SystemExceptionBase"/> с обобщённым описанием.</returns>
    public static SystemExceptionBase Unexpected(Exception ex = null) =>
      new(new ErrorItem
      {
        Code = ErrorCode.Unknown,
        Description = ex == null
          ? "Произошла неизвестная ошибка, которая не должна была возникнуть."
          : $"Неожиданная ошибка: {ex.Message}"
      });
  }
}
