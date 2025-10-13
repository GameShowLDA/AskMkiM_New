using DTO.Base.Models;
using EventCore.Adapters;
using EventCore.Events;
using Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UI.Components;
using UI.Components.ArchiveControls;
using UI.Components.FileComparerControls;
using UI.Components.MultiEditorMethods;
using UI.Controls;
using UI.Controls.Runner;
using UI.Controls.TextEditor;
using UI.Windows.WpfDocking.Windows.Docking;
using static Utilities.LoggerUtility;

namespace UI.Services
{
  public class DockItemService
  {
    /// <summary>
    /// Отображает новую вкладку в контейнере.
    /// </summary>
    /// <param name="textEditorContainer">Контейнер с текстовыми редакторами, в котором необходимо открыть файл.</param>
    /// <param name="dockItem">Новая вкладка.</param>
    public void ShowDockItem(TextEditorContainer textEditorContainer, DockItem dockItem)
    {
      try
      {
        var dockControl = textEditorContainer?.DockManager;

        if (dockControl == null)
        {
          LogError("DockControl не найден (null). Невозможно отобразить вкладку.");
          return;
        }

        LogInformation($"Попытка показать DockItem. Title: {dockItem.Title}, IsLoaded: {dockControl.IsLoaded}, DockItems.Count: {dockControl.DockItems.Count}");

        if (!dockControl.IsLoaded)
        {
          LogWarning("DockControl ещё не загружен. Подписка на Loaded...");

          var capturedDockItem = dockItem;
          dockControl.Loaded += (s, e) =>
          {
            try
            {
              LogInformation("DockControl загрузился. Показываем вкладку.");
              capturedDockItem.Show(dockControl, DockPosition.Document);
              LogInformation("DockItem отображён после загрузки.");
            }
            catch (Exception ex)
            {
              LogException("Ошибка при отображении DockItem после загрузки:", ex);
            }
          };
        }
        else
        {
          dockItem.Show(dockControl, DockPosition.Document);
          LogInformation("DockItem отображён немедленно.");
        }
      }
      catch (Exception ex)
      {
        LogException("Ошибка при отображении DockItem:", ex);
      }
    }

