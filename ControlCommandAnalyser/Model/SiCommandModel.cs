using AppConfiguration.Error.Translation;
using ControlCommandAnalyser.Model.Chains;

namespace ControlCommandAnalyser.Model
{

  /// <summary>
  /// Модель для команды СИ (сопротивление изоляции).
  /// </summary>
  [AllowedKeys(ControlCommandAnalyser.AlgorithmKey.К,
    ControlCommandAnalyser.AlgorithmKey.С, ControlCommandAnalyser.AlgorithmKey.П,
     ControlCommandAnalyser.AlgorithmKey.Т,
      ControlCommandAnalyser.AlgorithmKey.И,
    ControlCommandAnalyser.AlgorithmKey.Г, ControlCommandAnalyser.AlgorithmKey.Т1)]
  public class SiCommandModel : BaseCommandModel, IHasScheme
  {
    public override string Mnemonic => Utilities.EnumExtensions.GetDisplayInfo(DTO.Enum.Measurement.MeasurementTypeCommand.CI).DisplayName;

    /// <summary>
    /// Значение напряжения (например, "100В", "1кВ").
    /// </summary>
    public string? VoltageSource { get; set; }
    public double? Voltage { get; set; }

    /// <summary>
    /// Единицы измерения сопротивления (например, "МОм", "кОм" и т.п.)
    /// </summary>
    public string? ResistanceUnit { get; set; }

    /// <summary>
    /// Значение сопротивления (например, "100<МОм").
    /// </summary>
    public string? ResistanceSource { get; set; }
    public double? Resistance { get; set; }

    /// <summary>
    /// Значение времени (например, "1c").
    /// </summary>
    public string? TimeSource { get; set; }
    public double? Time { get; set; }

    /// <summary>
    /// Список точек измерения.
    /// </summary>
    public SchemeModel Scheme { get; set; }

    /// <summary>
    /// Остаток строки с нераспознанными параметрами.
    /// </summary>
    public string? UnparsedParameters { get; set; }

    public override IPointError PointErrors => new SiErrors();
  }
}
