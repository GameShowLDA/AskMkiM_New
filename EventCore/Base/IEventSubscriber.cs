using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.Base
{
  /// <summary>
  /// Определяет поведение подписчика событий.
  /// Предоставляет методы для подписки и отписки от событий определённого типа.
  /// </summary>
  public interface IEventSubscriber
  {
    /// <summary>
    /// Регистрирует обработчик для указанного типа события.
    /// </summary>
    /// <typeparam name="TEvent">Тип события, реализующий <see cref="IEvent"/>.</typeparam>
    /// <param name="handler">Метод, вызываемый при публикации события данного типа.</param>
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent;

    /// <summary>
    /// Удаляет обработчик из списка подписчиков указанного типа события.
    /// </summary>
    /// <typeparam name="TEvent">Тип события, реализующий <see cref="IEvent"/>.</typeparam>
    /// <param name="handler">Метод, который нужно отписать от события.</param>
    void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent;
  }
}
