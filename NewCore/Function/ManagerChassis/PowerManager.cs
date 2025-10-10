using DTO.Device.Chassis;
using DTO.Device.Chassis.Capabilities;
using DTO.Service;
using NewCore.Communication;
using static AppConfiguration.Execution.ExecutionConfig;

namespace NewCore.Function.ManagerChassis
{
  /// <summary>
  /// Класс для управления питанием шасси.
  /// </summary>
  public class PowerManager : IPowerManagerChassis
  {
    /// <summary>
    /// Интерфейс управления шасси.
    /// </summary>
    private IChassisManager _chassisModel { get; set; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="PowerManager"/>.
    /// </summary>
    /// <param name="managerChassis">Экземпляр менеджера шасси.</param>
    public PowerManager(IChassisManager managerChassis) => _chassisModel = managerChassis;

    /// <inheritdoc />
    public async Task StartPowerAsync(IUserMessageService? userMessageService = null)
    {
      if (await GetIsIdleModeEnabled())
      {
        return;
      }

      var cmd = new DeviceCommand(2, 1, 1);
      await _chassisModel.DeviceProtocol.QueryAsync(cmd.ToString());
    }

    /// <inheritdoc />
    public async Task StopPowerAsync(IUserMessageService? userMessageService = null)
    {
      if (await GetIsIdleModeEnabled())
      {
        return;
      }

      var cmd = new DeviceCommand(2, 2, 1);
      await _chassisModel.DeviceProtocol.QueryAsync(cmd.ToString());
    }
  }
}
