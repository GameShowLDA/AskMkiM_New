using Microsoft.EntityFrameworkCore;
using DTO.Base.Models;
using DTO.SettingsModels;

namespace DataBaseConfiguration.Context
{
  public partial class AppDbContext
  {
    /// <summary>
    /// Таблица настроек протокола.
    /// </summary>
    public DbSet<SettingsProtocolModel> SettingsProtocol { get; set; }

    /// <summary>
    /// Таблица настроек выполнения.
    /// </summary>
    public DbSet<SettingsExecutionModel> Execution { get; set; }

    /// <summary>
    /// Таблица корневых архивов пользователей.
    /// </summary>
    public DbSet<UserArchiveRootEntity> UserArchiveRootEntities { get; set; }

    /// <summary>
    /// Таблица горячих клавиш файлов.
    /// </summary>
    public DbSet<FileHotkeyEntity> FileHotKeys { get; set; }
  }
}
