using AppConfiguration;
using DataBaseConfiguration.Services.Device;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewCore.Base.Interface.Main;
using NewCore.Device;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using static Utilities.LoggerUtility;

namespace MainWindowProgram
{
  public partial class App : Application
  {
    public static IHost AppHost { get; private set; }

    [Flags]
    public enum EXECUTION_STATE : uint
    {
      ES_CONTINUOUS = 0x80000000,
      ES_DISPLAY_REQUIRED = 0x00000002,
      ES_SYSTEM_REQUIRED = 0x00000001,
    }

    [DllImport("kernel32.dll")]
    public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

    private Thread? _splashThread;
    private readonly ManualResetEventSlim _splashShown = new(false);
    private SplashWindow? _splash;

    public static string[] CommandLineArgs { get; private set; } = Array.Empty<string>();

    protected override async void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      CommandLineArgs = e?.Args ?? Array.Empty<string>();

      StartSplashOnDedicatedSta();
      _splashShown.Wait();

      try
      {
        var mainWindow = new MainWindow { Visibility = Visibility.Hidden };

        await mainWindow.InitializeAsync();

        AppHost = Host.CreateDefaultBuilder()
          .ConfigureServices(svc =>
          {
            svc.AddSingleton<IBreakdownTester, GPT79904>();
            svc.AddSingleton<BreakdownTesterServices>();
          })
          .Build();

        ServiceLocator.Initialize(AppHost);

        var chassisNumber = new ChassisManagerServices().GetAll().FirstOrDefault();
        var tester = ServiceLocator
          .GetRequired<BreakdownTesterServices>()
          .GetDevicesByNumberChassis(chassisNumber.Number)
          .FirstOrDefault();

        if (_splash is not null)
          await _splash.WaitForCloseAsync();

        _splashThread?.Join();

        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED);

        mainWindow.Visibility = Visibility.Visible;
        mainWindow.Closed += (_, __) => SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);

        Current.MainWindow = mainWindow;
      }
      catch (Exception ex)
      {
        try
        {
          if (_splash is not null)
            await _splash.WaitForCloseAsync();
          _splashThread?.Join();
        }
        catch { }

        LogException(ex, "Произошла ошибка запуска приложения.");
        Message.MessageBoxCustom.Show(
          "Произошла ошибка запуска приложения. Сообщите о данной ошибке вашему администратору или повторите попытку.",
          "FATAL ERROR", MessageBoxButton.OK, MessageBoxImage.Error);

        Current.Shutdown();
      }
    }

    private void StartSplashOnDedicatedSta()
    {
      _splashThread = new Thread(() =>
      {
        var dispatcher = Dispatcher.CurrentDispatcher;
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));

        _splash = new SplashWindow();
        _splash.Closed += (_, __) => dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
        _splash.SourceInitialized += (_, __) => _splashShown.Set();

        _splash.Show();
        Dispatcher.Run();
      })
      {
        IsBackground = true
      };

      _splashThread.SetApartmentState(ApartmentState.STA);
      _splashThread.Start();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
      base.OnExit(e);

      AppConfiguration.Base.EventAggregator.RaiseSaveSession();

      try
      {
        var svc = ServiceLocator.GetRequired<IBreakdownTester>();
        LogInformation($"OnExit: IBreakdownTester instance = {svc.GetHashCode()} | {svc.GetType().FullName}");
        svc?.ConnectableManager?.DisconnectAsync().GetAwaiter().GetResult();
      }
      catch { }

      GC.Collect();
      GC.WaitForPendingFinalizers();

      // TODO : Раскомментировать, когда будет готово
      // await Core.Communication.CommunicationManager.ResetAllSystem();
      // await Task.Delay(1000);
      // await Core.ManagerShassy.Function.StopPowerAsync(ConfigCollector.GetManagerShassyIp());
    }
  }
}