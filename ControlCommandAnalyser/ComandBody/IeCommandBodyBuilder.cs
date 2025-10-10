using System.Text;
using ControlCommandAnalyser.Model;

namespace ControlCommandAnalyser.ComandBody
{
  public class IeCommandBodyBuilder : ICommandBody
  {
    public bool CanCreate(BaseCommandModel model) => model is IeCommandModel;

    public StringBuilder Create(BaseCommandModel model, StringBuilder newSourseLines)
    {
      if (model is not IeCommandModel ie)
      {
        return newSourseLines;
      }
      var commandBody = new StringBuilder();
      if (ie.LowerLimitCapacity != null && ie.CapacityUnit != null)
      {
        commandBody.Append($"{ie.LowerLimitCapacity}<{ie.CapacityUnit}");
      }
      if (ie.HigherLimitCapacity != null)
      {
        commandBody.Append($"<{ie.HigherLimitCapacity} ");
      }

      newSourseLines.Append(commandBody.ToString());

      return newSourseLines;
    }
  }
}
