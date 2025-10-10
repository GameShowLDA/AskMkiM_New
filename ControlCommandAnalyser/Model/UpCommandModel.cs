namespace ControlCommandAnalyser.Model
{
  /// <summary>
  /// Модель команды УП (условный переход).
  /// </summary>
  public class UpCommandModel : BaseCommandModel
  {
    public override string Mnemonic => "УП";

    /// <summary>
    /// Номер перехода (метка, на которую надо перейти).
    /// </summary>
    public string TargetLabel { get; set; }
  }
}
