namespace AppConfiguration.Execution
{
  /// <summary>
  /// Класс конфигурации выполнений режимов для <see cref="ExecutionConfig"/>.
  /// </summary>    /// <summary>
  /// Модель данных <see cref="MeasurementErrorModel"/> для режима ИЕ.
  /// </summary>
  public static class ExecutionConfig
  {
    static public Action<ExecutionModel> SaveExecutionEvent;

    static ExecutionModel ExecutionModel = new ExecutionModel();

    /// <summary>
    /// Событие на изменение холостого режима
    /// </summary>
    static public event EventHandler<bool> IdleModeChange;

    #region Set.

    /// <summary>
    /// Включает или выключает холостой режим.
    /// </summary>
    /// <param name="enable">true для включения, false для выключения.</param>
    public static async Task SetIdleMode(bool enable)
    {
      await Task.Run(() =>
      {
        ExecutionModel.IdleModeExecution = enable;
        IdleModeChange?.Invoke(null, enable);
      });
    }

    /// <summary>
    /// Устанавливает режим по шагам.
    /// </summary>
    /// <param name="enable">true для включения, false для выключения.</param>
    public static async Task SetStepByStepMode(bool enable)
    {
      await Task.Run(() =>
      {
        ExecutionModel.StepByStepMode = enable;
        Base.EventAggregator.StepByStepModeFlag = enable;
      });
    }

    /// <summary>
    /// Устанавливает флаг остановки выполнения при ошибке.
    /// </summary>
    /// <param name="enable">true для включения, false для выключения.</param>
    public static async Task SetStopOnError(bool enable)
    {
      await Task.Run(() =>
      {
        ExecutionModel.StopOnError = enable;
      });
    }

    /// <summary>
    /// Включает или выключает режим симуляции ошибок.
    /// </summary>
    /// <param name="enable">true для включения, false для выключения.</param>
    public static async Task SetIsErrorSimulationMode(bool enable)
    {
      await Task.Run(() =>
      {
        ExecutionModel.IsErrorSimulationMode = enable;
      });
    }

    public static async Task SetExecutionModel(ExecutionModel protocolModel)
    {
      await Task.Run(() =>
      {
        ExecutionModel = protocolModel;
      });
    }

    #endregion

    #region Get.

    /// <summary>
    /// Проверяет, активен ли холостой режим.
    /// </summary>
    /// <returns>true, если включен; false, если выключен.</returns>
    public static Task<bool> GetIsIdleModeEnabled() => Task.FromResult(ExecutionModel?.IdleModeExecution ?? false);

    /// <summary>
    /// Проверяет, установлен ли флаг остановки при ошибке.
    /// </summary>
    /// <returns>true, если включен; false, если выключен.</returns>
    public static Task<bool> GetIsStopOnErrorEnabled() => Task.FromResult(ExecutionModel?.StopOnError ?? false);

    /// <summary>
    /// Возвращает, включена ли симуляция ошибок в холостом режиме.
    /// </summary>
    /// <returns>true, если включена; false, если выключена.</returns>
    public static Task<bool> GetIsErrorSimulationEnabled() => Task.FromResult(ExecutionModel?.IsErrorSimulationMode ?? false);

    /// <summary>
    /// Возвращает, включен ли пошаговый режим.
    /// </summary>
    /// <returns>true, если включен; false, если выключена.</returns>
    public static Task<bool> GetIsStepByStepModeEnabled() => Task.FromResult(ExecutionModel?.StepByStepMode ?? false);

    public static async Task<ExecutionModel> GetExecitonModel()
    {
      return await Task.Run(() =>
      {
        ExecutionModel executionModel = new ExecutionModel();
        executionModel.IdleModeExecution = ExecutionModel.IdleModeExecution;
        executionModel.IsErrorSimulationMode = ExecutionModel.IsErrorSimulationMode;
        executionModel.StepByStepMode = ExecutionModel.StepByStepMode;
        executionModel.StopOnError = ExecutionModel.StopOnError;
        return executionModel;
      });
    }
    #endregion

    public static async Task SaveExecutionModel(ExecutionModel execution)
    {
      await Task.Run(() =>
      {
        ExecutionModel.IdleModeExecution = execution.IdleModeExecution;
        ExecutionModel.IsErrorSimulationMode = execution.IsErrorSimulationMode;
        ExecutionModel.StepByStepMode = execution.StepByStepMode;
        ExecutionModel.StopOnError = execution.StopOnError;
      });

      SaveExecutionEvent?.Invoke(execution);
    }
  }
}
