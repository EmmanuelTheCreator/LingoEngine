using System;
using System.Collections.Generic;
using System.Linq;
using LingoEngine.Movies;
using LingoEngine.Commands;

namespace LingoEngine.Director.Core.Stages
{
    public sealed record RotateSpritesCommand(
        IReadOnlyDictionary<LingoSprite, float> StartRotations,
        IReadOnlyDictionary<LingoSprite, float> EndRotations) : ILingoCommand
    {
        public Action ToUndo(Action updateSelectionBox)
        {
            var undo = StartRotations.ToDictionary(kv => kv.Key, kv => kv.Value);
            return () =>
            {
                foreach (var kv in undo)
                    kv.Key.Rotation = kv.Value;
                updateSelectionBox();
            };
        }

        public Action ToRedo(Action updateSelectionBox)
        {
            var redo = EndRotations.ToDictionary(kv => kv.Key, kv => kv.Value);
            return () =>
            {
                foreach (var kv in redo)
                    kv.Key.Rotation = kv.Value;
                updateSelectionBox();
            };
        }
    }
}
