using NewCore.Base.Function.Breakdown.Capabilities;
using NewCore.Function.GPT.Data;
using Utilities.Interface;
using static NewCore.Function.GPT.Command.ManualCommandManager;

namespace NewCore.Base.Function.Breakdown
{
  /// <summary>
  /// Управление режимом ACW на пробойной установке.
  /// </summary>
  public interface IAcwModeBreakdown 
  {

    /// <summary>
    /// Тип режима работы устройства (например, ACW, DCW, IR и т.д.).
    /// </summary>
    TypeMode ModeType => TypeMode.ACW;

    /// <summary>
    /// Управление режимом работы устройства (установка и проверка текущего режима).
    /// </summary>
    IModeConfigurable Mode { get; set; }

    /// <summary>
    /// Управление напряжением (установка и считывание параметра Voltage).
    /// </summary>
    IVoltageConfigurable Voltage { get; set; }

    /// <summary>
    /// Управление пределами тока (верхний и нижний токовые лимиты).
    /// </summary>
    ICurrentLimitsConfigurable CurrentLimits { get; set; }

    /// <summary>
    /// Управление временными параметрами (время теста и время нарастания напряжения).
    /// </summary>
    ITimeConfigurable Time { get; set; }

    /// <summary>
    /// Управление параметром смещения (Offset).
    /// </summary>
    IOffsetConfigurable Offset { get; set; }

    /// <summary>
    /// Управление параметром тока дуги (Arc Current).
    /// </summary>
    IArcCurrentConfigurable ArcCurrent { get; set; }

    IFrequencyConfigurable FrequencyConfigurable { get; set; }

    IMeasurable Measure { get; set; }

    IConfigurationProvider<AcwConfiguration> Config { get; set; }
  }
}
