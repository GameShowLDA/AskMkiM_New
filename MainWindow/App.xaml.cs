using AppConfiguration;
using AppConfiguration.Parameter;
using AppConfiguration.Protocol;
using ConsoleUI.ConsoleLogic;
using DataBaseConfiguration.Services.Device;
using DTO.Device.Breakdown;
using EventCore.Adapters;
using MainWindowProgram.Init;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewCore.Device;
using System.Runtime.InteropServices;
using System.Windows;
using UI.Theme;
using static Utilities.LoggerUtility;

namespace MainWindowProgram
{
  /// <summary>
  /// Interaction logic for App.xaml.
  /// Класс приложения, отвечающий за запуск и обработку необработанных исключений.
  /// </summary>
  public partial class App : Application
  {
    [DllImport("kernel32.dll")] private static extern IntPtr GetConsoleWindow();
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [Flags]
    public enum EXECUTION_STATE : uint
    {
      ES_CONTINUOUS = 0x80000000,
      ES_DISPLAY_REQUIRED = 0x00000002,
      ES_SYSTEM_REQUIRED = 0x00000001,
    }

    [DllImport("kernel32.dll")]
    public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

    /// <summary>
    /// Содержит аргументы командной строки, переданные при запуске приложения.
    /// </summary>
    public static string[] CommandLineArgs { get; private set; }

    /// <summary>
    /// Запускает приложение.
    /// </summary>
    /// <param name="e"></param>
    protected override async void OnStartup(StartupEventArgs e)
    {
      SplashScreenManager.ShowSplash();

      await Task.Run(async () =>
      {
        await PreStartupInitializer.Initialize();
        await InitializeTheme();
      });

      base.OnStartup(e);

      CommandLineArgs = e.Args;
      Console.SetOut(new ConsoleRedirector());

      try
      {
        var mainWindow = new MainWindow
        {
          Visibility = Visibility.Hidden
        };

        await mainWindow.InitializeAsync();

        await SplashScreenManager.CloseSplashAsync();


        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
        mainWindow.Visibility = Visibility.Visible;


        Application.Current.MainWindow = mainWindow;

        // гарантируем, что окно окажется поверх всех
        mainWindow.Topmost = true;          // временно делаем поверх всех
        mainWindow.Activate();              // активируем фокус
        mainWindow.Focus();                 // переносим фокус внутрь

        // отслеживаем закрытие
        mainWindow.Closed += (s, _) =>
        {
          SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        };
      }
      catch (Exception ex)
      {
        LogException(ex, "Произошла ошибка запуска приложения.");
        Message.MessageBoxCustom.Show("Произошла ошибка запуска приложения. Сообщите о данной ошибке вашему администратору или повторите попытку.", "FATAL ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        Application.Current.Shutdown();
      }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
      base.OnExit(e);

      SessionEventAdapter.RaiseSaveSession();

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

    private async Task InitializeTheme()
    {

      var parameterTask = ParameterSettingsManager.ReadParameterModeAsync();
      await Task.WhenAll(parameterTask);

      ThemeManager.Initialize();
      await LanguageSettings.InitializeAsync();
      await ThemeSettings.InitializeAsync();
    }
  }
}