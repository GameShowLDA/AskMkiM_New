using AppConfiguration;
using DataBaseConfiguration.Services.Device;
using DTO.Device.Breakdown;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewCore.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainWindowProgram.Init
{
  /// <summary>
  /// Выполняет предварительную инициализацию приложения перед запуском основного окна.
  /// </summary>
  /// <remarks>
  /// Класс <see cref="PreStartupInitializer"/> отвечает за выполнение всех необходимых действий,
  /// которые должны быть завершены до отображения главного окна приложения.
  /// 
  /// В частности, при вызове метода <see cref="Initialize"/> выполняются следующие шаги:
  /// <list type="number">
  /// <item>
  /// <description>
  /// Проверка наличия уже запущенного экземпляра приложения с помощью 
  /// <see cref="SingleInstanceManager.EnsureSingleInstance"/>.  
  /// Если экземпляр уже существует, новый запуск будет предотвращён.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Инициализация и проверка базы данных с помощью 
  /// <see cref="DatabaseInitializer.InitializeAsync"/> — включая подготовку подключения,
  /// создание структуры при необходимости и загрузку исходных данных.
  /// </description>
  /// </item>
  /// </list>
  /// 
  /// После успешного выполнения всех этапов приложение готово к запуску основной логики
  /// и отображению пользовательского интерфейса.
  /// </remarks>
  internal class PreStartupInitializer
  {
    public static IHost AppHost { get; private set; }


    /// <summary>
    /// Запускает процедуру предварительной инициализации приложения.
    /// </summary>
    /// <remarks>
    /// Выполняет проверку единственного экземпляра приложения и инициализацию базы данных.
    /// Этот метод должен вызываться один раз — до загрузки основного окна.
    /// </remarks>
    static internal async Task Initialize()
    {
      SingleInstanceManager.EnsureSingleInstance();
      await DatabaseInitializer.InitializeAsync();

      AppHost = Host.CreateDefaultBuilder()
        .ConfigureServices(svc =>
        {
          svc.AddSingleton<IBreakdownTester, GPT79904>();
          svc.AddSingleton<BreakdownTesterServices>();
        }).Build();

      ServiceLocator.Initialize(AppHost);
      var chassisNumber = new ChassisManagerServices().GetAll().FirstOrDefault();
      if (chassisNumber != null)
      {
        var tester = ServiceLocator.GetRequired<BreakdownTesterServices>().GetDevicesByNumberChassis(chassisNumber.Number).FirstOrDefault();
      }
    }
  }
}
