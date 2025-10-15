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


        public IReadOnlyList<byte> GetColorComponents()
        {
            var tokens = GetFlatValues();
            if (tokens.Count == 0)
                return Array.Empty<byte>();

            var values = new List<byte>(tokens.Count);
            foreach (var token in tokens)
            {
                if (token.TryGetColorComponent(out var component))
                    values.Add(component);
            }

            return values;
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
            if (!Peek()?.IsCompositeC1(0x04) ?? true) return false;
            var components = GetColorComponents();
            if (components.Count == 0) return false;
            var r = components.ElementAtOrDefault(0);
            var g = components.ElementAtOrDefault(1);
            var b = components.ElementAtOrDefault(2);
            color = new BlLegacyColor(r, g, b);
            return true;
        }

    }
}
