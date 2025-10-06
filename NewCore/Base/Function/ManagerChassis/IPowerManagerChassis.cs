using Utilities.Interface;

namespace NewCore.Base.Function.ManagerChassis
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
