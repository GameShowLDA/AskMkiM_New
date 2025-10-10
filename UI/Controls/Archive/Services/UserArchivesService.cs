using System.IO;
using UI.Controls.Archive.Models;

namespace UI.Controls.Archive.Services
{
  /// <summary>
  /// Операции над пользовательскими архивами .apkw на уровне файловой системы:
  /// создание, переименование, перемещение, удаление и базовая валидация.
  /// </summary>
  /// <remarks>
  /// Сервис работает только с расширением <c>.apkw</c>. Для содержимого используйте <see cref="OpkFilesService"/>.
  /// </remarks>
  public static class UserArchivesService
  {
    /// <summary>
    /// Создаёт пустой архив (.apkw) с минимальной структурой.
    /// </summary>
    /// <param name="archivePath">Полный путь к архиву.</param>
    public static async Task<bool> CreateEmpty(string archivePath, NewArchiveModel info)
    {
      var creator = new ArchiveCreator();
      return await creator.CreateAsync(archivePath, info);
    }

    /// <summary>
    /// Удаляет архив .apkw, если он существует.
    /// </summary>
    /// <param name="apkwPath">Путь к .apkw.</param>
    /// <returns><c>true</c>, если файл был удалён; иначе <c>false</c>.</returns>
    public static bool Delete(string apkwPath)
    {
      if (string.IsNullOrWhiteSpace(apkwPath)) return false;
      apkwPath = EnsureApkwExtension(Path.GetFullPath(apkwPath));
      if (!File.Exists(apkwPath)) return false;
      File.Delete(apkwPath);
      return true;
    }

    /// <summary>
    /// Переименовывает архив .apkw (без перемещения между каталогами).
    /// </summary>
    /// <param name="apkwPath">Исходный путь .apkw.</param>
    /// <param name="newFileNameWithoutExt">Новое имя файла без расширения.</param>
    /// <returns>Полный путь к переименованному файлу.</returns>
    /// <exception cref="FileNotFoundException">Если исходный файл отсутствует.</exception>
    public static string Rename(string apkwPath, string newFileNameWithoutExt)
    {
      apkwPath = EnsureApkwExtension(Path.GetFullPath(apkwPath));
      if (!File.Exists(apkwPath)) throw new FileNotFoundException("Архив не найден", apkwPath);

      var dir = Path.GetDirectoryName(apkwPath)!;
      var dest = Path.Combine(dir, newFileNameWithoutExt + ".apkw");
      File.Move(apkwPath, dest, overwrite: true);
      return dest;
    }

    /// <summary>
    /// Перемещает (или копирует с заменой) архив .apkw в другой каталог.
    /// </summary>
    /// <param name="apkwPath">Исходный путь .apkw.</param>
    /// <param name="destinationFolder">Целевой каталог.</param>
    /// <param name="overwrite">Перезаписывать, если файл уже существует.</param>
    /// <returns>Полный путь к перемещённому файлу.</returns>
    /// <exception cref="FileNotFoundException">Если исходный файл отсутствует.</exception>
    public static string MoveTo(string apkwPath, string destinationFolder, bool overwrite = true)
    {
      apkwPath = EnsureApkwExtension(Path.GetFullPath(apkwPath));
      if (!File.Exists(apkwPath)) throw new FileNotFoundException("Архив не найден", apkwPath);

      Directory.CreateDirectory(destinationFolder);
      var dest = Path.Combine(destinationFolder, Path.GetFileName(apkwPath));
      if (overwrite && File.Exists(dest)) File.Delete(dest);
      File.Move(apkwPath, dest);
      return dest;
    }

    /// <summary>
    /// Быстрая проверка корректности пути и расширения .apkw.
    /// </summary>
    public static bool IsValidApkw(string path)
    {
      if (string.IsNullOrWhiteSpace(path)) return false;
      return Path.GetExtension(path).Equals(".apkw", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Гарантирует наличие расширения .apkw у пути.
    /// </summary>
    private static string EnsureApkwExtension(string path)
      => Path.ChangeExtension(path, ".apkw");
  }
}
