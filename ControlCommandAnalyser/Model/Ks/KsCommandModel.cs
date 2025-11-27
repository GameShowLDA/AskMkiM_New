using ControlCommandAnalyser.Attributes;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandAnalyser.Model.Interface;
using DTO.Enum;
using Errors.Translation;

namespace ControlCommandAnalyser.Model.Ks
{
  /// <summary>
  /// Модель для команды КС (контроль сопротивения).
  /// </summary>
  [AllowedKeys(TranslationKey.AlgorithmKey.Б, TranslationKey.AlgorithmKey.Д)]
  [MeasurementDevice(MeasurementDevice.Multimeter)]
  public class KsCommandModel : BaseCommandModel
  {

    public override string Mnemonic => Utilities.EnumExtensions.GetDisplayInfo(DTO.Enum.Measurement.MeasurementTypeCommand.KC).DisplayName;

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
    /// Ошибки связанные с замыканием точек.
    /// </summary>
    public override IPointError PointErrors => new KsErrors();

    /// <summary>
    /// Сбор данных в сообщение.
    /// </summary>
    public override IDislpayInfo BuildDislpayInfo => new KsMessageBuild();
  }
}
