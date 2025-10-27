using System.Windows;
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
using Mode.Metrology.PI;
using UI.Controls.ProtocolNew;
using Utilities;
using Utilities.Help;
using static DTO.Enum.Metrology;
using static NewCore.Enum.MetrologyEnum;

namespace Mode.Metrology.KN
{
  /// <summary>
  /// Логика взаимодействия для KnACWMetrologyControl.xaml
  /// </summary>
  public partial class KnACWMetrologyControl : UserControl, IExecution
  {
    MetrologicalModeRole metrologicalModeRole => MetrologicalModeRole.KN;

    KnMeasurement testMeasurement = new KnMeasurement();

    (bool Success, string Message, DataModel DataModel) Data;
    public KnACWMetrologyControl()
    {
      InitializeComponent();
      InitializeSettings();

      // Регистрируем обработчик движения мыши
      MouseMove += (s, e) =>
      {
        // Обновляем последний элемент под курсором
        HelpProvider.SetHelpKey(this, "UtilityModeKN");
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
    }

    public ITextAdapter GetControl()
    {
      return ProtocolUI;
    }

    private class KnMeasurement : BaseMeasurement
    {
      public KnMeasurement() : base() { }

      /// <inheritdoc />
      public override async Task ConfigureMeter(IUserMessageService messageService, MetrologicalModeRole metrologicalModeRole, DataModel dataModel = null)
      {
        await base.ConfigureMeter(messageService, metrologicalModeRole, dataModel);
        var fastMeter = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IFastMeter>().FirstOrDefault() : null;

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => fastMeter.AcVoltageManager.SetACVoltageModeAsync(messageService), messageService))
          throw AcExceptionFactory.SetModeFailed(fastMeter.Name, fastMeter.NumberChassis, fastMeter.Number);
      }

      /// <inheritdoc />
      public override async Task<bool> PerformMeasurement(MetrologicalModeRole metrologicalModeRole, double param, ProtocolUI protocolUI)
      {
        var fastMeter = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IFastMeter>().FirstOrDefault() : null;
        await protocolUI.ShowMessageAsync(new ShowMessageModel(header: "Выполнение измерения напряжения(ACW)"));

        (LowerBound, UpperBound, var delta) = MeasurementErrorDefaults.CalculateToleranceRange(MetrologyTypeCommand.KN_ACW, param);
        await fastMeter.AcVoltageManager.MeasureACVoltageAsync(param, userMessageService: protocolUI);

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
        Measurements.Add(result);

        await protocolUI.ShowMessageAsync(new ShowMessageModel($"\tДиапазон допускаемых значений", message: $"{LowerBound:F2}-{UpperBound:F2}"), skipPause: true);

        var answer = (result >= LowerBound && result <= UpperBound) ? false : true;

        ShowMessageModel showMessageModel = new ShowMessageModel($"\tРезультат измерения напряжения", null, $"{result:F2}");
        showMessageModel.Status = (result >= LowerBound && result <= UpperBound) ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error;
        showMessageModel.ExecutionError = (result >= LowerBound && result <= UpperBound) ? false : true;
        showMessageModel.CanBeDeleted = showMessageModel.ExecutionError;
        await protocolUI.ShowMessageAsync(showMessageModel, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("\tПогрешность измерения", message: $"{delta}В", type: showMessageModel.Status), skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления изоляции", message: $"{result} Ом", type: (result >= LowerBound && result <= UpperBound ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 1 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", message: $"от {LowerBound} до {UpperBound} Ом") { IndentLevel = 2 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Погрешность измерения", message: $"{(Math.Abs(result - param))} Ом", type: (result >= LowerBound && result <= UpperBound ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 2 }, skipPause: true);

        return true;
      }

      public override async Task FinalizeMeasurement(IUserMessageService messageService)
      {
        await base.FinalizeMeasurement(messageService);
        await PrintResult(messageService, MetrologyTypeCommand.CI);
      }
    }

  }
}
