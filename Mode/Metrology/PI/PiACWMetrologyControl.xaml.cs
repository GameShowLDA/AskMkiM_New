using AppConfiguration.Error.Device;
using AppConfiguration.Error.Device.Breakdown;
using AppConfiguration.Interface;
using DTO.Base.Models;
using DTO.Base.Models.MeasurementError;
using DTO.Device.Breakdown;
using DTO.Service;
using Mode.Base;
using Mode.Metrology.MeasurementSystem;
using System.Windows;
using System.Windows.Controls;
using UI.Controls.ProtocolNew;
using Utilities;
using Utilities.Help;
using static DTO.Enum.Measurement;

namespace Mode.Metrology.PI
{
  /// <summary>
  /// Логика взаимодействия для PiACWMetrologyControl.xaml.
  /// </summary>
  public partial class PiACWMetrologyControl : UserControl, IExecution
  {
    MeasurementTypeCommand metrologicalModeRole => MeasurementTypeCommand.PI_ACW;

    PiMeasurement testMeasurement = new PiMeasurement();

    (bool Success, string Message, DataModel DataModel) Data;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="PiACWMetrologyControl"/>.
    /// </summary>
    public PiACWMetrologyControl()
    {
      InitializeComponent();
      InitializeSettings();

      MouseMove += (s, e) =>
      {
        HelpProvider.SetHelpKey(this, "UtilityModePI");
      };
    }

    /// <summary>
    /// Инициализирует все необходимые настройки для компонента.
    /// Очищает предыдущий контент и добавляет новые элементы управления.
    /// </summary>
    public async void InitializeSettings()
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
      Data = UIValidationHelper.TryValidateAndParseInputWithEquipment(ProtocolUI, timeCheck: true, voltageCheck: true, timeRampCheck: true);
      if (!Data.Success)
      {
        await ProtocolUI.ShowMessageAsync(new ShowMessageModel("Ошибка", ShowMessageModel.ErrorMessage.TitleColor, Data.Message), SkipStepModeCheck: true);
        return;
      }

      var first = Data.DataModel.FirstPoint;
      var second = Data.DataModel.SecondPoint;
      var param = Data.DataModel.Param;

      await NewCore.Communication.DeviceCommandSender.ResetAllSystem();

      var connect = await testMeasurement.ConnectToEquipment(first, second, metrologicalModeRole, ProtocolUI);
      if (!connect.Connect)
      {
        await ProtocolUI.ShowMessageAsync(new ShowMessageModel("Ошибка", ShowMessageModel.ErrorMessage.TitleColor, connect.Message), SkipStepModeCheck: true);
        return;
      }

      await testMeasurement.SetupCommutation(ProtocolUI, first, second, metrologicalModeRole);
      await testMeasurement.ConfigureMeter(ProtocolUI, metrologicalModeRole, Data.DataModel);
      await UserActionHelper.RunWithUserRepeatAsync(async () => await testMeasurement.PerformMeasurement(metrologicalModeRole, param, ProtocolUI), ProtocolUI, true);
    }

    public ITextAdapter GetControl()
    {
      return ProtocolUI;
    }

    private class PiMeasurement : BaseMeasurement
    {
      public PiMeasurement() : base() { }

      /// <inheritdoc />
      public override async Task ConfigureMeter(IUserMessageService messageService, MeasurementTypeCommand metrologicalModeRole, DataModel dataModel = null)
      {
        var breakDown = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IBreakdownTester>().FirstOrDefault() : null;
        string name = breakDown.Name;
        int chassis = breakDown.NumberChassis;
        int numer = breakDown.Number;

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.ConnectableManager.InitializeAsync(messageService)).Connect, messageService))
          throw ConnectionExceptionFactory.ConnectFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.AcwManger.Mode.SetModeAsync(messageService)).Success, messageService))
          throw AcwExceptionFactory.SetModeFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.AcwManger.Time.SetTestTimeAsync(dataModel.Time, messageService)).Success, messageService))
          throw AcwExceptionFactory.SetTestTimeFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.AcwManger.Time.SetRampTimeAsync(dataModel.RampTime, messageService)).Success, messageService))
          throw AcwExceptionFactory.SetRampTimeFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.AcwManger.FrequencyConfigurable.SetFrequencyAsync(50, messageService)).Success, messageService))
          throw AcwExceptionFactory.SetFrequencyFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.AcwManger.CurrentLimits.SetLowCurrentLimitAsync(0, messageService)).Success, messageService))
          throw AcwExceptionFactory.SetLowLimitFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.AcwManger.CurrentLimits.SetHighCurrentLimitAsync(10, messageService)).Success, messageService))
          throw AcwExceptionFactory.SetHighLimitFailed(name, chassis, numer);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.AcwManger.Voltage.SetVoltageAsync(dataModel.Param, messageService)).Success, messageService))
          throw AcwExceptionFactory.SetVoltageFailed(name, chassis, numer);
      }

      /// <inheritdoc />
      public override async Task<bool> PerformMeasurement(MeasurementTypeCommand metrologicalModeRole, double param, ProtocolUI protocolUI)
      {
        var meterDevice = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IBreakdownTester>().FirstOrDefault() : null;
        await protocolUI.ShowMessageAsync(new ShowMessageModel(header: "Выполнение измерения сопротивления изоляции", headerColor: ShowMessageModel.SuccessMessage.TitleColor));

        (LowerBound, UpperBound, var delta) = MeasurementErrorDefaults.CalculateToleranceRange(MeasurementTypeCommand.PI_ACW, param);
        await meterDevice.AcwManger.Measure.MeasureAsync(param, userMessageService: protocolUI);

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
        Measurements.Add(result);

        await protocolUI.ShowMessageAsync(new ShowMessageModel("Результат измерения напряжения", message: $"{result} В", type: (result >= LowerBound && result <= UpperBound ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 1 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", message: $"от {LowerBound} до {UpperBound} В") { IndentLevel = 2 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Погрешность измерения", message: $"{(Math.Abs(result - param))} В", type: (result >= LowerBound && result <= UpperBound ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 2 }, skipPause: true);

        return true;
      }

      public override async Task FinalizeMeasurement(IUserMessageService messageService)
      {
        await base.FinalizeMeasurement(messageService);
        await PrintResult(messageService, MeasurementTypeCommand.PI_ACW);
      }
    }
  }
}
