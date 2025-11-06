using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.IO.Legacy.Scores;
using BlingoEngine.IO.Legacy.Scores.Datas;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Director;

internal static class BlLegacyScoreSpriteBuilder
{
    public static List<Blingo2DSpriteDTO> Build(BlLegacyScore? score)
    {
        if (score is null)
            return new List<Blingo2DSpriteDTO>();

        var channelLookup = BuildChannelLookup(score.Frames);
        var sprites = new List<Blingo2DSpriteDTO>();

        foreach (var spriteData in score.Sprites)
        {
            if (!channelLookup.TryGetValue(spriteData.Channel, out var framesByNumber))
                continue;

            var sprite = BuildSpriteDto(spriteData, framesByNumber);
            if (sprite is not null)
                sprites.Add(sprite);
        }

        return sprites;
    }

    private static Dictionary<int, Dictionary<int, List<BlScoreToken>>> BuildChannelLookup(IReadOnlyList<BlScoreRawFrame> frames)
    {
        var lookup = new Dictionary<int, Dictionary<int, List<BlScoreToken>>>();

        foreach (var frame in frames)
        {
            foreach (var token in frame.Tokens)
            {
                if (!lookup.TryGetValue(token.Channel, out var frameMap))
                {
                    frameMap = new Dictionary<int, List<BlScoreToken>>();
                    lookup[token.Channel] = frameMap;
                }

                if (!frameMap.TryGetValue(frame.FrameNum, out var list))
                {
                    list = new List<BlScoreToken>();
                    frameMap[frame.FrameNum] = list;
                }

                list.Add(token);
            }
        }

        return lookup;
    }

    private static Blingo2DSpriteDTO? BuildSpriteDto(BlSpriteRawData spriteData, Dictionary<int, List<BlScoreToken>> framesByNumber)
    {
        if (!TryReadSpriteDefaults(spriteData, framesByNumber))
            return null;

        var startFrame = spriteData.StartFrame;
        var state = SpriteState.FromSprite(spriteData);

        if (framesByNumber.TryGetValue(startFrame, out var baseTokens))
        {
            foreach (var token in baseTokens)
                ApplyTokenToState(state, token);
        }

        var dto = CreateSpriteDto(spriteData, state);
        var animator = new BlingoSpriteAnimatorDTO();

        ApplyTweenOptions(spriteData, animator);

        var keyFrames = BuildFrameList(spriteData, framesByNumber);
        var previousState = state;

        foreach (var frame in keyFrames)
        {
            if (!framesByNumber.TryGetValue(frame, out var tokens))
                continue;

            var nextState = previousState.Clone();
            foreach (var token in tokens)
                ApplyTokenToState(nextState, token);

            AddKeyFrames(animator, previousState, nextState, frame);
            previousState = nextState;
        }

        if (HasAnimation(animator))
            dto.Animator = animator;

        return dto;
    }

    private static bool TryReadSpriteDefaults(BlSpriteRawData sprite, Dictionary<int, List<BlScoreToken>> framesByNumber)
    {
        BlScoreToken? initialToken = null;
        if (framesByNumber.TryGetValue(sprite.StartFrame, out var startTokens))
            initialToken = startTokens.FirstOrDefault(t => t.Payload.Length >= 0x30);

        if (initialToken is null)
            initialToken = framesByNumber.Values.SelectMany(v => v).FirstOrDefault(t => t.Payload.Length >= 0x30);

        if (initialToken is null)
            return false;

        using var memory = new MemoryStream(initialToken.Payload);
        var reader = new BlStreamReader(memory);
        sprite.ReadKeyFrame(reader);
        return true;
    }

    private static List<int> BuildFrameList(BlSpriteRawData sprite, Dictionary<int, List<BlScoreToken>> framesByNumber)
    {
        var frameSet = new SortedSet<int>();
        foreach (var offset in sprite.KeyFrameOffsets)
        {
            var frame = sprite.StartFrame + offset;
            if (frame <= sprite.EndFrame)
                frameSet.Add(frame);
        }

        frameSet.RemoveWhere(frame => frame <= sprite.StartFrame);
        return frameSet.ToList();
    }

