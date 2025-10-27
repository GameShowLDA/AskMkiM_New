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
    /// Используется в пользовательском интерфейсе или при генерации документации
    /// для более понятного представления поля.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Единица измерения, связанная с данным параметром (например, "В", "мА", "%").
    /// Позволяет автоматически выводить корректную физическую единицу при визуализации значения.
    /// </summary>
    public string Unit { get; }

    /// <summary>
    /// Нижняя граница диапазона измерений.
    /// Указывает минимальное значение, которое может быть корректно измерено или отображено данной командой.
    /// </summary>
    public double? LowerLimit { get; }

    /// <summary>
    /// Верхняя граница диапазона измерений.
    /// Указывает максимальное значение, которое может быть корректно измерено или отображено данной командой.
    /// </summary>
    public double? UpperLimit { get; }

    /// <summary>
    /// Инициализирует новый экземпляр атрибута <see cref="CommandDisplayInfoAttribute"/>.
    /// Атрибут используется для аннотирования полей, описывающих команды или параметры, и содержит
    /// дополнительную информацию об их названии, единице измерения и диапазоне измерений.
    /// </summary>
    /// <param name="displayName">Отображаемое имя команды или параметра.</param>
    /// <param name="unit">Единица измерения, связанная с параметром.</param>
    /// <param name="lowerLimit">Нижняя граница диапазона измерений (опционально).</param>
    /// <param name="upperLimit">Верхняя граница диапазона измерений (опционально).</param>
    public CommandDisplayInfoAttribute(string displayName, string unit, double lowerLimit, double upperLimit)
    {
      DisplayName = displayName;
      Unit = unit;
      LowerLimit = lowerLimit;
      UpperLimit = upperLimit;
    }
  }
}
