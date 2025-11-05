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

      // нормализуем переводы строк
      text = text.Replace("\r\n", "\n");

      // удаляем блочные комментарии /* ... */ — полностью,
      // но сохраняем количество строк, чтобы нумерация не сбилась
      text = Regex.Replace(
          text,
          @"/\*[\s\S]*?\*/",
          m =>
          {
            int lineCount = CountLines(m.Value);
            // Заменяем содержимое комментария на lineCount-1 символов '\n'
            // чтобы сохранить количество строк
            return new string('\n', lineCount - 1);
          },
          RegexOptions.Singleline);

      // удаляем однострочные комментарии // ...
      text = Regex.Replace(
          text,
          @"//.*",
          string.Empty);

      // Убираем из фигурных скобок только большие блоки (> MaxShortCommentLines строк),
      // но также сохраняем количество строк
      text = Regex.Replace(
          text,
          @"\{[\s\S]*?\}",
          m =>
          {
            int lineCount = CountLines(m.Value);
            if (lineCount > MaxShortCommentLines)
              return new string('\n', lineCount - 1);
            return m.Value;
          },
          RegexOptions.Singleline);

      return text;
    }

  }
}
