using LingoEngine.Commands;
using LingoEngine.Primitives;
using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Stages.Commands
{
    public sealed record MoveSpritesCommand(
        IReadOnlyDictionary<LingoSprite, LingoPoint> StartPositions,
        IReadOnlyDictionary<LingoSprite, LingoPoint> EndPositions) : ILingoCommand
    {
        public Action ToUndo(Action updateSelectionBox)
        {
            var undo = StartPositions.ToDictionary(kv => kv.Key, kv => kv.Value);
            return () =>
            {
                foreach (var kv in undo)
                {
                    kv.Key.LocH = kv.Value.X;
                    kv.Key.LocV = kv.Value.Y;
                }
                updateSelectionBox();
            };
        }

        public Action ToRedo(Action updateSelectionBox)
        {
            var redo = EndPositions.ToDictionary(kv => kv.Key, kv => kv.Value);
            return () =>
            {
                foreach (var kv in redo)
                {
                    kv.Key.LocH = kv.Value.X;
                    kv.Key.LocV = kv.Value.Y;
                }
                updateSelectionBox();
            };
        }
    }
}
