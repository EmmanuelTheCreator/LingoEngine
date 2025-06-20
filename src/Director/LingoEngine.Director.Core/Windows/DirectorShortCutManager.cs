
using LingoEngine.Commands;
using LingoEngine.Core;

namespace LingoEngine.Director.Core.Menus
{
    public interface IDirectorShortCutManager
    {
        bool Execute(string keyCombination);
        DirectorShortCutMap CreateShortCut(string name, string keyCombination, Func<DirectorShortCutMap, ILingoCommand> command, string? description = null);
    }
    public class DirectorShortCutManager : IDirectorShortCutManager
    {
        private Dictionary<string, DirectorShortCutMap> _shortCuts = new();
        private readonly ILingoCommandManager _lingoCommandManager;

        public DirectorShortCutManager(ILingoCommandManager lingoCommandManager)
        {
            _lingoCommandManager = lingoCommandManager;
        }

        public DirectorShortCutMap CreateShortCut(string name, string keyCombination,Func<DirectorShortCutMap, ILingoCommand> command, string? description = null)
        {
            // Implementation for creating a shortcut
            var shortcut = new DirectorShortCutMap(name, command, description, keyCombination);
            _shortCuts[name] = shortcut;
            return shortcut;
        }

        public bool Execute(string keyCombination)
        {
            // Implementation for calling a shortcut by its key combination
            foreach (var shortcut in _shortCuts.Values)
            {
                if (shortcut.KeyCombination == keyCombination)
                {
                    // Execute the action associated with this shortcut
                    return _lingoCommandManager.Handle(shortcut.GetCommand());
                }
            }
            throw new KeyNotFoundException($"Shortcut with key combination '{keyCombination}' not found.");
        }
    }
}
