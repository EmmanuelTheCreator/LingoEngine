using System.Globalization;

namespace BlingoEngine.IO.Legacy.Texts.Data
{
    public sealed class BlXmedToken
    {
        public enum TokenType { Split01, Split02, Split03, C1, C2, C3, B_81, B_82, PrefixedHex, Ascii, Block00, Byte }
        public TokenType Type { get; }
        public int Start { get; }
        public int Length { get; }
        public string? Ascii { get; }
        public int? Value { get; }
        public int? TypeValue { get; }
        public bool LinkToPrevious { get; }
        public byte[]? Data { get; }
        public TokenType PrefixedHex { get; }


        public BlXmedToken(TokenType type, int start, int length, string? ascii = null, int? value = null,
                int? typeValue = null, bool linkToPrevious = false, byte[]? data = null)
        {
            (Type, Start, Length, Ascii, Value, TypeValue, LinkToPrevious, Data)
                = (type, start, length, ascii, value, typeValue, linkToPrevious, data);
        }

      

        public bool IsTextBlock() => Type == TokenType.Block00 && Value != 40 && Value != 44;
        public bool IsAsciiValue(string value) => Type == TokenType.Ascii && string.Equals(Ascii, value, StringComparison.OrdinalIgnoreCase);

        public bool IsBlockBoundary() => Type == TokenType.Block00 || IsPrefixedHex(0x03) || Type == TokenType.C1 || Type == TokenType.C2;
        public bool IsFieldSeparator() => Type == TokenType.B_81;
        public bool IsFieldTerminator() => Type == TokenType.B_82;

        public bool IsPrefixedHex(byte expectedType) => Type == TokenType.PrefixedHex && TypeValue == expectedType;
        public bool IsPrefixedHex01() => IsPrefixedHex(0x01);
        public bool IsPrefixedHex02() => IsPrefixedHex(0x02);
        public bool IsPrefixedHex03() => IsPrefixedHex(0x03);

        public bool IsCompositeC1(byte id) => Type == TokenType.C1 && TypeValue == id;
        public bool IsCompositeC2(byte id) => Type == TokenType.C2 && TypeValue == id;
        public bool IsCompositeOpen() => Type == TokenType.C1 || Type == TokenType.C2 || Type == TokenType.C3;

        public bool IsC1() => Type == TokenType.C1;
        public bool IsC2() => Type == TokenType.C2;

        public bool IsBoolean() =>
             Type == TokenType.PrefixedHex &&
             Data is { Length: > 0 } &&
             (Data[0] == 0x00 || Data[0] == 0x01);

        public bool GetBool() => IsBoolean() && Data![0] == 0x01;
        public bool TryGetBool(out bool value)
        {
            if (IsBoolean()) { value = Data![0] == 0x01; return true; }
            value = false; return false;
        }


        public bool TryGetNumericValue(out int value)
        {
            if (Value.HasValue)
            {
                value = Value.Value;
                return true;
            }

            value = 0;
            if (Ascii is not { } ascii || ascii.Length == 0)
            {
                return false;
            }

            var text = ascii.Trim();
            bool negative = text.StartsWith("-", StringComparison.Ordinal);
            if (negative)
                text = text[1..];

            if (text.Length == 0)
                return false;

            if (!int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed))
                return false;

            value = negative ? -parsed : parsed;
            return true;
        }

        public bool TryGetColorComponent(out byte component)
        {
            component = 0;
            if (string.IsNullOrWhiteSpace(Ascii))
                return false;

            string text = Ascii.Trim();
            if (text.Length > 2)
                text = text[..2];

            return byte.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out component);
        }

        public IReadOnlyList<int> ReadBlock00Numbers()
        {
            if (Type != TokenType.Block00)
                return Array.Empty<int>();

            var payload = Data ?? Array.Empty<byte>();
            if (payload.Length == 0)
                return Array.Empty<int>();

            var values = new List<int>();
            int offset = 0;
            while (offset + 3 < payload.Length)
            {
                if (payload[offset] != 0x01)
                {
                    offset++;
                    continue;
                }

                values.Add(payload[offset + 1]);
                offset += 4;
            }

            return values;
        }

        public bool IsFontTable00() => Type == TokenType.Block00 && Value == 40;
        public bool IsTail00() => Type == TokenType.Block00 && Value == 44;

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Ascii))
                return $"{PrefixedHex:X2}:{Ascii}";
            if (IsC1() || IsC2())
                return $"{PrefixedHex:X2}({TypeValue:X2})";
            return $"{PrefixedHex:X2}";
        }
    }
}
