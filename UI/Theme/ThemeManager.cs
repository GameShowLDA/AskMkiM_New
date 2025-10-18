using System;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Media;
using AppConfiguration.Parameter;
using UI.Resources.Theme;

namespace UI.Theme
{
  public static class ThemeManager
  {
    /// <summary>
    /// Применяет тему: "Dark" или "Light"
    /// </summary>
    public static void ApplyThemeAsync(DTO.Enum.ThemeEnums.Theme theme)
    {
      if (Application.Current.Dispatcher.CheckAccess())
      {
        ApplyThemeInternal(theme);
      }
      else
      {
        Application.Current.Dispatcher.Invoke(() => ApplyThemeInternal(theme));
      }
    }

    /// <summary>
    /// Вся логика смены темы выполняется только в UI-потоке
    /// </summary>
    private static void ApplyThemeInternal(DTO.Enum.ThemeEnums.Theme theme)
    {
      ResourceManager manager = theme == DTO.Enum.ThemeEnums.Theme.Light
          ? new ResourceManager("UI.Resources.Theme.Colors.Light", typeof(Resources.Theme.Colors).Assembly)
          : new ResourceManager("UI.Resources.Theme.Colors", typeof(Resources.Theme.Colors).Assembly);

      var colors = ThemeProvider.LoadColors(manager);

      foreach (var kvp in colors)
      {
        var brushKey = $"{kvp.Key}Brush";

        if (Application.Current.Resources[brushKey] is SolidColorBrush existingBrush)
        {
          if (existingBrush.IsFrozen)
          {
            Application.Current.Resources[brushKey] = new SolidColorBrush(kvp.Value);
          }
          else
          {
            existingBrush.Color = kvp.Value;
          }
        }
        else
        {
          Application.Current.Resources[brushKey] = new SolidColorBrush(kvp.Value);
        }
      }

    }

    static ThemeManager()
    {
      ThemeSettings.ThemeChanged += (s) => ApplyThemeAsync(s);
    }
  }
}
