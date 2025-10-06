using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Interface;

namespace NewCore.Base.Function.Breakdown.Capabilities
{
  /// <summary>
  /// Интерфейс для режимов, поддерживающих установку и получение параметра времени теста.
  /// </summary>
  public interface ITimeTestConfigurable
  {
    /// <summary>
    /// Устанавливает время теста и проверяет, что устройство приняло значение.
    /// </summary>
    /// <param name="value">
    /// Значение времени в секундах, которое требуется задать устройству.
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
    Task<(bool Success, string Message)> SetTestTimeAsync(double value, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Получает текущее установленное время теста с устройства.
    /// </summary>
    /// <returns>
    /// Время теста в секундах, считанное с устройства.  
    /// В случае ошибок может возвращаться 0 или иное значение по умолчанию,
    /// в зависимости от реализации.
    /// </returns>
    Task<double> GetTestTimeAsync();
  }
}
