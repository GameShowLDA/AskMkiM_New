namespace DTO.Attributes
{
  /// <summary>
  /// Пространство имён <c>DTO.Attributes</c> содержит пользовательские атрибуты,
  /// предназначенные для описания дополнительных метаданных, связанных с объектами передачи данных (DTO).
  /// </summary>
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
  public class CommandDisplayInfoAttribute : Attribute
  {
    /// <summary>
    /// Отображаемое имя команды или параметра.
    /// Используется в пользовательском интерфейсе или при генерации документации для более понятного представления поля.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Единица измерения, связанная с данным параметром (например, "В", "мА", "%").
    /// Позволяет автоматически выводить корректную физическую единицу при визуализации значения.
    /// </summary>
    public string Unit { get; }

    /// <summary>
    /// Инициализирует новый экземпляр атрибута <see cref="CommandDisplayInfoAttribute"/>.
    /// Атрибут используется для аннотирования полей, описывающих команды или параметры, и содержит
    /// дополнительную информацию об их названии, единице измерения и значениях по умолчанию.
    /// </summary>
    /// <param name="displayName">Отображаемое имя команды или параметра.</param>
    /// <param name="unit">Единица измерения, связанная с параметром.</param>
    public CommandDisplayInfoAttribute(string displayName, string unit)
    {
      DisplayName = displayName;
      Unit = unit;
    }
  }
}
