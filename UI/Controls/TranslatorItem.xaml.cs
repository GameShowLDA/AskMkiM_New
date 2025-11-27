using System.Windows.Controls;
using ControlCommandAnalyser.Model;
using Errors.Models;
using EventCore.Adapters;
using UI.Controls.TextEditor;

namespace UI.Controls
{
  /// <summary>
  /// Логика взаимодействия для TranslatorContainer.xaml
  /// </summary>
  public partial class TranslatorItem : UserControl
  {
    public string FirstFilePath { get; set; }
    public string SecondFilePath { get; set; }

    public int ErrorCount { get; private set; } = 0;
    public int WarningCount { get; private set; } = 0;
    public int GeneralCount { get; private set; } = 0;

    private List<BaseCommandModel> translationModels = new List<BaseCommandModel>();
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
          if (model.Warnings.Count > 0)
          {
            SetWarning(model.Warnings);
          }
        }
      }
    }

    public TranslatorItem()
    {
      InitializeComponent();
      ErrorListBoxVertical.ErrorItemDoubleClicked += ErrorListBoxVertical_ErrorItemDoubleClicked;
      ErrorListBoxVertical.WarningItemDoubleClicked += ErrorListBoxVertical_WarningItemDoubleClicked;
    }

    private void ErrorListBoxVertical_ErrorItemDoubleClicked(ErrorItem error)
    {
      // Левый редактор (исходник)
      var leftLine = error.SourceLineNumber;
      var leftEditor = GetLeftEditor();

      if (leftLine > 0 && leftLine <= leftEditor.Document.LineCount)
      {
        var line = leftEditor.Document.GetLineByNumber(leftLine);
        leftEditor.ScrollToLine(leftLine);
        leftEditor.Select(line.Offset, line.Length);
        leftEditor.Focus();
      }

      // Правый редактор (трансляция)
      var rightLine = error.FormattedLineNumber;
      var rightEditor = GetRightEditor();

      if (rightLine > 0 && rightLine <= rightEditor.Document.LineCount)
      {
        var line = rightEditor.Document.GetLineByNumber(rightLine);
        rightEditor.ScrollToLine(rightLine);
        rightEditor.Select(line.Offset, line.Length);
        rightEditor.Focus();
      }
    }

    private void ErrorListBoxVertical_WarningItemDoubleClicked(WarningItem warning)
    {
      // Левый редактор (исходник)
      var leftLine = warning.SourceLineNumber;
      var leftEditor = GetLeftEditor();

      if (leftLine > 0 && leftLine <= leftEditor.Document.LineCount)
      {
        var line = leftEditor.Document.GetLineByNumber(leftLine);
        leftEditor.ScrollToLine(leftLine);
        leftEditor.Select(line.Offset, line.Length);
        leftEditor.Focus();
      }

      // Правый редактор (трансляция)
      var rightLine = warning.FormattedLineNumber;
      var rightEditor = GetRightEditor();

      if (rightLine > 0 && rightLine <= rightEditor.Document.LineCount)
      {
        var line = rightEditor.Document.GetLineByNumber(rightLine);
        rightEditor.ScrollToLine(rightLine);
        rightEditor.Select(line.Offset, line.Length);
        rightEditor.Focus();
      }
    }

    private void ErrorClear()
    {
      ErrorListBoxVertical.Errors.Clear();
      GeneralCount = 0;
      ErrorListBoxVertical.Warnings.Clear();
      WarningCount = 0;
      GeneralCount = 0;
    }

    public void SetLeftEditor(TextEditorUI textEditorUI)
    {
      if (textEditorUI == null)
        return;

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

      LeftBox.Children.Clear(); // Можно просто очистить всё, если нужно заменить
      LeftBox.Children.Add(textEditorUI);
    }


    public void SetRightEditor(TextEditorUI textEditorUI)
    {
      if (RightBox == null || textEditorUI == null)
      {
        return;
      }

      RightBox.Children.Clear();
      RightBox.Children.Add(textEditorUI);
    }

    private void SetError(List<ErrorItem> errorItems)
    {
      foreach (ErrorItem errorItem in errorItems)
      {
        ErrorListBoxVertical.Errors.Add(errorItem);
        GeneralCount++;
        GeneralCount++;
      }

      if (GeneralCount > 0)
      {
        MessageEventAdapter.RaiseInfoMessage($"Общее кол-во ошибок и предупреждений: {GeneralCount}");
      }
    }

    private void SetWarning(List<WarningItem> warningItems)
    {
      foreach (WarningItem warningItem in warningItems)
      {
        ErrorListBoxVertical.Warnings.Add(warningItem);
        WarningCount++;
        GeneralCount++;
      }

      if (GeneralCount > 0)
      {
        MessageEventAdapter.RaiseInfoMessage($"Общее кол-во ошибок и предупреждений: {GeneralCount}");
      }
    }

    public TextEditorUI GetRightEditor()
    {
      if (RightBox == null)
      {
        return null;
      }

      return RightBox.Children[0] as TextEditorUI;
    }

    public TextEditorUI GetLeftEditor()
    {
      if (LeftBox == null)
      {
        return null;
      }

      return LeftBox.Children[0] as TextEditorUI;
    }

    public string GetLeftEditorName()
    {
      return FirstFileName.Text;
    }

    public string GetRightEditorName()
    {
      return SecondFileName.Text;
    }

    public void SetRightEditorName(string newText)
    {
      SecondFileName.Text = newText;
    }

    public void SetLeftEditorName(string newText)
    {
      FirstFileName.Text = newText;
    }
  }
}
