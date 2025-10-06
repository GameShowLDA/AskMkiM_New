using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using UI.Controls.Archive.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static Utilities.LoggerUtility;

namespace UI.Controls.Archive.Services
{
  /// <summary>
  /// Отвечает за создание новых архивов (.apkw).
  /// При создании формируется минимальная структура: 
  /// индекс (index.json) и контрольная сумма (control.yaml).
  /// </summary>
  public class ArchiveCreator
  {
    private const string IndexFileName = "index.json";
    private const string ControlFileName = "control.yaml";

    /// <summary>
    /// Создаёт новый архив с базовой структурой.
    /// </summary>
    /// <param name="archivePath">Полный путь к файлу архива (.apkw).</param>
    /// <returns><c>true</c>, если архив успешно создан; иначе <c>false</c>.</returns>
    public async Task<bool> CreateAsync(string archivePath, NewArchiveModel info)
    {
      try
      {
        if (File.Exists(archivePath))
        {
          LogWarning($"Файл {archivePath} уже существует");
          return false;
        }

        // Создаём новый zip-архив
        using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
        {
          // index.json (пустой массив)
          var indexEntry = archive.CreateEntry(IndexFileName);
          using (var writer = new StreamWriter(indexEntry.Open()))
          {
            string json = JsonConvert.SerializeObject(Array.Empty<object>(), Formatting.Indented);
            await writer.WriteAsync(json);
          }

          // control.yaml (например, версия + дата)
          var controlEntry = archive.CreateEntry(ControlFileName);
          using (var writer = new StreamWriter(controlEntry.Open()))
          {
            var serializer = new SerializerBuilder()
              .WithNamingConvention(CamelCaseNamingConvention.Instance)
              .Build();

            var controlData = new
            {
              version = 1,
              created = DateTime.UtcNow
            };

            string yaml = serializer.Serialize(controlData);
            await writer.WriteAsync(yaml);
          }
        }

        LogInformation($"Архив {archivePath} успешно создан");
        return true;
      }
      catch (Exception ex)
      {
        LogException($"Ошибка при создании архива {archivePath}", ex);
        return false;
      }
    }
  }
}
