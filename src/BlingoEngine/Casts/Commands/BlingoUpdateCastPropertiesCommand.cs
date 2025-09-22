using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Primitives;
using BlingoEngine.Casts;

namespace BlingoEngine.Casts.Commands;

public sealed record BlingoUpdateCastPropertiesCommand : BlingoCastBaseCommand
{
    public BlingoUpdateCastPropertiesCommand(BlingoCastRef castReference, IReadOnlyList<APropertyValue> changes)
        : base(castReference)
    {
        Changes = changes?.ToArray() ?? Array.Empty<APropertyValue>();
    }

    public IReadOnlyList<APropertyValue> Changes { get; }
}
