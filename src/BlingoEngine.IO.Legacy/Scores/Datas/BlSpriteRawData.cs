using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Scores.Datas
{
        internal class BlSpriteRawData
        {
            /// <summary>
            /// Flags used in Director sprite control byte (Flip/Edit/Trail/Lock/etc).
            /// </summary>
            [Flags]
            internal enum SpriteRawFlags : byte
            {
                None = 0,

                /// <summary>Sprite is flipped horizontally.</summary>
                FlipH = 1 << 0, // 0x01

                /// <summary>Sprite is flipped vertically.</summary>
                FlipV = 1 << 6, // 0x40

                /// <summary>Sprite is editable at runtime (typically for text sprites).</summary>
                Editable = 1 << 5, // 0x20

                /// <summary>Sprite is moveable during playback.</summary>
                Moveable = 1 << 4, // 0x10

                /// <summary>Sprite leaves a trail behind while moving.</summary>
                Trails = 1 << 3, // 0x08

                /// <summary>Sprite is locked and cannot be edited.</summary>
                Locked = 1 << 2  // 0x04
            }
        // OLD
        //internal enum BlScoreTag : ushort
        //{
        //    KeyFrame = 0x00,            // 00 0000  Keyframe Or ink
        //                                // Ink:
        //                                //      Transparent : 0120=10 81
        //                                //      Reverse     : 0120=10 82
        //                                //      Ghost       : 0120=10 83
        //                                //TempoChange     = 0x02,     // 00 0010
        //    ColorRed = 0x02,            // 00 0010
        //    SetSound = 0x06,            // 00 0110
        //    Position = 0x0C,            // 00 1100
        //    Pallete = 0x12,             // 01 0010          Select the palette index           
        //    Size = 0x10,                // 01 0000
        //    AnimationTarget = 0x14,     // 01 0100
        //                                //          Background 20 00 + RGB Color         Example : 20 00 81 00 00 E3 00 E3
        //                                //          Foreground 10 00 + RGB Color
        //                                //          Blend      00 07 + level             Example : 00 07 91 00  
        //    KeyframeControl = 0x16,     // 01 0110  Keyframe control: 01 = real KF, 81 = tween continuation 
        //    ColorGreenBlue = 0x18,      // 01 1000  
        //    Rotation = 0x1E,            // 01 1110
        //    Skew = 0x22,                // 10 0010
        //}
        public enum BlSpriteProperty
        {
            Flags1 = 0x00,
            Ink = 0x01,
            ForeColor = 0x02,
            BackColor = 0x03,
            MemberCastLib = 0x04, // UInt16
            MemberNum = 0x06, // UInt16
            UnknownA = 0x08, // UInt16
            PropertiesOffset = 0x0A, // UInt16
            LocV = 0x0C, // Int16
            LocH = 0x0E, // Int16
            Height = 0x10, // Int16
            Width = 0x12, // Int16
            ScoreColor = 0x14, // Byte
            Blend = 0x15, // Byte
            FlipFlags = 0x16, // Byte
            Rotation = 0x1C, // Int32
            Skew = 0x20  // Int32
        }
        public sealed class SpritePropSpec
        {
            public required BlSpriteProperty Prop { get; init; }
            public required int Size { get; init; } // bytes
        }
        private static readonly Dictionary<int, SpritePropSpec> _properties = new()
        {
            [(int)BlSpriteProperty.Flags1 ] = new() { Prop = BlSpriteProperty.Flags1 , Size = 1 }, // Flags1
            [(int)BlSpriteProperty.Ink ] = new() { Prop = BlSpriteProperty.Ink , Size = 1 }, // Ink
            [(int)BlSpriteProperty.ForeColor ] = new() { Prop = BlSpriteProperty.ForeColor , Size = 1 }, // ForeColor (R)
            [(int)BlSpriteProperty.BackColor ] = new() { Prop = BlSpriteProperty.BackColor , Size = 1 }, // BackColor (R)
            [(int)BlSpriteProperty.MemberCastLib ] = new() { Prop = BlSpriteProperty.MemberCastLib , Size = 1 }, // Castlib
            [(int)BlSpriteProperty.MemberNum ] = new() { Prop = BlSpriteProperty.MemberNum , Size = 1 }, // Member
            [(int)BlSpriteProperty.UnknownA ] = new() { Prop = BlSpriteProperty.UnknownA , Size = 1 }, // UnknownA
            [(int)BlSpriteProperty.PropertiesOffset ] = new() { Prop = BlSpriteProperty.PropertiesOffset , Size = 1 }, // PropertiesOffset
            [(int)BlSpriteProperty.LocV ] = new() { Prop = BlSpriteProperty.LocV , Size = 2 }, // LocV
            [(int)BlSpriteProperty.LocH ] = new() { Prop = BlSpriteProperty.LocH , Size = 2 }, // LocH
            [(int)BlSpriteProperty.Height ] = new() { Prop = BlSpriteProperty.Height , Size = 2 }, // Height
            [(int)BlSpriteProperty.Width ] = new() { Prop = BlSpriteProperty.Width , Size = 2 }, // Width
            [(int)BlSpriteProperty.ScoreColor ] = new() { Prop = BlSpriteProperty.ScoreColor , Size = 1 }, // ScoreColor / FG/BG select
            [(int)BlSpriteProperty.Blend ] = new() { Prop = BlSpriteProperty.Blend , Size = 1 }, // Blend
            [(int)BlSpriteProperty.FlipFlags ] = new() { Prop = BlSpriteProperty.FlipFlags , Size = 1 }, // FlipFlags
            [(int)BlSpriteProperty.Rotation ] = new() { Prop = BlSpriteProperty.Rotation , Size = 4 }, // Rotation (Int32)
            [(int)BlSpriteProperty.Skew] = new() { Prop = BlSpriteProperty.Skew, Size = 4 }, // Skew (Int32)
        };

        public static bool TryGetPropertySpec(short tag, out SpritePropSpec? spec)
            => _properties.TryGetValue(tag % 0x30, out spec);

        public struct SpriteRawTweenFlags
            {
                public bool TweeningEnabled;
                public bool Path;
                public bool Size;
                public bool Rotation;
                public bool Skew;
                public bool Blend;
                public bool ForeColor;
                public bool BackColor;
                public override string ToString()
                {
                    List<string> flags = new();
                    if (Path) flags.Add("Path");
                    if (Size) flags.Add("Size");
                    if (Rotation) flags.Add("Rotation");
                    if (Skew) flags.Add("Skew");
                    if (Blend) flags.Add("Blend");
                    if (ForeColor) flags.Add("ForeColor");
                    if (BackColor) flags.Add("BackColor");
                    return $"Tweening: {(TweeningEnabled ? "On" : "Off")} | " + string.Join(", ", flags);
                }

                public byte ToByte()
                {
                    byte result = 0;
                    if (TweeningEnabled) result |= 0x01;
                    if (Path) result |= 0x02;
                    if (Size) result |= 0x04;
                    if (Rotation) result |= 0x08;
                    if (Skew) result |= 0x10;
                    if (Blend) result |= 0x20;
                    if (ForeColor) result |= 0x40;
                    if (BackColor) result |= 0x80;
                    return result;
                }
            }


            public int StartFrame { get; internal set; }
            public int EndFrame { get; internal set; }
            public int Unknown1 { get; internal set; }
            public int Unknown2 { get; internal set; }
            public int SpriteNumber { get; internal set; }
            public int UnknownAlwaysOne { get; internal set; }
            public int UnkownA { get; internal set; }
            public int UnkownB { get; internal set; }
            public int UnknownE1 { get; internal set; }
            public int UnknownFD { get; internal set; }
            public int Unknown7 { get; internal set; }
            public int Unknown8 { get; internal set; }
            public int Unknown9 { get; internal set; }
            public int Unknown10 { get; internal set; }
            public List<int> KeyFrameOffsets { get; } = new();
            public int Channel { get; internal set; }
            public List<BlingoMemberRefDTO> Behaviors { get; internal set; } = new List<BlingoMemberRefDTO>();
            public bool FlipH { get; internal set; }
            public bool FlipV { get; internal set; }
            public bool Editable { get; internal set; }
            public bool Moveable { get; internal set; }
            public bool Trails { get; internal set; }
            public bool IsLocked { get; internal set; }
            public int Index { get; }

            public int MemberCastLib { get; set; }
            public int MemberNum { get; set; }
            public int SpritePropertiesOffset { get; internal set; }
            public int LocH { get; internal set; }
            public int LocV { get; internal set; }
            public int Width { get; internal set; }
            public int Height { get; internal set; }
            public float Rotation { get; internal set; }
            public float Skew { get; internal set; }
            public int Ink { get; internal set; }
            public int ForeColor { get; internal set; }
            public int BackColor { get; internal set; }
            public int ScoreColor { get; internal set; }
            public int Blend { get; internal set; }
            public byte EaseIn { get; set; }
            public byte EaseOut { get; set; }
            public ushort Curvature { get; set; }
            public SpriteRawTweenFlags TweenFlags { get; set; }

            public int LocZ { get; set; }

            public BlSpriteRawData(byte[] data,byte[] memberBeheviorData, int index)
            {
                Index = index;
                var stream = new BlStreamReader(new MemoryStream(data));
                if (stream.Length < 44)
                    return ;

                StartFrame = stream.ReadInt32();
                EndFrame = stream.ReadInt32();
                Unknown1 = stream.ReadInt32();
                Unknown2 = stream.ReadInt16();

                // Correctly cast to flag enum and extract bitfield
                var flags = (SpriteRawFlags)stream.ReadInt16();

                FlipH = flags.HasFlag(SpriteRawFlags.FlipH);
                FlipV = flags.HasFlag(SpriteRawFlags.FlipV);
                Editable = flags.HasFlag(SpriteRawFlags.Editable);
                Moveable = flags.HasFlag(SpriteRawFlags.Moveable);
                Trails = flags.HasFlag(SpriteRawFlags.Trails);
                IsLocked = flags.HasFlag(SpriteRawFlags.Locked);


                Channel = stream.ReadInt32();
                UnknownAlwaysOne = stream.ReadInt16();
                UnkownA = stream.ReadInt16();   // 00
                UnkownB = stream.ReadInt16();   // Almost always 0F
                UnknownE1 = stream.ReadByte();  // E1       = 225
                UnknownFD = stream.ReadByte();  // FD       = 253

                Unknown7 = stream.ReadInt32();  // 00
                Unknown8 = stream.ReadInt32();  // 00
                Unknown9 = stream.ReadInt32();  // 00
                Unknown10 = stream.ReadInt32(); // 00

                // Extra values has something to do with key frames, if at the end a keyframe is set there will be an extra value?
                while (stream.Position + 4 <= stream.Length)
                    KeyFrameOffsets.Add(stream.ReadInt32());

                // Read member behaviors 
                var behaviorCount = Math.Floor(memberBeheviorData.Length / 8f);
                for (int i = 0; i < behaviorCount; i++)
                {
                    var castLibNum = memberBeheviorData.ReadInt16(i * 3);
                    var memberNum = memberBeheviorData.ReadInt16(i * 3 + 2);
                    var unkonown = memberBeheviorData.ReadInt32(i * 3 + 4); // Always 0?
                    Behaviors.Add(new BlingoMemberRefDTO { CastLibNum = castLibNum, MemberNum = memberNum });
                }
            }

            public string ToDescriptionString() => $"Item Desc. {Index}: Start={StartFrame}, End={EndFrame}, Channel={Channel}, U1={Unknown1}, Flip={FlipH},{FlipV},🔒={IsLocked},Trails={Trails},Editable={Editable},Moveable={Moveable}, U3={UnknownAlwaysOne}, U4={UnkownA},{UnkownB}, U5={UnknownE1} {UnknownFD}, U7={Unknown7},{Unknown8},{Unknown9},{Unknown10}, KF={string.Join(',', KeyFrameOffsets)}";

            internal string ToLog()
            {
                return ToDescriptionString();
            }

          


            public void ReadKeyFrame(BlStreamReader stream)
            {
                var flags1 = stream.ReadByte();
                byte inkByte = stream.ReadByte();
                Ink = inkByte & 0x7F;
                ForeColor = stream.ReadByte();
                BackColor = stream.ReadByte();
                MemberCastLib = stream.ReadUInt16();
                MemberNum = stream.ReadUInt16();
                stream.Skip(2); // unknown
                SpritePropertiesOffset = stream.ReadUInt16(); //18,27,30,33,36
                LocV = stream.ReadInt16();
                LocH = stream.ReadInt16();
                Height = stream.ReadInt16();
                Width = stream.ReadInt16();
                byte colorcode = stream.ReadByte();
                Editable = (colorcode & 0x40) != 0;
                ScoreColor = colorcode & 0x0F;
                var blend = stream.ReadByte();
                Blend = (int)Math.Round(100f - blend / 255f * 100f);
                byte flag2 = stream.ReadByte();
                FlipV = (flag2 & 0x04) != 0;
                FlipH = (flag2 & 0x02) != 0;
                stream.Skip(5);
                if (stream.Length > 28)
                {
                    //var test = stream.ReadInt16();
                    Rotation = stream.ReadInt32() / 100f;
                    Skew = stream.ReadInt32() / 100f;
                }
            }
        
    }
}
