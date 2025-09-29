using System;
using System.Collections.Generic;
using System.IO;
using AbstUI.Components.Buttons;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Components.Texts;
using AbstUI.Primitives;
using AbstUI.Windowing;
using AbstUI.Tasks;
using BlingoEngine.Casts;
using BlingoEngine.Director.Core.Events;
using BlingoEngine.Director.Core.FileSystems;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Projects;
using BlingoEngine.FrameworkCommunication;

namespace BlingoEngine.Director.Core.Casts;

public class DirCastImportDialog
{
    private const string FileFilter = "*.* ; All Files";

    private readonly IBlingoFrameworkFactory _factory;
    private readonly IDirFilePicker _filePicker;
    private readonly IDirFolderPicker _folderPicker;
    private readonly IDirectorEventMediator _mediator;
    private readonly IDirCastImportService _importService;
    private readonly BlingoProjectSettings _projectSettings;
    private readonly IAbstWindowManager _windowManager;

    private readonly List<string> _selectedFiles = new();
    private readonly AbstPanel _root;
    private readonly AbstLabel _castLabel;
    private readonly AbstLabel _slotLabel;
    private readonly AbstInputText _targetFolderInput;
    private readonly AbstButton _importButton;
    private readonly AbstWrapPanel _fileListPanel;
    private readonly AbstWrapPanel _messagePanel;

    private IBlingoCast? _cast;
    private int _startSlot;
    private string? _targetFolder;

    public DirCastImportDialog(
        IBlingoFrameworkFactory factory,
        IDirFilePicker filePicker,
        IDirFolderPicker folderPicker,
        IDirectorEventMediator mediator,
        IDirCastImportService importService,
        BlingoProjectSettings projectSettings,
        IAbstWindowManager windowManager)
    {
        _factory = factory;
        _filePicker = filePicker;
        _folderPicker = folderPicker;
        _mediator = mediator;
        _importService = importService;
        _projectSettings = projectSettings;
        _windowManager = windowManager;

        _root = BuildRoot(
            out _castLabel,
            out _slotLabel,
            out _targetFolderInput,
            out _importButton,
            out _fileListPanel,
            out _messagePanel);
    }

    public IAbstFrameworkPanel GetFrameworkPanel() => _root.Framework<IAbstFrameworkPanel>();

    public void Configure(IBlingoCast cast, int startSlot)
    {
        _cast = cast ?? throw new ArgumentNullException(nameof(cast));
        _startSlot = Math.Max(1, startSlot);

        _castLabel.Text = $"Cast: {cast.Name ?? $"Cast {cast.Number}"}";
        _slotLabel.Text = $"Start slot: {_startSlot}";

        _targetFolder = Directory.Exists(_projectSettings.ProjectFolder)
            ? _projectSettings.ProjectFolder
            : string.Empty;
        _targetFolderInput.Text = _targetFolder ?? string.Empty;

        _selectedFiles.Clear();
        UpdateFileList();
        ClearMessages();
        UpdateImportButtonState();
    }

    private AbstPanel BuildRoot(
        out AbstLabel castLabel,
        out AbstLabel slotLabel,
        out AbstInputText targetFolderInput,
        out AbstButton importButton,
        out AbstWrapPanel fileListPanel,
        out AbstWrapPanel messagePanel)
    {
        var root = _factory.CreatePanel("CastImportRoot");
        root.Width = 520;
        root.Height = 420;
        root.BackgroundColor = DirectorColors.BG_WhiteMenus;

        var content = _factory.CreateWrapPanel(AOrientation.Vertical, "CastImportContent");
        content.Width = 500;
        content.Height = 400;
        content.ItemMargin = new APoint(0, 8);
        content.Margin = new AMargin(10, 10, 10, 10);
        root.AddItem(content);

        var castInfo = _factory.CreateWrapPanel(AOrientation.Vertical, "CastImportHeader");
        castInfo.Width = 480;
        castInfo.ItemMargin = new APoint(0, 4);
        content.AddItem(castInfo);

        castLabel = _factory.CreateLabel("CastImportCastLabel", string.Empty);
        castInfo.AddItem(castLabel);

        slotLabel = _factory.CreateLabel("CastImportSlotLabel", string.Empty);
        castInfo.AddItem(slotLabel);

        var fileButtons = _factory.CreateWrapPanel(AOrientation.Horizontal, "CastImportFileButtons");
        fileButtons.Width = 480;
        fileButtons.ItemMargin = new APoint(6, 0);
        content.AddItem(fileButtons);

        var selectFiles = _factory.CreateButton("CastImportSelectFiles", "Select files...");
        selectFiles.Width = 140;
        selectFiles.Height = 26;
        selectFiles.Pressed += OnSelectFiles;
        fileButtons.AddItem(selectFiles);

        var clearFiles = _factory.CreateButton("CastImportClearFiles", "Clear");
        clearFiles.Width = 80;
        clearFiles.Height = 26;
        clearFiles.Pressed += () =>
        {
            _selectedFiles.Clear();
            UpdateFileList();
        };
        fileButtons.AddItem(clearFiles);

        var fileScroll = _factory.CreateScrollContainer("CastImportFileScroll");
        fileScroll.Width = 480;
        fileScroll.Height = 140;
        content.AddItem(fileScroll);

        fileListPanel = _factory.CreateWrapPanel(AOrientation.Vertical, "CastImportFilesPanel");
        fileListPanel.Width = 460;
        fileListPanel.ItemMargin = new APoint(0, 2);
        fileScroll.AddItem(fileListPanel);

        var folderRow = _factory.CreateWrapPanel(AOrientation.Horizontal, "CastImportFolderRow");
        folderRow.Width = 480;
        folderRow.ItemMargin = new APoint(6, 0);
        content.AddItem(folderRow);

        targetFolderInput = _factory.CreateInputText("CastImportTargetFolder", 0, OnTargetFolderChanged);
        targetFolderInput.Width = 360;
        targetFolderInput.Height = 26;
        folderRow.AddItem(targetFolderInput);

        var browseButton = _factory.CreateButton("CastImportBrowseFolder", "Browse...");
        browseButton.Width = 100;
        browseButton.Height = 26;
        browseButton.Pressed += OnBrowseFolder;
        folderRow.AddItem(browseButton);

        importButton = _factory.CreateButton("CastImportRunButton", "Import");
        importButton.Width = 120;
        importButton.Height = 30;
        importButton.Pressed += ExecuteImport;
        content.AddItem(importButton);

        messagePanel = _factory.CreateWrapPanel(AOrientation.Vertical, "CastImportMessages");
        messagePanel.Width = 480;
        messagePanel.Height = 120;
        messagePanel.ItemMargin = new APoint(0, 2);
        content.AddItem(messagePanel);

        return root;
    }

