using DTO.Base.Models;
using DTO.Device.Breakdown;
using DTO.Service;
using Errors.Models;
using Mode.Base;
using Mode.TestSuite.Metrology.MethodExecutor;
using System.Windows.Controls;
using UI.Controls.ProtocolNew;
using static Mode.Base.UIValidationHelper;

namespace TestWPF
{
  /// <summary>
  /// Логика взаимодействия для TestProtocolControl.xaml
  /// </summary>
  public partial class TestProtocolControl : UserControl
  {
    public TestProtocolControl()
    {
      InitializeComponent();
      InitializeSettingsAsync().ConfigureAwait(true);
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
      var connect = await testMeasurement.ConnectToEquipment(data.FirstPoint, data.SecondPoint, ProtocolUI);
      if (!connect.Connect)
      {
        await ProtocolUI.ShowMessageAsync(new ShowMessageModel("Ошибка", message: connect.Message, type: ShowMessageModel.MessageType.Error));
        return;
      }

      await testMeasurement.SetupCommutation(ProtocolUI, data.FirstPoint, data.SecondPoint, data.ActiveBus);
      await testMeasurement.RunParallelModuleTasksAsync(ProtocolUI, data);
      await testMeasurement.FinalizeAsync(ProtocolUI);
    }
  }
  public class TestMeasurement : BaseMethodExecutor
  {
    public TestMeasurement() : base() { }

    /// <inheritdoc />
    public override async Task ConfigureMeter(IUserInteractionService messageService, DataModel dataModel = null)
    {
      var breakDown = Devices.OfType<IBreakdownTester>().FirstOrDefault();
      await breakDown.ConnectableManager.ConnectAsync(messageService);
      await breakDown.IrManger.Mode.SetModeAsync();
      await breakDown.IrManger.Voltage.SetVoltageAsync(dataModel.Voltage);
      await breakDown.IrManger.Time.SetTestTimeAsync(dataModel.Time);
    }

    /// <inheritdoc />
    public override async Task PerformMeasurement(IUserInteractionService messageService, DataModel dataModel)
    {
      //var breakDown = Devices.OfType<IBreakdownTester>().FirstOrDefault();
      await messageService.ShowMessageAsync(new ShowMessageModel("\tИзмерение сопротивления изоляции"));

      // var answer = await breakDown.IrManger.MeasureResistanceAsync();
      var answer = 0;
      var type = ShowMessageModel.MessageType.Success;

      if (answer < (dataModel.Param * 1000))
      {
        type = ShowMessageModel.MessageType.Error;
      }

      await messageService.ShowMessageAsync(new ShowMessageModel($"\t\tРезультат измерения разряда {HighestBitCount}({GetBitString()})", message: $"{answer.ToString()} МОм", type: type));
    }

    public override async Task FinalizeAsync(IUserInteractionService messageService)
    {
      await base.FinalizeAsync(messageService);
      var breakDown = Devices.OfType<IBreakdownTester>().FirstOrDefault();
      await breakDown.ConnectableManager.DisconnectAsync(messageService);
    }
  }
}
