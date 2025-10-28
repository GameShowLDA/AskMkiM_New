using AppConfiguration.Error.Device.Multimeter;
using AppConfiguration.Interface;
using DTO.Base.Models;
using DTO.Base.Models.MeasurementError;
using DTO.Device.FastMeter;
using DTO.Device.RelaySwitchModule;
using DTO.Device.RelaySwitchModule.Model;
using DTO.Enum;
using DTO.Service;
using Mode.Base;
using Mode.Metrology.MeasurementSystem;
using System.Windows.Controls;
using UI.Controls.ProtocolNew;
using Utilities;
using Utilities.Help;
using static DTO.Enum.Measurement;

namespace Mode.Metrology.EHT
{
  /// <summary>
  /// Логика взаимодействия для EhtMetrologyControl.xaml
  /// </summary>
  public partial class EhtMetrologyControl : UserControl, IExecution
  {
    MeasurementTypeCommand metrologicalModeRole => MeasurementTypeCommand.EHT;
    EhtMeasurement testMeasurement = new EhtMeasurement();
    (bool Success, string Message, DataModel DataModel) Data;
    public EhtMetrologyControl()
    {
      InitializeComponent();
      InitializeSettings();

      MouseMove += (s, e) =>
      {
        // Обновляем последний элемент под курсором
        HelpProvider.SetHelpKey(this, "UtilityModeKC");
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
      Data = UIValidationHelper.TryValidateAndParseInputWithEquipment(ProtocolUI, timeCheck: true, voltageCheck: true);
      if (!Data.Success)
      {
        await ProtocolUI.ShowMessageAsync(new ShowMessageModel("Ошибка", message: Data.Message, type: ShowMessageModel.MessageType.Error), SkipStepModeCheck: true);
        throw new Exception();
      }

      var first = Data.DataModel.FirstPoint;
      var second = Data.DataModel.SecondPoint;
      var param = Data.DataModel.Param;

      var connect = await testMeasurement.ConnectToEquipment(first, second, metrologicalModeRole, ProtocolUI);
      if (!connect.Connect)
      {
        await ProtocolUI.ShowMessageAsync(new ShowMessageModel("Ошибка", message: connect.Message, type: ShowMessageModel.MessageType.Error), SkipStepModeCheck: true);
        throw new Exception();
      }

      await testMeasurement.SetupCommutation(ProtocolUI, first, second, metrologicalModeRole);
      await testMeasurement.ConfigureMeter(ProtocolUI, metrologicalModeRole);

      await UserActionHelper.RunWithUserRepeatAsync(async () => await testMeasurement.PerformMeasurement(metrologicalModeRole, param, ProtocolUI), ProtocolUI, true);
    }

    public ITextAdapter GetControl()
    {
      return ProtocolUI;
    }

    private class EhtMeasurement : BaseMeasurement
    {

      public EhtMeasurement() : base() { }

      /// <inheritdoc />
      public override async Task ConfigureMeter(IUserMessageService messageService, MeasurementTypeCommand metrologicalModeRole, DataModel dataModel = null)
      {
        await base.ConfigureMeter(messageService, metrologicalModeRole, dataModel);
        var fastMeter = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IFastMeter>().FirstOrDefault() : null;

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => fastMeter.ResistanceManager.SetResistanceModeAsync(messageService), messageService))
        {
          throw ResistanceExceptionFactory.SetModeFailed(fastMeter.Name, fastMeter.NumberChassis, fastMeter.Number);
          throw new Exception($"Ошибка установка режима измерения сопротивления {fastMeter.Name}({fastMeter.NumberChassis}.{fastMeter.Number})");
        }
      }

