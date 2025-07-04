using ProjectorRays.Common;
using static ProjectorRays.director.Scores.RaysScoreChunk;
using static ProjectorRays.director.Scores.RaysScoreFrameParser;

namespace ProjectorRays.director.Scores
{
    internal class RayScoreTags
    {
        public static ScoreKeyframeTag? TryParseTag(ushort rawTag)
        {
            if (Enum.IsDefined(typeof(ScoreKeyframeTag), (int)rawTag))
                return (ScoreKeyframeTag)rawTag;
            return null;
        }
        public enum ScoreKeyframeTag
        {
            Ink = 0x0136,
            Colors = 0x0212,
            Blend = 0x0190,
            Rotation = 0x019E,
            Skew = 0x01D2,
            Position = 0x015C, // LocH, LocV (Path)
            Size = 0x0130, // Width/Height 
            Custom_01C6 = 0x01C6,
            Custom_01F6 = 0x01F6,
            Custom_01FC = 0x01FC,
            Custom_01FE = 0x01FE,
            Custom_0202 = 0x0202,
            Custom_0200 = 0x0200,
            Custom_0210 = 0x0210,
            Custom_0212 = 0x0212,
            Custom_0132 = 0x0132,
            Custom_0150 = 0x0150,
            Custom_015A = 0x015A,
            Custom_0166 = 0x0166,
            Custom_0172 = 0x0172,
            Custom_018A = 0x018A,
            Custom_01B0 = 0x01B0,
            Custom_01BA = 0x01BA,
            Custom_01CE = 0x01CE,
            Custom_01D2 = 0x01D2
        }

       // this is stil seriously wrong
        public static int GetDataLength(ScoreKeyframeTag tag)
        {
            return tag switch
            {                                                                       // composed
                                                                                    // tag = 01 90     0001 1001 0000 = Size + Blend

                ScoreKeyframeTag.Size => 2, //                                         tag = 01 30     0001 0011 0000 = Size
                ScoreKeyframeTag.Ink => 2, // 0x0136                                   
                ScoreKeyframeTag.Colors => 2, // 0x0182 (FG + BG)                      tag = 02 12     0010 0001 0010
                ScoreKeyframeTag.Blend => 6, // 0x0190                                 ?????? test  tag = 01 20     0001 0010 0000 = Blend  ???????       <- is wrong probably
                ScoreKeyframeTag.Rotation => 2, // 0x019E                              tag = 01 9E     0001 1001 1110
                ScoreKeyframeTag.Skew => 2, // 0x01A2                                  tag = 01 D2     0001 1010 0010
                ScoreKeyframeTag.Position => 4, // 0x01EC (LocH + LocV)                tag = 01 5C     0001 0101 1100

                // These are unknown/customs but often seen with a fixed length of 0 or placeholder
                ScoreKeyframeTag.Custom_01C6 => 4,
                ScoreKeyframeTag.Custom_01F6 => 2,
                ScoreKeyframeTag.Custom_01FC => 4,
                ScoreKeyframeTag.Custom_01FE => 2,
                ScoreKeyframeTag.Custom_0202 => 4,
                ScoreKeyframeTag.Custom_0200 => 2,
                ScoreKeyframeTag.Custom_0210 => 2,
                ScoreKeyframeTag.Custom_0132 => 2,
                ScoreKeyframeTag.Custom_0150 => 0,
                ScoreKeyframeTag.Custom_015A => 0,
                ScoreKeyframeTag.Custom_0166 => 2,
                ScoreKeyframeTag.Custom_0172 => 2,
                ScoreKeyframeTag.Custom_018A => 0,
                ScoreKeyframeTag.Custom_01B0 => 0,
                ScoreKeyframeTag.Custom_01BA => 0,
                ScoreKeyframeTag.Custom_01CE => 2,

                _ => 0 // Unknown or unhandled
            };
        }
        // this is stil seriously wrong
        public static RayKeyFrame CreateKeyFrameFromTags(FrameDelta frameDelta)
        {
            var keyframe = new RayKeyFrame();

            foreach (var item in frameDelta.Items)
            {
                if (item.Type == null || item.Data.Length < 2)
                    continue;

                var reader = new ReadStream(item.Data, item.Data.Length, Endianness.BigEndian);

                // Skip tag (2 bytes)
                ushort tag = reader.ReadUint16();

                switch (item.Type)
                {
                    case ScoreKeyframeTag.Position:
                        keyframe.LocH = reader.ReadInt16();
                        keyframe.LocV = reader.ReadInt16();
                        break;

                    case ScoreKeyframeTag.Blend:
                        keyframe.Width = reader.ReadInt16();
                        keyframe.Height = reader.ReadInt16();
                        var blendRaw = reader.ReadUint8();
                        keyframe.Blend = (int)Math.Round(100f - blendRaw / 255f * 100f);
                        break;

                    case ScoreKeyframeTag.Rotation:
                        keyframe.Rotation = reader.ReadFloat();
                        break;

                    case ScoreKeyframeTag.Skew:
                        keyframe.Skew = reader.ReadFloat();
                        break;

                    case ScoreKeyframeTag.Ink:
                        keyframe.Ink = reader.ReadInt16();
                        break;

                    case ScoreKeyframeTag.Colors:
                        keyframe.ForeColor = reader.ReadUint8();
                        keyframe.BackColor = reader.ReadUint8();
                        break;

                    default:
                        // You can log unknown/custom tags if needed
                        break;
                }
            }

            return keyframe;
        }

        private const int BaseTag = 0x0136;
        private const int BaseChannel = 6;
        private const int Step = 0x30;

        /// <summary>
        /// Gets the tag corresponding to a given sprite channel.
        /// </summary>
        public static int GetTagForChannel(int channel)
        {
            return BaseTag + (channel - BaseChannel) * Step;
        }

        /// <summary>
        /// Gets the sprite channel corresponding to a tag.
        /// </summary>
        public static int GetChannelForTag(int tag)
        {
            return ((tag - BaseTag) / Step) + BaseChannel;
        }
    }
    
}
