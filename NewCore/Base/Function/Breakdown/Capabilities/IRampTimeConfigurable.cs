using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Interface;

namespace NewCore.Base.Function.Breakdown.Capabilities
{
  /// <summary>
  /// Интерфейс для режимов, поддерживающих установку и получение времени нарастания напряжения (Ramp Time).
  /// </summary>
  public interface IRampTimeConfigurable
  {
    /// <summary>
    /// Устанавливает время нарастания напряжения (Ramp Time) и проверяет, что устройство приняло значение.
    /// </summary>
    /// <param name="value">
    /// Время нарастания в секундах (обычно в диапазоне 0.1 – 999.9).
    /// </param>
    /// <param name="userMessageService">
    /// (Необязательно) Сервис для отображения сообщений пользователю.  
    /// Может быть <c>null</c>, если сообщения выводить не требуется.
    /// </param>
    /// <returns>
    /// Кортеж: 
    /// <list type="bullet">
    ///   <item><description><c>bool Success</c> — признак успешного выполнения операции.</description></item>
    ///   <item><description><c>string Message</c> — сообщение об ошибке, если установка не удалась.</description></item>
    /// </list>
    /// </returns>
    Task<(bool Success, string Message)> SetRampTimeAsync(double value, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Получает текущее установленное время нарастания напряжения (Ramp Time).
    /// </summary>
    /// <returns>
    /// Значение времени нарастания напряжения в секундах.  
    /// В случае ошибок может возвращаться 0 или иное значение по умолчанию,
    /// в зависимости от реализации.
    /// </returns>
    Task<double> GetRampTimeAsync();
  }
}
