using Errors.Translation;

namespace ControlCommandAnalyser.Model.Interface
{
  public interface IError
  {
    IPointError PointErrors { get; }
  }
}
