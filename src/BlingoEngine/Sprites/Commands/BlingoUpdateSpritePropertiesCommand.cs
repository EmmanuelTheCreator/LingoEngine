using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Primitives;
using BlingoEngine.Sprites;

namespace BlingoEngine.Sprites.Commands;

public sealed record BlingoUpdateSpritePropertiesCommand : BlingoSpriteBaseCommand
{
    public BlingoUpdateSpritePropertiesCommand(BlingoSpriteRef sprite, IReadOnlyList<APropertyValue> changes)
        : base(sprite)
    {
        Changes = changes?.ToArray() ?? Array.Empty<APropertyValue>();
    }

    public IReadOnlyList<APropertyValue> Changes { get; }
}
