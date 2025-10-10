using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Service
{
  /// <summary>
  /// Интерфейс сервиса управления отображением кнопок пользовательского интерфейса.
  /// Предоставляет методы для изменения видимости кнопок в зависимости от текущего состояния программы.
  /// </summary>
  public interface IButtonService
  {
    /// <summary>
    /// Скрывает все кнопки интерфейса.
    /// </summary>
    void SetNonVisibleAllButton();

    /// <summary>
    /// Отображает только кнопку запуска (Start).
    /// Используется при подготовке к выполнению программы или начале нового процесса.
    /// </summary>
    void ShowOnlyStartButton();

    /// <summary>
    /// Отображает только кнопки остановки (Stop) и завершения (Finish).
    /// Применяется во время активного выполнения программы.
    /// </summary>
    void ShowOnlyStopAndFinishButtons();

    /// <summary>
    /// Отображает только кнопку выхода (Exit).
    /// Используется при завершении работы приложения или переходе в состояние завершения.
    /// </summary>
    void ShowOnlyExitButton();

    /// <summary>
    /// Отображает набор кнопок, соответствующих состоянию паузы.
    /// Обычно включает кнопку продолжения и кнопку выхода из паузы.
    /// </summary>
    void ShowButtonsOnPause();
  }

}
