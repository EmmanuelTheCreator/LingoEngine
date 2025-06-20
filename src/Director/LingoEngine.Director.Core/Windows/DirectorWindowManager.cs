using LingoEngine.Commands;
using LingoEngine.Core;
using LingoEngine.Director.Core.Projects;

namespace LingoEngine.Director.Core.Menus
{
    public interface IDirectorWindowManager
    {
        IDirectorWindowManager Register<TWindow>(string windowCode, Func<IServiceProvider, IDirectorWindow> constructor, DirectorShortCutMap? shortCutMap = null)
             where TWindow : IDirectorWindow;
        IDirectorWindowManager Register<TWindow>(string windowCode, DirectorShortCutMap? shortCutMap = null)
             where TWindow : IDirectorWindow, new();
        bool OpenWindow(string windowCode);
        bool CloseWindow(string windowCode);
    }
    public class DirectorWindowManager : IDirectorWindowManager,
        ICommandHandler<OpenWindowCommand>,
        ICommandHandler<CloseWindowCommand>,
        ICommandHandler<ExecuteShortCutCommand>
    {
        private readonly Dictionary<string, WindowRegistration> _windowRegistrations = new();
        private readonly IServiceProvider _serviceProvider;

        public DirectorWindowManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IDirectorWindowManager Register<TWindow>(string windowCode, Func<IServiceProvider, IDirectorWindow> constructor, DirectorShortCutMap? shortCutMap = null)
            where TWindow : IDirectorWindow
        {
            if (_windowRegistrations.ContainsKey(windowCode))
                throw new InvalidOperationException($"Window with code '{windowCode}' is already registered.");

            _windowRegistrations[windowCode] = new WindowRegistration(windowCode, () =>
            {
                var instance = constructor(_serviceProvider);
                return instance;
            }, shortCutMap);

            return this;
        }
        public IDirectorWindowManager Register<TWindow>(string windowCode, DirectorShortCutMap? shortCutMap = null) 
            where TWindow : IDirectorWindow, new()
        {
            if (_windowRegistrations.ContainsKey(windowCode))
                throw new InvalidOperationException($"Window with code '{windowCode}' is already registered.");

            _windowRegistrations[windowCode] = new WindowRegistration(windowCode, () =>
            {
                var instance = new TWindow();
                return instance;
            }, shortCutMap);

            return this;
        }




        public bool OpenWindow(string windowCode)
        {
            if (!_windowRegistrations.TryGetValue(windowCode, out var registration)) return false;
            registration.Instance.OpenWindow();
            return true;
        } 
        public bool CloseWindow(string windowCode)
        {
            if (!_windowRegistrations.TryGetValue(windowCode, out var registration)) return false;
            registration.Instance.CloseWindow();
            return true;
        }

        public bool WindowExists(string windowCode) => _windowRegistrations.ContainsKey(windowCode);
        public bool WindowExists(DirectorShortCutMap shortCutMap) => _windowRegistrations.Values.Any(X => X.ShortCutMap == shortCutMap);

        public bool CanExecute(OpenWindowCommand command) => WindowExists(command.WindowCode);
        public bool CanExecute(CloseWindowCommand command) => WindowExists(command.WindowCode);
        public bool CanExecute(ExecuteShortCutCommand command) => WindowExists(command.ShortCut);

        public bool Handle(OpenWindowCommand command) => OpenWindow(command.WindowCode);
        public bool Handle(CloseWindowCommand command) => CloseWindow(command.WindowCode);
        public bool Handle(ExecuteShortCutCommand command)
        {
            var registration = _windowRegistrations.Values.First(x => x.ShortCutMap == command.ShortCut);
            registration.Instance.CloseWindow();
            return true;
        }

      

        private class WindowRegistration
        {
            private Func<IDirectorWindow> _constructor;
            private IDirectorWindow? _instance;
            public string WindowCode { get; }
            public DirectorShortCutMap? ShortCutMap { get; }
            public WindowRegistration(string windowCode, Func<IDirectorWindow> constructor, DirectorShortCutMap? shortCutMap)
            {
                WindowCode = windowCode;
                _constructor = constructor;
                ShortCutMap = shortCutMap;
            }
            public IDirectorWindow Instance
            {
                get
                {
                    if (_instance == null)
                        _instance = _constructor();
                    
                    return _instance;
                }
            }
        }
    }
}
