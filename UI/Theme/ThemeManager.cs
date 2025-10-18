using System;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Media;
using AppConfiguration.Parameter;
using DTO.Enum;
using UI.Resources.Theme;

namespace UI.Theme
{
  public static class ThemeManager
  {
    /// <summary>
    /// Применяет тему: "Dark" или "Light"
    /// </summary>
    public static void ApplyThemeAsync(ThemeEnums.Theme theme)
    {
      Application.Current.Resources.MergedDictionaries.Clear();

      // 1. Сначала подключаем цвета (Dark или Light)
      var colorsUri = theme == ThemeEnums.Theme.Dark
          ? new Uri("/UI;component/Resources/Theme/Colors.Dark.xaml", UriKind.Relative)
          : new Uri("/UI;component/Resources/Theme/Colors.Light.xaml", UriKind.Relative);

      Application.Current.Resources.MergedDictionaries.Add(
          new ResourceDictionary { Source = colorsUri });
    }

    static ThemeManager()
    {
      ThemeSettings.ThemeChanged += theme =>
      {
        // Если вызов не из UI-потока — перебросим
        if (Application.Current.Dispatcher.CheckAccess())
          ApplyThemeAsync(theme);
        else
          Application.Current.Dispatcher.Invoke(() => ApplyThemeAsync(theme));
      };
    }
  }
}
