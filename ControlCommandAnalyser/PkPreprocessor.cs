using System.Text.RegularExpressions;

namespace ControlCommandAnalyser
{
  public static class PkPreprocessor
  {
    private const int MaxShortCommentLines = 2; // 1–2 строки оставляем, >2 — удаляем

    private static int CountLines(string s)
    {
      // количество строк = число переводов строк + 1, но нам важно только "больше 2"
      int nl = 0;
      foreach (var ch in s) if (ch == '\n') nl++;
      return nl + 1;
    }

    public static string PreprocessText(string text)
    {
      if (string.IsNullOrEmpty(text)) return text;

      // нормализуем переводы строк, чтобы подсчёт был корректным
      text = text.Replace("\r\n", "\n");

      // 1) /* ... */ — удаляем только если > 2 строк
      text = Regex.Replace(
          text,
          @"/\*[\s\S]*?\*/",
          m => CountLines(m.Value) > MaxShortCommentLines ? "" : m.Value,
          RegexOptions.Singleline);

      // 2) { ... } — удаляем только если > 2 строк
      // (простой вариант: без поддержки вложенности)
      text = Regex.Replace(
          text,
          @"\{[\s\S]*?\}",
          m => CountLines(m.Value) > MaxShortCommentLines ? "" : m.Value,
          RegexOptions.Singleline);

      return text;
    }
  }
}
