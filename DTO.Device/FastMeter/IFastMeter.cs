using DTO.Device.Base;
using DTO.Device.FastMeter.Capabilities;

namespace DTO.Device.FastMeter

{
  /// <summary>
  /// Интерфейс для быстрого измерителя.
  /// </summary>
  public interface IFastMeter : IAttachableDevice
  {
    /// <summary>
    /// Управление измерением переменного напряжения.
    /// </summary>
    public IAcVoltageMeasurement AcVoltageManager { get; set; }

    /// <summary>
    /// Управление измерением ёмкости.
    /// </summary>
    public ICapacitanceMeasurement CapacitanceManager { get; set; }

    /// <summary>
    /// Управление измерением проводимости (прозвонка).
    /// </summary>
    public IContinuityMeasurement ContinuityManager { get; set; }

    /// <summary>
    /// Управление измерением постоянного напряжения.
    /// </summary>
    public IDcVoltageMeasurement DcVoltageManager { get; set; }

    /// <summary>
    /// Управление измерением сопротивления.
    /// </summary>
    public IResistanceMeasurement ResistanceManager { get; set; }

    /// <summary>
    /// Максимальное сопротивление (Ом), при котором считается срабатывание прозвонки.
    /// </summary>
    public int MaxContinuityResistance { get; set; }
  }
}
