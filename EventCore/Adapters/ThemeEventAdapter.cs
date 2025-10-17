using DTO.Enum;
using EventCore.Events;
using EventCore.Services;
using static DTO.Enum.ThemeEnums;

namespace EventCore.Adapters
{
  /// <summary>
  /// Статический класс, предоставляющий адаптационный слой для управления темой интерфейса
  /// через событийную систему <see cref="EventCore"/>.
  /// </summary>
  public static class ThemeEventAdapter
  {
    /// <summary>
    /// Публикует событие запроса смены темы интерфейса.
    /// </summary>
    /// <param name="newTheme">Тема, которую необходимо применить.</param>
    public static void RaiseChangeTheme(Theme newTheme) =>
      EventAggregator.Publish(new ThemeEvent.Change(newTheme));

    /// <summary>
    /// Публикует событие подтверждения того, что тема успешно изменилась.
    /// </summary>
    /// <param name="activeTheme">Тема, которая теперь активна.</param>
    public static void RaiseThemeChanged(Theme activeTheme) =>
      EventAggregator.Publish(new ThemeEvent.Changed(activeTheme));
  }
}
