using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Primitives;
using BlingoEngine.Movies;

namespace BlingoEngine.Director.Core.Movies.Commands;

public sealed record BlingoUpdateMoviePropertiesCommand : BlingoMovieBaseCommand
{
    public BlingoUpdateMoviePropertiesCommand(BlingoMovieRef movieReference, IReadOnlyList<APropertyValue> changes)
        : base(movieReference)
    {
        Changes = changes?.ToArray() ?? Array.Empty<APropertyValue>();
    }

    public IReadOnlyList<APropertyValue> Changes { get; }
}
