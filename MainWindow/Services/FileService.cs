using System.Windows;
using AppConfiguration.Base;
using AppConfiguration.Protocol;
using DataBaseConfiguration.Models.Session;
using DTO.Base.Models;
using EventCore.Adapters;
using EventCore.Events;
using Microsoft.Win32;
using UI.Components.FileComparerControls;
using UI.Controls.ProtocolNew;
using UI.Controls.Search;
using UI.Controls.TextEditor;


namespace MainWindowProgram.Services
{
  /// <summary>
  /// Реализация сервиса для работы с файлами.
  /// Содержит команды и методы для открытия, создания, сохранения, печати, поиска и других операций с файлами.
  /// </summary>
  public class FileService
  {
    /// <summary>
    /// Сервис управления многооконным интерфейсом.
    /// </summary>
    private readonly MultiWindowService _multiWindow;

    private readonly MainWindow _mainWindow;

    /// <summary>
    /// Делегат, предоставляющий актуальное значение состояния блокировки приложения.
    /// </summary>
    private readonly Func<bool> _isLockedProvider;

    private bool _isSearchWindowOpen;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="FileService"/>.
    /// </summary>
    /// <param name="multiWindow">Сервис для работы с окнами редакторов.</param>
    /// <param name="isLockedProvider">Функция, возвращающая признак блокировки интерфейса.</param>
    public FileService(MainWindow mainWindow, MultiWindowService multiWindow, Func<bool> isLockedProvider)
    {
      _multiWindow = multiWindow;
      _mainWindow = mainWindow;
      _mainWindow.SearchWindow = new SearchWindow();
      _isLockedProvider = isLockedProvider;
      
      EventCore.Services.EventAggregator.Subscribe<SearchEvents.SearchWindowClosing>(e => OnSearchWindowClosing(e.IsClosing));

      EventCore.Services.EventAggregator.Unsubscribe<FileInteractionEvents.ViewProtocol>(e => ViewProtocol(e.Protocol));
      EventCore.Services.EventAggregator.Subscribe<FileInteractionEvents.ViewProtocol>(e => ViewProtocol(e.Protocol));

      EventCore.Services.EventAggregator.Subscribe<SessionEvents.SaveSession>(e => SaveSession());
      EventCore.Services.EventAggregator.Subscribe<SessionEvents.OpenSession>(e => SaveSession());

      EventCore.Services.EventAggregator.Unsubscribe<FileInteractionEvents.GetProtocolInfo>(e => OnGetProtocolInfo(e.Protocol));
      EventCore.Services.EventAggregator.Subscribe<FileInteractionEvents.GetProtocolInfo>(e => OnGetProtocolInfo(e.Protocol));
    }

