using ControlCommandAnalyser.Attributes;
using ControlCommandAnalyser.Model;
using DTO.Enum;

namespace ControlCommandAnalyser
{
  public static class KeysHelper
  {
    private static readonly Dictionary<Type, TranslationKey.AlgorithmKey[]> Cache = new();

    public static TranslationKey.AlgorithmKey[] GetAllowedKeysForModel(BaseCommandModel model)
    {
      Type type = model.GetType();

      if (Cache.TryGetValue(type, out var keys))
        return keys;

      var attr = type.GetCustomAttributes(typeof(AllowedKeysAttribute), true)
                     .Cast<AllowedKeysAttribute>()
                     .FirstOrDefault();

      keys = attr?.Keys ?? Array.Empty<TranslationKey.AlgorithmKey>();
      Cache[type] = keys;

      return keys;
    }
    public static TranslationKey.AlgorithmKey[] GetNotAllowedKeysForModel(BaseCommandModel model)
    {
      // 1. Получаем разрешённые ключи через уже существующий кэш
      Type type = model.GetType();

      if (!Cache.TryGetValue(type, out var allowedKeys))
      {
        var attr = type.GetCustomAttributes(typeof(AllowedKeysAttribute), true)
                       .Cast<AllowedKeysAttribute>()
                       .FirstOrDefault();

        allowedKeys = attr?.Keys ?? Array.Empty<TranslationKey.AlgorithmKey>();
        Cache[type] = allowedKeys;
      }

      // 2. Получаем полный список всех ключей enum
      var allKeys = (TranslationKey.AlgorithmKey[])Enum.GetValues(typeof(TranslationKey.AlgorithmKey));

      // 3. Вычисляем запрещённые
      return allKeys
          .Where(k => !allowedKeys.Contains(k))
          .ToArray();
    }
  }
}
