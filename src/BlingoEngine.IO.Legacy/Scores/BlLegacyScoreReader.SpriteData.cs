using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Scores
{
    
    internal partial class BlLegacyScoreReader
    {
        private class SpriteRawData
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

            public SpriteRawData(byte[] data,byte[] memberBeheviorData, int index)
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

            public string ToDescriptionString()
            {
                return $"Item Desc. {Index}: Start={StartFrame}, End={EndFrame}, Channel={Channel}, U1={Unknown1}, Flip={FlipH},{FlipV},🔒={IsLocked},Trails={Trails},Editable={Editable},Moveable={Moveable} , U3={UnknownAlwaysOne}, U4A={UnkownA}, U4B={UnkownB}, U5={UnknownE1}, U6={UnknownFD}, ExtraVal={String.Join(',',KeyFrameOffsets)}";
                
            }
        }
    }
}
