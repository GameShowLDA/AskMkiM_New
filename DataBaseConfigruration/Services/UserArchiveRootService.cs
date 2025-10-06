using System.IO;
using System.Threading.Tasks;
using DataBaseConfiguration.Models.Archive;
using Microsoft.EntityFrameworkCore;

namespace DataBaseConfiguration.Services
{
  public static class UserArchiveRootService
  {
    /// <summary>
    /// Добавляет корневую папку архивов в БД или обновляет существующую запись (по пути).
    /// Возвращает Id записи.
    /// </summary>
    public static async Task<int> AddRootAsync(
      string displayName,
      string folderPath,
      bool recursive = true,
      string? description = null)
    {
      if (string.IsNullOrWhiteSpace(displayName))
        throw new System.ArgumentException("DisplayName is empty", nameof(displayName));
      if (string.IsNullOrWhiteSpace(folderPath))
        throw new System.ArgumentException("FolderPath is empty", nameof(folderPath));

      var normalized = NormalizePath(folderPath);

      using var db = DataBaseConfig.Context;

      // Ищем по нормализованному пути
      var existing = await db.UserArchiveRootEntities
        .FirstOrDefaultAsync(x => x.FolderPath == normalized);

      if (existing != null)
      {
        // Обновим метаданные, если изменились
        bool changed = false;

        if (existing.DisplayName != displayName) { existing.DisplayName = displayName; changed = true; }
        if (existing.SearchRecursively != recursive) { existing.SearchRecursively = recursive; changed = true; }
        if (existing.Description != description) { existing.Description = description; changed = true; }

        if (changed)
          await db.SaveChangesAsync();

        return existing.Id;
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
    /// Удаляет запись корневой папки по Id. Возвращает true, если что-то удалено.
    /// </summary>
    public static async Task<bool> RemoveRootAsync(int id)
    {
      using var db = DataBaseConfig.Context;

      var entity = await db.UserArchiveRootEntities.FirstOrDefaultAsync(x => x.Id == id);
      if (entity == null) return false;

      db.UserArchiveRootEntities.Remove(entity);
      await db.SaveChangesAsync();
      return true;
    }

    /// <summary>
    /// Удаляет запись корневой папки по пути (безопасно с нормализацией). Возвращает true, если что-то удалено.
    /// </summary>
    public static async Task<bool> RemoveRootAsync(string folderPath)
    {
      if (string.IsNullOrWhiteSpace(folderPath)) return false;

      var normalized = NormalizePath(folderPath);

      using var db = DataBaseConfig.Context;

      var entity = await db.UserArchiveRootEntities.FirstOrDefaultAsync(x => x.FolderPath == normalized);
      if (entity == null) return false;

      db.UserArchiveRootEntities.Remove(entity);
      await db.SaveChangesAsync();
      return true;
    }

    /// <summary>
    /// Проверяет существование записи по пути.
    /// </summary>
    public static async Task<bool> ExistsAsync(string folderPath)
    {
      if (string.IsNullOrWhiteSpace(folderPath)) return false;

      var normalized = NormalizePath(folderPath);

      using var db = DataBaseConfig.Context;
      return await db.UserArchiveRootEntities.AnyAsync(x => x.FolderPath == normalized);
    }

    /// <summary>
    /// Возвращает все корневые папки (упорядоченные по DisplayName).
    /// </summary>
    public static async Task<List<UserArchiveRootEntity>> GetAllAsync()
    {
      using var db = DataBaseConfig.Context;
      return await db.UserArchiveRootEntities
        .AsNoTracking()
        .OrderBy(x => x.DisplayName)
        .ToListAsync();
    }

    private static string NormalizePath(string path)
    {
      // Приводим к абсолютному, убираем завершающие слэши для консистентности ключа
      var full = Path.GetFullPath(path);
      return full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
  }
}
