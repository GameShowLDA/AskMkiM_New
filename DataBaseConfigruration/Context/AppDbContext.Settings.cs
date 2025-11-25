using DTO.Base.Models;
using DTO.Settings.SettingsModels;
using DTO.SettingsModels;
using Microsoft.EntityFrameworkCore;

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

    /// <summary>
    /// Таблица настроек интерфейса программы
    /// </summary>
    public DbSet<UserInterfaceModel> UserInterface { get; set; }

    /// <summary>
    /// Таблица настроек отображения информации об устройствах.
    /// </summary>
    public DbSet<DeviceDisplaySettingsModel> DeviceDisplaySettings { get; set; }
  }
}
