using LingoEngine.Commands;

namespace LingoEngine.Director.Core.Menus
{
    public class DirectorShortCutMap
    {
        private readonly Func<DirectorShortCutMap, ILingoCommand> commandCtor;

        public string ShortCut { get; set; }
        public string? Description { get; set; }
        public string KeyCombination { get; }

        public DirectorShortCutMap(string shortCut, Func<DirectorShortCutMap,Commands.ILingoCommand> commandCtor, string? description = null, string keyCombination = null)
        {
            ShortCut = shortCut;
            this.commandCtor = commandCtor;
            Description = description;
            KeyCombination = keyCombination;
        }

        public ILingoCommand GetCommand() => commandCtor(this);
    }
}
