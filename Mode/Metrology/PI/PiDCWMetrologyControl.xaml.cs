using AppConfiguration.Interface;
using DTO.Base.Models;
using DTO.Base.Models.MeasurementError;
using DTO.Device.Breakdown;
using DTO.Service;
using Errors.Device.Breakdown;
using Mode.Metrology.MeasurementSystem;
using System.Windows;
using System.Windows.Controls;
using UI.Controls.ProtocolNew;
using Utilities;
using Utilities.Help;
using static DTO.Enum.Measurement;
using static Mode.Base.UIValidationHelper;

namespace Mode.Metrology.PI
{
  /// <summary>
  /// Логика взаимодействия для PiMetrologyControl.xaml.
  /// </summary>
  public partial class PiDCWMetrologyControl : UserControl, IExecution
  {
    MeasurementTypeCommand metrologicalModeRole => MeasurementTypeCommand.PI_DCW;

    PiMeasurement testMeasurement = new PiMeasurement();

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="PiDCWMetrologyControl"/>.
    /// </summary>
    public PiDCWMetrologyControl()
    {
      InitializeComponent();
      InitializeSettings();

      // Регистрируем обработчик движения мыши
      MouseMove += (s, e) =>
      {
        // Обновляем последний элемент под курсором
        HelpProvider.SetHelpKey(this, "UtilityModePI");
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ExecuteMeasurementProcess(CancellationToken cancellationToken)
    {
      var data = await EnsureValidMetrologyInputAsync(ProtocolUI, timeCheck: true, timeRampCheck: true);
      await NewCore.Communication.DeviceCommandSender.ResetAllSystem();

      await testMeasurement.ConnectToEquipment(data.FirstPoint, data.SecondPoint, metrologicalModeRole, ProtocolUI);
      await testMeasurement.SetupCommutation(ProtocolUI, data.FirstPoint, data.SecondPoint, metrologicalModeRole);
      await testMeasurement.ConfigureMeter(ProtocolUI, metrologicalModeRole, data);

      var (LowerBound, UpperBound, delta) = MeasurementErrorDefaults.CalculateToleranceRange(MeasurementTypeCommand.PI_DCW, data.Param);

      await ProtocolUI.AppendEmptyLineAsync();
      await ProtocolUI.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", headerColor: ShowMessageModel.SuccessMessage.TitleColor, message: $"от {LowerBound} до {UpperBound} В"));

      await UserActionHelper.RunWithUserRepeatAsync(async () => await testMeasurement.PerformMeasurement(metrologicalModeRole, data.Param, ProtocolUI), ProtocolUI, true);
    }

    public ITextAdapter GetControl()
    {
      return ProtocolUI;
    }

    private class PiMeasurement : BaseMeasurement
    {
      public PiMeasurement() : base() { }

      /// <inheritdoc />
      public override async Task ConfigureMeter(IUserInteractionService messageService, MeasurementTypeCommand metrologicalModeRole, DataModel dataModel = null)
      {
        var breakDown = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IBreakdownTester>().FirstOrDefault() : null;
        string name = breakDown.Name;
        int chassis = breakDown.NumberChassis;
        int numer = breakDown.Number;

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.ConnectableManager.InitializeAsync(messageService)).Connect, messageService))
          throw new Exception($"Нет подключения к {breakDown.Name}({breakDown.NumberChassis}.{breakDown.Number})");

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.DcwManger.Mode.SetModeAsync(messageService)).Success, messageService))
          throw DcwExceptionFactory.SetModeFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.DcwManger.Time.SetTestTimeAsync(dataModel.Time, messageService)).Success, messageService))
          throw DcwExceptionFactory.SetTestTimeFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.DcwManger.Time.SetRampTimeAsync(dataModel.RampTime, messageService)).Success, messageService))
          throw DcwExceptionFactory.SetRampTimeFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.DcwManger.CurrentLimits.SetHighCurrentLimitAsync(10, messageService)).Success, messageService))
          throw DcwExceptionFactory.SetHighLimitFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.DcwManger.CurrentLimits.SetLowCurrentLimitAsync(0, messageService)).Success, messageService))
          throw DcwExceptionFactory.SetLowLimitFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.DcwManger.Voltage.SetVoltageAsync(dataModel.Param, messageService)).Success, messageService))
          throw DcwExceptionFactory.SetVoltageFailed(name, chassis, numer);
      }

      /// <inheritdoc />
      public override async Task<bool> PerformMeasurement(MeasurementTypeCommand metrologicalModeRole, double param, ProtocolUI protocolUI)
      {
        var meterDevice = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IBreakdownTester>().FirstOrDefault() : null;
        await protocolUI.ShowMessageAsync(new ShowMessageModel(header: "Выполнение измерения сопротивления изоляции", headerColor: ShowMessageModel.SuccessMessage.TitleColor));

        (LowerBound, UpperBound, var delta) = MeasurementErrorDefaults.CalculateToleranceRange(MeasurementTypeCommand.PI_DCW, param);
        await meterDevice.DcwManger.Measure.MeasureAsync();

        var result = await Application.Current.Dispatcher.InvokeAsync(() =>
        {
          VoltageValue chassisManagerWindow = new VoltageValue();
          protocolUI.Effect = new System.Windows.Media.Effects.BlurEffect();

          bool? dialogResult = chassisManagerWindow.ShowDialog();
          protocolUI.Effect = null;

          if (dialogResult == true)
          {
            return chassisManagerWindow.VoltageResult;
          }
          else
          {
            return -1;
          }
        });

        var answer = (result >= LowerBound && result <= UpperBound) ? false : true;
        var err = result - param;

        Measurements.Add(err);

        await protocolUI.ShowMessageAsync(new ShowMessageModel("Результат измерения напряжения", message: $"{result} В", type: (result >= LowerBound && result <= UpperBound ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 1 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Погрешность измерения", message: $"{err} В", type: (result >= LowerBound && result <= UpperBound ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 2 }, skipPause: true);

        return true;
      }

      public override async Task FinalizeMeasurement(IUserInteractionService messageService)
      {
        await base.FinalizeMeasurement(messageService);
        await PrintResult(messageService, MeasurementTypeCommand.PI_DCW);
        await messageService.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", headerColor: ShowMessageModel.SuccessMessage.TitleColor, message: $"от {LowerBound} до {UpperBound} В"));

        Measurements.Clear();
      }
    }
  }
}
