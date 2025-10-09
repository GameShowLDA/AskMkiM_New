using System.Windows;
using DTO.Service.Models;

namespace UI.Controls.ProtocolNew
{
  public class ErrorManager
  {
    private int ErrorCount { get; set; } = 0;
    private ErrorList.ErrorListControl ErrorListBoxVertical;

    public void AddError(ErrorItem errorItem)
    {
      Application.Current.Dispatcher?.Invoke(() =>
      {
        ErrorListBoxVertical.Errors.Add(errorItem);
        ErrorCount++;

        if (ErrorCount > 0)
        {
          AppConfiguration.Base.EventAggregator.RaiseInfoMessage($"Общее кол-во ошибок: {ErrorCount}");
        }
      });
    }

    internal void ErrorClear()
    {
      Application.Current.Dispatcher?.Invoke(() =>
      {
        ErrorListBoxVertical.Errors.Clear();
        ErrorCount = 0;
      });
    }

    public ErrorManager(ErrorList.ErrorListControl errorListBoxVertical)
    {
      ErrorCount = 0;
      ErrorListBoxVertical = errorListBoxVertical;
    }
  }
}
