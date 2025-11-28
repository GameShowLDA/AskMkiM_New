using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Ok;
using ControlCommandExecutor.Execution;
using Errors.Models;
using EventCore.Adapters;
using Message;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using UI.Controls.ProtocolNew;
using UI.Controls.TextEditor;
using WindowsInput;
using static Utilities.LoggerUtility;

namespace UI.Controls.Runner
{
  /// <summary>
  /// Логика взаимодействия для RunControl.xaml
  /// </summary>
  public partial class RunControl : UserControl
  {
    List<BaseCommandModel> ControlProgram = null;

    private bool _userResizing = false;

    private const double MaxAutoHeight = 250.0;

    public int ErrorCount { get; private set; } = 0;

    private ProtocolUI ProtocolUI { get; set; }

    public string FileName { get; set; }

    public string OpkFilePath { get; set; }

    private List<BaseCommandModel> translationModels = new List<BaseCommandModel>();

    public IReadOnlyCollection<int> Breakpoints { get; set; } = Array.Empty<int>();

    private TextEditorUI? _editor;

    private CommandExecutionManager? _executionManager;

    private TextEditorUI _leftEditor;
    public List<BaseCommandModel> TranslationModels
    {
      get
      {
        return translationModels;
      }
      set
      {
        translationModels = value;
        ErrorClear();

        foreach (var model in value)
        {
          if (model.Errors.Count > 0)
          {
            SetError(model.Errors);
          }
        }
      }
    }
    public string HeaderFile
    {
      get
      {
        return headerFile.Text;
      }
      set
      {
        headerFile.Text = value;
      }
    }

    bool task = false;
    public RunControl()
    {
      InitializeComponent();
      ProtocolUI = new ProtocolUI(true);
      ProtocolUI.ErrorListBoxVerticalVisibility = Visibility.Collapsed;
      MainContent.Content = ProtocolUI;
      ErrorListBoxVertical.ErrorItemDoubleClicked += ErrorItemDoubleClicked;

      Loaded += RunControl_Loaded;
      LeftBox.AddHandler(UIElement.PreviewGotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(LeftBox_PreviewGotKeyboardFocus), true);
    }

    private async void ErrorItemDoubleClicked(ErrorItem obj)
    {
      var protocolUI = MainContent.Content as ProtocolUI;
      if (protocolUI != null)
      {
        if (obj.SourceLineNumber >= 0)
        {
          await protocolUI.MoveToLineAsync(obj.SourceLineNumber);
        }

        if (obj.FormattedLineNumber >= 0)
        {
          _leftEditor?.GoToLine(obj.FormattedLineNumber);
        }
      }

    }

    private void RunControl_Loaded(object sender, RoutedEventArgs e)
    {
      FocusMainContent();
    }
    private void LeftBox_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      // Отменяем фокусировку и возвращаем в MainContent
      e.Handled = true;
      FocusMainContent();
    }
    private void FocusMainContent()
    {
      if (MainContent.Content is IInputElement focusable && focusable.Focusable)
      {
        Keyboard.Focus(focusable);
      }
      else if (MainContent.Content is FrameworkElement fe)
      {
        fe.Loaded += (_, _) =>
        {
          fe.Focus();
          Keyboard.Focus(fe);
        };
      }
    }

    private void SetError(List<ErrorItem> errorItems)
    {
      foreach (ErrorItem errorItem in errorItems)
      {
        ErrorListBoxVertical.Errors.Add(errorItem);
        ErrorCount++;
      }

      if (ErrorCount > 0)
      {
        MessageEventAdapter.RaiseInfoMessage($"Общее кол-во ошибок: {ErrorCount}");
      }
    }

    public void SetLeftEditor(TextEditorUI textEditorUI)
    {
      LogInformation("SetLeftEditor вызван: " + this.GetHashCode());

      if (textEditorUI == null)
        return;

      _editor = textEditorUI;

      if (textEditorUI.Parent is Panel oldParent)
      {
        oldParent.Children.Remove(textEditorUI);
      }
      else if (textEditorUI.Parent is ContentControl oldContent)
      {
        oldContent.Content = null;
      }
      else if (textEditorUI.Parent is Decorator decorator)
      {
        decorator.Child = null;
      }

      LeftBox.Children.Clear();
      LeftBox.Children.Add(textEditorUI);

      _leftEditor = textEditorUI;
    }


