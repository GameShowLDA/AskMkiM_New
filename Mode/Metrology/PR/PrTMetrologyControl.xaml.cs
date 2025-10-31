using AppConfiguration.Interface;
using DTO.Base.Models;
using DTO.Base.Models.MeasurementError;
using DTO.Device.FastMeter;
using DTO.Device.PowerSourceModule;
using DTO.Service;
using Errors.Device.Multimeter;
using Errors.Models;
using Mode.Base;
using Mode.Metrology.MeasurementSystem;
using NewCore.Base.DeviceResponses;
using System.Text.Json;
using System.Windows.Controls;
using UI.Controls.ProtocolNew;
using Utilities;
using Utilities.Help;
using static DTO.Enum.DeviceEnums;
using static DTO.Enum.Measurement;
using static Mode.Base.UIValidationHelper;
using static Utilities.LoggerUtility;

namespace Mode.Metrology.PR
{
  /// <summary>
  /// Логика взаимодействия для PrMetrologyControl.xaml.
  /// </summary>
  public partial class PrTMetrologyControl : UserControl, IExecution
  {
    MeasurementTypeCommand metrologicalModeRole => MeasurementTypeCommand.PR;

    PrMeasurement testMeasurement = new PrMeasurement();

    DataModel Data;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="PrTMetrologyControl"/>.
    /// </summary>
    public PrTMetrologyControl()
    {
      InitializeComponent();
      InitializeSettings();

      // Регистрируем обработчик движения мыши
      MouseMove += (s, e) =>
      {
        // Обновляем последний элемент под курсором
        HelpProvider.SetHelpKey(this, "UtilityModePR");
      };
    }

    /// <summary>
    /// Инициализирует все необходимые настройки для компонента.
    /// Очищает предыдущий контент и добавляет новые элементы управления.
    /// </summary>
    public void InitializeSettings()
    {
      try
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
      catch (Exception ex)
      {
        var methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
        LogError($"Ошибка загрузки элемента метрологии СИ в методе {methodName}: {ex.Message}");
      }
    }

    /// <summary>
    /// Выполнение контроля.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns></returns>
    private async Task ExecuteMeasurementProcess(CancellationToken cancellationToken)
    {
      var data = await EnsureValidMetrologyInputAsync(ProtocolUI);
      await NewCore.Communication.DeviceCommandSender.ResetAllSystem();

      var connect = await testMeasurement.ConnectToEquipment(data.FirstPoint, data.SecondPoint, metrologicalModeRole, ProtocolUI);
      if (!connect.Connect)
      {
        await ProtocolUI.ShowMessageAsync(new ShowMessageModel("Ошибка", message: connect.Message, type: ShowMessageModel.MessageType.Error), SkipStepModeCheck: true);
        return;
      }

      await testMeasurement.SetupCommutation(ProtocolUI, data.FirstPoint, data.SecondPoint, metrologicalModeRole);
      await testMeasurement.ConfigureMeter(ProtocolUI, metrologicalModeRole, Data);
      await testMeasurement.MintSettings(Data);
      await UserActionHelper.RunWithUserRepeatAsync(async () => await testMeasurement.PerformMeasurement(metrologicalModeRole, data.Param, ProtocolUI), ProtocolUI, true);
    }

    public ITextAdapter GetControl()
    {
      return ProtocolUI;
    }

    private class PrMeasurement : BaseMeasurement
    {
      public PrMeasurement() : base() { }

      /// <summary>
      /// Устанавливает настройки на модуль источника напряжения и тока.
      /// </summary>
      /// <param name="dataModel">Модель введенных данных.</param>
      /// <returns></returns>
      public async Task MintSettings(DataModel dataModel)
      {
        var mint = Devices.TryGetValue(MeasurementTypeCommand.PR, out var meter) ? meter.OfType<IPowerSourceModule>().FirstOrDefault() : null;
        var data = SelectOptimalCurrentAndVoltage(dataModel.Param, mint);

        int integerPart = data.IntegerCurrent;
        int decimalPart = data.DecimalCurrent;

        await mint.VoltageManager.SetSourceVoltageAsync(data.Voltage);
        await mint.CurrentManager.SetCurrentLevelAsync(integerPart, decimalPart);
      }

      /// <inheritdoc />
      public override async Task ConfigureMeter(IUserMessageService messageService, MeasurementTypeCommand metrologicalModeRole, DataModel dataModel = null)
      {
        var fastMeter = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IFastMeter>().FirstOrDefault() : null;

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => fastMeter.DcVoltageManager.SetDCVoltageModeAsync(messageService), messageService))
          throw DcExceptionFactory.SetModeFailed(fastMeter.Name, fastMeter.NumberChassis, fastMeter.Number);
      }

