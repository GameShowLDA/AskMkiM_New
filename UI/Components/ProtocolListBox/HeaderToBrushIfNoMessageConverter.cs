using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DTO.Base.Models;

namespace UI.Components.ProtocolListBox
{
  public class HeaderToBrushIfNoMessageConverter : IMultiValueConverter
  {
    // Зеленый цвет "НОРМА"
    private static readonly SolidColorBrush SuccessBrush = new SolidColorBrush(ShowMessageModel.SuccessMessage.TitleColor);

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      string header = values[0] as string;
      string message = values[1] as string;
      SolidColorBrush headerColor = values[2] as SolidColorBrush;

      if (!string.IsNullOrEmpty(header) && string.IsNullOrEmpty(message))
      {
        if (AppConfiguration.Parameter.ParameterConfig.GetSyntaxHighlighting())
        {
          return SuccessBrush;
        }

        return new SolidColorBrush((Color)Application.Current.Resources["tests.protocol.message.header.foreground"]);
      }
      return headerColor ?? new SolidColorBrush(Colors.White); // Цвет по умолчанию
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
