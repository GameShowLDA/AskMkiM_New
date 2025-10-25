using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MainWindowProgram.Init
{
  /// <summary>
  /// Отвечает за обеспечение единственного экземпляра приложения.
  /// </summary>
  internal static class SingleInstanceManager
  {
    private const string MutexName = "Global\\AxionHolding.ASK-MKI-M";

    /// <summary>
    /// Проверяет, запущен ли уже экземпляр приложения, и предотвращает запуск нескольких копий.
    /// </summary>
    public static void EnsureSingleInstance()
    {
      bool isNewInstance;
      using var mutex = new Mutex(true, MutexName, out isNewInstance);

      if (!isNewInstance)
      {
        MessageBox.Show(
          "Вы не можете запускать несколько экземпляров для АСК-МКИ-М.",
          "ВНИМАНИЕ!",
          MessageBoxButton.OK,
          MessageBoxImage.Information);

        LogWarning("Попытка запустить несколько экземпляров.");
        Application.Current.Shutdown();
      }
    }

    private static void LogWarning(string message)
    {
      // Здесь можно вызвать твой LoggerUtility.LogWarning(message);
      System.Diagnostics.Debug.WriteLine(message);
    }
  }
}
