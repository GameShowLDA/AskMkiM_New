using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.Base
{
  /// <summary>
  /// Определяет поведение издателя событий.
  /// Предоставляет методы для публикации событий и уведомления всех активных подписчиков.
  /// </summary>
  public interface IEventPublisher
  {
    /// <summary>
    /// Публикует событие указанного типа и уведомляет всех зарегистрированных подписчиков.
    /// </summary>
    /// <typeparam name="TEvent">Тип события, реализующий <see cref="IEvent"/>.</typeparam>
    /// <param name="eventData">Экземпляр события, содержащий данные для передачи подписчикам.</param>
    void Publish<TEvent>(TEvent eventData) where TEvent : IEvent;
  }
}
