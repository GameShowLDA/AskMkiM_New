using Errors.Device;
using Errors.Device.Adapters;

namespace AppConfiguration.Error.Device.ModuleVoltageCurrent
{
  /// <summary>
  /// Фабрика исключений для операций управления напряжением в модуле МИНТ.
  /// </summary>
  public static class VoltageExceptionFactory
  {
    /// <summary>
    /// Исключение при ошибке установки источника напряжения.
    /// </summary>
    public static DeviceException SetSourceFailed(string source, string reason = null) => VoltageExceptionAdapter.SetLevelFailed(source, reason);

    /// <summary>
    /// Исключение при ошибке установки уровня напряжения.
    /// </summary>
    public static DeviceException SetLevelFailed(string value, string reason = null) => VoltageExceptionAdapter.SetLevelFailed(value, reason);
  }
}
