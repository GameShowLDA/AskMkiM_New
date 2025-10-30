using ControlCommandAnalyser.Model.Ok;

namespace ControlCommandAnalyser.Model
{
  public class KscCommandModel : BaseCommandModel
  {
    public OkCommandModel OkCommandModel { get; set; }
    public override string Mnemonic => Utilities.EnumExtensions.GetDisplayOrganizationalInfo(DTO.Enum.Measurement.OrganizationalComands.KSC).DisplayName;
  }
}