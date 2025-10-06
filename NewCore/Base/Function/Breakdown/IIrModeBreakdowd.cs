using NewCore.Base.Function.Breakdown.Capabilities;
using NewCore.Function.GPT.Data;
using Utilities.Interface;

namespace NewCore.Base.Function.Breakdown
{
  /// <summary>
  /// Интерфейс для режима измерения сопротивления изоляции (IR).
  /// </summary>
  public interface IIrModeBreakdown
  {
    TypeMode ModeType => TypeMode.IR;

    /// <summary>
    /// Управление режимом работы устройства (установка и проверка текущего режима).
    /// </summary>
    IModeConfigurable Mode { get; set; }

    /// <summary>
    /// Управление напряжением (установка и считывание параметра Voltage).
    /// </summary>
    IVoltageConfigurable Voltage { get; set; }

    /// <summary>
    /// Управление временными параметрами (время теста и время нарастания напряжения).
    /// </summary>
    ITimeConfigurable Time { get; set; }

    /// <summary>
    /// Управление параметром смещения (Offset).
    /// </summary>
    IOffsetConfigurable Offset { get; set; }

    IMeasurable Measure { get; set; }

    IConfigurationProvider<IrConfiguration> Config { get; set; }

    IResistanceLimitsConfigurable ResistanceLimits { get; set; }

  }
}
