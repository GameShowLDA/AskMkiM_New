using ControlCommandAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCommandAnalyser.ComandBody
{
  public class KscCommandBodyBuilder : ICommandBody
  {
    public bool CanCreate(BaseCommandModel model) => model is KscCommandModel;

    public StringBuilder Create(BaseCommandModel model, StringBuilder newSourseLines)
    {
      if (model is not KscCommandModel ksc)
      {
        return newSourseLines;
      }
      var commandBody = new StringBuilder();

      return newSourseLines.Append(commandBody.ToString());
    }
  }
}
