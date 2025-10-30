namespace ControlCommandAnalyser.Model
{
  public class CpCommandModel : BaseCommandModel
  {
    public override string Mnemonic => Utilities.EnumExtensions.GetDisplayOrganizationalInfo(DTO.Enum.Measurement.OrganizationalComands.CP).DisplayName;
  }
}
