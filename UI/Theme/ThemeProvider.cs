using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UI.Theme
{
  public static class ThemeProvider
  {
    private static readonly string[] ColorKeys =
    {
            "LightPrimaryColor", "PrimaryColor", "SecondaryColor", "ForegroundColor",
            "ForegroundColor60", "ActiveColor", "ActiveColor80", "IsCheckedColor",
            "IsCheckedColor80", "RedColor", "GreenColor", "YellowColor", "LightBlueColor"
        };

    /// <summary>
    /// Загружает все цвета из указанного ресурса и возвращает их как словарь.
    /// </summary>
    public static Dictionary<string, Color> LoadColors(ResourceManager resourceManager)
    {
      var colors = new Dictionary<string, Color>();

      foreach (var key in ColorKeys)
      {
        string hex = resourceManager.GetString(key);
        if (string.IsNullOrWhiteSpace(hex))
          continue;

        colors[key] = (Color)ColorConverter.ConvertFromString(hex);
      }

      return colors;
    }
  }
}
