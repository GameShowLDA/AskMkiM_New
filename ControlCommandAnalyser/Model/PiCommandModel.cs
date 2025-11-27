using ControlCommandAnalyser.Attributes;
using ControlCommandAnalyser.Model.Chains;
using DTO.Enum;
using Errors.Translation;

namespace ControlCommandAnalyser.Model
{
  [AllowedKeys(TranslationKey.AlgorithmKey.Г, TranslationKey.AlgorithmKey.К, TranslationKey.AlgorithmKey.Т1)]
  [MeasurementDevice(MeasurementDevice.BreakdownTester)]
  public class PiCommandModel : BaseCommandModel, IHasScheme
  {
    public override string Mnemonic => Utilities.EnumExtensions.GetDisplayInfo(DTO.Enum.Measurement.MeasurementTypeCommand.PI).DisplayName;

    /// <summary>
    /// Модль команды СИ.
    /// </summary>
    public SiCommandModel SiCommand { get; set; }

    /// <summary>
    /// Значение напряжения (например, "100В", "1кВ").
    /// </summary>
    public string? VoltageSource { get; set; }

    public double? Voltage { get; set; }

    /// <summary>
    /// Тип напряжения.
    /// </summary>
    public VoltageEnum.Type VoltageType { get; set; }

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

    public override IPointError PointErrors => new PiErrors();

  }
}
