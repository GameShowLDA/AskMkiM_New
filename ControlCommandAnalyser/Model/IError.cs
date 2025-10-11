using AppConfiguration.Error.Translation;

namespace ControlCommandAnalyser.Model
{
  public interface IError
  {
    IPointError PointErrors { get; }
  }
}
