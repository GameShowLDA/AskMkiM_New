using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UI.Theme
{
  public static class ThemeProvider
  {

    public static Dictionary<string, Color> LoadColors(ResourceManager resourceManager)
    {
      var colors = new Dictionary<string, Color>();
      var resourceSet = resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

      foreach (System.Collections.DictionaryEntry entry in resourceSet)
      {
        if (entry.Value is string hex && !string.IsNullOrWhiteSpace(hex))
        {
          colors[entry.Key.ToString()] = (Color)ColorConverter.ConvertFromString(hex);
        }
      }

      return colors;
    }
  }
}
