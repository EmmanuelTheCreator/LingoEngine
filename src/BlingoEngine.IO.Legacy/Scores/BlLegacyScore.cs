using System;
using System.Collections.Generic;
using BlingoEngine.IO.Legacy.Scores.Datas;

namespace BlingoEngine.IO.Legacy.Scores;

internal sealed class BlLegacyScore
{
    public BlLegacyScore(IReadOnlyList<BlSpriteRawData> sprites, IReadOnlyList<BlScoreRawFrame> frames)
    {
        Sprites = sprites ?? Array.Empty<BlSpriteRawData>();
        Frames = frames ?? Array.Empty<BlScoreRawFrame>();
    }

    public IReadOnlyList<BlSpriteRawData> Sprites { get; }
    public IReadOnlyList<BlScoreRawFrame> Frames { get; }
}
