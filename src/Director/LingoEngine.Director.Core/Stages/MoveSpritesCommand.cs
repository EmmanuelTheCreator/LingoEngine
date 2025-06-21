using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using LingoEngine.Movies;
using LingoEngine.Commands;

namespace LingoEngine.Director.Core.Stages
{
    public sealed record MoveSpritesCommand(
        IReadOnlyDictionary<LingoSprite, Vector2> StartPositions,
        IReadOnlyDictionary<LingoSprite, Vector2> EndPositions) : ILingoCommand
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
    }
}
