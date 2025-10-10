using EventCore.Events;
using EventCore.Services;

namespace EventCore.Adapters
{
  /// <summary>
  /// Предоставляет адаптационный слой между старой моделью управления состоянием системы
  /// и новой архитектурой событий <see cref="EventCore"/>.
  /// </summary>
  /// <remarks>
  /// Адаптер используется для генерации событий <see cref="SystemStateEvents"/>,
  /// обеспечивая обратную совместимость со старой системой через привычные методы
  /// (<see cref="RaisePowerChanged"/>, <see cref="RaiseLockedChanged"/>, <see cref="RaiseAdminRightsChanged"/>).
  /// </remarks>
  public static class SystemStateEventAdapter
  {
    /// <summary>
    /// Генерирует событие изменения состояния питания.
    /// </summary>
    /// <param name="isPowered">Новое состояние питания: true — питание включено; false — отключено.</param>
    /// <example>
    /// <code>
    /// SystemStateEventAdapter.RaisePowerChanged(true);
    /// </code>
    /// </example>
    public static void RaisePowerChanged(bool isPowered)
    {
      EventAggregator.Publish(new SystemStateEvents.PowerChanged(isPowered));
    }

    /// <summary>
    /// Генерирует событие изменения состояния блокировки интерфейса.
    /// </summary>
    /// <param name="isLocked">Новое состояние блокировки: true — интерфейс заблокирован; false — разблокирован.</param>
    /// <example>
    /// <code>
    /// SystemStateEventAdapter.RaiseLockedChanged(false);
    /// </code>
    /// </example>
    public static void RaiseLockedChanged(bool isLocked)
    {
      EventAggregator.Publish(new SystemStateEvents.LockedChanged(isLocked));
    }

    /// <summary>
    /// Генерирует событие изменения прав администратора.
    /// </summary>
    /// <param name="isAdmin">Новое состояние прав администратора: true — активен режим администратора; false — обычный пользователь.</param>
    /// <example>
    /// <code>
    /// SystemStateEventAdapter.RaiseAdminRightsChanged(true);
    /// </code>
    /// </example>
    public static void RaiseAdminRightsChanged(bool isAdmin)
    {
      EventAggregator.Publish(new SystemStateEvents.AdminRightsChanged(isAdmin));
    }
  }
}
