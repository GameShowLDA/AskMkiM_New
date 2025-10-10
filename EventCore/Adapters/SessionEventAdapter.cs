using EventCore.Events;
using EventCore.Services;

namespace EventCore.Adapters
{
  /// <summary>
  /// Адаптер для генерации событий <see cref="SessionEvents"/>. 
  /// Предоставляет удобные методы вызова действий, связанных с сохранением и восстановлением сессий.
  /// </summary>
  /// <remarks>
  /// Используется для обратной совместимости со старой системой <c>EventAggregator</c>
  /// и для упрощения вызова событий из различных частей приложения.
  /// </remarks>
  public static class SessionEventAdapter
  {
    /// <summary>
    /// Генерирует событие сохранения текущей сессии пользователя.
    /// </summary>
    /// <example>
    /// <code>
    /// SessionEventAdapter.RaiseSaveSession();
    /// </code>
    /// </example>
    public static void RaiseSaveSession()
      => EventAggregator.Publish(new SessionEvents.SaveSession());

    /// <summary>
    /// Генерирует событие открытия ранее сохранённой сессии пользователя.
    /// </summary>
    /// <example>
    /// <code>
    /// SessionEventAdapter.RaiseOpenSession();
    /// </code>
    /// </example>
    public static void RaiseOpenSession()
      => EventAggregator.Publish(new SessionEvents.OpenSession());
  }
}
