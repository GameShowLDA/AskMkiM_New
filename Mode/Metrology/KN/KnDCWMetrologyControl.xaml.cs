using AppConfiguration.Interface;
using DTO.Base.Models;
using DTO.Base.Models.MeasurementError;
using DTO.Device.FastMeter;
using DTO.Service;
using Errors.Device.Multimeter;
using Errors.Models;
using Mode.Base;
using Mode.Metrology.MeasurementSystem;
using Mode.Metrology.PI;
using System.Windows;
using System.Windows.Controls;
using UI.Controls.ProtocolNew;
using Utilities;
using Utilities.Help;
using static DTO.Enum.Measurement;
using static Mode.Base.UIValidationHelper;
namespace Mode.Metrology.KN
{
  /// <summary>
  /// Логика взаимодействия для KnDCWMetrologyControl.xaml
  /// </summary>
  public partial class KnDCWMetrologyControl : UserControl, IExecution
  {
    MeasurementTypeCommand metrologicalModeRole => MeasurementTypeCommand.KN_DCW;

    KnMeasurement testMeasurement = new KnMeasurement();

    DataModel Data;

    public KnDCWMetrologyControl()
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
      var data = await EnsureValidMetrologyInputAsync(ProtocolUI);
      
      await testMeasurement.ConnectToEquipment(data.FirstPoint, data.SecondPoint, metrologicalModeRole, ProtocolUI);
      await testMeasurement.SetupCommutation(ProtocolUI, data.FirstPoint, data.SecondPoint, metrologicalModeRole);
      await testMeasurement.ConfigureMeter(ProtocolUI, metrologicalModeRole);
      await UserActionHelper.RunWithUserRepeatAsync(async () => await testMeasurement.PerformMeasurement(metrologicalModeRole, data.Param, ProtocolUI), ProtocolUI, true);
    }

    public ITextAdapter GetControl()
    {
      return ProtocolUI;
    }

    private class KnMeasurement : BaseMeasurement
    {
      public KnMeasurement() : base() { }

      /// <inheritdoc />
      public override async Task ConfigureMeter(IUserMessageService messageService, MeasurementTypeCommand metrologicalModeRole, DataModel dataModel = null)
      {
        await base.ConfigureMeter(messageService, metrologicalModeRole, dataModel);
        var fastMeter = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IFastMeter>().FirstOrDefault() : null;

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => fastMeter.DcVoltageManager.SetDCVoltageModeAsync(messageService), messageService))
          throw DcExceptionFactory.SetModeFailed(fastMeter.Name, fastMeter.NumberChassis, fastMeter.Number);
      }

      /// <inheritdoc />
      public override async Task<bool> PerformMeasurement(MeasurementTypeCommand metrologicalModeRole, double param, ProtocolUI protocolUI)
      {
        protocolUI.GetCancellationToken().ThrowIfCancellationRequested();
        var fastMeter = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IFastMeter>().FirstOrDefault() : null;

        var resultFastMeterMeasured = await MeasuredFastMeter(fastMeter, protocolUI, param);
        var resultReferenceMeterMeasured = await MeasuredReferenceMeter(fastMeter, protocolUI, param);

        (LowerBound, UpperBound, var delta) = MeasurementErrorDefaults.CalculateToleranceRange(MeasurementTypeCommand.KN_DCW, resultReferenceMeterMeasured);
        await protocolUI.ShowMessageAsync(new ShowMessageModel(header: "Результат проверки"));
        var result = resultFastMeterMeasured >= LowerBound && resultFastMeterMeasured <= UpperBound;

        var err = resultFastMeterMeasured - resultReferenceMeterMeasured;
        Measurements.Add(err);

        await protocolUI.ShowMessageAsync(new ShowMessageModel($"Значение эталоного напряжения ", null, $"{resultReferenceMeterMeasured:F2} В") { IndentLevel = 1 });
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Результат измерения напряжение", message: $"{resultFastMeterMeasured} В", type: (result ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 1 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", message: $"от {LowerBound} до {UpperBound} В") { IndentLevel = 2 }, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("Погрешность измерения", message: $"{err} В", type: (result ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 2 }, skipPause: true);

        return true;
      }

      public override async Task FinalizeMeasurement(IUserMessageService messageService)
      {
        await base.FinalizeMeasurement(messageService);
        await PrintResult(messageService, MeasurementTypeCommand.KN_DCW);
        Measurements.Clear();
      }

      private async Task<double> MeasuredFastMeter(IFastMeter fastMeter, IUserMessageService userMessageService, double param)
      {
        var result = await fastMeter.DcVoltageManager.MeasureDCVoltageAsync(param);
        return result;
      }

      private async Task<double> MeasuredReferenceMeter(IFastMeter fastMeter, ProtocolUI userMessageService, double param)
      {
        var result = await Application.Current.Dispatcher.InvokeAsync(() =>
        {
          VoltageValue chassisManagerWindow = new VoltageValue();
          userMessageService.Effect = new System.Windows.Media.Effects.BlurEffect();

          bool? dialogResult = chassisManagerWindow.ShowDialog();
          userMessageService.Effect = null;

          if (dialogResult == true)
          {
            return chassisManagerWindow.VoltageResult;
          }
          else
          {
            return -1;
          }
        });

        return result;
      }
    }
  }
}
