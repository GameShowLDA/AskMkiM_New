using System.Text;
using ControlCommandAnalyser.Model;

namespace ControlCommandAnalyser.ComandBody
{
  public class PiCommandBodyBuilder : ICommandBody
  {
    // TODO: ключи для ПИ и СИ перезаписывать
    public bool CanCreate(BaseCommandModel model) => model is PiCommandModel;

    public StringBuilder Create(BaseCommandModel model, StringBuilder newSourseLines)
    {
      if (model is not PiCommandModel pi)
      {
        return newSourseLines;
      }
      var commandBody = new StringBuilder();
      var siCommand = string.Empty;
      if (pi.SiCommand != null)
      {
        var siBuilder = new SiCommandBodyBuilder();
        commandBody = siBuilder.Create(pi.SiCommand, commandBody);
      }
      if (pi.VoltageType == VoltageEnum.Type.DCW)
      {
        commandBody.Append('+');
      }
      if (!string.IsNullOrEmpty(pi.VoltageSource))
      {
        commandBody.Append($"{pi.VoltageSource}");
      }
      if (!string.IsNullOrEmpty(pi.TimeSource))
      {
        commandBody.Append($", {pi.TimeSource}");
      }

      return newSourseLines.Append(commandBody.ToString());
    }
  }
}
