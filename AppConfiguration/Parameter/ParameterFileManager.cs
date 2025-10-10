using AppConfiguration.Base;
using AppConfiguration.Protocol;
using DTO.SettingsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.FilesUtility;

namespace AppConfiguration.Parameter
{
  internal class ParameterFileManager : ConfigurationManagerBase<SettingsParameterModel>
  {
    private readonly YamlUtility<SettingsParameterModel> _yamlHelper;


    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="SettingsProtocolModel"/> с заданным путем к файлу.
    /// </summary>
    /// <param name="pathFile">Путь к YAML файлу, в котором будет храниться конфигурация модели выполнения.</param>
    internal ParameterFileManager(string pathFile) : base(pathFile)
    {
      _yamlHelper = new YamlUtility<SettingsParameterModel>(pathFile);
    }

    public override async Task<SettingsParameterModel> ReadFileAsync() => await _yamlHelper.ReadAsync();

    public override async Task RewriteFileAsync(SettingsParameterModel data) => await _yamlHelper.RewriteAsync(data);
  }
}