    /// <summary>
    /// Отображает новую вкладку с транслятором.
    /// </summary>
    /// <param name="nameFile">Название файла.</param>
    /// <param name="textEditorContainer">Контейнер для транслятора.</param>
    /// <param name="textEditor">Текстовый редактор с транслируемым документом.</param>
    /// <param name="translatorEditor">Текстовый редактор с странслированным документом.</param>
    /// <returns>Асинхронную задачу, представляющую результат создания экземпляра <see cref="TranslatorItem"/>.</returns>
    public async Task<TranslatorItem> ShowNewDockItem(string nameFile, TextEditorContainer textEditorContainer, TextEditorUI textEditor, TextEditorUI translatorEditor)
    {
      try
      {
        var translatorItem = new TranslatorItem();
        translatorItem.SetLeftEditor(textEditor);
        translatorItem.SetRightEditor(translatorEditor);
        translatorItem.SetRightEditorName(translatorEditor.TextEditorModel.FileName);
        translatorItem.SetLeftEditorName(textEditor.TextEditorModel.FileName);
        var dockItem = new DockItem
        {
          Title = nameFile,
          TabText = nameFile,
          Content = translatorItem
        };

        dockItem.ItemClosed += (sender) =>
        {
          var controlManager = new ControlManager(_fileManager.EditorWorkspaceModel);
          var foundPage = _fileManager.EditorWorkspaceModel.OpenPages.FirstOrDefault(page => page.Text == EditorType.Translator.ToString());
          TextEditorContainer translatorContainer = _fileManager.ContainerService.GetContainer(EditorType.Translator);
          if (translatorContainer != null && translatorContainer.DockManager.DockItems.Count(item => item.DockPosition != DockPosition.Hidden) == 0)
          {
            _fileManager.ContainerService.RemoveTextEditorContainer(translatorContainer, EditorType.Translator);
          }

          EditorEventAdapter.RaiseTextEditorContainerClosing(true, nameFile);
        };


        await Task.Delay(1).ConfigureAwait(true);

        ShowDockItem(textEditorContainer, dockItem);

        return translatorItem;
      }
      catch (Exception ex)
      {
        MessageBoxCustom.Show($"Системная ошибка: {ex}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
        LogError($"Системная ошибка: {ex}");
        return null;
      }
    }


    /// <summary>
    /// Обрабатывает добавление или открытие файла с учётом уже открытых файлов в редакторе.
    /// При необходимости добавляет новый DockItem или показывает существующий.
    /// </summary>
    /// <param name="nameFile">Имя файла.</param>
    /// <param name="textEditorContainer">Контейнер редактора, в котором будут размещаться DockItem'ы.</param>
    /// <param name="textEditor">Экземпляр редактора для отображения содержимого файла.</param>
    internal async void ShowNewDockItem(string nameFile, TextEditorContainer textEditorContainer, UserControl textEditor, EditorType editorType = null)
    {
      LogDebug($"Создание DockItem для файла {nameFile}");
      var dockItem = new DockItem
      {
        Title = nameFile,
        TabText = nameFile,
        Content = textEditor
      };

      EventCore.Services.EventAggregator.Unsubscribe<FileInteractionEvents.OpenOpk>(e => _fileManager.ArchiveService.OpenOpkFile(e.Control, e.FileName));
      EventCore.Services.EventAggregator.Subscribe<FileInteractionEvents.OpenOpk>(e => _fileManager.ArchiveService.OpenOpkFile(e.Control, e.FileName));

      if (dockItem.Content is TextEditorUI && editorType == EditorType.Archive || dockItem.Content is RunControl && editorType == EditorType.Run)
      {
        InitializeItemWithoutSave(dockItem, editorType);
      }
      else if (dockItem.Content is TextEditorUI || dockItem.Content is FileCompareControl)
      {
        if (editorType != EditorType.Protocol)
        {
          editorType = EditorType.TextEditor;
        }
        else
        {
          (dockItem.Content as TextEditorUI).IsReadOnly = true;
        }

        InitializeItemNeedSave(nameFile, textEditorContainer, textEditor, editorType, dockItem);
      }
      else if (dockItem.Content is TableAllArchivesControl || dockItem.Content is TableApkArchiveControl)
      {
        editorType = EditorType.Archive;
        editorType = InitializeItemWithoutSave(dockItem, editorType);
      }

      await Task.Delay(1).ConfigureAwait(true);

      ShowDockItem(textEditorContainer, dockItem);
      _fileManager.ControlManagerService.ShowControl(textEditorContainer, editorType);
    }

    private EditorType InitializeItemWithoutSave(DockItem dockItem, EditorType editorType)
    {
      dockItem.ItemClosed += (sender) =>
      {
        var controlManager = new ControlManager(_fileManager.EditorWorkspaceModel);
        var foundPage = _fileManager.EditorWorkspaceModel.OpenPages.FirstOrDefault(page => page.Text == editorType.ToString());
        TextEditorContainer translatorContainer = _fileManager.ContainerService.GetContainer(editorType);

        if (translatorContainer != null && translatorContainer.DockManager.DockItems.Count(item => item.DockPosition != DockPosition.Hidden) == 0)
        {
          _fileManager.ContainerService.RemoveTextEditorContainer(translatorContainer, editorType);
        }
      };
      return editorType;
    }

    private void InitializeItemNeedSave(string nameFile, TextEditorContainer textEditorContainer, UserControl textEditor, EditorType editorType, DockItem dockItem)
    {
      LogDebug($"Тип редактора для файла {nameFile}: {editorType.ToString()}");

      dockItem.CloseItem += (sender) =>
      {
        LogDebug($"Закрытие файла {nameFile}.");

        if (textEditorContainer != null && editorType != null)
        {
          var controlManager = new ControlManager(_fileManager.EditorWorkspaceModel);
          var foundPage = _fileManager.EditorWorkspaceModel.OpenPages.FirstOrDefault(page => page.Text == editorType.ToString());
          controlManager.RemoveControl(foundPage, textEditor).ConfigureAwait(true);
          _fileManager.EditorWorkspaceModel.FilePaths.Remove(dockItem.TabText);
          if (_fileManager.EditorWorkspaceModel.FilePaths.Count == 0)
          {
            LogDebug($"Закрытие контейнера типа \"{editorType.ToString()}\".");
            _fileManager.ContainerService.RemoveTextEditorContainer(textEditorContainer, editorType);
          }

          EditorEventAdapter.RaiseTextEditorContainerClosing(true, nameFile);

        }
      };
    }

    public DockItemService(UI.Components.MultiEditorMethods.FileManager fileManager)
    {
      _fileManager = fileManager;
    }

    UI.Components.MultiEditorMethods.FileManager _fileManager;
  }
}
