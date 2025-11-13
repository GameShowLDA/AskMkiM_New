namespace Utilities.TextEditor
{
  /// <summary>
  /// Универсальный фасад для взаимодействия с редактором текста.
  /// </summary>
  public interface ITextEditorAdapter
  {
    /// <summary>
    /// Установить маркер на указанную строку, очищая остальные.
    /// </summary>
    public void SetActiveLine(int lineNumber);

    public string Text { get; set; }
  }
}
