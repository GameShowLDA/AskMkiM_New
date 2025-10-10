using System.Text.Json;
using DTO.Base.Models.Session;
using Microsoft.EntityFrameworkCore;

namespace DataBaseConfiguration.Services
{
  /// <summary>
  /// Сервис для сохранения и восстановления пользовательской сессии приложения.
  /// Сессия сериализуется в JSON и хранится в таблице <c>UserSessions</c> под фиксированным ключом (Id = 1).
  /// </summary>
  public class SessionService
  {
    /// <summary>
    /// Сохраняет текущую сессию пользователя в базу данных.
    /// </summary>
    /// <param name="session">Модель сессии, которая будет сериализована в JSON и сохранена.</param>
    /// <remarks>
    /// Данные записываются в запись с <c>Id = 1</c>. Если записи нет, она будет создана.
    /// Поле <c>SavedAt</c> устанавливается в текущее локальное время.
    /// </remarks>
    public async Task SaveSessionAsync(SessionModel session)
    {
      var json = JsonSerializer.Serialize(session);

      using var db = DataBaseConfig.Context;
      var entity = await db.UserSessions.FindAsync(1);
      if (entity == null)
      {
        entity = new UserSessionEntity { Id = 1 };
        db.UserSessions.Add(entity);
      }

      entity.JsonData = json;
      entity.SavedAt = DateTime.Now;

      await db.SaveChangesAsync();
    }

    /// <summary>
    /// Загружает сохранённую сессию пользователя из базы данных.
    /// </summary>
    /// <returns>
    /// Десериализованная модель <see cref="SessionModel"/> или <c>null</c>, если запись отсутствует,
    /// данные пусты либо произошла ошибка десериализации.
    /// </returns>
    public async Task<SessionModel?> LoadSessionAsync()
    {
      using var db = DataBaseConfig.Context;
      var entity = await db.UserSessions.FirstOrDefaultAsync(x => x.Id == 1);
      if (entity == null || string.IsNullOrWhiteSpace(entity.JsonData))
        return null;

      try
      {
        return JsonSerializer.Deserialize<SessionModel>(entity.JsonData);
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// Проверяет, есть ли сохранённая сессия с хотя бы одной вкладкой.
    /// </summary>
    /// <returns>
    /// <c>true</c>, если сохранённая сессия существует и содержит вкладки; иначе <c>false</c>.
    /// </returns>
    public async Task<bool> HasSessionWithTabsAsync()
    {
      using var db = DataBaseConfig.Context;
      var entity = await db.UserSessions.FirstOrDefaultAsync(x => x.Id == 1);

      if (entity == null || string.IsNullOrWhiteSpace(entity.JsonData))
        return false;

      try
      {
        var session = JsonSerializer.Deserialize<SessionModel>(entity.JsonData);
        return session?.Tabs?.Count > 0;
      }
      catch
      {
        return false;
      }
    }
  }
}
