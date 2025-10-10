using DTO.Base.Models;
using DTO.Device.FastMeter;
using DTO.Device.PowerSourceModule;
using DTO.Device.PowerSourceModule.Capabilities;
using DTO.Device.SwitchingDevice;
using DTO.Service;
using NewCore.Communication;
using static DTO.Enum.DeviceEnums;

namespace NewCore.Function.ModuleVoltageCurrentSource.SelfCheck
{
  public class SelfTestManager : ISelfTestCheckerModuleVoltageCurrentSource
  {

    /// <inheritdoc />
    public async Task StartSelfCheck(CancellationToken cancellationToken, IUserMessageService messageService, System.Enum selectedType, ISwitchingDevice dbc = null, IPowerSourceModule powerDevice = null, IFastMeter meter = null)
    {
      if (selectedType is not PowerSourceModuleTypeConnector type)
      {
        await messageService.ShowMessageAsync(new ShowMessageModel(
          "Ошибка",
          message: "Неверный тип проверки: требуется TypeConnector",
          type: ShowMessageModel.MessageType.Error));

        return;
      }

      if (!await CheckConnectionsAsync(messageService, dbc, meter, powerDevice))
      {
        return;
      }


      switch (type)
      {

        case PowerSourceModuleTypeConnector.FullCheck:
          await DeviceCommandSender.ResetAllSystem();
          await SettingsMeter(meter, messageService);
          await powerDevice.BusManager.ConnectBusToPositiveAsync(SwitchingBus.A2, messageService);
          await powerDevice.BusManager.ConnectBusToNegativeAsync(SwitchingBus.B2, messageService);
          await dbc.DeviceProtocol.QueryAsync(new DeviceCommand(5, 2, 2, 1).ToString());
          await VoltageCheckService.GenerateDiscreteVoltageCheck(cancellationToken, messageService, meter, powerDevice);

          await DeviceCommandSender.ResetAllSystem();
          await SwitchingSelfControl.CheckSwitching(cancellationToken, messageService, meter, powerDevice, dbc);

          await DeviceCommandSender.ResetAllSystem();
          await ResistanceMeasurementCheckService.PerformResistanceCheckAsync(cancellationToken, messageService, meter, powerDevice, dbc);
          break;


        case PowerSourceModuleTypeConnector.OutputVoltageCheck:
          await SettingsMeter(meter, messageService);
          await powerDevice.BusManager.ConnectBusToPositiveAsync(SwitchingBus.A2, messageService);
          await powerDevice.BusManager.ConnectBusToNegativeAsync(SwitchingBus.B2, messageService);
          await dbc.DeviceProtocol.QueryAsync(new DeviceCommand(5, 2, 2, 1).ToString());
          await VoltageCheckService.GenerateDiscreteVoltageCheck(cancellationToken, messageService, meter, powerDevice);
          break;

        case PowerSourceModuleTypeConnector.CommutationCheck:
          await DeviceCommandSender.ResetAllSystem();
          await SwitchingSelfControl.CheckSwitching(cancellationToken, messageService, meter, powerDevice, dbc);
          break;

        case PowerSourceModuleTypeConnector.OutputCurrentCheck:
          await DeviceCommandSender.ResetAllSystem();
          await ResistanceMeasurementCheckService.PerformResistanceCheckAsync(cancellationToken, messageService, meter, powerDevice, dbc);
          break;
      }

    }

    private static async Task<bool> CheckConnectionsAsync(IUserMessageService messageService, ISwitchingDevice device, IFastMeter meter, IPowerSourceModule powerSource)
    {
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("Проверка подключения устройств");
      var result1 = await device.ConnectableManager.InitializeAsync(messageService);
      var result2 = await meter.ConnectableManager.InitializeAsync(messageService);
      var result3 = await powerSource.ConnectableManager.InitializeAsync(messageService);
      Console.ForegroundColor = ConsoleColor.White;

      if (result1.Connect && result2.Connect && result3.Connect)
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Оба устройства подключены");
        return true;
      }
      if (!result1.Connect)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("УКШ не подключено");
        Console.ForegroundColor = ConsoleColor.White;
      }
      if (!result2.Connect)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Мультиметр не подключен");
        Console.ForegroundColor = ConsoleColor.White;
      }
      if (!result3.Connect)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("МИНТ не подключен");
        Console.ForegroundColor = ConsoleColor.White;
      }
      Console.ForegroundColor = ConsoleColor.White;
      return false;
    }

    private static async Task SettingsMeter(IFastMeter meter, IUserMessageService messageService)
    {
      await meter.ConnectableManager.ConnectAsync(messageService);
      await meter.DcVoltageManager.SetDCVoltageModeAsync(messageService);
    }

    public Type GetTestTypeEnum()
    {
      return typeof(PowerSourceModuleTypeConnector);
    }
  }
}
