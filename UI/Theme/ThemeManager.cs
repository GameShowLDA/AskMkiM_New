using System;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Media;
using UI.Resources.Theme;

namespace UI.Theme
{
  public static class ThemeManager
  {
    /// <summary>
    /// Применяет тему: "Dark" или "Light"
    /// </summary>
    public static void ApplyTheme(string theme)
    {
      ResourceManager manager;

      if (theme.Equals("Light", StringComparison.OrdinalIgnoreCase))
      {
        manager = new ResourceManager("UI.Resources.Theme.Colors.Light", typeof(Resources.Theme.Colors).Assembly);
      }
      else
      {
        manager = new ResourceManager("UI.Resources.Theme.Colors", typeof(Resources.Theme.Colors).Assembly);
      }

      var colors = ThemeProvider.LoadColors(manager);

      foreach (var kvp in colors)
      {
        Application.Current.Resources[kvp.Key] = kvp.Value;
        Application.Current.Resources[$"{kvp.Key}Brush"] = new SolidColorBrush(kvp.Value);
      }
    }
  }
}
