using System;
using System.Linq;
using System.Windows;
using AppConfiguration.Parameter;
using DTO.Enum;
using Utilities;
using static Utilities.LoggerUtility;

namespace UI.Theme
{
  public static class ThemeManager
  {
    private static ResourceDictionary? _currentThemeDict;

    public static void ApplyThemeAsync(ThemeEnums.Theme theme)
    {
      LogInformation($"[ThemeManager] Вызван ApplyThemeAsync. Тема = {theme}");

      var uri = theme == ThemeEnums.Theme.Dark
          ? new Uri("/UI;component/Resources/Theme/dark.xaml", UriKind.Relative)
          : new Uri("/UI;component/Resources/Theme/light.xaml", UriKind.Relative);

      var newThemeDict = (ResourceDictionary)Application.LoadComponent(uri);
      LogInformation($"[ThemeManager] Словарь успешно загружен: {uri}");

      Application.Current.Resources.Clear();
      Application.Current.Resources.MergedDictionaries.Add(newThemeDict);
    }


    private static bool _initialized;

    public static void Initialize()
    {
      if (_initialized) return;
      _initialized = true;
      ThemeSettings.ThemeChanged += ApplyThemeAsync;
    }
  }
}
