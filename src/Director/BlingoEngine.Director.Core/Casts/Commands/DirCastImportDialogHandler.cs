using System;
using AbstUI.Commands;
using AbstUI.Windowing;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Director.Core.Casts.Commands;

public class DirCastImportDialogHandler : IAbstCommandHandler<OpenCastImportDialogCommand>
{
    private readonly IServiceProvider _services;
    private readonly IAbstWindowManager _windowManager;
    private readonly BlingoProjectSettings _projectSettings;

    public DirCastImportDialogHandler(
        IServiceProvider services,
        IAbstWindowManager windowManager,
        BlingoProjectSettings projectSettings)
    {
        _services = services;
        _windowManager = windowManager;
        _projectSettings = projectSettings;
    }

    public bool CanExecute(OpenCastImportDialogCommand command) => command.Cast != null;

    public bool Handle(OpenCastImportDialogCommand command)
    {
        if (!_projectSettings.HasValidSettings)
        {
            _windowManager.OpenWindow(DirectorMenuCodes.ProjectSettingsWindow);
            _windowManager.ShowNotification(
                "Configure the project settings before importing members.",
                AbstUINotificationType.Warning);
            return false;
        }

        var dialog = _services.GetRequiredService<DirCastImportDialog>();
        dialog.Configure(command.Cast, command.StartSlot);
        _windowManager.ShowCustomDialog("Import cast members", dialog.GetFrameworkPanel());
        return true;
    }
}
