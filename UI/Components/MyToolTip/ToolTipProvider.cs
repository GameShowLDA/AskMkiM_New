using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Components.MyToolTip
{
  public static class ToolTipProvider
  {
    private static readonly Dictionary<string, (string Title, string Description)> _descriptions = new()
        {
            { "File", ("Работа с файлами", "Раздел для работы с файлами проекта: открытие, создание, сравнение и завершение работы приложения.") },
            { "File.Archive", ("Архив", "Добавить описание.") },
            { "File.Open", ("Открыть", "Добавить описание.") },
            { "File.New", ("Создать", "Добавить описание.") },
        };

    public static (string Title, string Description) GetDescription(string key)
    {
      if (_descriptions.TryGetValue(key, out var result))
        return result;

      return ("Описание", $"Описание не найдено для ключа: {key}");
    }
  }
}
