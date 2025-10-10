using DTO.Device.Base;
using DTO.Device.PowerSourceModule.Capabilities;

namespace DTO.Device.PowerSourceModule
{
  /// <summary>
  /// Интерфейс для модуля источника напряжения и тока.
  /// </summary>
  public interface IPowerSourceModule : IAttachableDevice
  {

    /// <summary>
    /// Управление подключением и отключением шин.
    /// </summary>
    public IBusManager BusManager { get; set; }

    /// <summary>
    /// Управление настройками тока.
    /// </summary>
    public ICurrentManager CurrentManager { get; set; }

    /// <summary>
    /// Управление настройками напряжения.
    /// </summary>
    public IVoltageManager VoltageManager { get; set; }
    public ISelfTestCheckerModuleVoltageCurrentSource SelfTestManager { get; set; }

    /// <summary>
    /// JSON-строка с калибровочными коэффициентами по диапазонам сопротивления
    /// </summary>
    public string? ResistanceCalibrationJson { get; set; }
  }
}
