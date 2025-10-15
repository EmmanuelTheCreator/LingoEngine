using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Texts.Data;
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


       
        // in BlXmedTokenReader
        public bool TryReadNumericPairInC2(byte type, out int a, out int b, Action<BlXmedToken> onSkip)
        {
            a = b = 0;
            var t = Peek(); if (t is null || !t.IsCompositeC2(type)) return false;
            ReadNext(); // consume C2(type)
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

        public bool TryGetColor(out BlLegacyColor? color)
        {
            color = null;
            var t = Peek();
            if (t is null || !t.IsCompositeC1(0x04)) return false;

            var comps = GetColorComponents(); // consumes C1(04) … 82
            if (comps.Count == 0) return false;

            var r = comps.ElementAtOrDefault(0);
            var g = comps.ElementAtOrDefault(1);
            var b = comps.ElementAtOrDefault(2);
            color = new BlLegacyColor(r, g, b);
            return true;
        }

        public IReadOnlyList<byte> GetColorComponents()
        {
            var t = Peek();
            if (t is null || !t.IsCompositeC1(0x04)) return Array.Empty<byte>();

            ReadNext(); // consume C1(04)
            var vals = new List<byte>(3);

            while (!IsAtEnd)
            {
                t = Peek(); if (t is null) break;
                if (t.IsFieldTerminator()) { ReadNext(); break; }

                if (t.TryGetColorComponent(out var c))
                { vals.Add(c); ReadNext(); continue; }

                ReadNext(); // skip (e.g., 81)
            }
            return vals;
        }
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

        public bool TryReadBooleanInC2(byte type, out bool value, Action<BlXmedToken> onSkip)
        {
            value = false;
            var t = Peek();
            if (t is null || !t.IsCompositeC2(type)) return false;

            ReadNext(); // consume C2(type)
            while (!IsAtEnd)
            {
                t = Peek(); if (t is null) break;
                if (t.IsFieldTerminator()) { ReadNext(); break; }

                if (t.IsPrefixedHex02() && t.TryGetNumericValue(out var v))
                {
                    value = v != 0;
                    ReadNext();
                    return true;
                }

                onSkip(t);
                ReadNext();
            }
            return false;
        }

    }
}
