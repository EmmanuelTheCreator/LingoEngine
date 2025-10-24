using Microsoft.Extensions.DependencyInjection;
using AbstUI.Commands;
using AbstUI.Windowing.Commands;
using AbstUI.Tools;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using System;
using AbstUI.Components;

namespace AbstUI.Windowing;

public interface IAbstFrameworkWindowManager
{
    
    void SetActiveWindow(IAbstWindow window);
    IAbstWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult);
    IAbstWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel, APoint? position = null);
    IAbstWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? dialog = null, APoint? position = null)
        where TDialog : class, IAbstDialog;
    IAbstWindowDialogReference? ShowNotification(string message, AbstUINotificationType type);
}



public interface IAbstWindowManager
{
    IAbstWindow? ActiveWindow { get; }

    event Action<IAbstWindow>? NewWindowCreated;
    event Action<IAbstWindow>? WindowActivated;
    event Action<IAbstWindow?>? BeforeWindowActivated;
    public event Action<IAbstWindow>? WindowOpened;
    public event Action<IAbstWindow>? WindowClosed;

    bool OpenWindow(string windowCode);
    bool OpenWindow<TWindow>(string windowCode, Action<TWindow>? actionOnOpen = null)
        where TWindow : class, IAbstWindow;
    bool SwapWindowOpenState(string windowCode);
    bool CloseWindow(string windowCode);
    void Init(IAbstFrameworkWindowManager frameworkWindowManager);
    void SetActiveWindow(string windowCode);
    IAbstWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult);
    IAbstWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel, APoint? position = null);
    IAbstWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? dialog = null, APoint? position = null)
        where TDialog : class, IAbstDialog;
    IAbstWindowDialogReference? ShowNotification(string message, AbstUINotificationType type);
    void SetWindowSize(string windowCode, int width, int height);
    IEnumerable<IAbstWindow> GetWindows();
    IEnumerable<(string Code, ARect Rect)> GetRects();
}
public class AbstWindowManager : IAbstWindowManager, IDisposable,
    IAbstCommandHandler<OpenWindowCommand>,
    IAbstCommandHandler<CloseWindowCommand>
{
    
    private readonly AbstMainWindow _mainWindow;
    private readonly IAbstWindowFactory _windowFactory;
    private IAbstFrameworkWindowManager _frameworkWindowManager = null!;
    private IAbstWindowRegistration? _activeWindow;

    public event Action<IAbstWindow>? NewWindowCreated;
    public event Action<IAbstWindow?>? BeforeWindowActivated;
    public event Action<IAbstWindow>? WindowActivated;
    public event Action<IAbstWindow>? WindowOpened;
    public event Action<IAbstWindow>? WindowClosed;

    public IAbstWindow? ActiveWindow => _activeWindow?.Instance;
    public AbstWindowManager(AbstMainWindow mainWindow, IAbstWindowFactory abstWindowFactory)
    {
        _mainWindow = mainWindow;
        _windowFactory = abstWindowFactory;
        _windowFactory.NewWindowCreated += New_WindowCreated;
    }
    public void Dispose()
    {
        _windowFactory.NewWindowCreated -= New_WindowCreated;
    }
    private void New_WindowCreated(IAbstWindow window)
    {
        NewWindowCreated?.Invoke(window);
    }

    public void Init(IAbstFrameworkWindowManager frameworkWindowManager)
        => _frameworkWindowManager = frameworkWindowManager;
   
    public void SetActiveWindow(string windowCode) => SetActiveWindow(_windowFactory.GetWindowRegistration(windowCode));

    public void SetActiveWindow(IAbstWindowRegistration? registration)
    {
        if (registration == null) return;
        if (_activeWindow != null && _activeWindow.Instance != null)
        {
            if (registration == _activeWindow) return; // Already active
            _activeWindow.Instance.SetActivated(false);
        }

        registration.Instance.SetActivated(true);
        BeforeWindowActivated?.Invoke(_activeWindow?.Instance);
        _activeWindow = registration;
        _frameworkWindowManager.SetActiveWindow(registration.Instance);
        WindowActivated?.Invoke(registration.Instance);
    }

    public IAbstWindowDialogReference? ShowConfirmDialog(string title, string message, Action<bool> onResult)
        => _frameworkWindowManager.ShowConfirmDialog(title, message, onResult);

    public IAbstWindowDialogReference? ShowCustomDialog(string title, IAbstFrameworkPanel panel, APoint? position = null)
        => _frameworkWindowManager.ShowCustomDialog(title, panel,position);

    public IAbstWindowDialogReference? ShowCustomDialog<TDialog>(string title, IAbstFrameworkPanel panel, TDialog? dialog = null, APoint? position = null)
        where TDialog : class, IAbstDialog
        => _frameworkWindowManager.ShowCustomDialog<TDialog>(title, panel, dialog, position);

    public IAbstWindowDialogReference? ShowNotification(string message, AbstUINotificationType type)
        => _frameworkWindowManager.ShowNotification(message, type);

    public bool SwapWindowOpenState(string windowCode)
    {
        if (!_windowFactory.TryGetRegistration(windowCode, out var registration)) return false;
        var instance = registration.Instance;
        if (instance.IsOpen)
        {
            instance.CloseWindow();
            WindowClosed?.Invoke(registration.Instance);
            return true;
        }
        instance.OpenWindow();
        SetActiveWindow(registration);
        WindowOpened?.Invoke(registration.Instance);
        return true;
    }

    public bool OpenWindow(string windowCode) => OpenWindow(windowCode, (Action<IAbstWindow>?)null);

    public bool OpenWindow<TWindow>(string windowCode, Action<TWindow>? actionOnOpen = null)
        where TWindow : class, IAbstWindow
        => OpenWindow(
            windowCode,
            actionOnOpen is null
                ? null
                : window =>
                {
                    if (window is not TWindow typedWindow)
                    {
                        throw new InvalidOperationException($"Window '{windowCode}' is not of type {typeof(TWindow).FullName}.");
                    }

                    actionOnOpen(typedWindow);
                });

    private bool OpenWindow(string windowCode, Action<IAbstWindow>? actionOnOpen)
    {
        if (!_windowFactory.TryGetRegistration(windowCode, out var registration)) return false;
        var instance = registration.Instance;
        actionOnOpen?.Invoke(instance);
        instance.OpenWindow();
        SetActiveWindow(registration);
        WindowOpened?.Invoke(registration.Instance);
        return true;
    }

    public IEnumerable<IAbstWindow> GetWindows() => _windowFactory.GetWindows();
    public IEnumerable<(string Code, ARect Rect)> GetRects() => _windowFactory.GetRects();

    public bool CloseWindow(string windowCode)
    {
        if (!_windowFactory.TryGetRegistration(windowCode, out var registration)) return false;
        registration.Instance.CloseWindow();
        WindowClosed?.Invoke(registration.Instance);
        return true;
    }

    

    public bool CanExecute(OpenWindowCommand command) => _windowFactory.WindowExists(command.WindowCode);
    public bool CanExecute(CloseWindowCommand command) => _windowFactory.WindowExists(command.WindowCode);

    public bool Handle(OpenWindowCommand command) => OpenWindow(command.WindowCode);
    public bool Handle(CloseWindowCommand command) => CloseWindow(command.WindowCode);

    public void SetWindowSize(string windowCode, int width, int height)
    {
        if (!_windowFactory.TryGetRegistration(windowCode, out var registration)) return;
        registration.Instance.SetSize(width, height);
    }

   
}

