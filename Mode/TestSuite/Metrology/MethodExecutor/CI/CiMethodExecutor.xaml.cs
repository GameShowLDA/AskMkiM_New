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

namespace Mode.TestSuite.Metrology.MethodExecutor.CI
{
  /// <summary>
  /// Логика взаимодействия для CiMethodExecutor.xaml.
  /// </summary>
  public partial class CiMethodExecutor : UserControl
  {
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CiMethodExecutor"/>.
    /// </summary>
    public CiMethodExecutor()
    {
      InitializeComponent();
      InitializeSettingsAsync().ConfigureAwait(true);

      // Регистрируем обработчик движения мыши
      MouseMove += (s, e) =>
      {
        // Обновляем последний элемент под курсором
        HelpProvider.SetHelpKey(this, "TestSIGroupMethod");
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
      var data = await EnsureValidMetrologyInputAsync(ProtocolUI, timeCheck: true, voltageCheck: true, busCheck: true);
      TestMeasurement testMeasurement = new TestMeasurement();
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

    private class TestMeasurement : BaseMethodExecutor
    {
      public TestMeasurement() : base() { }

      /// <inheritdoc />
      public override async Task ConfigureMeter(IUserMessageService messageService, DataModel dataModel = null)
      {
        var breakDown = Devices.OfType<IBreakdownTester>().FirstOrDefault();
        var name = breakDown.Name;
        var chassis = breakDown.NumberChassis;
        var number = breakDown.Number;

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.ConnectableManager.InitializeAsync(messageService)).Connect, messageService))
          throw ConnectionExceptionAdapter.ConnectFailed(name, chassis, number);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.IrManger.Mode.SetModeAsync(messageService)).Success, messageService))
          throw IrExceptionFactory.SetModeFailed(name, chassis, number);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.IrManger.Voltage.SetVoltageAsync(dataModel.Voltage, messageService)).Success, messageService))
          throw IrExceptionFactory.SetVoltageFailed(name, chassis, number);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.IrManger.Time.SetTestTimeAsync(dataModel.Time, messageService)).Success, messageService))
          throw IrExceptionFactory.SetTestTimeFailed(name, chassis, number);
      }

      /// <inheritdoc />
      public override async Task PerformMeasurement(IUserMessageService messageService, DataModel dataModel)
      {
        await UserActionHelper.RunWithUserRepeatAsync(async () =>
        {
          messageService.GetCancellationToken().ThrowIfCancellationRequested();

          var breakDown = Devices.OfType<IBreakdownTester>().FirstOrDefault();
          await messageService.ShowMessageAsync(new ShowMessageModel("\tИзмерение сопротивления изоляции"));

          var answer = await breakDown.IrManger.Measure.MeasureAsync(dataModel.Param, dataModel.Param, 60000, messageService);
          var type = ShowMessageModel.MessageType.Success;
          if (answer < dataModel.Param)
          {
            type = ShowMessageModel.MessageType.Error;
          }

          await messageService.ShowMessageAsync(new ShowMessageModel($"\t\tРезультат измерения разряда {HighestBitCount}({GetBitString()})", message: $"{answer.ToString()} МОм", type: type), skipPause: true);
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