    private void OnGetProtocolInfo(ProtocolModel protocolModel)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        ProtocolInfoWindow protocolInfoWindow = new ProtocolInfoWindow(protocolModel);
        Application.Current.MainWindow.Effect = new System.Windows.Media.Effects.BlurEffect();
        bool? dialogResult = protocolInfoWindow.ShowDialog();
        Application.Current.MainWindow.Effect = null;
      });
    }

    private void OnSearchWindowClosing(bool closing)
    {
      _isSearchWindowOpen = false;
      MessageEventAdapter.RaiseInfoMessage(string.Empty);
    }

    /// <summary>
    /// Открывает диалог выбора файла и загружает его в редактор.
    /// </summary>
    public async Task OpenFileAsync()
    {
      if (_isLockedProvider())
      {
        Message.MessageBoxCustom.Show("В данный момент идёт работа с аппаратурой! Пожалуйста завершите выполнение!", "Ошибка!", MessageBoxButton.OK);
      }
      else
      {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
          Filter = "PK files (*.pk, *.Pk, *.PK)|*.pk; *.Pk; *.PK|ACS files (*.ACS, *.Acs, *.acs)|*.ACS; *.Acs; *.ACs; *.AcS; *.aCS; *.acs|Text files (*.txt)|*.txt|All files (*.*)|*.*",
          Title = "Выберите файл",
        };

        if (openFileDialog.ShowDialog() == true)
        {
          string filePath = openFileDialog.FileName;
          await _multiWindow.OpenFileInEditor(filePath);
        }
      }
    }

    public async void ViewProtocol(ProtocolModel protocol)
    {
      if (_isLockedProvider())
      {
        Message.MessageBoxCustom.Show("В данный момент идёт работа с аппаратурой! Пожалуйста завершите выполнение!", "Ошибка!", MessageBoxButton.OK);
      }
      else
      {
        await _multiWindow.ViewProtocol(protocol, await ProtocolConfig.GetShowProtocolInSoftware());
      }
    }

    /// <summary>
    /// Открывает диалог выбора файла и загружает его в редактор.
    /// </summary>
    public async Task OpenFileAsync(string filePath)
    {
      if (_isLockedProvider())
      {
        Message.MessageBoxCustom.Show("В данный момент идёт работа с аппаратурой! Пожалуйста завершите выполнение!", "Ошибка!", MessageBoxButton.OK);
      }
      else
      {
        await _multiWindow.OpenFileInEditor(filePath);
      }
    }

    /// <summary>
    /// Сохраняет активную сессию файлов.
    /// </summary>
    public async void SaveSession()
    {
      await _multiWindow.SaveSession();
    }

    /// <summary>
    /// Сохраняет активную сессию файлов.
    /// </summary>
    public async void OpenSession()
    {
      SessionModel session = new DataBaseConfiguration.Services.SessionService().LoadSessionAsync().Result ?? new SessionModel();
      await _multiWindow.OpenSession(session);
    }

    /// <summary>
    /// Создаёт новый файл в редакторе.
    /// </summary>
    public async Task CreateNewFileAsync()
    {
      if (_isLockedProvider())
      {
        Message.MessageBoxCustom.Show("В данный момент идёт работа с аппаратурой! Пожалуйста завершите выполнение!", "Ошибка!", MessageBoxButton.OK);
      }
      else
      {
        await _multiWindow.CreateNewFile();
      }
    }

    /// <summary>
    /// Сохраняет текущий файл.
    /// </summary>
    public async Task SaveFileAsync()
    {
      await _multiWindow.SaveFile();
    }

    /// <summary>
    /// Сохраняет текущий файл под другим именем.
    /// </summary>
    public async Task SaveFileAsAsync()
    {
      await _multiWindow.SaveFileAs();
    }

    /// <summary>
    /// Отправляет текущий файл на печать.
    /// </summary>
    public async Task PrintFileAsync()
    {
      await _multiWindow.PrintFile();
    }

    /// <summary>
    /// Закрывает приложение.
    /// </summary>
    public async Task ExitApplicationAsync()
    {
      Application.Current.Shutdown();
      await Task.CompletedTask;
    }

    /// <summary>
    /// Выполняет поиск в текущем файле.
    /// </summary>
    public async Task SearchFileAsync()
    {
      TextEditorUI activeEditor = await _multiWindow.GetActiveTextEditor();

      if (_isSearchWindowOpen == false && activeEditor != null)
      {
        _mainWindow.SearchWindow.Owner = _mainWindow;
        _mainWindow.SearchWindow.SelectFileForSearch += OpenFileAsync;
        _mainWindow.SearchWindow.ShowWindow();
        _isSearchWindowOpen = true;
      }

      if (activeEditor != null)
      {
        string selectedText = activeEditor?.TextArea.Selection.GetText();

        if (!string.IsNullOrEmpty(selectedText))
        {
          SearchEventAdapter.RaiseSearchTextRequested(selectedText);
        }
      }
      else
      {
        return;
      }
    }

    /// <summary>
    /// Выполняет операцию сравнения файлов.
    /// </summary>
    public async Task CompareFileAsync()
    {
      _mainWindow.Effect = new System.Windows.Media.Effects.BlurEffect();
      var fileCompareWindow = new FileCompareWindow();
      fileCompareWindow.DialogClosed += Dialog_Closed;
      fileCompareWindow.Owner = _mainWindow;
      fileCompareWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      fileCompareWindow.ShowDialog();
      _mainWindow.Effect = null;
    }

    private async void Dialog_Closed(object sender, EventArgs e)
    {
      _mainWindow.Effect = null;
    }


    /// <summary>
    /// Открывает диалог выбора архива и загружает его в редактор.
    /// </summary>
    public async Task OpenArchiveAsync()
    {
      await _multiWindow.OpenArchiveAsync();
    }

    /// <summary>
    /// Создает новый файл трансляции (.opkw) в редакторе.
    /// </summary>
    public TextEditorUI CreateTranslationFileAsync()
    {
      if (_isLockedProvider())
      {
        Message.MessageBoxCustom.Show("В данный момент идёт работа с аппаратурой! Пожалуйста завершите выполнение!", "Ошибка!", MessageBoxButton.OK, image: MessageBoxImage.Error);
        return null;
      }
      else
      {
        return _multiWindow.CreateTranslationFileAsync();
      }
    }

    internal async Task OpenFolder()
    {
      await _multiWindow.OpenFolder();

    }
  }
}