    public async Task Start(List<BaseCommandModel> models)
    {
      LogInformation($"[RunControl] Breakpoints: {string.Join(", ", Breakpoints)}");
      ProtocolUI.MenuButtonVisibility(false);
      ControlProgram = models;

      KeyboardManager.OnNextBreakpointPressed = () =>
      {
        Application.Current.Dispatcher.Invoke(() => ContinueFromBreakpoint());
      };

      var ok = models[0];
      if (ok.Mnemonic != "ОК")
      {
        return;
      }

      ProtocolUI.Header = (ok as OkCommandModel).ObjectCode;
      ProtocolUI.SetSettings(this, StartDelegate: StartTest, false);
      this.FileName = ProtocolUI.Header;

      await ProtocolUI.StartAsync();
    }

    private async Task StartTest(CancellationToken cancellationToken)
    {
      // Берём редактор, в котором пользователь расставил точки
      TextEditorUI? editor = null;

      Application.Current.Dispatcher.Invoke(() =>
      {
        editor = LeftBox.Children.Count > 0
          ? LeftBox.Children[0] as TextEditorUI
          : _editor;
      });

      if (editor == null)
      {
        MessageBoxCustom.Show("Редактор не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      // Лог для проверки, что брейкпоинты реально дошли до RunControl
      LogInformation($"[RunControl] Breakpoints: {string.Join(", ", Breakpoints)}");

      // создаём менеджер с брейкпоинтами
      _executionManager = new CommandExecutionManager(
        ProtocolUI,
        editor,
        ControlProgram,
        OpkFilePath,
        Breakpoints
      );

      _executionManager.ClearError += ErrorClear;
      _executionManager.AddError += AddError;

      // передаём туда cancellationToken
      await _executionManager.ExecuteAllAsync(cancellationToken);
    }

    private void AddError(ErrorItem errorItem)
    {
      Application.Current.Dispatcher?.Invoke(() =>
      {
        ErrorListBoxVertical.Errors.Add(errorItem);
        ErrorCount++;

        if (ErrorCount > 0)
        {
          MessageEventAdapter.RaiseInfoMessage($"Общее кол-во ошибок: {ErrorCount}");
        }
      });
    }

    private void ErrorClear()
    {
      Application.Current.Dispatcher?.Invoke(() =>
      {
        ErrorListBoxVertical.Errors.Clear();
        ErrorCount = 0;
      });
    }

    public void ContinueFromBreakpoint()
    {
      _executionManager?.ContinueFromBreakpoint();
    }


    private void ArrowButton_Click(object sender, RoutedEventArgs e)
    {
      var test = this.LeftBox.Children[0];
      if (test != null && test is TextEditorUI textEditor)
      {
        if (textEditor.TextEditorModel != null
          && !string.IsNullOrEmpty(textEditor.TextEditorModel.FilePath)
          && File.Exists(textEditor.TextEditorModel.FilePath))
        {
          FileInteractionEventAdapter.RaiseOpenFileInEditorAgain(textEditor.TextEditorModel.FilePath);
          EditorEventAdapter.RaiseCloseRunItem(this);
        }
      }
      else
      {
        MessageBoxCustom.Show("Ошибка обнаружения исходного файла", "Ошибка открытия файла", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
    }

    // Пользователь начал тянуть сплиттер – не вмешиваемся
    private void BottomSplitter_OnDragStarted(object sender, DragStartedEventArgs e)
    {
      _userResizing = true;
      BottomRow.Height = new GridLength(ErrorListBoxVertical.ActualHeight);
      ErrorListBoxVertical.MaxHeight = double.PositiveInfinity;
    }

    // Закончил тянуть – теперь снова можем автоподстраивать при изменении контента
    private void BottomSplitter_OnDragCompleted(object sender, DragCompletedEventArgs e)
    {
      _userResizing = false;
    }

    // Панель ошибок изменила размер (добавились/убрались строки)
    private void ErrorListBoxVertical_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (_userResizing)
        return;

      double desired = ErrorListBoxVertical.ActualHeight;

      if (desired > MaxAutoHeight)
        desired = MaxAutoHeight;

      // Автоматический режим — строка остаётся Auto, но мы ограничиваем контент
      BottomRow.Height = GridLength.Auto;
      ErrorListBoxVertical.MaxHeight = desired;
    }
  }
}
