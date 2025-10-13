using System.Diagnostics;
using System.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using static Utilities.LoggerUtility;

namespace UI.Services.ProtocolManager
{
  /// <summary>
  /// Сервис для генерации и сохранения протоколов проверки в формате PDF.
  /// 
  /// Основные задачи:
  /// <list type="bullet">
  ///   <item>Создание PDF-документа на основе переданного содержимого.</item>
  ///   <item>Формирование структуры папок для хранения протоколов (по дате запуска).</item>
  ///   <item>Автоматическое открытие сгенерированного файла после сохранения.</item>
  ///   <item>Логирование ошибок в процессе сохранения PDF.</item>
  /// </list>
  /// </summary>
  public class PdfProtocolGenerator
  {
    /// <summary>
    /// Генерирует PDF-документ протокола и сохраняет его в папке <c>History</c>, создавая подкаталог с текущей датой.
    /// 
    /// Имя файла формируется по шаблону: <c>{programName}_HHmmss.pdf</c>.
    /// После успешного сохранения файл автоматически открывается в стандартном приложении для просмотра PDF.
    /// </summary>
    /// <param name="programName">Имя программы или теста, по которому формируется протокол.</param>
    /// <param name="content">Текстовое содержимое протокола, которое будет записано в PDF.</param>
    public void GenerateAndSavePdfProtocol(string programName, string content)
    {
      var directory = new DirectoryInfo(AppContext.BaseDirectory);
      var parent1 = directory.Parent;
      var parent2 = parent1?.Parent;
      var historyPath = Path.Combine(parent2.FullName, "History");
      var dateFolderName = DateTime.Now.ToString("yyyy-MM-dd");
      var datePath = Path.Combine(historyPath, dateFolderName);
      var fileName = $"{programName}_{DateTime.Now.ToString("HHmmss")}.pdf";
      var fullFilePath = Path.Combine(datePath, fileName);

      // Создаём документ
      var document = new Document();
      var section = document.AddSection();

      // Заголовок
      var paragraphContent = section.AddParagraph(content);
      paragraphContent.Format.Font.Size = 12;
      paragraphContent.Format.Font.Name = "Consolas";

      // Генерация PDF
      var renderer = new PdfDocumentRenderer(true)
      {
        Document = document
      };

      try
      {
        renderer.RenderDocument();
        renderer.PdfDocument.Save(fullFilePath);
        Process.Start(new ProcessStartInfo(fullFilePath) { UseShellExecute = true });
      }
      catch (Exception ex)
      {
        Message.MessageBoxCustom.Show("Ошибка при сохранении PDF: " + ex.Message);
        LogError("Ошибка при сохранении PDF: " + ex.Message);
      }
    }
  }
}
