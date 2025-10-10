// UI.Controls.Archive/Services/ArchiveTreeService.cs
using System.IO;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using UI.Controls.Archive.Models;

namespace UI.Controls.Archive.Services
{
  /// <summary>
  /// Сервис построения древовидной модели для UI из корневых папок,
  /// хранящихся в БД: Folder → .apkw → .opkw.
  /// </summary>
  /// <remarks>
  /// Содержимое архивов не кешируется в БД: чтение файловой системы и .zip выполняется на лету.
  /// </remarks>
  internal class ArchiveTreeService
  {
    // UI.Controls.Archive.Services/ArchiveTreeService.cs (фрагменты)
    public async Task<List<ArchiveFolder>> BuildTreeAsyncCombined(CancellationToken ct = default)
    {
      var result = new List<ArchiveFolder>();
      result.AddRange(await BuildSystemTreeAsync());        // системные (IsSystem = true)
      result.AddRange(await BuildUserTreeAsync(ct));        // пользовательские (IsSystem = false)
      return result;
    }

    // системные — заглушка; убери/оставь как хочешь
    public Task<List<ArchiveFolder>> BuildSystemTreeAsync()
    {
      var list = new List<ArchiveFolder>
  {
    new ArchiveFolder
    {
      FolderName = "Системный пакет",
      FolderPath = "<internal>",
      IsSystem   = true,                                 // ← важно
      Archives   = new List<ArchiveModel>
      {
        new ArchiveModel { ArchiveName = "base.apkw", IsSystem = true, OpkFiles = new() }
      }
    }
  };
      return Task.FromResult(list);
    }

    public async Task<List<ArchiveFolder>> BuildUserTreeAsync(CancellationToken ct = default)
    {
      using var db = DataBaseConfiguration.DataBaseConfig.Context;
      var roots = await db.UserArchiveRootEntities.AsNoTracking().OrderBy(x => x.DisplayName).ToListAsync(ct);

      var list = new List<ArchiveFolder>();
      foreach (var root in roots)
      {
        if (!Directory.Exists(root.FolderPath)) continue;

        var f = new ArchiveFolder
        {
          FolderName = root.DisplayName,
          FolderPath = root.FolderPath,
          IsSystem = false,                                // ← пользовательский
          Archives = new List<ArchiveModel>()
        };

        var opt = root.SearchRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        foreach (var apkw in Directory.EnumerateFiles(root.FolderPath, "*.apkw", opt))
        {
          var a = await ReadArchiveAsync(apkw, ct);
          a.ArchivePath = apkw;                              // ← путь нужен для ПКМ
          f.Archives.Add(a);
        }

        list.Add(f);
      }
      return list;
    }


    /// <summary>
    /// Читает содержимое архива .apkw и возвращает модель с перечислением .opkw.
    /// </summary>
    /// <param name="apkwPath">Полный путь к архиву .apkw.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns><see cref="ArchiveModel"/> с заполненным списком .opkw.</returns>
    /// <remarks>
    /// Внутри используется <see cref="ZipFile.OpenRead(string)"/>; читаются только «листовые» записи с расширением .opkw.
    /// </remarks>
    internal Task<ArchiveModel> ReadArchiveAsync(string apkwPath, CancellationToken ct = default)
    {
      return Task.Run(() =>
      {
        var model = new ArchiveModel
        {
          ArchiveName = Path.GetFileName(apkwPath),
          IsSystem = false,
          OpkFiles = new List<OpkModel>()
        };

        if (!File.Exists(apkwPath)) return model;

        try
        {
          using var zip = ZipFile.OpenRead(apkwPath);
          foreach (var e in zip.Entries)
          {
            if (ct.IsCancellationRequested) break;
            // пропускаем каталоги и не-*.opkw
            if (e.FullName.EndsWith("/", StringComparison.Ordinal)) continue;
            if (!e.Name.EndsWith(".opkw", StringComparison.OrdinalIgnoreCase)) continue;

            model.OpkFiles.Add(new OpkModel
            {
              Name = Path.GetFileNameWithoutExtension(e.Name),
              OpkFilename = e.Name, // имя записи внутри архива
              Creation = e.LastWriteTime.DateTime
            });
          }
        }
        catch
        {
          // глотаем ошибки чтения архива: вернём пустой список .opkw
        }

        return model;
      }, ct);
    }
  }
}
