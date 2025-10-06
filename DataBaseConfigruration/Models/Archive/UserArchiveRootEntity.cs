using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseConfiguration.Models.Archive
{
  /// <summary>
  /// Корневая папка, в которой пользователь хранит .apkw архивы.
  /// Только это храним в БД.
  /// </summary>
  public class UserArchiveRootEntity
  {
    public int Id { get; set; }

    /// <summary>Отображаемое имя папки в UI.</summary>
    [MaxLength(256)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Полный путь к папке на диске.</summary>
    [MaxLength(1024)]
    public string FolderPath { get; set; } = string.Empty;

    /// <summary>Искать .apkw рекурсивно во всех подпапках.</summary>
    public bool SearchRecursively { get; set; } = true;

    /// <summary>Пометка на будущее: шифровать пользовательские архивы.</summary>
    public bool IsEncryptionPlanned { get; set; } = false;

    /// <summary>Необязательное описание.</summary>
    public string? Description { get; set; }
  }
}
