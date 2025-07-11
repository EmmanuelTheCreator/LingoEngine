﻿using LingoEngine.Commands;

namespace LingoEngine.Director.Core.Bitmaps.Commands
{
    /// <summary>
    /// Command to flood fill starting at the given canvas coordinate.
    /// </summary>
    public sealed record PainterFillCommand(int X, int Y) : ILingoCommand;
}