    private static Blingo2DSpriteDTO CreateSpriteDto(BlSpriteRawData sprite, SpriteState state)
    {
        return new Blingo2DSpriteDTO
        {
            SpriteNum = sprite.Channel,
            Member = sprite.MemberNum > 0 || sprite.MemberCastLib > 0 ? new BlingoMemberRefDTO(sprite.MemberNum, sprite.MemberCastLib) : null,
            DisplayMember = sprite.MemberNum,
            SpritePropertiesOffset = sprite.SpritePropertiesOffset,
            Visibility = true,
            LocH = state.LocH,
            LocV = state.LocV,
            LocZ = state.LocZ,
            Rotation = state.Rotation,
            Skew = state.Skew,
            RegPoint = new BlingoPointDTO(),
            Ink = state.Ink,
            ForeColor = new BlingoColorDTO(state.ForeR, state.ForeG, state.ForeB),
            BackColor = new BlingoColorDTO(state.BackR, state.BackG, state.BackB),
            Blend = state.Blend,
            Editable = state.Editable,
            FlipH = state.FlipH,
            FlipV = state.FlipV,
            ScoreColor = state.ScoreColor,
            Width = state.Width,
            Height = state.Height,
            BeginFrame = sprite.StartFrame,
            EndFrame = sprite.EndFrame,
            Lock = sprite.IsLocked
        };
    }

    private static void ApplyTweenOptions(BlSpriteRawData sprite, BlingoSpriteAnimatorDTO animator)
    {
        animator.PositionOptions.Enabled = sprite.TweenFlags.TweeningEnabled && sprite.TweenFlags.Path;
        animator.RotationOptions.Enabled = sprite.TweenFlags.TweeningEnabled && sprite.TweenFlags.Rotation;
        animator.SkewOptions.Enabled = sprite.TweenFlags.TweeningEnabled && sprite.TweenFlags.Skew;
        animator.BlendOptions.Enabled = sprite.TweenFlags.TweeningEnabled && sprite.TweenFlags.Blend;
        animator.ForegroundColorOptions.Enabled = sprite.TweenFlags.TweeningEnabled && sprite.TweenFlags.ForeColor;
        animator.BackgroundColorOptions.Enabled = sprite.TweenFlags.TweeningEnabled && sprite.TweenFlags.BackColor;
    }

    private static void ApplyTokenToState(SpriteState state, BlScoreToken token)
    {
        foreach (var property in token.Properties)
        {
            switch (property.Property)
            {
                case BlSpriteRawData.BlSpriteRawProperty.LocH:
                    state.LocH = (short)property.Value;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.LocV:
                    state.LocV = (short)property.Value;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.Rotation:
                    state.Rotation = property.Value / 100f;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.Skew:
                    state.Skew = property.Value / 100f;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.Blend:
                    state.Blend = ConvertBlend(property.Value);
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.ForeColorR:
                    state.ForeR = (byte)property.Value;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.ForeColorG:
                    state.ForeG = (byte)property.Value;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.ForeColorB:
                    state.ForeB = (byte)property.Value;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.BackColorR:
                    state.BackR = (byte)property.Value;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.BackColorG:
                    state.BackG = (byte)property.Value;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.BackColorB:
                    state.BackB = (byte)property.Value;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.ScoreColor:
                    state.ScoreColor = property.Value;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.Width:
                    state.Width = (short)property.Value;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.Height:
                    state.Height = (short)property.Value;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.Ink:
                    state.Ink = property.Value;
                    break;
                case BlSpriteRawData.BlSpriteRawProperty.FlipFlags:
                    var flags = (byte)property.Value;
                    state.FlipH = (flags & 0x02) != 0;
                    state.FlipV = (flags & 0x04) != 0;
                    break;
            }
        }
    }

