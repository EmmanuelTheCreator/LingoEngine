using LingoEngine.Commands;
using System;

namespace LingoEngine.Director.Core.Tools
{
    public interface IDirectorShortCutManager
    {
        event Action<DirectorShortCutMap>? ShortCutAdded;
        event Action<DirectorShortCutMap>? ShortCutRemoved;
        bool Execute(string keyCombination);
        DirectorShortCutMap CreateShortCut(string name, string keyCombination, Func<DirectorShortCutMap, ILingoCommand> command, string? description = null);
        void RemoveShortCut(string name);
        IEnumerable<DirectorShortCutMap> GetShortCuts();
    }
    public class DirectorShortCutManager : IDirectorShortCutManager
    {
        private readonly Dictionary<string, DirectorShortCutMap> _shortCuts = new();
        private readonly ILingoCommandManager _lingoCommandManager;

        public event Action<DirectorShortCutMap>? ShortCutAdded;
        public event Action<DirectorShortCutMap>? ShortCutRemoved;

        public DirectorShortCutManager(ILingoCommandManager lingoCommandManager)
        {
            _lingoCommandManager = lingoCommandManager;
        }

        public DirectorShortCutMap CreateShortCut(string name, string keyCombination, Func<DirectorShortCutMap, ILingoCommand> command, string? description = null)
        {
            var shortcut = new DirectorShortCutMap(name, command, description, keyCombination);
            _shortCuts[name] = shortcut;
            ShortCutAdded?.Invoke(shortcut);
            return shortcut;
        }

        public void RemoveShortCut(string name)
        {
            if (_shortCuts.Remove(name, out var map))
                ShortCutRemoved?.Invoke(map);
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

        public IEnumerable<DirectorShortCutMap> GetShortCuts() => _shortCuts.Values;
    }
}
