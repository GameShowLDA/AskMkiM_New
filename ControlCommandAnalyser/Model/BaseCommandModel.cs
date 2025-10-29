using AppConfiguration.Error.Translation;
using Errors.Models;
namespace ControlCommandAnalyser.Model
{
  /// <summary>
  /// Базовая модель любой команды после разбора.
  /// </summary>
  public abstract class BaseCommandModel : IError
  {
    public List<string> SourceLines { get; set; } = new List<string>();

    public List<ErrorItem> Errors { get; set; } = new List<ErrorItem>();

    /// <summary>
    /// Номер строки, с которой начинается команда (в исходном тексте).
    /// </summary>
    public int StartLineNumber { get; set; }

    /// <summary>
    /// Номер строки, с которой начинается команда в отформатированном (трансляционном) тексте.
    /// Проставляется после форматирования.
    /// </summary>
    public int FormattedStartLineNumber { get; set; } = -1;

    /// <summary>
    /// Ключи алгоритма проверки, указанные в команде.
    /// </summary>
    public List<string> AlgorithmKey { get; set; } = new();

    /// <summary>
    /// Комментарии, указанные в команде.
    /// </summary>
    public List<string> Comment { get; set; } = new();

    public virtual IPointError PointErrors => null;

    public string CommandNumber { get; set; }
    public virtual string Mnemonic { get; set; }
    public string PointsSourse { get; set; }

    public virtual T GetModel<T>(BaseCommandModel baseCommandModel) where T : class
    {
      return baseCommandModel as T;
    }
  }
}
