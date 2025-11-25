using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandExecutor.BaseStrategies.Data;
using ControlCommandExecutor.Execution;
using DTO.Service;
using static ControlCommandExecutor.BaseStrategies.NodeFullChecker;

/// <summary>
/// Представляет контекст выполнения метода измерения в исполнительном модуле.
/// Содержит все необходимые данные, параметры и сервисы для проведения измерения
/// и обработки его результатов.
/// </summary>
internal class MethodExecutionContext : ExecutorContext
{
  /// <summary>
  /// Делегат, выполняющий операцию измерения.
  /// Вызывается методами исполнительного модуля для получения результата измерения.
  /// </summary>
  internal PerformMeasurementAsync PerformMeasurementAsync { get; set; }
}
