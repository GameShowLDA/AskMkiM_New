using System.IO;

namespace DTO.Base.Models
{
  public class ProtocolModel
  {
    /// <summary>
    /// Дата протокола.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Обозначение сборочной единицы.
    /// </summary>
    public string Designation { get; set; }


    /// <summary>
    /// Номер сборочной единицы.
    /// </summary>
    public string Number { get; set; }

    /// <summary>
    /// Исполнитель.
    /// </summary>
    public string Executor { get; set; }

    /// <summary>
    /// Путь к программе контроля.
    /// </summary>
    public string ProgramPath { get; set; }

    /// <summary>
    /// Название программы контроля.
    /// </summary>
    public string ProgramName { get; set; }

    /// <summary>
    /// Представитель ОК.
    /// </summary>
    public string Agent { get; set; }

    /// <summary>
    /// Представитель заказчика(ВП).
    /// </summary>
    public string Customer { get; set; }

    /// <summary>
    /// Режим выполнения.
    /// </summary>
    public string Mode { get; set; }

    /// <summary>
    /// Список ошибок программы.
    /// </summary>
    public Dictionary<string, List<ShowMessageModel>> Errors { get; set; } = new Dictionary<string, List<ShowMessageModel>>();

    /// <summary>
    /// Время начала выполнения.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Время конца исполнения.
    /// </summary>
    public DateTime EndTime
    {
      get;
      set;
    }

    /// <summary>
    /// Общее время выполнения.
    /// </summary>
    public TimeSpan ExecutionTime
    {
      get
      {
        return EndTime - StartTime;
      }
    }

    private static string Template { get; set; } = string.Empty;

    public ProtocolModel()
    {
      StartTime = DateTime.Now;
    }

    static public void SetTemplate(string templatePath)
    {
      Template = templatePath;
    }

    static public string GetPathProtocol(ProtocolModel protocolModel)
    {

      // Формируем финальный текст протокола
      string formattedText = Template
          .Replace("$ДАТА", protocolModel.Date.ToString("dd.MM.yyyy"))
          .Replace("$ОБОЗНАЧЕНИЕ", protocolModel.Designation)
          .Replace("$РЕЖИМ", protocolModel.Mode)
          .Replace("$НОМЕР", protocolModel.Number.ToString())
          .Replace("$ПРОГРАММА", protocolModel.ProgramName)
          .Replace("$НАЧАЛО", protocolModel.StartTime.ToString("HH:mm:ss:ff"))
          .Replace("$КОНЕЦ", protocolModel.EndTime.ToString("HH:mm:ss:ff"))
          .Replace("$ВРЕМЯ", protocolModel.ExecutionTime.ToString(@"hh\:mm\:ss\:ff"))
          .Replace("$ИСПОЛНИТЕЛЬ", protocolModel.Executor)
          .Replace("$ПРЕДСТАВИТЕЛЬ", protocolModel.Agent)
          .Replace("$ЗАКАЗЧИК", protocolModel.Customer);

      try
      {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        var parent1 = directory.Parent;
        var parent2 = parent1?.Parent;

        if (parent2 != null)
        {
          var historyPath = Path.Combine(parent2.FullName, "History");
          if (!Directory.Exists(historyPath))
          {
            Directory.CreateDirectory(historyPath);
          }

          var dateFolderName = DateTime.Now.ToString("yyyy-MM-dd");
          var datePath = Path.Combine(historyPath, dateFolderName);

          if (!Directory.Exists(datePath))
          {
            Directory.CreateDirectory(datePath);
          }

          var fileName = $"{protocolModel.ProgramName}_{DateTime.Now.ToString("HHmmss")}.lstw";
          var fullFilePath = Path.Combine(datePath, fileName);

          using (StreamWriter writer = new StreamWriter(fullFilePath))
          {
            writer.WriteLine(formattedText);
          }


          return fullFilePath;
        }
        else
        {
          Console.WriteLine("Не удалось получить родительскую директорию");
          return null;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Произошла ошибка: {ex.Message}");
        return null;
      }

    }

    static public string GetProtocolText(ProtocolModel protocolModel)
    {
      // Формируем финальный текст протокола
      string formattedText = Template
          .Replace("$ДАТА", protocolModel.Date.ToString("dd.MM.yyyy"))
          .Replace("$ОБОЗНАЧЕНИЕ", protocolModel.Designation)
          .Replace("$РЕЖИМ", protocolModel.Mode)
          .Replace("$НОМЕР", protocolModel.Number.ToString())
          .Replace("$ПРОГРАММА", protocolModel.ProgramName)
          .Replace("$НАЧАЛО", protocolModel.StartTime.ToString("HH:mm:ss:ff"))
          .Replace("$КОНЕЦ", protocolModel.EndTime.ToString("HH:mm:ss:ff"))
          .Replace("$ВРЕМЯ", protocolModel.ExecutionTime.ToString(@"hh\:mm\:ss\:ff"))
          .Replace("$ИСПОЛНИТЕЛЬ", protocolModel.Executor)
          .Replace("$ПРЕДСТАВИТЕЛЬ", protocolModel.Agent)
          .Replace("$ЗАКАЗЧИК", protocolModel.Customer);
      return formattedText;
    }

    // TODO: формировать текст протокола с ошибками
    public static string GetProtocolWithErrorsText(ProtocolModel protocolModel)
    {
      // TODO: изначально в template не тот текст протокола, нужно как-то добавить протокол с ошибками вместо
      // протокола без ошибок
      string formattedText = Template
          .Replace("$ДАТА", protocolModel.Date.ToString("dd.MM.yyyy"))
          .Replace("$ОБОЗНАЧЕНИЕ", protocolModel.Designation)
          .Replace("$РЕЖИМ", protocolModel.Mode)
          .Replace("$НОМЕР", protocolModel.Number.ToString())
          .Replace("$ПРОГРАММА", protocolModel.ProgramName)

          .Replace("$БРАК(не )", "не ")
          //.Replace("$НАИМЕНОВАНИЕ", protocolModel..ProgramName)

          .Replace("$ИСПОЛНИТЕЛЬ", protocolModel.Executor)
          .Replace("$ПРЕДСТАВИТЕЛЬ", protocolModel.Agent)
          .Replace("$ЗАКАЗЧИК", protocolModel.Customer);

      int totalErrors = protocolModel.Errors.Values.Sum(list => list.Count);
      formattedText += $"\r\n\r\nОшибки программы (всего: {totalErrors}):";

      foreach (var item in protocolModel.Errors.Keys)
      {
        formattedText += $"\r\n\tОшибки команды: {item}";

        var errors = protocolModel.Errors[item];
        foreach (var error in errors)
        {
          formattedText += $"\r\n\t\t{error.ToString()}";
        }
      }

      return formattedText;
    }
  }
}
