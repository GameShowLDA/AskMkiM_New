using System.ComponentModel.DataAnnotations;

namespace DTO.Base.Models.Session
{
  /// <summary>
  /// Класс <see cref="UserSessionEntity"/> представляет собой сущность,
  /// описывающую сохранённое состояние пользовательской сессии редактора.  
  /// Хранит сериализованное содержимое сессии в формате JSON и метаданные о моменте её сохранения.
  /// </summary>
  public class UserSessionEntity
  {

    /// <summary>
    /// Уникальный идентификатор записи сессии.  
    /// Всегда равен 1, так как в системе хранится только одна активная сессия пользователя.
    /// </summary>
    [Key]
    public int Id { get; set; } = 1;

    /// <summary>
    /// Сериализованное содержимое пользовательской сессии в формате JSON.  
    /// Обычно содержит список вкладок, их состояние, активную вкладку и другие параметры среды редактора.
    /// </summary>
    public string JsonData { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время сохранения сессии.  
    /// Используется для отслеживания актуальности данных и определения последнего момента сохранения.
    /// </summary>
    public DateTime SavedAt { get; set; } = DateTime.Now;
  }
}
