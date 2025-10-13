using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Services.FileManager
{
  /// <summary>
  /// Сервис для управления именами файлов в редакторе.
  /// 
  /// Основные задачи:
  /// <list type="bullet">
  ///   <item>Контроль уникальности открытых файлов с одинаковыми именами.</item>
  ///   <item>Автоматическая генерация уникального имени при конфликте.</item>
  ///   <item>Формирование различий в путях для добавления контекста к имени файла.</item>
  /// </list>
  /// 
  /// Сервис используется при открытии или регистрации новых файлов в редакторе, чтобы избежать конфликтов имен.
  /// </summary>
  public class FileNameService
  {
    private readonly EditorWorkspaceModel _context;

    /// <summary>
    /// Создаёт новый экземпляр сервиса управления именами файлов.
    /// </summary>
    /// <param name="editorWorkspaceModel">Контекст рабочего пространства редактора, содержащий информацию об открытых файлах.</param>
    public FileNameService(EditorWorkspaceModel editorWorkspaceModel)
    {
      _context = editorWorkspaceModel;
    }

    /// <summary>
    /// Регистрирует новый файл в системе и обеспечивает его уникальное имя в случае конфликта.
    /// 
    /// Если файл с таким именем уже открыт, а его путь отличается от текущего, к имени будет добавлена часть пути для различия.
    /// </summary>
    /// <param name="path">Полный путь к новому файлу.</param>
    /// <param name="fileName">Исходное имя файла.</param>
    /// <returns>
    /// Уникальное имя файла, под которым он будет зарегистрирован в системе.
    /// </returns>
    internal string EnsureUniqueFileName(string path, string nameFile)
    {
      if (!_context.FilePaths.ContainsKey(nameFile))
      {
        _context.FilePaths.Add(nameFile, path);
      }
      else
      {
        var fileWithSameNamePath = _context.FilePaths.FirstOrDefault(file => file.Key == nameFile);
        if (fileWithSameNamePath.Value != path)
        {
          nameFile = BuildUniqueNameFromPaths(fileWithSameNamePath.Value, path);
          _context.FilePaths.Add(nameFile, path);
        }
      }
      return nameFile;
    }

    /// <summary>
    /// Формирует уникальное имя файла на основе различий в его пути и пути уже открытого файла с тем же именем.
    /// </summary>
    /// <param name="existingPath">Путь к уже открытому файлу с тем же именем.</param>
    /// <param name="newPath">Путь к открываемому файлу.</param>
    /// <returns>Строка с уникальным именем, включающая часть пути.</returns>
    public string BuildUniqueNameFromPaths(string existingPath, string newPath)
    {
      var existingParts = existingPath.Split(Path.DirectorySeparatorChar);
      var newParts = newPath.Split(Path.DirectorySeparatorChar);

      int minLength = Math.Min(existingParts.Length, newParts.Length);
      int commonLength = 0;

      // Находим индекс, где пути перестают совпадать
      for (int i = 0; i < minLength; i++)
      {
        if (!string.Equals(existingParts[i], newParts[i], StringComparison.OrdinalIgnoreCase))
          break;

        commonLength++;
      }

      // Гарантируем, что хотя бы одна дополнительная папка будет в ключе
      int startIndex = Math.Max(0, newParts.Length - 2); // минимум: папка + файл

      // Но если всё отличается, берём всю вторую часть после общего пути
      if (commonLength < newParts.Length - 1)
        startIndex = commonLength;

      return string.Join(Path.DirectorySeparatorChar.ToString(), newParts.Skip(startIndex));
    }

  }
}
