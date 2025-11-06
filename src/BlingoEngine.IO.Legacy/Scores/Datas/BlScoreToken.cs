using System.Diagnostics;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Scores.Datas
{
    [DebuggerDisplay("{LogString}")]
    internal class BlScoreTokenPropChange
    {
        public BlSpriteRawData.BlSpriteProperty Property { get; private set; }
        public int Value { get; private set; }
        public BlScoreTokenPropChange(BlSpriteRawData.BlSpriteProperty property, int value)
        {
            Property = property;
            Value = value;
        }

        public string LogString => $"Set {Property}={Value}";

    }

    [DebuggerDisplay("Token:{HexString}")]
    internal class BlScoreToken
    {
        //public BlScoreTag Tag { get; }
        public List<BlScoreTokenPropChange> Properties { get; set; } = new List<BlScoreTokenPropChange>();
        public short AddressOffset { get; }
        public int Channel { get; }
        public byte[] Payload { get; }
        public BlScoreToken(short addressOffset, byte[] payload)
        {
            AddressOffset = addressOffset;
            Payload = payload;
            Channel = ResolveChannel(addressOffset);
            if (addressOffset >= 0x120)
            {
                // its classic sprite property
                int ofs = 0;
                while (ofs < payload.Length)
                {
                    var fieldAddr = (short)(addressOffset + ofs);
                    if (!BlSpriteRawData.TryGetPropertySpec(fieldAddr, out var spec) || spec == null || spec.Size <= 0)
                    {
                        ofs++; // unknown byte, advance safely
                        continue;
                    }

                    if (ofs + spec.Size > payload.Length) break; // incomplete field

                    int value = spec.Size switch
                    {
                        1 => payload.ReadByteOrDefault(ofs),
                        2 => payload.ReadInt16(ofs),
                        4 => payload.ReadInt32(ofs),
                        _ => payload.ReadByteOrDefault(ofs)
                    };

                    Properties.Add(new BlScoreTokenPropChange(spec.Prop, value));
                    ofs += spec.Size;
                }
            }
        }
        private static int ResolveChannel(short addr)
        {
            // system lanes
            if (addr >= 0x0000 && addr < 0x0030) return 0;
            if (addr >= 0x0030 && addr < 0x0060) return 1;
            if (addr >= 0x0060 && addr < 0x0090) return 2;
            if (addr >= 0x0090 && addr < 0x00C0) return 3;
            if (addr >= 0x00C0 && addr < 0x00F0) return 4;
            if (addr >= 0x00F0 && addr < 0x0120) return 5;
            // sprite channels (6+)
            return 6 + ((addr - 0x0120) / 0x30);
        }

        public string HexString
        {
            get
            {
                var hex = string.Join(" ", Payload.Select(b => b.ToString("X2")));
                var sets = string.Join('|', Properties.Select(x => x.LogString));
                return string.Format("{0:X4}={1,-48} {2,4}) {3}", AddressOffset, hex, Channel, sets);
            }
        }
    }
}
