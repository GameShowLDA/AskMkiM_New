using System.Windows;
using System.Windows.Controls;
using DTO.Base.Models;
using static AppConfiguration.AdminConfig;
using static AppConfiguration.SystemStateManager;
using static Utilities.LoggerUtility;


namespace AppConfiguration.Base
{
  /// <summary>
  /// Статический класс, предоставляющий централизованный механизм управления событиями для взаимодействия компонентов приложения.
  /// Используется для оповещения об изменении состояния питания, блокировки, прав администратора, а также для вывода сообщений различного уровня (ошибки, предупреждения, информация).
  /// </summary>
  public static class EventAggregator
  {
    /// <summary>
    /// Событие, которое вызывается при изменении состояния пошагового.
    /// </summary>
    static public event Action<bool> StepByStepModeChanged;




   
    /// <summary>
    /// Событие для запроса показа окна прогресса с блюром на главном окне
    /// </summary>
    public static event Action RequestShowProgress;

    /// <summary>
    /// Событие для запроса закрытия окна прогресса и снятия блюра с главного окна
    /// </summary>
    public static event Action RequestCloseProgress;

    /// <summary>
    /// Событие, которое вызывается для открытия нового Opk-файла.
    /// </summary>
    static public event Action<UserControl, string> OpenOpk;

    /// <summary>
    /// Событие, которое вызывается при нажатии на кнопку "Сравнить".
    /// </summary>
    public static event Action<string, string> CompareFiles;

    /// <summary>
    /// Событие, которое вызывается при нажатии на кнопку возврата к редактирванию файла в текстовом редакторе.
    /// </summary>
    public static event Action<string> OpenFileInEditorAgain;

    /// <summary>
    /// Событие, которое вызывается при нажатии на кнопку возврата к редактирванию файла в текстовом редакторе.
    /// </summary>
    public static event Action<ProtocolModel> ViewProtocol;

    public static event Action SaveSession;
    public static event Action OpenSession;


    public static event Action<ProtocolModel> GetProtocolInfo;

    public static event Action<string, string, string, string, ProtocolModel> ProtocolInfoClose;

    /// <summary>
    /// Событие, которое вызывается при изменении статуса прав администратора.
    /// </summary>
    static internal bool StepByStepModeFlag
    {
      get => Execution.ExecutionConfig.GetIsStepByStepModeEnabled().Result;
      set
      {
        StepByStepModeChanged?.Invoke(value);
      }
    }

    static public void RaiseSaveSession()
    {
      SaveSession?.Invoke();
    }


    static public void RaiseOpenSession()
    {
      OpenSession?.Invoke();
    }

    /// <summary>
    /// Возвращает текущий статус прав администратора.
    /// </summary>
    /// <returns>true, если запущено с правами администратора; false в противном случае.</returns>
    static public bool GetAdminRights()
    {
      bool result = false;
      Application.Current.Dispatcher.Invoke(() => result = IsAdmin);
      return result;
    }

    public static void RaiseRequestShowProgress()
    {
      RequestShowProgress?.Invoke();
    }

    public static void RaiseRequestCloseProgress()
    {
      RequestCloseProgress?.Invoke();
    }

    /// <summary>
    /// Метод для вызова события добавления нового элемента.
    /// </summary>
    /// <param name="elementName">Имя нового элемента.</param>
    static public void RaiseOpenOpk(UserControl userControl, string elementName)
    {
      LogDebug($"Происходит вызов события для добавления нового элемента \"{elementName}\".");
      OpenOpk?.Invoke(userControl, elementName);
    }

    /// <summary>
    /// Метод для вызова события сравнения файлов.
    /// </summary>
    /// <param name="elementName">Имя нового элемента.</param>
    public static void RaiseCompareFiles(string firstFilePath, string secondFilePath)
    {
      CompareFiles?.Invoke(firstFilePath, secondFilePath);
    }

    /// <summary>
    /// Метод для вызова события при нажатии на кнопку возврата к редактирванию файла в текстовом редакторе.
    /// </summary>
    /// <param name="filePath">Путь к файлу.</param>
    public static void RaiseOpenFileInEditorAgain(string filePath)
    {
      OpenFileInEditorAgain?.Invoke(filePath);
    }

    /// <summary>
    /// Метод для вызова события для просмотра файла протокола в новом текстовом редакторе.
    /// </summary>
    /// <param name="filePath">Путь к файлу.</param>
    public static void RaiseViewProtocol(ProtocolModel protocol)
    {
      ViewProtocol?.Invoke(protocol);
    }
    public static void RaiseGetProtocolInfo(ProtocolModel protocolModel)
    {
      GetProtocolInfo?.Invoke(protocolModel);
    }

    public static void RaiseProtocolInfoClose(string number, string executor, string agent, string customer, ProtocolModel protocolModel)
    {
      ProtocolInfoClose?.Invoke(number, executor, agent, customer, protocolModel);
    }
  }
}