      /// <inheritdoc />
      public override async Task<bool> PerformMeasurement(MeasurementTypeCommand metrologicalModeRole, double param, ProtocolUI protocolUI)
      {
        var points = GetPoints();

        var Rt1 = await StepFirst(protocolUI, metrologicalModeRole, points.Point1, param);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления", message: $"{Rt1:F5} Ом") { IndentLevel = 1 });

        var Rt2 = await StepSecond(protocolUI, metrologicalModeRole, points.Point1, points.Point2, param);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления", message: $"{Rt2:F5} Ом") { IndentLevel = 1 });

        var Rt = await StepThird(protocolUI, metrologicalModeRole, points.Point1, points.Point2, param);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления", message: $"{Rt:F5} Ом") { IndentLevel = 1 });

        (LowerBound, UpperBound, var delta) = MeasurementErrorDefaults.CalculateToleranceRange(MeasurementTypeCommand.EHT, param);
        var result = Rt - (Rt1 + Rt2) / 2;
        Measurements.Add(result);

        await protocolUI.ShowMessageAsync(new ShowMessageModel("Итог измерений"));
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Результат сопротивления", message: $"{result:F5} Ом", type: (result >= LowerBound && result <= UpperBound ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 1 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", message: $"от {LowerBound} до {UpperBound} Ом") { IndentLevel = 2 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Погрешность измерения", message: $"{(Math.Abs(result - param)):F5} Ом", type: (result >= LowerBound && result <= UpperBound ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 2 }, skipPause: true);

        await StepReset(protocolUI, metrologicalModeRole, points.Point1, points.Point2, param);
        return true;
      }

      public override async Task FinalizeMeasurement(IUserMessageService messageService)
      {
        await base.FinalizeMeasurement(messageService);
        await PrintResult(messageService, MeasurementTypeCommand.EHT);
      }


