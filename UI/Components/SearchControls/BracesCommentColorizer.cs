using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace UI.Components.SearchControls
{
  /// <summary>
  /// Подсвечивает комментарии, заключённые в фигурные скобки { ... },
  /// с поддержкой вложенности и многострочных блоков.
  /// </summary>
  public class BracesCommentColorizer : DocumentColorizingTransformer
  {
    private readonly SolidColorBrush _commentBrush = new(Color.FromRgb(87, 166, 74)); // зелёный
    private string _cachedText;
    private List<(int start, int end)> _commentRanges = new();

    protected override void ColorizeLine(DocumentLine line)
    {
      var doc = CurrentContext.Document;

      if (_cachedText != doc.Text)
      {
        _cachedText = doc.Text;
        _commentRanges = ParseCommentsWithPriority(_cachedText);
      }

      int lineStart = line.Offset;
      int lineEnd = lineStart + line.Length;

      foreach (var (start, end) in _commentRanges)
      {
        if (lineStart < end && lineEnd > start)
        {
          int from = Math.Max(lineStart, start);
          int to = Math.Min(lineEnd, end);
          ChangeLinePart(from, to, e =>
              e.TextRunProperties.SetForegroundBrush(_commentBrush));
        }
      }
    }

    /// <summary>
    /// Основной парсер — строит диапазоны комментариев, учитывая вложения и пересечения типов.
    /// </summary>
    private static List<(int start, int end)> ParseCommentsWithPriority(string text)
    {
      var result = new List<(int start, int end)>();
      var stack = new Stack<(string type, int start)>();

      int i = 0;
      while (i < text.Length)
      {
        // --- открытие /* ---
        if (i + 1 < text.Length && text[i] == '/' && text[i + 1] == '*')
        {
          stack.Push(("slash", i));
          i += 2;
          continue;
        }

        // --- закрытие */ ---
        if (i + 1 < text.Length && text[i] == '*' && text[i + 1] == '/')
        {
          if (stack.Count > 0)
          {
            var last = stack.Peek();
            // закрываем только если верхний тип — slash
            if (last.type == "slash")
            {
              stack.Pop();
              result.Add((last.start, i + 2));
            }
          }
          i += 2;
          continue;
        }

        // --- открытие { ---
        if (text[i] == '{')
        {
          // открываем новый только если не внутри slash-блока
          if (!IsInsideSlash(stack))
            stack.Push(("brace", i));
          i++;
          continue;
        }

        // --- закрытие } ---
        if (text[i] == '}')
        {
          // закрываем только если верхний тип — brace
          if (stack.Count > 0 && stack.Peek().type == "brace")
          {
            var last = stack.Pop();
            result.Add((last.start, i + 1));
          }
          i++;
          continue;
        }

        i++;
      }

      // незакрытые комментарии → до конца файла
      foreach (var open in stack)
        result.Add((open.start, text.Length));

      return result;
    }

    private static bool IsInsideSlash(Stack<(string type, int start)> stack)
    {
      foreach (var s in stack)
        if (s.type == "slash") return true;
      return false;
    }
  }
}
