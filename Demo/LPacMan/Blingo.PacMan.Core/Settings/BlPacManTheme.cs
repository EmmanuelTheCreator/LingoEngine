using AbstUI.Primitives;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Sprites.ParentScripts;
using System;
using System.Collections.Generic;

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
        public static IReadOnlyDictionary<PMCharacterAnimationType, (ARect[] Frames, int FrameDelay)> Animations { get; } = BuildAnimations();

        private static IReadOnlyDictionary<PMCharacterAnimationType, (ARect[] Frames, int FrameDelay)> BuildAnimations()
        {
            var animations = new Dictionary<PMCharacterAnimationType, (ARect[] Frames, int FrameDelay)>()
            {
                [PMCharacterAnimationType.PacManLeft] = (CreateLoop(0), DefaultFrameDelay),
                [PMCharacterAnimationType.PacManRight] = (CreateLoop(0), DefaultFrameDelay),
                [PMCharacterAnimationType.PacManUp] = (CreateLoop(0), DefaultFrameDelay),
                [PMCharacterAnimationType.PacManDown] = (CreateLoop(0), DefaultFrameDelay),
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
        private const int FrightenedRowIndex = 5;
        private const int FrightenedBlueFirstColumn = 0;
        private const int FrightenedBlueSecondColumn = 2;
        private const int FrightenedWhiteFirstColumn = 1;
        private const int FrightenedWhiteSecondColumn = 3;
        private const int DefaultFrightenedFrameDelay = 6;
        public const int FrightenedFlashWindowFrames = 12;
        private const int DefaultFrameDelay = 2;
        private const int ScoreRowOffset = 110;
        private const int ScoreHorizontalAdjustment = -2;

        public static IReadOnlyDictionary<PMCharacterAnimationType, (ARect[] Frames, int FrameDelay)> Animations { get; } = BuildAnimations();

        private static IReadOnlyDictionary<PMCharacterAnimationType, (ARect[] Frames, int FrameDelay)> BuildAnimations()
        {
            var animations = new Dictionary<PMCharacterAnimationType, (ARect[] Frames, int FrameDelay)>()
            {
                [PMCharacterAnimationType.GhostLeft] = (CreateLoop(0, 0), DefaultFrameDelay),
                [PMCharacterAnimationType.GhostRight] = (CreateLoop(1, 0), DefaultFrameDelay),
                [PMCharacterAnimationType.GhostUp] = (CreateLoop(2, 0), DefaultFrameDelay),
                [PMCharacterAnimationType.GhostDown] = (CreateLoop(3, 0), DefaultFrameDelay),
            };

            return animations;
        }
        private static ARect[] CreateLoop(int ghostIndex,int startColumn)
        {
            return new[]
            {
                CreateFrame(ghostIndex,startColumn),
            };
        }
        private static ARect CreateFrame(int ghostIndex, int column)
        {
            var left = column * SpriteSize;
            var top = (ghostIndex +1) * SpriteSize;
            return new ARect(left, top, left + SpriteSize, top + SpriteSize);
        }

        public static IReadOnlyDictionary<MrGhost, ARect> Sprites { get; } = new Dictionary<MrGhost, ARect>
        {
            [MrGhost.Blinky] = ARect.New(SpriteSize * 0, SpriteSize * 1, SpriteSize, SpriteSize),
            [MrGhost.Pinky] = ARect.New(SpriteSize * 1, SpriteSize * 2, SpriteSize, SpriteSize),
            [MrGhost.Inky] = ARect.New(SpriteSize * 2, SpriteSize * 3, SpriteSize, SpriteSize),
            [MrGhost.Sue] = ARect.New(SpriteSize * 3, SpriteSize * 4, SpriteSize, SpriteSize),
        };

        public static IReadOnlyDictionary<MrGhost, float> HorizontalOffsets { get; } = new Dictionary<MrGhost, float>
        {
            [MrGhost.Blinky] = -Tiles.Size,
            [MrGhost.Pinky] = -Tiles.Size / 2f,
            [MrGhost.Inky] = Tiles.Size / 2f,
            [MrGhost.Sue] = Tiles.Size,
        };

        public static IReadOnlyDictionary<PMCharacterAnimationType, (ARect[] Frames, int FrameDelay)> FrightenedAnimations { get; }
            = BuildFrightenedAnimations();

        public static IReadOnlyDictionary<PMCharacterAnimationType, (ARect[] Frames, int FrameDelay)> ScoreAnimations { get; }
            = BuildScoreAnimations();

        private static IReadOnlyDictionary<PMCharacterAnimationType, (ARect[] Frames, int FrameDelay)> BuildFrightenedAnimations()
        {
            var blueFrames = new[]
            {
                CreateFrightenedFrame(FrightenedBlueFirstColumn),
                CreateFrightenedFrame(FrightenedBlueSecondColumn),
            };

            var flashFrames = new[]
            {
                CreateFrightenedFrame(FrightenedBlueFirstColumn),
                CreateFrightenedFrame(FrightenedWhiteFirstColumn),
                CreateFrightenedFrame(FrightenedBlueSecondColumn),
                CreateFrightenedFrame(FrightenedWhiteSecondColumn),
            };

            return new Dictionary<PMCharacterAnimationType, (ARect[] Frames, int FrameDelay)>()
            {
                [PMCharacterAnimationType.GhostFrightenedBlue] = (blueFrames, DefaultFrightenedFrameDelay),
                [PMCharacterAnimationType.GhostFrightenedWhite] = (flashFrames, DefaultFrightenedFrameDelay),
            };
        }

        private static ARect CreateFrightenedFrame(int column)
        {
            var left = column * SpriteSize;
            var top = FrightenedRowIndex * SpriteSize;
            return new ARect(left, top, left + SpriteSize, top + SpriteSize);
        }

        private static IReadOnlyDictionary<PMCharacterAnimationType, (ARect[] Frames, int FrameDelay)> BuildScoreAnimations()
        {
            return new Dictionary<PMCharacterAnimationType, (ARect[] Frames, int FrameDelay)>
            {
                [PMCharacterAnimationType.GhostScore200] = (new[] { CreateScoreFrame(0, ScoreHorizontalAdjustment) }, 0),
                [PMCharacterAnimationType.GhostScore400] = (new[] { CreateScoreFrame(1, ScoreHorizontalAdjustment) }, 0),
                [PMCharacterAnimationType.GhostScore800] = (new[] { CreateScoreFrame(2, ScoreHorizontalAdjustment) }, 0),
                [PMCharacterAnimationType.GhostScore1600] = (new[] { CreateScoreFrame(3, 0) }, 0),
            };
        }

        private static ARect CreateScoreFrame(int column, int adjustment)
        {
            var left = column * SpriteSize + adjustment;
            var top = ScoreRowOffset;
            return new ARect(left, top, left + SpriteSize, top + SpriteSize);
        }
    }

    /// <summary>
    /// Frames and offsets for the roaming bonus fruit and score popups.
    /// </summary>
    public static class Bonus
    {
        public const float VerticalOffset = -14f;

        public static int FrameSize => Actor.SpriteSize - 2;

        public static IReadOnlyDictionary<PMCharacterAnimationType, ARect[]> Animations { get; } = BuildAnimations();

        public static ARect DefaultFrame => Animations[PMCharacterAnimationType.BonusDefault][0];

        private static IReadOnlyDictionary<PMCharacterAnimationType, ARect[]> BuildAnimations()
        {
            var animations = new Dictionary<PMCharacterAnimationType, ARect[]>()
            {
                [PMCharacterAnimationType.BonusDefault] = [ CreateFrame(0, 0) ],
                [PMCharacterAnimationType.BonusScore100] = [ CreateFrame(0, FrameSize) ],
                [PMCharacterAnimationType.BonusScore200] = [ CreateFrame(FrameSize * 1, FrameSize) ],
                [PMCharacterAnimationType.BonusScore500] = [ CreateFrame(FrameSize * 2, FrameSize) ],
                [PMCharacterAnimationType.BonusScore700] = [ CreateFrame(FrameSize * 3, FrameSize) ],
                [PMCharacterAnimationType.BonusScore1000] = [ CreateFrame(FrameSize * 4, FrameSize) ],
                [PMCharacterAnimationType.BonusScore2000] = [ CreateFrame(FrameSize * 5, FrameSize) ],
                [PMCharacterAnimationType.BonusScore5000] = [ CreateFrame(FrameSize * 6, FrameSize) ],
            };

            return animations;
        }

        private static ARect CreateFrame(int offsetX, int offsetY)
        {
            return new ARect(offsetX, offsetY, offsetX + FrameSize, offsetY + FrameSize);
        }
    }
}
