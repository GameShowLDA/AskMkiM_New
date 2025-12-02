using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UI.Components.MyToolTip
{
  public static class ToolTipHelper
  {
    public static string GetDescriptionKey(DependencyObject obj)
    {
      return (string)obj.GetValue(DescriptionKeyProperty);
    }

    public static void SetDescriptionKey(DependencyObject obj, string value)
    {
      obj.SetValue(DescriptionKeyProperty, value);
    }

    public static readonly DependencyProperty DescriptionKeyProperty =
        DependencyProperty.RegisterAttached(
            "DescriptionKey",
            typeof(string),
            typeof(ToolTipHelper),
            new PropertyMetadata(null, OnDescriptionKeyChanged));

    private static void OnDescriptionKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is FrameworkElement element && e.NewValue is string key)
      {
        // 1) ищем текст подсказки по ключу
        var tooltipInfo = ToolTipProvider.GetDescription(key);

        // 2) создаём наш кастомный tooltip
        var tip = new CustomToolTip
        {
          Title = tooltipInfo.Title,
          Description = tooltipInfo.Description
        };

        // 3) назначаем
        element.ToolTip = tip;
      }
    }
  }
}
