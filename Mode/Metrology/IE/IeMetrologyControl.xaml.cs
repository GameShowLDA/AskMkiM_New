using System.Windows.Controls;
using AppConfiguration.Error.Device.Multimeter;
using AppConfiguration.Interface;
using AppConfiguration.MeasurementError;
using DTO.Base.Models;
using DTO.Base.Models.MeasurementError;
using DTO.Device.FastMeter;
using DTO.Service;
using Mode.Base;
using Mode.Metrology.MeasurementSystem;
using UI.Controls.ProtocolNew;
using Utilities;
using Utilities.Help;
using static DTO.Enum.Metrology;
using static NewCore.Enum.MetrologyEnum;

namespace Mode.Metrology.IE
{
  /// <summary>
  /// Логика взаимодействия для IeMetrologyControl.xaml.
  /// </summary>
  public partial class IeMetrologyControl : UserControl, IExecution
  {
    MetrologicalModeRole metrologicalModeRole => MetrologicalModeRole.IE;

    IeMeasurement testMeasurement = new IeMeasurement();

    (bool Success, string Message, DataModel DataModel) Data;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="IeMetrologyControl"/>.
    /// </summary>
    public IeMetrologyControl()
    {
      InitializeComponent();
      InitializeSettingsAsync().ConfigureAwait(true);

      // Регистрируем обработчик движения мыши
      MouseMove += (s, e) =>
      {
        // Обновляем последний элемент под курсором
        HelpProvider.SetHelpKey(this, "UtilityModeIE");
      };
    }

    /// <summary>
    /// Инициализирует все необходимые настройки для компонента.
    /// Очищает предыдущий контент и добавляет новые элементы управления.
    /// </summary>
    public async Task InitializeSettingsAsync()
    {

      ProtocolUI.SetSettings(
        this,
        StartDelegate: ExecuteMeasurementProcess,
        true);
    }

    /// <summary>
    /// Выполнение контроля.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ExecuteMeasurementProcess(CancellationToken cancellationToken)
    {
      Data = UIValidationHelper.TryValidateAndParseInputWithEquipment(ProtocolUI, timeCheck: true, voltageCheck: true);
      if (!Data.Success)
      {
        await ProtocolUI.ShowMessageAsync(new ShowMessageModel("Ошибка", message: Data.Message, type: ShowMessageModel.MessageType.Error), SkipStepModeCheck: true);
        return;
      }

      var first = Data.DataModel.FirstPoint;
      var second = Data.DataModel.SecondPoint;
      var param = Data.DataModel.Param;

      var connect = await testMeasurement.ConnectToEquipment(first, second, metrologicalModeRole, ProtocolUI);
      if (!connect.Connect)
      {
        await ProtocolUI.ShowMessageAsync(new ShowMessageModel("Ошибка", message: connect.Message, type: ShowMessageModel.MessageType.Error), SkipStepModeCheck: true);
        return;
      }

      await testMeasurement.SetupCommutation(ProtocolUI, first, second, metrologicalModeRole);
      await testMeasurement.ConfigureMeter(ProtocolUI, metrologicalModeRole);

      await UserActionHelper.RunWithUserRepeatAsync(async () => await testMeasurement.PerformMeasurement(metrologicalModeRole, param, ProtocolUI), ProtocolUI, true);

      await testMeasurement.FinalizeMeasurement(ProtocolUI);
    }

    public ITextAdapter GetControl()
    {
      return ProtocolUI;
    }

    private class IeMeasurement : BaseMeasurement
    {
      public IeMeasurement() : base() { }

      /// <inheritdoc />
      public override async Task ConfigureMeter(IUserMessageService messageService, MetrologicalModeRole metrologicalModeRole, DataModel dataModel = null)
      {
        await base.ConfigureMeter(messageService, metrologicalModeRole, dataModel);
        var fastMeter = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IFastMeter>().FirstOrDefault() : null;

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => fastMeter.CapacitanceManager.SetCapacitanceModeAsync(messageService), messageService))
          throw CapacitanceExceptionFactory.SetModeFailed(fastMeter.Name, fastMeter.NumberChassis, fastMeter.Number);
      }

      /// <inheritdoc />
      public override async Task<bool> PerformMeasurement(MetrologicalModeRole metrologicalModeRole, double param, ProtocolUI protocolUI)
      {
        var fastMeter = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IFastMeter>().FirstOrDefault() : null;
        await protocolUI.ShowMessageAsync(new ShowMessageModel(header: "Выполнение измерения ёмкости"));
        (LowerBound, UpperBound, var delta) = MeasurementErrorDefaults.CalculateToleranceRange(MetrologyTypeCommand.IE, param);

        var result = !await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? await fastMeter.CapacitanceManager.MeasureCapacitanceAsync(param, protocolUI) : !await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled() ? param : new Random().Next((int)LowerBound - 100, (int)UpperBound + 100);
        Measurements.Add(result);

        await protocolUI.ShowMessageAsync(new ShowMessageModel("Результат измерения ёмкости", message: $"{result} нФ", type: (result >= LowerBound && result <= UpperBound ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 1 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", message: $"от {LowerBound} до {UpperBound} нФ") { IndentLevel = 2 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Погрешность измерения", message: $"{(Math.Abs(result - param))} нФ", type: (result >= LowerBound && result <= UpperBound ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 2 }, skipPause: true);
        return true;
      }

      public override async Task FinalizeMeasurement(IUserMessageService messageService)
      {
        await base.FinalizeMeasurement(messageService);
        await PrintResult(messageService, MetrologyTypeCommand.IE);
      }
    }
  }
}
