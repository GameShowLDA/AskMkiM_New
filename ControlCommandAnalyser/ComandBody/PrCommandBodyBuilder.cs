using System.Text;
using ControlCommandAnalyser.Model;

namespace ControlCommandAnalyser.ComandBody
{
  public class PrCommandBodyBuilder : ICommandBody
  {
    public bool CanCreate(BaseCommandModel model) => model is PrCommandModel;

    public StringBuilder Create(BaseCommandModel model, StringBuilder newSourseLines)
    {
      if (model is not PrCommandModel pr)
      {
        return newSourseLines;
      }
      var commandBody = new StringBuilder();
      if (!string.IsNullOrEmpty(pr.LowerLimitResistanceSource))
      {
        commandBody.Append($"{pr.LowerLimitResistance}<");
      }
      if (!string.IsNullOrEmpty(pr.ResistanceUnit))
      {
        commandBody.Append($"{pr.ResistanceUnit}");
      }
      if (!string.IsNullOrEmpty(pr.HigherLimitResistanceSource))
      {
        commandBody.Append($"<{pr.HigherLimitResistance}");
      }

      return newSourseLines.Append(commandBody.ToString());
    }
  }
}
