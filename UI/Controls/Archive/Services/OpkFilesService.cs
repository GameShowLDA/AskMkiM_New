using System.IO;
using System.IO.Compression;
using UI.Controls.Archive.Models;

namespace UI.Controls.Archive.Services
{
  /// <summary>
  /// Операции над файлами .opkw, расположенными внутри архива .apkw (ZIP-контейнер).
  /// </summary>
  /// <remarks>
  /// Методы используют <see cref="ZipArchive"/> в режимах чтения и обновления.
  /// Все операции рассчитаны на то, что .apkw — обычный .zip c пользовательским расширением.
  /// </remarks>
  public static class OpkFilesService
  {
    /// <summary>
    /// Возвращает список .opkw из архива .apkw как коллекцию <see cref="OpkModel"/>.
    /// </summary>
    /// <param name="apkwPath">Путь к .apkw.</param>
    public static Task<List<OpkModel>> ListAsync(string apkwPath)
      => Task.Run(() =>
      {
        var result = new List<OpkModel>();
        if (!File.Exists(apkwPath)) return result;

        using var zip = ZipFile.OpenRead(apkwPath);
        foreach (var e in zip.Entries)
        {
          if (IsDirectory(e)) continue;
          if (!e.Name.EndsWith(".opkw", StringComparison.OrdinalIgnoreCase)) continue;

          result.Add(new OpkModel
          {
            Name = Path.GetFileNameWithoutExtension(e.Name),
            OpkFilename = e.FullName,
            Creation = e.LastWriteTime.DateTime
          });
        }
        return result;
      });

    /// <summary>
    /// Добавляет на верхний уровень архива новый файл .opkw из файловой системы.
    /// </summary>
    /// <param name="apkwPath">Путь к .apkw-архиву.</param>
    /// <param name="sourceFilePath">Путь к исходному .opkw на диске.</param>
    /// <param name="entryName">Необязательное имя записи внутри архива (по умолчанию — имя файла).</param>
    /// <param name="overwrite">Перезаписать, если запись с таким именем существует.</param>
    public static Task AddAsync(string apkwPath, string sourceFilePath, string? entryName = null, bool overwrite = false)
      => Task.Run(() =>
      {
        if (!File.Exists(sourceFilePath))
          throw new FileNotFoundException("Исходный .opkw не найден", sourceFilePath);
        if (!sourceFilePath.EndsWith(".opkw", StringComparison.OrdinalIgnoreCase))
          throw new ArgumentException("Источник должен иметь расширение .opkw", nameof(sourceFilePath));

        entryName ??= Path.GetFileName(sourceFilePath);

        using var zip = ZipFile.Open(apkwPath, ZipArchiveMode.Update);
        var existing = zip.GetEntry(entryName);
        if (existing != null)
        {
          if (!overwrite)
            throw new IOException($"Запись '{entryName}' уже существует в архиве.");
          existing.Delete();
        }

        var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
        using var src = File.OpenRead(sourceFilePath);
        using var dst = entry.Open();
        src.CopyTo(dst);
      });

    /// <summary>
    /// Заменяет существующую запись .opkw в архиве на содержимое из файла на диске.
    /// </summary>
    /// <param name="apkwPath">Путь к .apkw.</param>
    /// <param name="entryPath">Путь записи внутри архива (например, <c>file1.opkw</c>).</param>
    /// <param name="sourceFilePath">Путь к новому содержимому .opkw на диске.</param>
    public static Task ReplaceAsync(string apkwPath, string entryPath, string sourceFilePath)
      => Task.Run(() =>
      {
        if (!File.Exists(sourceFilePath))
          throw new FileNotFoundException("Исходный .opkw не найден", sourceFilePath);
        if (!sourceFilePath.EndsWith(".opkw", StringComparison.OrdinalIgnoreCase))
          throw new ArgumentException("Источник должен иметь расширение .opkw", nameof(sourceFilePath));

        using var zip = ZipFile.Open(apkwPath, ZipArchiveMode.Update);
        var existing = zip.GetEntry(entryPath);
        if (existing == null)
          throw new FileNotFoundException("Запись .opkw не найдена в архиве", entryPath);

        existing.Delete();

        var entry = zip.CreateEntry(entryPath, CompressionLevel.Optimal);
        using var src = File.OpenRead(sourceFilePath);
        using var dst = entry.Open();
        src.CopyTo(dst);
      });

    /// <summary>
    /// Удаляет запись .opkw из архива .apkw.
    /// </summary>
    /// <param name="apkwPath">Путь к .apkw.</param>
    /// <param name="entryPath">Путь записи внутри архива.</param>
    /// <returns><c>true</c>, если запись была удалена; иначе <c>false</c>.</returns>
    public static Task<bool> RemoveAsync(string apkwPath, string entryPath)
      => Task.Run(() =>
      {
        using var zip = ZipFile.Open(apkwPath, ZipArchiveMode.Update);
        var existing = zip.GetEntry(entryPath);
        if (existing == null) return false;
        existing.Delete();
        return true;
      });

    /// <summary>
    /// Извлекает .opkw из архива на диск.
    /// </summary>
    /// <param name="apkwPath">Путь к .apkw.</param>
    /// <param name="entryPath">Путь записи внутри архива.</param>
    /// <param name="destinationPath">Путь файла назначения на диске.</param>
    /// <param name="overwrite">Перезаписывать, если файл назначения существует.</param>
    public static Task ExtractAsync(string apkwPath, string entryPath, string destinationPath, bool overwrite = false)
      => Task.Run(() =>
      {
        using var zip = ZipFile.OpenRead(apkwPath);
        var entry = zip.GetEntry(entryPath);
        if (entry == null)
          throw new FileNotFoundException("Запись .opkw не найдена в архиве", entryPath);

        var dest = Path.GetFullPath(destinationPath);
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        if (File.Exists(dest) && !overwrite)
          throw new IOException($"Файл назначения уже существует: {dest}");

        using var src = entry.Open();
        using var dst = File.Create(dest);
        src.CopyTo(dst);
      });

    private static bool IsDirectory(ZipArchiveEntry e)
      => e.FullName.EndsWith("/", StringComparison.Ordinal);
  }
}
