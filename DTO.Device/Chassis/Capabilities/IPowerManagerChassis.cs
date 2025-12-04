using DTO.Service;

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
    Task StopPowerAsync(IUserInteractionService? userMessageService = null);

    /// <summary>
    /// Включает питание шасси.
    /// </summary>
    /// <returns>Асинхронная задача.</returns>
    Task StartPowerAsync(IUserInteractionService? userMessageService = null);
  }
}