    private static void AddKeyFrames(BlingoSpriteAnimatorDTO animator, SpriteState previous, SpriteState next, int frame)
    {
        if (previous.LocH != next.LocH || previous.LocV != next.LocV)
        {
            animator.Position.Add(new BlingoPointKeyFrameDTO
            {
                Frame = frame,
                Value = new BlingoPointDTO { X = next.LocH, Y = next.LocV },
                Ease = BlingoEaseTypeDTO.Linear
            });
        }

        if (Math.Abs(previous.Rotation - next.Rotation) > float.Epsilon)
        {
            animator.Rotation.Add(new BlingoFloatKeyFrameDTO
            {
                Frame = frame,
                Value = next.Rotation,
                Ease = BlingoEaseTypeDTO.Linear
            });
        }

        if (Math.Abs(previous.Skew - next.Skew) > float.Epsilon)
        {
            animator.Skew.Add(new BlingoFloatKeyFrameDTO
            {
                Frame = frame,
                Value = next.Skew,
                Ease = BlingoEaseTypeDTO.Linear
            });
        }

        if (Math.Abs(previous.Blend - next.Blend) > float.Epsilon)
        {
            animator.Blend.Add(new BlingoFloatKeyFrameDTO
            {
                Frame = frame,
                Value = next.Blend,
                Ease = BlingoEaseTypeDTO.Linear
            });
        }

        if (previous.ForeR != next.ForeR || previous.ForeG != next.ForeG || previous.ForeB != next.ForeB)
        {
            animator.ForegroundColor.Add(new BlingoColorKeyFrameDTO
            {
                Frame = frame,
                Value = new BlingoColorDTO(next.ForeR, next.ForeG, next.ForeB),
                Ease = BlingoEaseTypeDTO.Linear
            });
        }

        if (previous.BackR != next.BackR || previous.BackG != next.BackG || previous.BackB != next.BackB)
        {
            animator.BackgroundColor.Add(new BlingoColorKeyFrameDTO
            {
                Frame = frame,
                Value = new BlingoColorDTO(next.BackR, next.BackG, next.BackB),
                Ease = BlingoEaseTypeDTO.Linear
            });
        }
    }

    private static bool HasAnimation(BlingoSpriteAnimatorDTO animator)
    {
        return animator.Position.Count > 0
            || animator.Rotation.Count > 0
            || animator.Skew.Count > 0
            || animator.Blend.Count > 0
            || animator.ForegroundColor.Count > 0
            || animator.BackgroundColor.Count > 0;
    }

    private static float ConvertBlend(int rawValue)
    {
        if (rawValue < 0)
            rawValue = 0;

        if (rawValue > byte.MaxValue)
            rawValue = byte.MaxValue;

        return (float)Math.Round(100f - rawValue / 255f * 100f, 2);
    }

    private sealed class SpriteState
    {
        public int LocH { get; set; }
        public int LocV { get; set; }
        public float Rotation { get; set; }
        public float Skew { get; set; }
        public float Blend { get; set; }
        public byte ForeR { get; set; }
        public byte ForeG { get; set; }
        public byte ForeB { get; set; }
        public byte BackR { get; set; }
        public byte BackG { get; set; }
        public byte BackB { get; set; }
        public bool FlipH { get; set; }
        public bool FlipV { get; set; }
        public int ScoreColor { get; set; }
        public int Ink { get; set; }
        public bool Editable { get; set; }
        public int LocZ { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public static SpriteState FromSprite(BlSpriteRawData sprite)
        {
            return new SpriteState
            {
                LocH = sprite.LocH,
                LocV = sprite.LocV,
                Rotation = sprite.Rotation,
                Skew = sprite.Skew,
                Blend = sprite.Blend,
                ForeR = sprite.ForeColor.R,
                ForeG = sprite.ForeColor.G,
                ForeB = sprite.ForeColor.B,
                BackR = sprite.BackColor.R,
                BackG = sprite.BackColor.G,
                BackB = sprite.BackColor.B,
                FlipH = sprite.FlipH,
                FlipV = sprite.FlipV,
                ScoreColor = sprite.ScoreColor,
                Ink = sprite.Ink,
                Editable = sprite.Editable,
                LocZ = sprite.LocZ,
                Width = sprite.Width,
                Height = sprite.Height
            };
        }

        public SpriteState Clone()
        {
            return new SpriteState
            {
                LocH = LocH,
                LocV = LocV,
                Rotation = Rotation,
                Skew = Skew,
                Blend = Blend,
                ForeR = ForeR,
                ForeG = ForeG,
                ForeB = ForeB,
                BackR = BackR,
                BackG = BackG,
                BackB = BackB,
                FlipH = FlipH,
                FlipV = FlipV,
                ScoreColor = ScoreColor,
                Ink = Ink,
                Editable = Editable,
                LocZ = LocZ,
                Width = Width,
                Height = Height
            };
        }
    }
}
