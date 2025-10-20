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
          ? new Uri("/UI;component/Resources/Theme/Colors.Dark.xaml", UriKind.Relative)
          : new Uri("/UI;component/Resources/Theme/Colors.Light.xaml", UriKind.Relative);

      var newThemeDict = (ResourceDictionary)Application.LoadComponent(uri);
      LogInformation($"[ThemeManager] Словарь успешно загружен: {uri}");

      // ❗ Удаляем ТОЛЬКО предыдущий словарь темы
      var oldTheme = Application.Current.Resources.MergedDictionaries
          .FirstOrDefault(d => d.Source != null &&
              (d.Source.ToString().Contains("Colors.Dark.xaml") ||
               d.Source.ToString().Contains("Colors.Light.xaml")));

      if (oldTheme != null)
      {
        Application.Current.Resources.MergedDictionaries.Remove(oldTheme);
        LogInformation("[ThemeManager] Старый словарь темы удалён");
      }

      // ✅ Добавляем новый словарь в начало, чтобы он перекрывал другие
      Application.Current.Resources.MergedDictionaries.Insert(0, newThemeDict);

      LogInformation($"[ThemeManager] Применена тема: {theme}. Всего словарей: {Application.Current.Resources.MergedDictionaries.Count}");

      // ✅ Перерисовываем визуальное дерево
      foreach (Window window in Application.Current.Windows)
      {
        window.Resources.MergedDictionaries.Clear();
        window.Resources.MergedDictionaries.Add(newThemeDict);
        window.InvalidateVisual();
        window.UpdateLayout();
      }
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
