using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ControlCommandAnalyser.Parser.HelperParserParametr
{
  public class CapacityParser
  {
    /// <summary>
    /// Парсит выражения ёскости вида "94&lt;мкф&lt;106", "94&lt;мкф", "мкф&lt;106".
    /// </summary>
    /// <param name="input">Входная строка.</param>
    /// <returns>
    /// Кортеж: 
    /// - Min — минимальное значение сопротивления (если задано),
    /// - Max — максимальное значение сопротивления (если задано),
    /// - Unit — единица измерения сопротивления (Ом, кОм, МОм, ГОм),
    /// - Remainder — остаток строки после удаления выражения.
    /// </returns>
    public (string? Min, string? Max, string? Unit, string Remainder) ParseCapacityRange(string input)
    {
      var match = Regex.Match(input,
                              @"(?:(?<low>\d+(?:[.,]\d+)?)\s*<\s*)?(?<unit>нф|мкф|пф)(?:\s*<\s*(?<high>\d+(?:[.,]\d+)?))?",
                              RegexOptions.IgnoreCase);


      if (match.Success)
      {
        string? min = match.Groups["low"].Success ? match.Groups["low"].Value : null;
        string? max = match.Groups["high"].Success ? match.Groups["high"].Value : null;
        string unit = match.Groups["unit"].Value;

        string remainder = input.Remove(match.Index, match.Length).Trim(' ', ',');

        return (min, max, unit, remainder);
      }

      return (null, null, null, input);
    }
  }
}
