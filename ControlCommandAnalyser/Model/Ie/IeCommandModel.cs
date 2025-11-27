using ControlCommandAnalyser.Attributes;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandAnalyser.Model.Interface;
using ControlCommandAnalyser.Model.Ks;
using DTO.Enum;
using Errors.Translation;

namespace ControlCommandAnalyser.Model.Ie
{
  /// <summary>
  /// Модель для команды ИЕ (измерение емкости).
  /// </summary>
  [AllowedKeys(TranslationKey.AlgorithmKey.Д)]
  [MeasurementDevice(MeasurementDevice.Multimeter)]
  public class IeCommandModel : BaseCommandModel, IHasScheme
  {

    public override string Mnemonic => Utilities.EnumExtensions.GetDisplayInfo(DTO.Enum.Measurement.MeasurementTypeCommand.IE).DisplayName;

    /// <summary>
    /// Единицы измерения электрической ёмкости (например, "МОм", "кОм" и т.п.)
    /// </summary>
    public string? CapacityUnit { get; set; }

    /// <summary>
    /// Нижняя граница значеня элктрической ёмкости в строке (например, "100<МОм")
    /// </summary>
    public string? LowerLimitCapacitySource { get; set; }

    /// <summary>
    /// Нижняя граница значеня элктрической ёмкости в плавающей запятой.
    /// </summary>
    public double? LowerLimitCapacity { get; set; }

    /// <summary>
    /// Верхняя граница элктрической ёмкости (например, "МОм<100").
    /// </summary>
    public string? HigherLimitCapacitySource { get; set; }

    /// <summary>
    /// Верхняя граница значеня элктрической ёмкости в плавающей запятой.
    /// </summary>
    public double? HigherLimitCapacity { get; set; }

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
    public override IPointError PointErrors => new IeErrors();

    /// <summary>
    /// Сбор данных в сообщение.
    /// </summary>
    public override IDislpayInfo BuildDislpayInfo => new IeMessageBuild();
  }
}
