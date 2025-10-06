using System.IO;
using DataBaseConfiguration;
using DataBaseConfiguration.Models.Archive;
using Microsoft.EntityFrameworkCore;

namespace UI.Controls.Archive.Services
{
  /// <summary>
  /// Сервис-обёртка для работы с корневыми папками пользовательских архивов,
  /// которые хранятся в БД (таблица <c>UserArchiveRootEntities</c>).
  /// </summary>
  /// <remarks>
  /// В БД сохраняются только пути корневых папок и их метаданные.
  /// Содержимое архивов (.apkw/.opkw) не кэшируется в БД и читается с диска по запросу.
  /// </remarks>
  internal static class UserArchiveRootsUiService
  {
    /// <summary>
    /// Добавляет новую или обновляет существующую запись корневой папки по её пути.
    /// </summary>
    /// <param name="displayName">Отображаемое имя папки в UI.</param>
    /// <param name="folderPath">Полный путь к корневой папке на диске.</param>
    /// <param name="recursive">Признак рекурсивного поиска .apkw в подпапках.</param>
    /// <param name="description">Необязательное описание.</param>
    /// <returns>Идентификатор записи в БД.</returns>
    /// <exception cref="System.ArgumentException">Если имя или путь пустые.</exception>
    internal static async Task<int> AddOrUpdateAsync(string displayName, string folderPath, bool recursive = true, string? description = null)
    {
      if (string.IsNullOrWhiteSpace(displayName)) throw new System.ArgumentException(nameof(displayName));
      if (string.IsNullOrWhiteSpace(folderPath)) throw new System.ArgumentException(nameof(folderPath));

      var normalized = NormalizePath(folderPath);
      using var db = DataBaseConfig.Context;

      var exists = await db.UserArchiveRootEntities.FirstOrDefaultAsync(x => x.FolderPath == normalized);
      if (exists != null)
      {
        bool changed = false;
        if (exists.DisplayName != displayName) { exists.DisplayName = displayName; changed = true; }
        if (exists.SearchRecursively != recursive) { exists.SearchRecursively = recursive; changed = true; }
        if (exists.Description != description) { exists.Description = description; changed = true; }

        if (changed) await db.SaveChangesAsync();
        return exists.Id;
      }

      var entity = new UserArchiveRootEntity
      {
        DisplayName = displayName,
        FolderPath = normalized,
        SearchRecursively = recursive,
        Description = description
      };
      db.UserArchiveRootEntities.Add(entity);
      await db.SaveChangesAsync();
      return entity.Id;
    }

    /// <summary>
    /// Удаляет запись корневой папки по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор записи.</param>
    /// <returns><c>true</c>, если запись была удалена; иначе <c>false</c>.</returns>
    internal static async Task<bool> RemoveByIdAsync(int id)
    {
      using var db = DataBaseConfig.Context;
      var ent = await db.UserArchiveRootEntities.FirstOrDefaultAsync(x => x.Id == id);
      if (ent == null) return false;
      db.UserArchiveRootEntities.Remove(ent);
      await db.SaveChangesAsync();
      return true;
    }

    /// <summary>
    /// Удаляет запись корневой папки по пути.
    /// </summary>
    /// <param name="folderPath">Путь к корневой папке (может быть относительным).</param>
    /// <returns><c>true</c>, если запись была удалена; иначе <c>false</c>.</returns>
    internal static async Task<bool> RemoveByPathAsync(string folderPath)
    {
      if (string.IsNullOrWhiteSpace(folderPath)) return false;
      var normalized = NormalizePath(folderPath);
      using var db = DataBaseConfig.Context;
      var ent = await db.UserArchiveRootEntities.FirstOrDefaultAsync(x => x.FolderPath == normalized);
      if (ent == null) return false;
      db.UserArchiveRootEntities.Remove(ent);
      await db.SaveChangesAsync();
      return true;
    }

    /// <summary>
    /// Возвращает список всех корневых папок, отсортированный по имени.
    /// </summary>
    internal static async Task<List<UserArchiveRootEntity>> ListAsync()
    {
      using var db = DataBaseConfig.Context;
      return await db.UserArchiveRootEntities.AsNoTracking().OrderBy(x => x.DisplayName).ToListAsync();
    }

    /// <summary>
    /// Проверяет наличие записи по пути (с нормализацией).
    /// </summary>
    internal static async Task<bool> ExistsAsync(string folderPath)
    {
      if (string.IsNullOrWhiteSpace(folderPath)) return false;
      var normalized = NormalizePath(folderPath);
      using var db = DataBaseConfig.Context;
      return await db.UserArchiveRootEntities.AnyAsync(x => x.FolderPath == normalized);
    }

    /// <summary>
    /// Нормализует путь: приводит к абсолютному и убирает завершающие слэши.
    /// </summary>
    private static string NormalizePath(string path)
    {
      var full = Path.GetFullPath(path);
      return full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
  }
}
