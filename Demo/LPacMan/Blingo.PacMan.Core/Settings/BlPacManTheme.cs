using System;
using System.Collections.Generic;
using AbstUI.Primitives;
using Blingo.PacMan.Core.Game;

namespace Blingo.PacMan.Core.Settings;

/// <summary>
/// Centralises theme-specific geometry and animation declarations so swapping this file swaps the visual theme.
/// </summary>
public static class BlPacManTheme
{
    /// <summary>
    /// Stage dimensions shared by every front-end.
    /// </summary>
    public static class Stage
    {
        public const int Width = 224;
        public const int Height = 288;
    }

    /// <summary>
    /// Tile metrics used to derive sprite placement and movement distances.
    /// </summary>
    public static class Tiles
    {
        public const int Size = 8;
        public const float VerticalCenterOffset = Size / 8f;
    }

    /// <summary>
    /// Animation frames and sprite information for Pac-Man himself.
    /// </summary>
    public static class Actor
    {
        public const int SpriteSize = Tiles.Size * 2;
        public const int SpriteSheetY = 0;
        private const int DefaultFrameDelay = 2;

        /// <summary>
        /// Animation loops keyed by their behaviour label.
        /// </summary>
        public static IReadOnlyDictionary<string, (ARect[] Frames, int FrameDelay)> Animations { get; } = BuildAnimations();

        private static IReadOnlyDictionary<string, (ARect[] Frames, int FrameDelay)> BuildAnimations()
        {
            var animations = new Dictionary<string, (ARect[] Frames, int FrameDelay)>(StringComparer.OrdinalIgnoreCase)
            {
                ["left"] = (CreateLoop(0), DefaultFrameDelay),
                ["right"] = (CreateLoop(0), DefaultFrameDelay),
                ["up"] = (CreateLoop(0), DefaultFrameDelay),
                ["down"] = (CreateLoop(0), DefaultFrameDelay),
            };

            return animations;
        }

        private static ARect[] CreateLoop(int startColumn)
        {
            return new[]
            {
                CreateFrame(startColumn + 0),
                CreateFrame(startColumn + 1),
                CreateFrame(startColumn + 2),
                CreateFrame(startColumn + 1),
            };
        }

        private static ARect CreateFrame(int column)
        {
            var left = column * SpriteSize;
            var top = SpriteSheetY;
            return new ARect(left, top, left + SpriteSize, top + SpriteSize);
        }
    }

    /// <summary>
    /// Ghost sprite sheet coordinates and spawn offsets.
    /// </summary>
    public static class Ghosts
    {
        public const int SpriteSize = Tiles.Size * 2;

        public static IReadOnlyDictionary<MrGhost, ARect> Sprites { get; } = new Dictionary<MrGhost, ARect>
        {
            [MrGhost.Blinky] = new ARect(SpriteSize * 0, 0, SpriteSize * 1, SpriteSize),
            [MrGhost.Pinky] = new ARect(SpriteSize * 1, 0, SpriteSize * 2, SpriteSize),
            [MrGhost.Inky] = new ARect(SpriteSize * 2, 0, SpriteSize * 3, SpriteSize),
            [MrGhost.Clyde] = new ARect(SpriteSize * 3, 0, SpriteSize * 4, SpriteSize),
        };

        public static IReadOnlyDictionary<MrGhost, float> HorizontalOffsets { get; } = new Dictionary<MrGhost, float>
        {
            [MrGhost.Blinky] = -Tiles.Size,
            [MrGhost.Pinky] = -Tiles.Size / 2f,
            [MrGhost.Inky] = Tiles.Size / 2f,
            [MrGhost.Clyde] = Tiles.Size,
        };
    }

    /// <summary>
    /// Frames and offsets for the roaming bonus fruit and score popups.
    /// </summary>
    public static class Bonus
    {
        public const float VerticalOffset = -14f;

        public static int FrameSize => Actor.SpriteSize - 2;

        public static IReadOnlyDictionary<string, ARect[]> Animations { get; } = BuildAnimations();

        public static ARect DefaultFrame => Animations["default"][0];

        private static IReadOnlyDictionary<string, ARect[]> BuildAnimations()
        {
            var animations = new Dictionary<string, ARect[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["default"] = new[] { CreateFrame(0, 0) },
                ["score100"] = new[] { CreateFrame(0, FrameSize) },
                ["score200"] = new[] { CreateFrame(FrameSize * 1, FrameSize) },
                ["score500"] = new[] { CreateFrame(FrameSize * 2, FrameSize) },
                ["score700"] = new[] { CreateFrame(FrameSize * 3, FrameSize) },
                ["score1000"] = new[] { CreateFrame(FrameSize * 4, FrameSize) },
                ["score2000"] = new[] { CreateFrame(FrameSize * 5, FrameSize) },
                ["score5000"] = new[] { CreateFrame(FrameSize * 6, FrameSize) },
            };

            return animations;
        }

        private static ARect CreateFrame(int offsetX, int offsetY)
        {
            return new ARect(offsetX, offsetY, offsetX + FrameSize, offsetY + FrameSize);
        }
    }
}
