using Errors.Models;
using System.Globalization;
using System.Windows.Data;

namespace Utilities.Diagnostics
{
  /// <summary>
  /// Универсальный конвертер для получения тега предупреждения или ошибки.
  /// Принимает:
  /// - ErrorCode
  /// - WarningCode
  /// - string (CodeString)
  /// - IDiagnosticItem
  /// и возвращает строковой тег (атака TRN001, WARNGEN005).
  /// </summary
  public class DiagnosticCodeToTagConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is null)
        return string.Empty;

      // Если пришёл ErrorItem / WarningItem / любой диагностический объект
      if (value is IDisplayIssue item)
      {
        return item.CodeString ?? string.Empty;
      }

      // Если пришёл ErrorCode
      if (value is ErrorCode errCode)
      {
        return errCode.GetTag() ?? string.Empty;
      }

      // Если WarningCode
      if (value is WarningCode warnCode)
      {
        return warnCode.GetTag() ?? string.Empty;
      }

      // Если пришла строка (CodeString)
      if (value is string str)
      {
        return str;
      }

      return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
