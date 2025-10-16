using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Texts.Data;
using System.Collections.Generic;
using static BlingoEngine.IO.Legacy.Texts.Data.BlXmedToken;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class BlXmedTokenReader
    {
        private static readonly IReadOnlyList<BlXmedToken> _emptyTokenList = Array.Empty<BlXmedToken>();
        private readonly IReadOnlyList<BlXmedToken> _tokens;


        public int Position { get; private set; }
        public int Count => _tokens.Count;
        public bool IsAtEnd => Position >= Count;

        public BlXmedTokenReader(IReadOnlyList<BlXmedToken>? tokens, int position = 0)
        {
            _tokens = tokens ?? _emptyTokenList;
            Position = Math.Clamp(position, 0, _tokens.Count);
        }

        public BlXmedToken? Peek(int offset = 0)
        {
            int index = Position + offset;
            if (index < 0 || index >= Count)
                return null;

            return _tokens[index];
        }

        public BlXmedToken? ReadNext()
        {
            var token = Peek();
            if (token != null)
                Position++;

            return token;
        }

        public void Skip(int count = 1)
        {
            if (count == 0 || Count == 0)
                return;

            Position = Math.Clamp(Position + count, 0, Count);
        }
        public bool PeekIsTerminator => Peek()?.IsFieldTerminator() == true;
        public void Rewind(int position) => Position = Math.Clamp(position, 0, Count);

        public IReadOnlyList<IReadOnlyList<BlXmedToken>> GetValues(bool includeEmpty = false, bool consumeTerminator = true)
        {
            if (IsAtEnd)
                return Array.Empty<IReadOnlyList<BlXmedToken>>();

            var segments = new List<IReadOnlyList<BlXmedToken>>();
            var current = new List<BlXmedToken>();

            while (!IsAtEnd)
            {
                var token = _tokens[Position];

                if (token.IsFieldSeparator())
                {
                    Position++;
                    if (current.Count > 0 || includeEmpty)
                        segments.Add(current);

                    current = new List<BlXmedToken>();
                    continue;
                }

                if (token.IsFieldTerminator())
                {
                    if (consumeTerminator)
                        Position++;

                    break;
                }

                if (token.IsBlockBoundary())
                    break;

                current.Add(token);
                Position++;
            }

            if (current.Count > 0 || includeEmpty)
                segments.Add(current);

            return segments;
        }

        public IReadOnlyList<BlXmedToken> GetFlatValues(bool includeEmpty = false, bool consumeTerminator = true)
        {
            var segments = GetValues(includeEmpty, consumeTerminator);
            if (segments.Count == 0)
                return Array.Empty<BlXmedToken>();

            if (segments.Count == 1)
                return segments[0];

            return segments.SelectMany(static segment => segment).ToList();
        }

        public bool TryGetSingleNumeric(out int v)
        {
            v = 0;
            var token = Peek();
            if (token == null)
                return false;

            if (token.TryGetNumericValue(out v))
            {
                ReadNext();
                return true;
            }

            return false;
        }

        public IReadOnlyList<int> GetNumericValues()
        {
            var tokens = GetFlatValues();
            if (tokens.Count == 0)
                return Array.Empty<int>();

            var values = new List<int>(tokens.Count);
            foreach (var token in tokens)
            {
                if (token.TryGetNumericValue(out var numeric))
                    values.Add(numeric);
            }

            return values;
        }

        public IReadOnlyList<bool> GetBoolValues()
        {
            var v = GetFlatValues();
            if (v.Count == 0) return Array.Empty<bool>();
            var r = new List<bool>(v.Count);
            foreach (var t in v) 
                if (t.TryGetNumericValue(out var n)) 
                    r.Add(n == 1);
            return r;
        }


       
       
        public bool TryGetBool(out bool value)
        {
            value = false;
            var token = Peek();
            if (token == null)
                return false;

            if (token.IsBoolean())
            {
                value = token.GetBool();
                ReadNext();
                return true;
            }

            return false;
        }
        public bool TryGetString(out string? text)
        {
            text = null;
            var token = Peek();
            if (token == null) return false;
            if (!string.IsNullOrEmpty(token.Ascii))
            {
                text = token.Ascii;
                ReadNext();
                return true;
            }
            return false;
        }

        public bool SkipIf(TokenType type)
        {
            if (Peek()?.Type == type)
            {
                ReadNext();
                return true;
            }
            return false;
        }

        public bool IsNext(TokenType type) => Peek()?.Type == type;

      
       
        public bool TryReadBooleansInC2(byte type, out bool first, out bool second, Action<BlXmedToken> onSkip)
        {
            first = second = false;
            var t = Peek();
            if (t is null || !t.IsCompositeC2(type)) return false;

            ReadNext(); // consume C2(type)
            int count = 0;

            while (!IsAtEnd)
            {
                t = Peek(); if (t is null) break;
                if (t.IsFieldTerminator()) { ReadNext(); break; }

                if (t.IsPrefixedHex02() && t.TryGetNumericValue(out var v))
                {
                    if (count == 0) first = v != 0;
                    else if (count == 1) second = v != 0;
                    count++;
                    ReadNext();
                    continue;
                }

                onSkip(t);
                ReadNext();
            }

            return count > 0;
        }

     
        public bool TryReadNumericPairInC2(byte type, out int a, out int b, Action<BlXmedToken> onSkip)
        {
            a = b = 0;
            var t = Peek(); if (t is null || !t.IsCompositeC2(type)) return false;
            ReadNext();
            int? x = null, y = null;
            while (!IsAtEnd)
            {
                t = Peek(); if (t is null) break;
                if (t.IsFieldTerminator()) { ReadNext(); break; }
                if (t.IsPrefixedHex02() && t.TryGetNumericValue(out var v))
                { if (x is null) x = v; else if (y is null) y = v; ReadNext(); continue; }
                onSkip(t); ReadNext();
            }
            if (x is { } xx && y is { } yy) { a = xx; b = yy; return true; }
            return false;
        }

        public bool TryReadBooleanInC2(byte type, out bool value, Action<BlXmedToken> onSkip)
        {
            value = false;
            var t = Peek(); if (t is null || !t.IsCompositeC2(type)) return false;
            ReadNext();
            while (!IsAtEnd)
            {
                t = Peek(); if (t is null) break;
                if (t.IsFieldTerminator()) { ReadNext(); break; }
                if (t.IsPrefixedHex02() && t.TryGetNumericValue(out var v))
                { value = v != 0; ReadNext(); return true; }
                onSkip(t); ReadNext();
            }
            return false;
        }

        public bool TryGetColor(out BlLegacyColor? color)
        {
            color = null;
            var token = Peek();
            if (token is null || !token.IsC1())
                return false;

            byte mask = (byte)token.TypeValue.GetValueOrDefault();
            if (!IsColorCompositeMask(mask))
                return false;

            int start = Position;
            ReadNext();

            if (TryReadCompositeColorComponents(mask, out var components))
            {
                color = new BlLegacyColor(components.R, components.G, components.B);
                return true;
            }

            Rewind(start);
            return false;
        }

        public IReadOnlyList<byte> GetColorComponents()
        {
            var token = Peek();
            if (token is null || !token.IsC1())
                return Array.Empty<byte>();

            byte mask = (byte)token.TypeValue.GetValueOrDefault();
            if (!IsColorCompositeMask(mask))
                return Array.Empty<byte>();

            int start = Position;
            ReadNext();

            if (TryReadCompositeColorComponents(mask, out var components))
            {
                Rewind(start);
                return new[] { components.R, components.G, components.B };
            }

            Rewind(start);
            return Array.Empty<byte>();
        }

        private bool TryReadCompositeColorComponents(byte mask, out (byte R, byte G, byte B) components)
        {
            components = default;

            byte? r = null;
            byte? g = null;
            byte? b = null;

            bool readAny = false;

            if ((mask & 0x01) != 0)
            {
                if (!TryReadWordLowByte(out var r16) ||
                    !TryReadWordLowByte(out var g16) ||
                    !TryReadWordLowByte(out var b16))
                {
                    return false;
                }

                r = r16;
                g = g16;
                b = b16;
                readAny = true;
            }

            if ((mask & 0x04) != 0)
            {
                if (!TryReadCompositeByte(out var r8) ||
                    !TryReadCompositeByte(out var g8) ||
                    !TryReadCompositeByte(out var b8))
                {
                    return false;
                }

                r = r8;
                g = g8;
                b = b8;
                readAny = true;
            }

            if ((mask & 0x02) != 0)
                TryReadCompositeByte(out _);

            if ((mask & 0x08) != 0)
            {
                TryReadWordLowByte(out _);
                TryReadWordLowByte(out _);
                TryReadWordLowByte(out _);
            }

            if ((mask & 0x10) != 0)
            {
                TryReadCompositeByte(out _);
                TryReadCompositeByte(out _);
            }

            SkipCompositeRemainder();

            if (readAny && r.HasValue && g.HasValue && b.HasValue)
            {
                components = (r.Value, g.Value, b.Value);
                return true;
            }

            return false;
        }

        private static bool IsColorCompositeMask(byte mask)
        {
            return (mask & 0x01) != 0 || (mask & 0x02) != 0 || (mask & 0x04) != 0 || (mask & 0x08) != 0 || (mask & 0x10) != 0;
        }

        private bool TryReadCompositeByte(out byte value, out byte? prefix)
        {
            value = 0;
            prefix = null;
            byte? pendingSplit = null;

            while (!IsAtEnd)
            {
                var token = Peek();
                if (token is null)
                    break;

                if (token.IsFieldTerminator())
                    return false;

                if (token.IsFieldSeparator())
                {
                    ReadNext();
                    continue;
                }

                if (token.Type is BlXmedToken.TokenType.Split01 or BlXmedToken.TokenType.Split02 or BlXmedToken.TokenType.Split03)
                {
                    byte splitValue = token.Type switch
                    {
                        BlXmedToken.TokenType.Split01 => (byte)0x01,
                        BlXmedToken.TokenType.Split02 => (byte)0x02,
                        _ => (byte)0x03
                    };

                    ReadNext();

                    if (pendingSplit.HasValue)
                    {
                        value = pendingSplit.Value;
                        prefix = pendingSplit;
                        pendingSplit = splitValue;
                        return true;
                    }

                    pendingSplit = splitValue;
                    continue;
                }

                if (token.Type == BlXmedToken.TokenType.Byte && token.Value.HasValue)
                {
                    value = (byte)token.Value.Value;
                    prefix = pendingSplit;
                    pendingSplit = null;
                    ReadNext();
                    return true;
                }

                if (!string.IsNullOrEmpty(token.Ascii))
                {
                    var ascii = token.Ascii!;
                    if (ascii.Length >= 2 && token.TryGetNumericValue(out var numeric))
                        value = (byte)(numeric & 0xFF);
                    else
                        value = (byte)ascii[0];

                    prefix = pendingSplit;
                    pendingSplit = null;
                    ReadNext();
                    return true;
                }

                if (token.TryGetNumericValue(out var raw))
                {
                    value = (byte)(raw & 0xFF);
                    prefix = pendingSplit;
                    pendingSplit = null;
                    ReadNext();
                    return true;
                }

                ReadNext();
            }

            return false;
        }

        private bool TryReadCompositeByte(out byte value)
        {
            return TryReadCompositeByte(out value, out _);
        }

        private bool TryReadWordLowByte(out byte component)
        {
            component = 0;
            if (!TryReadCompositeByte(out var first, out var prefix))
                return false;

            byte second = 0;
            bool hasSecond = TryReadCompositeByte(out second, out _);

            if (prefix == 0x01)
            {
                int value16 = (first << 8) | (hasSecond ? second : 0);
                component = (byte)((value16 >> 8) & 0xFF);
                return true;
            }

            component = first;
            return true;
        }

        private void SkipCompositeRemainder()
        {
            while (!IsAtEnd)
            {
                var token = Peek();
                if (token is null)
                    break;

                if (token.IsFieldTerminator())
                {
                    ReadNext();
                    break;
                }

                if (token.IsBlockBoundary())
                    break;

                ReadNext();
            }
        }

    }
}
