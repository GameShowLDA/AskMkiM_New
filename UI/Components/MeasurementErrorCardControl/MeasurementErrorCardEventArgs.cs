using static DTO.Enum.Metrology;

namespace UI.Components.MeasurementErrorCard
{
  public class MeasurementErrorCardEventArgs : EventArgs
  {
    public MetrologyTypeCommand TypeCommand { get; }
    public double PercentageValue { get; }
    public double NumericValue { get; }

    public MeasurementErrorCardEventArgs(MetrologyTypeCommand typeCommand, double percentageValue, double numericValue)
    {
      TypeCommand = typeCommand;
      PercentageValue = percentageValue;
      NumericValue = numericValue;
    }
  }
}