    private void OnSelectFiles()
    {
        var startPath = !string.IsNullOrWhiteSpace(_targetFolder) && Directory.Exists(_targetFolder)
            ? _targetFolder
            : _projectSettings.ProjectFolder;

        _filePicker.PickFiles(files =>
        {
            if (files == null || files.Count == 0)
                return;

            _selectedFiles.Clear();
            foreach (var file in files)
            {
                if (!string.IsNullOrWhiteSpace(file))
                    _selectedFiles.Add(file);
            }

            UpdateFileList();
        }, FileFilter, startPath);
    }

    private void OnBrowseFolder()
    {
        var startPath = !string.IsNullOrWhiteSpace(_targetFolder) && Directory.Exists(_targetFolder)
            ? _targetFolder
            : _projectSettings.ProjectFolder;

        _folderPicker.PickFolder(folder =>
        {
            if (string.IsNullOrWhiteSpace(folder))
                return;

            _targetFolder = folder;
            _targetFolderInput.Text = folder;
            UpdateImportButtonState();
        }, startPath);
    }

    private void OnTargetFolderChanged(string folder)
    {
        _targetFolder = folder;
        UpdateImportButtonState();
    }

    private void ExecuteImport()
    {
        if (_cast == null)
            return;

        var targetFolder = _targetFolder ?? string.Empty;
        if (string.IsNullOrWhiteSpace(targetFolder))
            return;

        var result = _importService.ImportMembers(
            _cast,
            _projectSettings.ProjectFolder,
            targetFolder,
            _startSlot,
            _selectedFiles);

        ShowMessages(result.Messages);
        NotifyMessages(result.Messages);

        if (result.ImportedCount > 0)
        {
            if (result.LastImportedMember is { } member)
            {
                _mediator.RaiseFindMember(member);
                _mediator.RaiseMemberSelected(member);
            }
            _windowManager.ShowNotification($"Imported {result.ImportedCount} member(s).", AbstUINotificationType.Info);
        }
        else if (result.Messages.Count == 0)
        {
            _windowManager.ShowNotification("No members were imported.", AbstUINotificationType.Warning);
        }

        UpdateImportButtonState();
    }

    private void UpdateFileList()
    {
        _fileListPanel.RemoveAll();
        if (_selectedFiles.Count == 0)
        {
            var label = _factory.CreateLabel("CastImportNoFiles", "No files selected.");
            label.FontColor = DirectorColors.TextColorDisabled;
            _fileListPanel.AddItem(label);
        }
        else
        {
            for (var i = 0; i < _selectedFiles.Count; i++)
            {
                var file = _selectedFiles[i];
                var label = _factory.CreateLabel($"CastImportFile_{i}", Path.GetFileName(file));
                _fileListPanel.AddItem(label);
            }
        }

        UpdateImportButtonState();
    }

    private void ClearMessages()
    {
        _messagePanel.RemoveAll();
        var label = _factory.CreateLabel("CastImportNoMessages", "Messages will appear here.");
        label.FontColor = DirectorColors.TextColorDisabled;
        _messagePanel.AddItem(label);
    }

    private void ShowMessages(IReadOnlyList<DirCastImportMessage> messages)
    {
        _messagePanel.RemoveAll();

        if (messages.Count == 0)
        {
            ClearMessages();
            return;
        }

        var index = 0;
        foreach (var message in messages)
        {
            var label = _factory.CreateLabel($"CastImportMessage_{index}", message.Text);
            label.FontColor = GetMessageColor(message.Type);
            _messagePanel.AddItem(label);
            index++;
        }
    }

    private void UpdateImportButtonState()
    {
        _importButton.Enabled = _selectedFiles.Count > 0 && !string.IsNullOrWhiteSpace(_targetFolder);
    }

    private void NotifyMessages(IReadOnlyList<DirCastImportMessage> messages)
    {
        foreach (var message in messages)
        {
            _windowManager.ShowNotification(message.Text, ToNotificationType(message.Type));
        }
    }

    private static AbstUINotificationType ToNotificationType(TaskMessageType type) => type switch
    {
        TaskMessageType.Error => AbstUINotificationType.Error,
        TaskMessageType.Warning => AbstUINotificationType.Warning,
        _ => AbstUINotificationType.Info
    };

    private static AColor GetMessageColor(TaskMessageType type) => type switch
    {
        TaskMessageType.Error => new AColor(180, 0, 0),
        TaskMessageType.Warning => new AColor(170, 110, 0),
        _ => DirectorColors.TextColorLabels
    };
}
