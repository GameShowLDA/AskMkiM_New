using AppConfiguration.Base;
using DTO.SettingsModels;
using Utilities.FilesUtility;

namespace AppConfiguration.Parameter
{
  internal class ParameterFileManager : ConfigurationManagerBase<UserInterfaceModel>
  {
    private readonly YamlUtility<UserInterfaceModel> _yamlHelper;


    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="SettingsProtocolModel"/> с заданным путем к файлу.
    /// </summary>
    /// <param name="pathFile">Путь к YAML файлу, в котором будет храниться конфигурация модели выполнения.</param>
    internal ParameterFileManager(string pathFile) : base(pathFile)
    {
      _yamlHelper = new YamlUtility<UserInterfaceModel>(pathFile);
    }

    public override async Task<UserInterfaceModel> ReadFileAsync() => await _yamlHelper.ReadAsync();

    public override async Task RewriteFileAsync(UserInterfaceModel data) => await _yamlHelper.RewriteAsync(data);
  }
}
