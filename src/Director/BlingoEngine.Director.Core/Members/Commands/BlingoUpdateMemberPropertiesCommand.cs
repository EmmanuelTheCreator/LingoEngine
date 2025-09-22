using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Primitives;
using BlingoEngine.Members;

namespace BlingoEngine.Director.Core.Members.Commands;

public sealed record BlingoUpdateMemberPropertiesCommand : BlingoMemberBaseCommand
{
    public BlingoUpdateMemberPropertiesCommand(BlingoMemberRef memberReference, IReadOnlyList<APropertyValue> changes)
        : base(memberReference)
    {
        Changes = changes?.ToArray() ?? Array.Empty<APropertyValue>();
    }

    public IReadOnlyList<APropertyValue> Changes { get; }
}
