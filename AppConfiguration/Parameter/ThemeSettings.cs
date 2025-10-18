using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO.Enum;

namespace AppConfiguration.Parameter
{
  public static class ThemeSettings
  {
    private static ThemeEnums.Theme _currentTheme = ThemeEnums.Theme.Dark;

    public static ThemeEnums.Theme CurrentTheme => _currentTheme;

    public static event Action<ThemeEnums.Theme>? ThemeChanged;

    /// <summary>
    /// Загружает язык из конфигурации при запуске.
    /// </summary>
    public static async Task InitializeAsync()
    {
      var themeItem = await ParameterConfig.GetTheme();
      ThemeChanged?.Invoke(themeItem);
    }

    /// <summary>
    /// Устанавливает новый язык и сохраняет его в конфигурации.
    /// </summary>
    public static async Task SetThemeAsync(ThemeEnums.Theme theme)
    {
      if (theme == _currentTheme)
        return;

      _currentTheme = theme;
      ThemeChanged?.Invoke(_currentTheme);
    }
  }
}
