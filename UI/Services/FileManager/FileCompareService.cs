using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DTO.Base.Interface;
using Message;
using Utilities.TextEditor;

namespace UI.Services.FileManager
{
  /// <summary>
  /// Сервис для сравнения содержимого открытого файла с содержимым редактора.
  /// Предназначен для определения, были ли внесены изменения в текст по сравнению с сохранённой версией на диске.
  /// 
  /// Основные функции:
  /// <list type="bullet">
  ///   <item>Проверяет, отличается ли содержимое редактора от сохранённого файла.</item>
  ///   <item>Определяет, является ли файл новым (ещё не сохранённым на диск).</item>
  ///   <item>Отображает уведомление, если файл был удалён или повреждён на диске.</item>
  /// </list>
  /// 
  /// Данный сервис обычно используется перед сохранением файла или при закрытии вкладки, чтобы предупредить пользователя о несохранённых изменениях.
  /// </summary>
  public class FileCompareService
  {
    private readonly EditorWorkspaceModel _context;

    /// <summary>
    /// Создаёт новый экземпляр сервиса сравнения файлов.
    /// </summary>
    /// <param name="editorWorkspaceModel">Контекст редактора, содержащий пути к открытым файлам.</param>
    public FileCompareService(EditorWorkspaceModel editorWorkspaceModel)
    {
      _context = editorWorkspaceModel;
    }

    /// <summary>
    /// Проверяет, изменилось ли содержимое открытого файла в редакторе по сравнению с сохранённой версией на диске.
    /// </summary>
    /// <param name="control">Объект <see cref="IDockItem"/>, представляющий открытую вкладку с редактируемым текстом.</param>
    /// <returns>
    /// Возвращает <c>true</c>, если файл был изменён (содержимое отличается, файл не найден или является новым);
    /// <c>false</c>, если содержимое файла не изменялось.
    /// </returns>
    public bool HasFileChanged(IDockItem control)
    {
      var fileName = control.Title;
      if (string.IsNullOrEmpty(fileName))
        return false;

      if (fileName.Contains(".opk"))
        return false;

      if (!_context.FilePaths.TryGetValue(fileName, out var filePath))
      {
        return true;
      }

      if (string.IsNullOrEmpty(filePath))
      {
        if (control.Content is ITextEditorAdapter textEditor)
        {
          return !string.IsNullOrWhiteSpace(textEditor.Text);
        }

        return false;
      }

      if (File.Exists(filePath))
      {
        var diskContent = File.ReadAllText(filePath);

        if (control.Content is ITextEditorAdapter textEditor)
        {
          return diskContent != textEditor.Text;
        }

        return false;
      }
      else
      {
        MessageBoxCustom.Show("Файл был удален или поврежден", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
        return true;
      }
    }
  }
}
