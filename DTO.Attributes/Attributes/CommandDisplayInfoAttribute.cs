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
    /// Значение по умолчанию в процентах, если параметр предполагает процентное представление.
    /// Может использоваться для предварительной инициализации или расчётов.
    /// </summary>
    public double DefaultPercentage { get; }

    /// <summary>
    /// Числовое значение по умолчанию, если параметр имеет числовую природу (например, ток, напряжение).
    /// Применяется в случаях, когда процентное представление не требуется.
    /// </summary>
    public double DefaultNumeric { get; }

    /// <summary>
    /// Инициализирует новый экземпляр атрибута <see cref="CommandDisplayInfoAttribute"/>.
    /// Атрибут используется для аннотирования полей, описывающих команды или параметры, и содержит
    /// дополнительную информацию об их названии, единице измерения и значениях по умолчанию.
    /// </summary>
    /// <param name="displayName">Отображаемое имя команды или параметра.</param>
    /// <param name="unit">Единица измерения, связанная с параметром.</param>
    /// <param name="defaultPercentage">Значение по умолчанию в процентах. По умолчанию 0.</param>
    /// <param name="defaultNumeric">Числовое значение по умолчанию. По умолчанию 0.</param>
    public CommandDisplayInfoAttribute(string displayName, string unit, double defaultPercentage = 0, double defaultNumeric = 0)
    {
      DisplayName = displayName;
      Unit = unit;
      DefaultPercentage = defaultPercentage;
      DefaultNumeric = defaultNumeric;
    }
  }
}
