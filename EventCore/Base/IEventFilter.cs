using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.Base
{
  /// <summary>
  /// Определяет механизм фильтрации событий перед их обработкой подписчиками.
  /// Используется для ограничения или маршрутизации событий по заданным критериям.
  /// </summary>
  public interface IEventFilter
  {
    /// <summary>
    /// Определяет, должен ли обработчик получить данное событие.
    /// </summary>
    /// <param name="eventData">Экземпляр события, прошедшего через систему.</param>
    /// <returns>Возвращает true, если событие должно быть обработано; иначе false.</returns>
    bool ShouldProcess(IEvent eventData);
  }
}
