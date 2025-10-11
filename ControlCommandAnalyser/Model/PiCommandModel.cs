using AppConfiguration.Error.Translation;
using ControlCommandAnalyser.Model.Chains;

namespace ControlCommandAnalyser.Model
{
  [AllowedKeys(ControlCommandAnalyser.AlgorithmKey.Г, ControlCommandAnalyser.AlgorithmKey.К, ControlCommandAnalyser.AlgorithmKey.Т1)]
  public class PiCommandModel : BaseCommandModel, IHasScheme
  {
    public override string Mnemonic => "ПИ";

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
