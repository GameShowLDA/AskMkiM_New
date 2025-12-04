using DTO.Base.Models;
using DTO.Device.Breakdown;
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

namespace Mode.TestSuite.Metrology.MethodExecutor.PI
{
  /// <summary>
  /// Логика взаимодействия для PiDCWMethodExecutorControl.xaml.
  /// </summary>
  public partial class PiDCWMethodExecutorControl : UserControl
  {
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="PiDCWMethodExecutorControl"/>.
    /// </summary>
    public PiDCWMethodExecutorControl()
    {
      InitializeComponent();
      InitializeSettingsAsync().ConfigureAwait(true);

      // Регистрируем обработчик движения мыши
      MouseMove += (s, e) =>
      {
        // Обновляем последний элемент под курсором
        HelpProvider.SetHelpKey(this, "TestPIGroupMethod");
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
      var data = await EnsureValidMetrologyInputAsync(ProtocolUI, timeCheck: true, timeRampCheck: true, voltageCheck: true, busCheck: true);
      
      PiDCWMethodExecutorMeasurement testMeasurement = new PiDCWMethodExecutorMeasurement();
      try
      {
        var connect = await testMeasurement.ConnectToEquipment(data.FirstPoint, data.SecondPoint, ProtocolUI);
        if (!connect.Connect)
        {
          await ProtocolUI.ShowMessageAsync(new ShowMessageModel("Ошибка", message: connect.Message, type: ShowMessageModel.MessageType.Error), SkipStepModeCheck: true);
          return;
        }

        await testMeasurement.SetupCommutation(ProtocolUI, data.FirstPoint, data.SecondPoint, data.ActiveBus);
        await testMeasurement.ConfigureMeter(ProtocolUI, data);
        await testMeasurement.RunParallelModuleTasksAsync(ProtocolUI, data);
      }
      finally
      {
        await testMeasurement.FinalizeAsync(ProtocolUI);
      }
    }

    private class PiDCWMethodExecutorMeasurement : BaseMethodExecutor
    {
      public PiDCWMethodExecutorMeasurement() : base() { }

      /// <inheritdoc />
      public override async Task ConfigureMeter(IUserMessageService messageService, DataModel dataModel = null)
      {
        var breakDown = Devices.OfType<IBreakdownTester>().FirstOrDefault();
        var name = breakDown.Name;
        var chassis = breakDown.NumberChassis;
        var number = breakDown.Number;

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.ConnectableManager.InitializeAsync(messageService)).Connect, messageService))
          throw ConnectionExceptionAdapter.ConnectFailed(name, chassis, number);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.DcwManger.Mode.SetModeAsync(messageService)).Success, messageService))
          throw DcwExceptionFactory.SetModeFailed(name, chassis, number);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.DcwManger.Voltage.SetVoltageAsync(dataModel.Voltage, messageService)).Success, messageService))
          throw DcwExceptionFactory.SetVoltageFailed(name, chassis, number);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.DcwManger.Time.SetTestTimeAsync(dataModel.Time, messageService)).Success, messageService))
          throw DcwExceptionFactory.SetTestTimeFailed(name, chassis, number);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.DcwManger.Time.SetRampTimeAsync(dataModel.RampTime, messageService)).Success, messageService))
          throw DcwExceptionFactory.SetRampTimeFailed(name, chassis, number);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.DcwManger.CurrentLimits.SetHighCurrentLimitAsync(dataModel.Param, messageService)).Success, messageService))
          throw DcwExceptionFactory.SetHighLimitFailed(name, chassis, number);
      }

      /// <inheritdoc />
      public override async Task PerformMeasurement(IUserMessageService messageService, DataModel dataModel)
      {
        var breakDown = Devices.OfType<IBreakdownTester>().FirstOrDefault();

        await messageService.ShowMessageAsync(new ShowMessageModel("\tИспытания прочности изоляции(DCW)"));
        await UserActionHelper.RunWithUserRepeatAsync(async () =>
        {
          var answer = await breakDown.DcwManger.Measure.MeasureAsync();
          var type = ShowMessageModel.MessageType.Success;

          if (answer.value >= dataModel.Param)
          {
            type = ShowMessageModel.MessageType.Error;
          }

          // await protocolUI.ShowMessageAsync(new ShowMessageModel($"\t\tРезультат измерения разряда ({GetBitString()})", message: $"{answer.ToString()} мА", type: type));
          return type == ShowMessageModel.MessageType.Success ? true : false;

        }, messageService);
      }

      public override async Task FinalizeAsync(IUserMessageService messageService)
      {
        await base.FinalizeAsync(messageService);
      }
    }
  }
}
