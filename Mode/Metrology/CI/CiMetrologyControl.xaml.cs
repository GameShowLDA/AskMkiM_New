using AppConfiguration.Interface;
using DTO.Base.Models;
using DTO.Base.Models.MeasurementError;
using DTO.Device.Breakdown;
using DTO.Service;
using Errors.Device;
using Errors.Device.Adapters;
using Errors.Device.Breakdown;
using Errors.Models;
using Mode.Base;
using Mode.Metrology.MeasurementSystem;
using System.Windows.Controls;
using UI.Controls.ProtocolNew;
using Utilities;
using Utilities.Help;
using static DTO.Enum.Measurement;
using static Mode.Base.UIValidationHelper;

namespace Mode.Metrology.CI
{
  /// <summary>
  /// Логика взаимодействия для CiMetrologyControl.xaml.
  /// </summary>
  public partial class CiMetrologyControl : UserControl, IExecution
  {
    MeasurementTypeCommand metrologicalModeRole => MeasurementTypeCommand.CI;

    CiMeasurement testMeasurement = new CiMeasurement();

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CiMetrologyControl"/>.
    /// </summary>
    public CiMetrologyControl()
    {
      InitializeComponent();
      InitializeSettings();

      // Регистрируем обработчик движения мыши
      MouseMove += (s, e) =>
      {
        // Обновляем последний элемент под курсором
        HelpProvider.SetHelpKey(this, "UtilityModeSI");
      };
    }

    /// <summary>
    /// Инициализирует все необходимые настройки для компонента.
    /// Очищает предыдущий контент и добавляет новые элементы управления.
    /// </summary>
    public void InitializeSettings()
    {
      ProtocolUI.SetSettings(
        this,
        StartDelegate: ExecuteMeasurementProcess,
        true,
        StopDelegate: async (CancellationToken token) =>
        {
          await testMeasurement.FinalizeMeasurement(ProtocolUI);
        });
    }

    /// <summary>
    /// Выполнение контроля.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns></returns>
    private async Task ExecuteMeasurementProcess(CancellationToken cancellationToken)
    {
      var data = await EnsureValidMetrologyInputAsync(ProtocolUI, timeCheck: true, voltageCheck: true);

      await NewCore.Communication.DeviceCommandSender.ResetAllSystem();

      await testMeasurement.ConnectToEquipment(data.FirstPoint, data.SecondPoint, metrologicalModeRole, ProtocolUI);
      await testMeasurement.SetupCommutation(ProtocolUI, data.FirstPoint, data.SecondPoint, metrologicalModeRole);
      await testMeasurement.ConfigureMeter(ProtocolUI, metrologicalModeRole, data);
      var (LowerBound, UpperBound, delta) = MeasurementErrorDefaults.CalculateToleranceRange(MeasurementTypeCommand.CI, data.Param);

      await ProtocolUI.AppendEmptyLineAsync();
      await ProtocolUI.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", headerColor: ShowMessageModel.SuccessMessage.TitleColor, message: $"от {LowerBound} до {UpperBound} Ом"));

      await UserActionHelper.RunWithUserRepeatAsync(async () => await testMeasurement.PerformMeasurement(metrologicalModeRole, data.Param, ProtocolUI), ProtocolUI, true);
    }

    public ITextAdapter GetControl()
    {
      return ProtocolUI;
    }

    private class CiMeasurement : BaseMeasurement
    {
      public CiMeasurement() : base() { }

      /// <inheritdoc />
      public override async Task ConfigureMeter(IUserMessageService messageService, MeasurementTypeCommand metrologicalModeRole, DataModel dataModel = null)
      {
        await base.ConfigureMeter(messageService, metrologicalModeRole, dataModel);
        var breakDown = Devices.TryGetValue(MeasurementTypeCommand.CI, out var meter) ? meter.OfType<IBreakdownTester>().FirstOrDefault() : null;
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
      public override async Task<bool> PerformMeasurement(MeasurementTypeCommand metrologicalModeRole, double param, ProtocolUI protocolUI)
      {
        var meterDevice = Devices.TryGetValue(MeasurementTypeCommand.CI, out var meter) ? meter.OfType<IBreakdownTester>().FirstOrDefault() : null;
        await protocolUI.ShowMessageAsync(new ShowMessageModel(header: "Выполнение измерения сопротивления изоляции"));
        (LowerBound, UpperBound, var delta) = MeasurementErrorDefaults.CalculateToleranceRange(MeasurementTypeCommand.CI, param);

        var result = !await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? await meterDevice.IrManger.Measure.MeasureAsync(param, LowerBound, UpperBound, protocolUI) : !await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled() ? param : new Random().Next((int)LowerBound - 100, (int)UpperBound + 100);

        var err = result - param;
        Measurements.Add(err);

        await protocolUI.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления изоляции", message: $"{result} Ом", type: (result >= LowerBound && result <= UpperBound ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 1 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Погрешность измерения", message: $"{err} Ом", type: (result >= LowerBound && result <= UpperBound ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 2 }, skipPause: true);

        return true;
      }

      public override async Task FinalizeMeasurement(IUserMessageService messageService)
      {
        await base.FinalizeMeasurement(messageService);
        await PrintResult(messageService, MeasurementTypeCommand.CI);
        await messageService.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", message: $"от {LowerBound} до {UpperBound} Ом") { IndentLevel = 1 }, skipPause: true);
        Measurements.Clear();
      }
    }
  }
}