      /// <inheritdoc />
      public override async Task<bool> PerformMeasurement(MeasurementTypeCommand metrologicalModeRole, double param, ProtocolUI protocolUI)
      {
        protocolUI.GetCancellationToken().ThrowIfCancellationRequested();
        var mint = Devices.TryGetValue(MeasurementTypeCommand.PR, out var power) ? power.OfType<IPowerSourceModule>().FirstOrDefault() : null;
        var meterDevice = Devices.TryGetValue(metrologicalModeRole, out var meter) ? meter.OfType<IFastMeter>().FirstOrDefault() : null;

        await protocolUI.ShowMessageAsync(new ShowMessageModel(header: "Выполнение проверки релейной"));

        var data = SelectOptimalCurrentAndVoltage(param, mint);
        double currentGenerial = (data.DecimalCurrent / 1000.0) + data.IntegerCurrent;

        var (firstNorm, lastNorm, delta) = MeasurementErrorDefaults.CalculateToleranceRange(MeasurementTypeCommand.PR, param);

        var voltage = await meterDevice.DcVoltageManager.MeasureDCVoltageAsync(param * (currentGenerial / 1000), protocolUI);
        double fakeCurrent = GetInterpolatedCurrent(param, mint);
        var result = voltage / (fakeCurrent / 1000.0);

        ShowMessageModel showMessageModel = new ShowMessageModel($"\tРезультат измерения сопротивления ({firstNorm:F2}-{lastNorm:F2})", null, $"{result:F2}");
        showMessageModel.Status = (result >= firstNorm && result <= lastNorm) ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error;
        showMessageModel.ExecutionError = (result >= firstNorm && result <= lastNorm) ? false : true;
        showMessageModel.CanBeDeleted = showMessageModel.ExecutionError;
        await protocolUI.ShowMessageAsync(showMessageModel);

        return true;
      }

      /// <summary>
      /// Возвращает диапазон параметров (коэффициенты, ток, напряжение), соответствующий указанному сопротивлению.
      /// </summary>
      /// <param name="resistance">Измеренное сопротивление (в Омах).</param>
      /// <param name="powerSourceModule">Модуль источника питания с диапазонами калибровки.</param>
      /// <returns>
      /// Объект <see cref="ResistanceCalibrationRange"/>, соответствующий диапазону сопротивления.
      /// Если диапазон не найден, возвращается пустой объект со значениями по умолчанию.
      /// </returns>
      private ResistanceCalibrationRange SelectOptimalCurrentAndVoltage(double resistance, IPowerSourceModule powerSourceModule)
      {
        var json = powerSourceModule.ResistanceCalibrationJson;

        var list = string.IsNullOrWhiteSpace(json)
          ? new List<ResistanceCalibrationRange>()
          : JsonSerializer.Deserialize<List<ResistanceCalibrationRange>>(json) ?? new List<ResistanceCalibrationRange>();

        var matched = list.FirstOrDefault(r =>
          resistance >= r.ResistanceMin && resistance <= r.ResistanceMax);

        return matched ?? new ResistanceCalibrationRange
        {
          ResistanceMin = 0,
          ResistanceMax = 0,

          IntegerCurrent = 0,
          DecimalCurrent = 0,

          DecimalCurrentFake = 0,
          IntegerCurrentFake = 0,

          Voltage = VoltageSources.Supply12V,
        };
      }

      public static double GetInterpolatedCurrent(double resistance, IPowerSourceModule module)
      {
        if (string.IsNullOrWhiteSpace(module.ResistanceCalibrationJson))
        {
          throw new InvalidOperationException("Calibration JSON пуст или отсутствует.");
        }

        var ranges = JsonSerializer.Deserialize<List<ResistanceCalibrationRange>>(module.ResistanceCalibrationJson);
        if (ranges == null || !ranges.Any())
        {
          throw new InvalidOperationException("Не удалось десериализовать калибровочные диапазоны.");
        }

        var range = ranges.FirstOrDefault(r => resistance >= r.ResistanceMin && resistance <= r.ResistanceMax);
        if (range == null)
        {
          throw new ArgumentOutOfRangeException(nameof(resistance),
              $"Сопротивление {resistance} Ом не входит ни в один из диапазонов.");
        }

        double percent = (resistance - range.ResistanceMin) / (range.ResistanceMax - range.ResistanceMin);
        double real = range.IntegerCurrent + range.DecimalCurrent / 1000.0;
        double fake = range.IntegerCurrentFake + range.DecimalCurrentFake / 1000.0;

        return real + (fake - real) * percent;
      }
    }
  }
}