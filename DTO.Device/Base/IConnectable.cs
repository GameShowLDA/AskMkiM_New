using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Interface;

namespace DTO.Device.Base
{
  /// <summary>
  /// Определяет интерфейс для компонентов, требующих инициализации,
  /// подключения и отключения.
  /// </summary>
  public interface IConnectable
  {
    /// <summary>
    /// Событие, которое вызывается при изменении состояния питания.
    /// </summary>
    public event Action IsReset;

    /// <summary>
    /// Выполняет инициализацию компонента, подготавливая его к подключению.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию. Возвращает <c>true</c>, если инициализация прошла успешно; иначе — <c>false</c>.</returns>
    Task<(bool Connect, string Answer)> InitializeAsync(IUserMessageService userMessageService = null);

    /// <summary>
    /// Выполняет подключение к компоненту.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию. Возвращает <c>true</c>, если подключение прошло успешно; иначе — <c>false</c>.</returns>
    Task<(bool Connect, string Answer)> ConnectAsync(IUserMessageService userMessageService = null);

    /// <summary>
    /// Выполняет отключение от компонента.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию. Возвращает <c>true</c>, если отключение прошло успешно; иначе — <c>false</c>.</returns>
    Task<bool> DisconnectAsync(IUserMessageService userMessageService = null);

    /// <summary>
    /// Выполняет сброс устройства.
    /// </summary>
    /// <returns></returns>
    Task<bool> ResetAsync(IUserMessageService userMessageService = null);
  }
}
