using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ControlCommandAnalyser.Parser.HelperParserParametr
{
  public class TimeParser
  {
    /// <summary>
    /// Парсит выражение времени вида "10мс", "200 ms", "5 с".
    /// </summary>
    /// <param name="input">Входная строка.</param>
    /// <returns>
    /// Кортеж: 
    /// - Value — числовое значение времени в виде строки (если найдено),
    /// - Unit — единица измерения времени (мс, ms, с, c),
    /// - Remainder — остаток строки после удаления выражения.
    /// </returns>
    public (string? Value, string? Unit, string Remainder) ParseTime(string input)
    {
      var match = Regex.Match(
                  input,
                  @"(?<val>\d+(?:[.,]\d+)?)\s*(?<unit>мс|ms|с|c)",
                  RegexOptions.IgnoreCase);

      if (match.Success)
      {
        string value = match.Groups["val"].Value;
        string unit = match.Groups["unit"].Value;
        string remainder = input.Remove(match.Index, match.Length).Trim(' ', ',');

        return (value, unit, remainder);
      }

      return (null, null, input);
    }
  }
}
