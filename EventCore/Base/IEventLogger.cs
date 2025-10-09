using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.Base
{
  /// <summary>
  /// Определяет контракт для системного логирования событий.
  /// Используется для записи информации о публикации, обработке и ошибках при работе событий.
  /// </summary>
  public interface IEventLogger
  {
    /// <summary>
    /// Записывает информацию о публикации события.
    /// </summary>
    /// <param name="eventName">Имя опубликованного события.</param>
    /// <param name="publisher">Источник, вызвавший публикацию.</param>
    void LogPublish(string eventName, string publisher);

    /// <summary>
    /// Записывает информацию о подписке на событие.
    /// </summary>
    /// <param name="eventName">Имя события, на которое выполнена подписка.</param>
    /// <param name="subscriber">Имя подписчика.</param>
    void LogSubscribe(string eventName, string subscriber);

    /// <summary>
    /// Записывает сообщение об ошибке, возникшей при обработке события.
    /// </summary>
    /// <param name="eventName">Имя события, в котором произошла ошибка.</param>
    /// <param name="exception">Информация об исключении.</param>
    void LogError(string eventName, Exception exception);
  }
}
