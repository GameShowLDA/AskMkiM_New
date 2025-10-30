using Errors.Translation;
using ControlCommandAnalyser.Model.Chains;

namespace ControlCommandAnalyser.Model
{
  [AllowedKeys(ControlCommandAnalyser.AlgorithmKey.К,
   ControlCommandAnalyser.AlgorithmKey.ЗР,
   ControlCommandAnalyser.AlgorithmKey.ЗС,
   ControlCommandAnalyser.AlgorithmKey.С, ControlCommandAnalyser.AlgorithmKey.П,
    ControlCommandAnalyser.AlgorithmKey.И,
    ControlCommandAnalyser.AlgorithmKey.Т,
   ControlCommandAnalyser.AlgorithmKey.Г, ControlCommandAnalyser.AlgorithmKey.Т1)]
  public class PrCommandModel : BaseCommandModel, IError, IHasScheme
  {
    public override string Mnemonic => Utilities.EnumExtensions.GetDisplayInfo(DTO.Enum.Measurement.MeasurementTypeCommand.PR).DisplayName;

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
