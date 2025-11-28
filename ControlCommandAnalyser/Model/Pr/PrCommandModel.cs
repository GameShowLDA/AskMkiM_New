using ControlCommandAnalyser.Attributes;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandAnalyser.Model.Interface;
using ControlCommandAnalyser.Model.Ks;
using DTO.Enum;
using Errors.Translation;

namespace ControlCommandAnalyser.Model.Pr
{
  [AllowedKeys(TranslationKey.AlgorithmKey.К,
   TranslationKey.AlgorithmKey.ЗР,
   TranslationKey.AlgorithmKey.ЗС,
   TranslationKey.AlgorithmKey.С, TranslationKey.AlgorithmKey.П,
    TranslationKey.AlgorithmKey.И,
    TranslationKey.AlgorithmKey.Т,
   TranslationKey.AlgorithmKey.Г, TranslationKey.AlgorithmKey.Т1)]
  [MeasurementDevice(MeasurementDevice.Multimeter)]
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

    /// <summary>
    /// Сбор данных в сообщение.
    /// </summary>
    public override IDislpayInfo BuildDislpayInfo => new PrMessageBuild();
  }
}
