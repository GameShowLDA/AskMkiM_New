using AppConfiguration.Error.Translation;
using ControlCommandAnalyser.Model.Chains;

namespace ControlCommandAnalyser.Model
{
  [AllowedKeys(ControlCommandAnalyser.AlgorithmKey.Д)]
  public class EhtCommandModel:BaseCommandModel
  {
    public override string Mnemonic => Utilities.EnumExtensions.GetDisplayInfo(DTO.Enum.Measurement.MeasurementTypeCommand.EHT).DisplayName;

    /// <summary>
    /// Единицы измерения сопротивления проводов (например, "МОм", "кОм" и т.п.)
    /// </summary>
    public string? CabelResistanceUnit { get; set; }

    /// <summary>
    /// Строка со значением сопротивления проводов (например, "100МОм")
    /// </summary>
    public string? CabelResistanceSource { get; set; }

    /// <summary>
    /// Значение сопротивления проводов (например, "100МОм")
    /// </summary>
    public double? CabelResistance { get; set; }

    /// <summary>
    /// Единицы измерения сопротивления (например, "МОм", "кОм" и т.п.)
    /// </summary>
    public string? ResistanceUnit { get; set; }

    /// <summary>
    /// Нижняя граница значеня сопротивления (например, "100<МОм")
    /// </summary>
    public string? LowerLimitResistanceSource { get; set; }

    /// <summary>
    /// Нижняя граница значеня сопротивления (например, "100<МОм")
    /// </summary>
    public double? LowerLimitResistance { get; set; }

    /// <summary>
    /// Верхняя граница значения сопротивления (например, "МОм<100").
    /// </summary>
    public string? HigherLimitResistanceSource { get; set; }

    /// <summary>
    /// Верхняя граница значения сопротивления (например, "МОм<100").
    /// </summary>
    public double? HigherLimitResistance { get; set; }

    /// <summary>
    /// Список точек измерения.
    /// </summary>
    public SchemeModel Scheme { get; set; }

    /// <summary>
    /// Остаток строки с нераспознанными параметрами.
    /// </summary>
    public string? UnparsedParameters { get; set; }

    /// <summary>
    /// Значение времени (например, "1c").
    /// </summary>
    public string? TimeSource { get; set; }
    public double? Time { get; set; }

    /// <summary>
    /// Ошибки связанные с замыканием точек.
    /// </summary>
    public override IPointError PointErrors => new PrErrors();
  }
}
