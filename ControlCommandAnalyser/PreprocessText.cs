using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCommandAnalyser
{
  internal class PreprocessText
  {
    /// <summary>
    /// Возвращает очищенные строки программы и список всех найденных комментариев.
    /// </summary>
    public static (List<string> CleanLines, List<(int LineIndex, string Text)> Comments)
PreprocessTextAndExtractComments(string text)
    {
      var lines = text.Replace("\r\n", "\n").Split('\n').ToList();
      var cleanLines = new List<string>();
      var comments = new List<(int LineIndex, string Text)>();

      // стек типов комментариев: "slash" (/* */) или "brace" ({ })
      var stack = new Stack<string>();
      var currentComment = new StringBuilder();

      for (int i = 0; i < lines.Count; i++)
      {
        string line = lines[i];
        int index = 0;
        var cleanBuilder = new StringBuilder();
        var commentLine = false;

        while (index < line.Length)
        {
          // === если мы внутри комментария ===
          if (stack.Count > 0)
          {
            // внутри комментария мы должны добавлять ВСЁ в currentComment,
            // но при этом отслеживать новые открытия/закрытия

            // смотрим на 2-символьные комбинации в первую очередь
            if (index < line.Length - 1 &&
                line[index] == '/' && line[index + 1] == '*')
            {
              // вложенный /* */
              stack.Push("slash");
              currentComment.Append("/*");
              index += 2;
              continue;
            }

            if (index < line.Length - 1 &&
                line[index] == '*' && line[index + 1] == '/' &&
                stack.Count > 0 && stack.Peek() == "slash")
            {
              // закрытие верхнего /* */
              stack.Pop();
              currentComment.Append("*/");
              index += 2;

              // если стек опустел — комментарий закончился
              if (stack.Count == 0)
              {
                comments.Add((i, currentComment.ToString().TrimEnd()));
                currentComment.Clear();
              }
              continue;
            }

            // фигурные скобки обрабатываем по одной
            if (line[index] == '{')
            {
              stack.Push("brace");
              currentComment.Append('{');
              index++;
              continue;
            }

            if (line[index] == '}' &&
                stack.Count > 0 && stack.Peek() == "brace")
            {
              stack.Pop();
              currentComment.Append('}');
              index++;

              if (stack.Count == 0)
              {
                comments.Add((i, currentComment.ToString().TrimEnd()));
                currentComment.Clear();
              }
              continue;
            }

            // обычный символ внутри комментария
            currentComment.Append(line[index]);
            index++;
            continue;
          }

          // === если мы ВНЕ комментариев ===

          // начало /* ... */
          if (index < line.Length - 1 && line[index] == '/' && line[index + 1] == '*')
          {
            // всё, что было до этого — код
            if (cleanBuilder.Length > 0)
            {
              cleanLines.Add(cleanBuilder.ToString().TrimEnd());
              commentLine = true;
            }

            stack.Push("slash");
            currentComment.Clear();
            currentComment.Append("/*");
            index += 2;
            continue;
          }

          // начало { ... }
          if (line[index] == '{')
          {
            if (cleanBuilder.Length > 0)
            {
              cleanLines.Add(cleanBuilder.ToString().TrimEnd());
              commentLine = true;
            }

            stack.Push("brace");
            currentComment.Clear();
            currentComment.Append('{');
            index++;
            continue;
          }

          // обычный код вне комментария
          cleanBuilder.Append(line[index]);
          index++;
        }

        // конец строки
        if (stack.Count == 0)
        {
          // строка вне комментария
          var cleanLine = cleanBuilder.ToString().TrimEnd();
          if (!string.IsNullOrWhiteSpace(cleanLine) && commentLine == false)
          {
            cleanLines.Add(cleanLine);
          }
        }
        else
        {
          // строка часть комментария — добавляем перевод строки
          currentComment.Append('\n');
        }
      }

      // если файл закончился, а комментарий так и не закрылся — всё равно добавим
      if (stack.Count > 0 && currentComment.Length > 0)
      {
        comments.Add((lines.Count - 1, currentComment.ToString().TrimEnd()));
      }

      return (cleanLines, comments);
    }
  }
}
