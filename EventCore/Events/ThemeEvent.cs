using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO.Enum;
using EventCore.Interfaces;

namespace EventCore.Events
{
  /// <summary>
  /// Содержит события пользовательского интерфейса, связанные с изменением темы оформления.
  /// </summary>
  public class ThemeEvent
  {
    /// <summary>
    /// Событие запроса смены темы интерфейса.
    /// </summary>
    public class Change : IEvent
    {
      /// <summary>
      /// Новая тема, которую необходимо применить.
      /// </summary>
      public ThemeEnums.Theme NewTheme { get; }

      /// <summary>
      /// Инициализирует событие смены темы интерфейса.
      /// </summary>
      /// <param name="newTheme">Тема, которую требуется применить (Light или Dark).</param>
      public Change(ThemeEnums.Theme newTheme)
      {
        NewTheme = newTheme;
      }
    }

    /// <summary>
    /// Событие, обозначающее, что тема была успешно применена.
    /// </summary>
    public class Changed : IEvent
    {
      /// <summary>
      /// Новая активная тема интерфейса.
      /// </summary>
      public ThemeEnums.Theme ActiveTheme { get; }

      /// <summary>
      /// Инициализирует событие подтверждения смены темы.
      /// </summary>
      /// <param name="activeTheme">Тема, которая теперь активна.</param>
      public Changed(ThemeEnums.Theme activeTheme)
      {
        ActiveTheme = activeTheme;
      }
    }

    /// <summary>
    /// Событие, обозначающее, что флаг подствеки была успешно применена.
    /// </summary>
    public class SyntaxHighlighting : IEvent
    {
      /// <summary>
      /// Флаг, отображающий подстветку темы.
      /// </summary>
      public bool IsEnabled { get; }

      /// <summary>
      /// Инициализирует событие подтверждения смены подстветки синтаксиса.
      /// </summary>
      /// <param name="enabled">Флаг подстветки синтаксиса.</param>
      public SyntaxHighlighting(bool enabled)
      {
        IsEnabled = enabled;
      }
    }
  }
}
