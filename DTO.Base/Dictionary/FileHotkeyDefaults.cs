using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Base.Dictionary
{
  /// <summary>
  /// Содержит список горячих клавиш по умолчанию для управления файлами.
  /// </summary>
  public static class FileHotkeyDefaults
  {
    /// <summary>
    /// Словарь горячих клавиш по умолчанию. Ключ — логическое имя действия, значение — строковое представление комбинации клавиш.
    /// </summary>
    public static readonly Dictionary<string, string> Defaults = new()
      {
        // Открытие архива
        { "OpenArchive", "Ctrl+Shift+O" },

        // Работа с файлами
        { "OpenFile", "Ctrl+O" },
        { "CreateNewFile", "Ctrl+N" },
        { "SaveFile", "Ctrl+S" },
        { "SaveFileAs", "Ctrl+Shift+S" },
        { "PrintFile", "Ctrl+P" },
        { "SearchFile", "Ctrl+F" },
        { "CompareFile", "Ctrl+K" },
        { "Build", "F9" },
        { "Run", "Ctrl+F5" },
        { "RunStepByStepMode", "Ctrl+F10" },
        { "BreakpointsMode", "Ctrl+F8" },

        // Включение питания
        { "Power", "Ctrl+Shift+P" },

        // Выход
        { "ExitApplication", "Alt+F4" }
      };
  }
}
