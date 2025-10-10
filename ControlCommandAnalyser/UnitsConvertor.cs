namespace ControlCommandAnalyser
{
  public class UnitsConvertor
  {
    /// <summary>
    /// Безопасно парсит строку и переводит в Омы.
    /// Возвращает null, если строка пустая или содержит ошибку.
    /// </summary>
    public static double? TryParseValue(string valueStr, string unit)
    {
      if (string.IsNullOrWhiteSpace(valueStr))
        return null;

      if (double.TryParse(valueStr.Replace(',', '.'),
                          System.Globalization.NumberStyles.Any,
                          System.Globalization.CultureInfo.InvariantCulture,
                          out double value))
      {
        if (unit.ToLowerInvariant().Contains("ом"))
        {
          return ConvertToOhms(value, unit);
        }
        else if (unit.ToLowerInvariant().Contains("ф"))
        {
          return ConvertToFarads(value, unit);
        }
        else if (unit.ToLowerInvariant().Contains("в"))
        {
          return ConvertToVolts(value, unit);
        }
      }

      return null;
    }


    /// <summary>
    /// Преобразует значение сопротивления в Омы (СИ).
    /// </summary>
    private static double ConvertToOhms(double value, string unit)
    {
      unit = unit.ToLowerInvariant();
      return unit switch
      {
        "ом" => value,
        "ком" => value * 1_000,
        "мом" => value * 1_000_000,
        "гом" => value * 1_000_000_000,
        _ => value
      };
    }

    /// <summary>
    /// Преобразует значение ёмкости в фарады (СИ).
    /// </summary>
    private static double ConvertToFarads(double value, string unit)
    {
      unit = unit.ToLowerInvariant();
      return unit switch
      {
        "ф" => value,                 // Фарад — базовая единица
        "мф" => value * 1e-3,          // миллифарад
        "мкф" => value * 1e-6,          // микрофарад
        "нф" => value * 1e-9,          // нанофарад
        "пф" => value * 1e-12,         // пикофарад
        _ => value                      // если не распознали, возвращаем как есть
      };
    }

    /// <summary>
    /// Преобразует значение напряжения в вольты (СИ).
    /// </summary>
    private static double ConvertToVolts(double value, string unit)
    {
      // Вольты
      if (unit == "МВ") return value * 1e6;

      unit = unit.ToLowerInvariant();
      return unit switch
      {
        "в" => value,
        "мв" => value * 1e-3,
        "кв" => value * 1e3,
        _ => value
      };
    }
  }
}
