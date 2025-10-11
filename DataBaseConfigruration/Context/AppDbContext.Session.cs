using Microsoft.EntityFrameworkCore;
using DTO.Base.Models.Session;

namespace DataBaseConfiguration.Context
{
  public partial class AppDbContext
  {
    /// <summary>
    /// Таблица сессий пользователей.  
    /// Сохраняет состояние открытых вкладок, активной вкладки и времени последнего сохранения.
    /// </summary>
    public DbSet<UserSessionEntity> UserSessions { get; set; }
  }
}
