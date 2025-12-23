using System.IO;
using System.Text.Json;

namespace UI.Controls.Settings.Warnings
{
  /// <summary>
  /// Сервис хранения настроек предупреждений.
  /// </summary>
  public sealed class WarningsSettingsStorage
  {
    private const string FileName = "warnings.settings.json";

    private static string FilePath =>
      Path.Combine(AppContext.BaseDirectory, FileName);

    /// <summary>
    /// Гарантирует наличие и актуальность файла настроек предупреждений.
    /// Вызывается один раз при старте приложения.
    /// </summary>
    public void Initialize()
    {
      _ = LoadAndSync();
    }

    public IReadOnlyList<WarningSetting> LoadAndSync()
    {
      var defaults = WarningCodeMetadata.ExtractDefaults();
      var defaultMap = defaults.ToDictionary(d => d.Code);

      List<WarningSetting> result;

      if (!File.Exists(FilePath))
      {
        result = defaults.ToList();
        Save(result);
        return result;
      }

      var json = File.ReadAllText(FilePath);
      var stored = JsonSerializer.Deserialize<List<WarningSetting>>(json)
                   ?? new List<WarningSetting>();

      var storedMap = stored.ToDictionary(s => s.Code);

      // 1. Добавляем новые
      foreach (var def in defaults)
      {
        if (!storedMap.ContainsKey(def.Code))
          stored.Add(def);
      }

      // 2. Удаляем устаревшие
      stored.RemoveAll(s => !defaultMap.ContainsKey(s.Code));

      // 3. Обновляем метаданные (Tag / Title), но НЕ трогаем IsEnabled
      foreach (var item in stored)
      {
        var def = defaultMap[item.Code];
        item.Tag = def.Tag;
        item.Title = def.Title;
      }

      Save(stored);
      return stored;
    }

    public void Save(IEnumerable<WarningSetting> settings)
    {
      var json = JsonSerializer.Serialize(
        settings,
        new JsonSerializerOptions { WriteIndented = true });

      File.WriteAllText(FilePath, json);
    }
  }
}
