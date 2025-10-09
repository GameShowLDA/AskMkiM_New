using Utilities.Interface;

namespace DTO.Device.Chassis.Capabilities
{
  /// <summary>
  /// Интерфейс для управления питанием шасси.
  /// </summary>
  public interface IPowerManagerChassis
  {
    /// <summary>
    /// Отключает питание шасси.
    /// </summary>
    /// <returns>Асинхронная задача.</returns>
    Task StopPowerAsync(IUserMessageService? userMessageService = null);

    /// <summary>
    /// Включает питание шасси.
    /// </summary>
    /// <returns>Асинхронная задача.</returns>
    Task StartPowerAsync(IUserMessageService? userMessageService = null);
  }
}