      private async Task<double> StepFirst(IUserMessageService userMessageService, MeasurementTypeCommand metrologicalModeRole, PointModel point1, double param)
      {
        await userMessageService.ShowMessageAsync(new ShowMessageModel(header: $"Подлючение точки {point1}"), IsBlockStart: true);

        var relayModule = GetRelayModules(metrologicalModeRole).First();

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => relayModule.PointManager.ConnectRelayAsync(DeviceEnums.BusPoint.A, point1.PointNumber, userMessageService), userMessageService))
          throw AppConfiguration.Error.Device.ModuleRelayControl.RelayExceptionFactory.ConnectPointFailed(point1.PointNumber.ToString(), relayModule.Name, relayModule.NumberChassis, relayModule.Number);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => relayModule.PointManager.ConnectRelayAsync(DeviceEnums.BusPoint.B, point1.PointNumber, userMessageService), userMessageService))
          throw AppConfiguration.Error.Device.ModuleRelayControl.RelayExceptionFactory.ConnectPointFailed(point1.PointNumber.ToString(), relayModule.Name, relayModule.NumberChassis, relayModule.Number);

        var fastMeter = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IFastMeter>().FirstOrDefault() : null;

        await userMessageService.ShowMessageAsync(new ShowMessageModel(header: $"Измерение сопротивления"), IsBlockStart: true);

        var result = !await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? await fastMeter.ResistanceManager.MeasureResistanceAsync() : !await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled() ? param / 2 : new Random().Next((int)param - 100, (int)param + 100);
        return result;
      }


      private async Task<double> StepSecond(IUserMessageService userMessageService, MeasurementTypeCommand metrologicalModeRole, PointModel point1, PointModel point2, double param)
      {
        await userMessageService.ShowMessageAsync(new ShowMessageModel(header: $"Отлючение точки {point1}"), IsBlockStart: true);
        var relayModule = GetRelayModules(metrologicalModeRole).First();

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => relayModule.PointManager.DisconnectRelayAsync(DeviceEnums.BusPoint.B, point1.PointNumber, userMessageService), userMessageService))
          throw AppConfiguration.Error.Device.ModuleRelayControl.RelayExceptionFactory.ConnectPointFailed(point1.PointNumber.ToString(), relayModule.Name, relayModule.NumberChassis, relayModule.Number);

        relayModule = GetRelayModules(metrologicalModeRole).Last();

        await userMessageService.ShowMessageAsync(new ShowMessageModel(header: $"Подлючение точки {point2}"), IsBlockStart: true);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => relayModule.PointManager.ConnectRelayAsync(DeviceEnums.BusPoint.A, point2.PointNumber, userMessageService), userMessageService))
          throw AppConfiguration.Error.Device.ModuleRelayControl.RelayExceptionFactory.ConnectPointFailed(point2.PointNumber.ToString(), relayModule.Name, relayModule.NumberChassis, relayModule.Number);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => relayModule.PointManager.ConnectRelayAsync(DeviceEnums.BusPoint.B, point2.PointNumber, userMessageService), userMessageService))
          throw AppConfiguration.Error.Device.ModuleRelayControl.RelayExceptionFactory.ConnectPointFailed(point2.PointNumber.ToString(), relayModule.Name, relayModule.NumberChassis, relayModule.Number);

        var fastMeter = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IFastMeter>().FirstOrDefault() : null;

        await userMessageService.ShowMessageAsync(new ShowMessageModel(header: $"Измерение сопротивления"), IsBlockStart: true);

        var result = !await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? await fastMeter.ResistanceManager.MeasureResistanceAsync() : !await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled() ? param / 2 : new Random().Next((int)param - 100, (int)param + 100);
        return result;
      }

      private async Task<double> StepThird(IUserMessageService userMessageService, MeasurementTypeCommand metrologicalModeRole, PointModel point1, PointModel point2, double param)
      {
        await userMessageService.ShowMessageAsync(new ShowMessageModel(header: $"Отлючение точки {point2}"), IsBlockStart: true);
        var relayModule = GetRelayModules(metrologicalModeRole).Last();

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => relayModule.PointManager.DisconnectRelayAsync(DeviceEnums.BusPoint.A, point2.PointNumber, userMessageService), userMessageService))
          throw AppConfiguration.Error.Device.ModuleRelayControl.RelayExceptionFactory.ConnectPointFailed(point2.PointNumber.ToString(), relayModule.Name, relayModule.NumberChassis, relayModule.Number);

        var fastMeter = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IFastMeter>().FirstOrDefault() : null;

        await userMessageService.ShowMessageAsync(new ShowMessageModel(header: $"Измерение сопротивления"), IsBlockStart: true);

        var result = !await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? await fastMeter.ResistanceManager.MeasureResistanceAsync() : !await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled() ? param * 1.5 : new Random().Next((int)param - 100, (int)param + 100);
        return result;
      }

      private async Task StepReset(IUserMessageService userMessageService, MeasurementTypeCommand metrologicalModeRole, PointModel point1, PointModel point2, double param)
      {
        await userMessageService.ShowMessageAsync(new ShowMessageModel(header: $"Отлючение точек"), IsBlockStart: true);
        var relayModule = GetRelayModules(metrologicalModeRole).First();

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => relayModule.PointManager.DisconnectRelayAsync(DeviceEnums.BusPoint.A, point1.PointNumber, userMessageService), userMessageService))
          throw AppConfiguration.Error.Device.ModuleRelayControl.RelayExceptionFactory.ConnectPointFailed(point1.PointNumber.ToString(), relayModule.Name, relayModule.NumberChassis, relayModule.Number);

        relayModule = GetRelayModules(metrologicalModeRole).Last();

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => relayModule.PointManager.DisconnectRelayAsync(DeviceEnums.BusPoint.B, point2.PointNumber, userMessageService), userMessageService))
          throw AppConfiguration.Error.Device.ModuleRelayControl.RelayExceptionFactory.ConnectPointFailed(point2.PointNumber.ToString(), relayModule.Name, relayModule.NumberChassis, relayModule.Number);
      }

      public override async Task ConnectRelayPointsAsync(List<IRelaySwitchModule> relayModules, PointModel point1, PointModel point2, ProtocolUI protocolUI)
      {
        return;
      }
    }
  }
}
