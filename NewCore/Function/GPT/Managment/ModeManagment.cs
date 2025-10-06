using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewCore.Base.Function.Breakdown.Capabilities;
using NewCore.Device;
using NewCore.Function.GPT.Data;
using NewCore.Function.GPT.Helper;
using Utilities.Interface;

namespace NewCore.Function.GPT.Managment
{
  /// <summary>
  /// Универсальный класс управления режимами работы устройства GPT-79904.
  /// Позволяет устанавливать и считывать текущий режим (ACW, DCW, IR и т.д.),
  /// а также выполнять автоматическую переинициализацию конфигурации.
  /// </summary>
  public class ModeManagment : IModeConfigurable
  {
    private readonly GPT79904 _gptModel;
    private readonly TypeMode _mode;
    private readonly int _delay;
    private readonly Func<Task> _reloadConfiguration;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="ModeManagment"/>.
    /// </summary>
    /// <param name="gptModel">Модель устройства GPT-79904.</param>
    /// <param name="mode">Режим работы (ACW, DCW, IR и т.д.).</param>
    /// <param name="delay">Задержка между командами, мс.</param>
    /// <param name="reloadConfiguration">Функция для повторного считывания конфигурации при смене режима.</param>
    public ModeManagment(GPT79904 gptModel, TypeMode mode, int delay, Func<Task> reloadConfiguration)
    {
      _gptModel = gptModel;
      _mode = mode;
      _delay = delay;
      _reloadConfiguration = reloadConfiguration;
    }

    /// <inheritdoc />
    /// <summary>
    /// Устанавливает режим прибора и выполняет считывание конфигурации при успехе.
    /// </summary>
    public async Task<(bool Success, string Message)> SetModeAsync(IUserMessageService? userMessageService = null)
    {
      var result = await ModeHelper.SetModeAsync(_gptModel, _mode, _delay);

      if (result.Success)
      {
        _gptModel.Mode = _mode;
        await _reloadConfiguration();
      }

      return result;
    }

    /// <inheritdoc />
    /// <summary>
    /// Проверяет текущий установленный режим устройства.
    /// </summary>
    public async Task<(bool Success, string Message)> GetModeAsync()
    {
      return await ModeHelper.GetModeAsync(_gptModel, _mode, _delay);
    }
  }
}
