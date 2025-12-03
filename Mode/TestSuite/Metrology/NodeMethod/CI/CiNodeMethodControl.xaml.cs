using DTO.Base.Models;
using DTO.Device.Breakdown;
using DTO.Enum;
using DTO.Service;
using Errors.Device;
using Errors.Device.Adapters;
using Errors.Device.Breakdown;
using Errors.Models;
using Mode.Base;
using System.Windows.Controls;
using UI.Controls.ProtocolNew;
using Utilities;
using Utilities.Help;
using static Mode.Base.UIValidationHelper;

namespace Mode.TestSuite.Metrology.NodeMethod.CI
{
  /// <summary>
  /// Логика взаимодействия для CiNodeMethodControl.xaml.
  /// </summary>
  public partial class CiNodeMethodControl : UserControl
  {
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CiNodeMethodControl"/>.
    /// </summary>
    public CiNodeMethodControl()
    {
      InitializeComponent();
      InitializeSettingsAsync().ConfigureAwait(true);

      // Регистрируем обработчик движения мыши
      MouseMove += (s, e) =>
      {
        // Обновляем последний элемент под курсором
        HelpProvider.SetHelpKey(this, "TestSINodeMethod");
      };
    }

    /// <summary>
    /// Инициализирует все необходимые настройки для компонента.
    /// Очищает предыдущий контент и добавляет новые элементы управления.
    /// </summary>
    public async Task InitializeSettingsAsync()
    {
      ProtocolUI.SetSettings(this, StartDelegate: ExecuteMeasurementProcess, true, null);
    }

    /// <summary>
    /// Выполнение контроля.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ExecuteMeasurementProcess(CancellationToken cancellationToken)
    {
      var data = await EnsureValidMetrologyInputAsync(ProtocolUI, timeCheck: true, voltageCheck: true);
      await NewCore.Communication.DeviceCommandSender.ResetAllSystem();

      CiNodeMethod testMeasurement = new CiNodeMethod();
      var connect = await testMeasurement.ConnectToEquipment(data.FirstPoint, data.SecondPoint, ProtocolUI);
      if (!connect.Connect)
      {
        await ProtocolUI.ShowMessageAsync(new ShowMessageModel("Ошибка", message: connect.Message, type: ShowMessageModel.MessageType.Error));
        return;
      }

      await testMeasurement.SetupCommutation(ProtocolUI, data.FirstPoint, data.SecondPoint, DeviceEnums.BusPoint.A);
      await testMeasurement.ConfigureMeter(ProtocolUI, data);
      await testMeasurement.PerformMeasurement(ProtocolUI, data);
      await testMeasurement.FinalizeAsync(ProtocolUI);
    }

    private class CiNodeMethod : BaseNodeTest
    {
      public CiNodeMethod() : base() { }

      /// <inheritdoc />
      public override async Task ConfigureMeter(IUserMessageService messageService, DataModel dataModel = null)
      {
        var breakDown = Devices.OfType<IBreakdownTester>().FirstOrDefault();
        string name = breakDown.Name;
        int chassis = breakDown.NumberChassis;
        int numer = breakDown.Number;

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.ConnectableManager.InitializeAsync(messageService)).Connect, messageService))
          throw ConnectionExceptionAdapter.ConnectFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.IrManger.Mode.SetModeAsync(messageService)).Success, messageService))
          throw IrExceptionFactory.SetModeFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.IrManger.Voltage.SetVoltageAsync(dataModel.Voltage, messageService)).Success, messageService))
          throw IrExceptionFactory.SetVoltageFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.IrManger.Time.SetTestTimeAsync(dataModel.Time, messageService)).Success, messageService))
          throw IrExceptionFactory.SetTestTimeFailed(name, chassis, numer);

      }

      /// <inheritdoc />
      public override async Task PerformMeasurement(ProtocolUI protocolUI, DataModel dataModel)
      {
        var breakDown = Devices.OfType<IBreakdownTester>().FirstOrDefault();
        var token = protocolUI.GetCancellationToken();

        while (true)
        {
          token.ThrowIfCancellationRequested();

          var connectResult = await GetNextPoint(protocolUI);
          if (connectResult.Step)
          {
            await protocolUI.ShowMessageAsync(new ShowMessageModel("Измерение сопротивления изоляции"));

            await UserActionHelper.RunWithUserRepeatAsync(async () =>
            {
              token.ThrowIfCancellationRequested();
              var answer = await breakDown.IrManger.Measure.MeasureAsync(dataModel.Param, 1000, 60000, protocolUI);
              var type = ShowMessageModel.MessageType.Success;

              if (answer < dataModel.Param)
              {
                type = ShowMessageModel.MessageType.Error;
              }

              // await protocolUI.ShowMessageAsync(new ShowMessageModel("\tРезультат измерения", message: $"{answer.ToString()} МОм", type: type), skipPause: true);

              return type == ShowMessageModel.MessageType.Success ? true : false;

            }, protocolUI);
          }
          else
          {
            break;
          }
        }
      }

      public override async Task FinalizeAsync(IUserMessageService messageService)
      {
        await base.FinalizeAsync(messageService);
      }
    }
  }
}
