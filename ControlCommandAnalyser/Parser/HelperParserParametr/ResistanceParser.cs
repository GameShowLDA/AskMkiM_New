using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ControlCommandAnalyser.Parser.HelperParserParametr
{
  public class ResistanceParser
  {
    /// <summary>
    /// Извлекает пороговое сопротивление в формате "R&gt;100МОм".
    /// Поддерживаются варианты "R > X Ом/МОм/ГОм".
    /// </summary>
    /// <param name="input">Входная строка.</param>
    /// <returns>
    /// Кортеж:
    /// - ThresholdResistance — найденное сопротивление или null,  
    /// - Remainder — остаток строки без сопротивления.
    /// </returns>
    public (string? ThresholdResistance, string Remainder) ParseThresholdResistance(string input)
    {
      // Совпадает с форматом: R>100МОм или R > 100МОм
      var match = Regex.Match(input, @"R\s*>\s*\d+\s*(Ом|МОм|ГОм|кОм)", RegexOptions.IgnoreCase);
      if (match.Success)
      {
        var resistance = match.Value.Trim();
        var remainder = input.Remove(match.Index, match.Length).Trim(' ', ',');
        return (resistance, remainder);
      }
      return (null, input);
    }
    /// <summary>
    /// Извлекает выражение сопротивления вида:
    /// "R &lt; 20 МОм", "100 &lt;= МОм" и т. п.
    /// </summary>
    /// <param name="input">Входная строка.</param>
    /// <returns>
    /// Кортеж:
    /// - Value — числовое значение сопротивления или null,  
    /// - Unit — единица измерения (Ом, кОм, МОм, ГОм),  
    /// - Operator — знак сравнения (&lt;, &lt;=, &gt;, &gt;=, =, ≤, ≥),  
    /// - Remainder — остаток строки без выражения.
    /// </returns>
    public (string? Value, string? Unit, string Remainder) ParseResistance(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
        return (null, null, input);

      // Вариант 1: "R < 20 МОм" / "R<=20МОм" (R или русская 'Р')
      var m = Regex.Match(input,
          @"(?<!\w)[RР]\s*(?<op><=|>=|<|>|=|≤|≥)\s*(?<val>\d+(?:[.,]\d+)?)\s*(?<unit>Ом|кОм|МОм|ГОм)\b",
          RegexOptions.IgnoreCase);
      if (m.Success)
      {
        string value = m.Groups["val"].Value;
        string unit = m.Groups["unit"].Value;
        string op = m.Groups["op"].Value;
        string remainder = RemoveMatchedWithNeighborComma(input, m.Index, m.Length);
        return (value, unit, remainder);
      }

      // Вариант 2: "100 < МОм" / "100 <= МОм" и т.п.
      m = Regex.Match(input,
          @"(?<!\w)(?<val>\d+(?:[.,]\d+)?)\s*(?<op><=|>=|<|>|=|≤|≥)\s*(?<unit>Ом|кОм|МОм|ГОм)\b",
          RegexOptions.IgnoreCase);
      if (m.Success)
      {
        string value = m.Groups["val"].Value;
        string unit = m.Groups["unit"].Value;
        string op = m.Groups["op"].Value;
        string remainder = RemoveMatchedWithNeighborComma(input, m.Index, m.Length);
        return (value, unit, remainder);
      }

      return (null, null, input);
    }

    /// <summary>
    /// Парсит выражения сопротивлений вида:
    /// "94&lt;кОм&lt;106", "94&lt;кОм", "кОм&lt;106".
    /// </summary>
    /// <param name="input">Входная строка.</param>
    /// <returns>
    /// Кортеж:
    /// - Min — минимальное значение сопротивления,  
    /// - Max — максимальное значение сопротивления,  
    /// - Unit — единица измерения (Ом, кОм, МОм, ГОм),  
    /// - Remainder — остаток строки без выражения.
    /// </returns>
    public (string? Min, string? Max, string? Unit, string Remainder) ParseResistanceRange(string input)
    {
      var match = Regex.Match(input,
                              @"(?:(?<low>\d+(?:[.,]\d+)?)\s*<\s*)?(?<unit>Ом|кОм|МОм|ГОм)(?:\s*<\s*(?<high>\d+(?:[.,]\d+)?))?",
                              RegexOptions.IgnoreCase);


      if (match.Success)
      {
        string? min = match.Groups["low"].Success ? match.Groups["low"].Value : null;
        string? max = match.Groups["high"].Success ? match.Groups["high"].Value : null;
        string unit = match.Groups["unit"].Value;

        var remainder = Regex.Replace(
          input,
          $@"\b{Regex.Escape(match.Value)}\s*,?",
          "",
          RegexOptions.IgnoreCase
          ).Trim();

        return (min, max, unit, remainder);
      }

      return (null, null, null, input);
    }

    /// <summary>
    /// Парсит выражения сопротивлений с использованием символа R:
    /// "10 Ом &lt; R &lt; 20 Ом", "R &lt; 10 МОм", "10 МОм &lt; R".
    /// </summary>
    /// <param name="input">Входная строка.</param>
    /// <returns>
    /// Кортеж:
    /// - Min — минимальное значение (если есть),  
    /// - Max — максимальное значение (если есть),  
    /// - Unit — единица измерения сопротивления,  
    /// - Remainder — остаток строки.
    /// </returns>
    public (string? Min, string? Max, string? Unit, string Remainder) ParseResistanceRangeWithR(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
        return (null, null, null, input);

      // 1) Диапазон: "10 Ом < R < 20 Ом"
      var m = Regex.Match(input,
          @"(?<!\w)(?<low>\d+(?:[.,]\d+)?)\s*(?<unit1>Ом|кОм|МОм|ГОм)?\s*<\s*[RР]\b\s*<\s*(?<high>\d+(?:[.,]\d+)?)
        \s*(?<unit2>Ом|кОм|МОм|ГОм)?",
          RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
      if (m.Success)
      {
        var unit = m.Groups["unit2"].Success && !string.IsNullOrEmpty(m.Groups["unit2"].Value)
                 ? m.Groups["unit2"].Value
                 : m.Groups["unit1"].Value;
        var remainder1 = RemoveMatchedWithNeighborComma(input, m.Index, m.Length);
        return (m.Groups["low"].Value,
                m.Groups["high"].Value,
                string.IsNullOrEmpty(unit) ? "Ом" : unit,
                remainder1);
      }

      // 2) Порог: "R < 10 Ом" или "R <= 10 Ом" (также ≥, >, >=)
      m = Regex.Match(input,
          @"(?<!\w)[RР]\s*(?<op><=|>=|<|>|≤|≥)\s*(?<val>\d+(?:[.,]\d+)?)\s*(?<unit>Ом|кОм|МОм|ГОм)\b",
          RegexOptions.IgnoreCase);
      if (m.Success)
      {
        var op = m.Groups["op"].Value;
        string? min = null, max = null;
        if (op is "<" or "<=" or "≤") max = m.Groups["val"].Value; else min = m.Groups["val"].Value;
        var remainder2 = RemoveMatchedWithNeighborComma(input, m.Index, m.Length);
        return (min, max, m.Groups["unit"].Value, remainder2);
      }

      // 3) Порог: "10 Ом < R" или "10 Ом <= R"
      m = Regex.Match(input,
          @"(?<!\w)(?<val>\d+(?:[.,]\d+)?)\s*(?<unit>Ом|кОм|МОм|ГОм)\s*(?<op><=|>=|<|>|≤|≥)\s*[RР]\b",
          RegexOptions.IgnoreCase);
      if (m.Success)
      {
        var op = m.Groups["op"].Value;
        string? min = null, max = null;
        if (op is "<" or "<=" or "≤") min = m.Groups["val"].Value; else max = m.Groups["val"].Value;
        var remainder3 = RemoveMatchedWithNeighborComma(input, m.Index, m.Length);
        return (min, max, m.Groups["unit"].Value, remainder3);
      }

      // 4) БЕЗ R: "Ом < 10" / "МОм <= 10"  (единица слева, число справа)
      m = Regex.Match(input,
          @"(?<!\w)(?<unit>Ом|кОм|МОм|ГОм)\s*(?<op><=|>=|<|>|≤|≥)\s*(?<val>\d+(?:[.,]\d+)?)\b",
          RegexOptions.IgnoreCase);
      if (m.Success)
      {
        var op = m.Groups["op"].Value;
        string? min = null, max = null;
        if (op is "<" or "<=" or "≤") max = m.Groups["val"].Value; else min = m.Groups["val"].Value;
        var remainder4 = RemoveMatchedWithNeighborComma(input, m.Index, m.Length);
        return (min, max, m.Groups["unit"].Value, remainder4);
      }

      // 5) БЕЗ R: "10 < МОм" / "10 <= МОм"  (число слева, единица справа)
      m = Regex.Match(input,
          @"(?<!\w)(?<val>\d+(?:[.,]\d+)?)\s*(?<op><=|>=|<|>|≤|≥)\s*(?<unit>Ом|кОм|МОм|ГОм)\b",
          RegexOptions.IgnoreCase);
      if (m.Success)
      {
        var op = m.Groups["op"].Value;
        string? min = null, max = null;
        if (op is "<" or "<=" or "≤") min = m.Groups["val"].Value; else max = m.Groups["val"].Value;
        var remainder5 = RemoveMatchedWithNeighborComma(input, m.Index, m.Length);
        return (min, max, m.Groups["unit"].Value, remainder5);
      }

      // Ничего не нашли
      return (null, null, null, input);
    }

    /// <summary>
    /// Вспомогательный метод для удаления найденного фрагмента
    /// и прилегающей запятой с пробелами.
    /// </summary>
    private string RemoveMatchedWithNeighborComma(string input, int index, int length)
    {
      var left = input.Substring(0, index);
      var right = input.Substring(index + length);

      // Сначала попробуем удалить запятую после найденного блока
      right = Regex.Replace(right, @"^\s*,\s*", "");

      // Если справа не было запятой — удалим запятую слева (если она есть)
      if (right == input.Substring(index + length))
        left = Regex.Replace(left, @"\s*,\s*$", "");

      return (left + right).Trim();
    }
  }
}
