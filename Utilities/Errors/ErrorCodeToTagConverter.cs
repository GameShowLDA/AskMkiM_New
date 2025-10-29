using System.Globalization;
using System.Windows.Data;
using Errors.Models;

namespace Utilities.Errors
{
  public class ErrorCodeToTagConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is ErrorCode code)
        return code.GetTag();

      return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
