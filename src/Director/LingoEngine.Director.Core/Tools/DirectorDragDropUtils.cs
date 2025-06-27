using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Primitives;
using System;

namespace LingoEngine.Director.Core.Tools
{

    public static class DirectorDragDropUtils
    {
        public static bool TryHandleMemberDrop(
                                LingoMovie? movie,
                                LingoPoint pos,
                                float channelHeight,
                                float frameWidth,
                                float leftMargin,
                                int defaultSpriteLength,
                                out int channel,
                                out int startFrame,
                                out int endFrame,
                                out ILingoMember? member
                            )
        {
            channel = -1;
            startFrame = 0;
            endFrame = 0;
            member = null;

            if (movie == null || !DirectorDragDropHolder.IsDragging || DirectorDragDropHolder.Member == null)
                return false;

            member = DirectorDragDropHolder.Member;
            if (member.Type == LingoMemberType.Sound)
                return false;

            channel = (int)(pos.Y / channelHeight);
            if (channel < 0 || channel >= movie.MaxSpriteChannelCount)
                return false;

            startFrame = LingoMath.Clamp(
                LingoMath.RoundToInt((pos.X - leftMargin) / frameWidth) + 1,
                1,
                movie.FrameCount
            );

            // Default to 10 frames if no next label/sprite
            endFrame = movie.GetNextLabelFrame(startFrame) - 1;
            if (endFrame < startFrame)
                endFrame = startFrame + 10;

            int nextSprite = movie.GetNextSpriteStart(channel, startFrame);
            if (nextSprite != -1)
                endFrame = Math.Min(endFrame, nextSprite - 1);

            endFrame = LingoMath.Clamp(endFrame, startFrame, movie.FrameCount);

            // NEW: collision detection
            int idx = 1;
            while (movie.TryGetAllTimeSprite(idx, out var sprite))
            {
                if (sprite.SpriteNum - 1 == channel)
                {
                    if (RangesOverlap(startFrame, endFrame, sprite.BeginFrame, sprite.EndFrame))
                        return false;
                }
                idx++;
            }

            return true;
        }

        private static bool RangesOverlap(int aStart, int aEnd, int bStart, int bEnd)
        {
            return aStart <= bEnd && bStart <= aEnd;
        }

    }

}





