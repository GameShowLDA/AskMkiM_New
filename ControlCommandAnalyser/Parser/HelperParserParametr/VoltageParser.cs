using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ControlCommandAnalyser.Parser.HelperParserParametr
{
  public class VoltageParser
  {
    /// <summary>
    /// Парсит выражение напряжения вида "220В", "49.2 мВ", "1,5 кВ".
    /// </summary>
    /// <param name="input">Входная строка.</param>
    /// <returns>
    /// Кортеж: 
    /// - Value — числовое значение напряжения в виде строки (если найдено),
    /// - Unit — единица измерения напряжения (В, кВ, мВ, МВ),
    /// - Remainder — остаток строки после удаления выражения.
    /// </returns>
    public (string? Value, string? Unit, string Remainder) ParseVoltage(string input)
    {
      var match = Regex.Match(input,
                              @"(?<val>\d+(?:[.,]\d+)?)\s*(?<unit>В|кВ|КВ|мВ|МВ)",
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
