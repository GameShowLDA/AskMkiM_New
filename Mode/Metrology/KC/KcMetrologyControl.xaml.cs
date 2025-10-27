using AppConfiguration.Error.Device.Multimeter;
using AppConfiguration.Interface;
using AppConfiguration.MeasurementError;
using CommonWin32;
using DTO.Base.Models;
using DTO.Base.Models.MeasurementError;
using DTO.Device.FastMeter;
using DTO.Service;
using Mode.Base;
using Mode.Metrology.MeasurementSystem;
using System.Windows.Controls;
using UI.Controls.ProtocolNew;
using Utilities;
using Utilities.Help;
using static DTO.Enum.Metrology;
using static NewCore.Enum.MetrologyEnum;

namespace Mode.Metrology.KC
{
  /// <summary>
  /// Логика взаимодействия для KcMetrologyControl.xaml.
  /// </summary>
  public partial class KcMetrologyControl : UserControl, IExecution
  {
    MetrologicalModeRole metrologicalModeRole => MetrologicalModeRole.KC;

    KcMeasurement testMeasurement = new KcMeasurement();

    (bool Success, string Message, DataModel DataModel) Data;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="KcMetrologyControl"/>.
    /// </summary>
    public KcMetrologyControl()
    {
      InitializeComponent();
      InitializeSettings();

      // Регистрируем обработчик движения мыши
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

    private class KcMeasurement : BaseMeasurement
    {

      public KcMeasurement() : base() { }

      /// <inheritdoc />
      public override async Task ConfigureMeter(IUserMessageService messageService, MetrologicalModeRole metrologicalModeRole, DataModel dataModel = null)
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
      public override async Task<bool> PerformMeasurement(MetrologicalModeRole metrologicalModeRole, double param, ProtocolUI protocolUI)
      {
        var fastMeter = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IFastMeter>().FirstOrDefault() : null;

        await protocolUI.ShowMessageAsync(new ShowMessageModel(header: "Выполнение измерения сопротивления"), IsBlockStart: true);
        var (firstNorm, lastNorm, delta) = MeasurementErrorDefaults.CalculateToleranceRange(MetrologyTypeCommand.KC, param);
        this.LowerBound = firstNorm;
        this.UpperBound = lastNorm;

        var result = !await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? await fastMeter.ResistanceManager.MeasureResistanceAsync(param, firstNorm, lastNorm) : !await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled() ? param : new Random().Next((int)firstNorm - 100, (int)lastNorm + 100);
        Measurements.Add(result);

        await protocolUI.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления", message: $"{result} Ом", type: (result >= firstNorm && result <= lastNorm ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 1 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", message: $"от {firstNorm} до {lastNorm} Ом") { IndentLevel = 2 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Погрешность измерения", message: $"{(Math.Abs(result - param))} Ом", type: (result >= firstNorm && result <= lastNorm ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 2 }, skipPause: true);
        return true;
      }

      public override async Task FinalizeMeasurement(IUserMessageService messageService)
      {
        await base.FinalizeMeasurement(messageService);
        await PrintResult(messageService, MetrologyTypeCommand.KC);
      }
    }
  }
}
