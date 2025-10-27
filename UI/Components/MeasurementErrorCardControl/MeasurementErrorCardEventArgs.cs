using static DTO.Enum.Measurement;

namespace UI.Components.MeasurementErrorCard
{
  public class MeasurementErrorCardEventArgs : EventArgs
  {
    public MeasurementTypeCommand TypeCommand { get; }
    public double PercentageValue { get; }
    public double NumericValue { get; }

    public MeasurementErrorCardEventArgs(MeasurementTypeCommand typeCommand, double percentageValue, double numericValue)
    {
      TypeCommand = typeCommand;
      PercentageValue = percentageValue;
      NumericValue = numericValue;
    }
  }
}
