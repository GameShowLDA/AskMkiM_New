using AppConfiguration.Interface;
using DTO.Base.Models;
using DTO.Base.Models.MeasurementError;
using DTO.Device.FastMeter;
using DTO.Service;
using Errors.Device.Multimeter;
using Mode.Base;
using Mode.Metrology.MeasurementSystem;
using Mode.Metrology.PI;
using System.Windows;
using System.Windows.Controls;
using UI.Controls.ProtocolNew;
using Utilities;
using Utilities.Help;
using static DTO.Enum.Measurement;
namespace Mode.Metrology.KN
{
  /// <summary>
  /// Логика взаимодействия для KnDCWMetrologyControl.xaml
  /// </summary>
  public partial class KnDCWMetrologyControl : UserControl, IExecution
  {
    MeasurementTypeCommand metrologicalModeRole => MeasurementTypeCommand.KN_DCW;

    KnMeasurement testMeasurement = new KnMeasurement();

    (bool Success, string Message, DataModel DataModel) Data;

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
        await protocolUI.ShowMessageAsync(new ShowMessageModel(header: "Выполнение измерения напряжения(DCW)"));

        (LowerBound, UpperBound, var delta) = MeasurementErrorDefaults.CalculateToleranceRange(MeasurementTypeCommand.KN_DCW, param);
        await fastMeter.DcVoltageManager.MeasureDCVoltageAsync(param, protocolUI);

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



        var answer = (result >= LowerBound && result <= UpperBound) ? false : true;

        ShowMessageModel showMessageModel = new ShowMessageModel($"\tРезультат измерения напряжения", null, $"{result:F2}");
        showMessageModel.Status = (!answer ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error);
        showMessageModel.ExecutionError = (result >= LowerBound && result <= UpperBound) ? false : true;
        showMessageModel.CanBeDeleted = showMessageModel.ExecutionError;
        await protocolUI.ShowMessageAsync(showMessageModel, skipPause: true);
        await protocolUI.ShowMessageAsync(new ShowMessageModel("\tПогрешность измерения", message: $"{delta}В", type: showMessageModel.Status), skipPause: true);

        return true;
      }

      public override async Task FinalizeMeasurement(IUserMessageService messageService)
      {
        await base.FinalizeMeasurement(messageService);
        await PrintResult(messageService, MeasurementTypeCommand.KN_DCW);
      }
    }
  }
}
