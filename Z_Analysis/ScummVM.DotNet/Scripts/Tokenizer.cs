using System.Numerics;
using System.Text;

namespace Director.Scripts
{
    /// <summary>
    /// Token types used by the Lingo tokenizer.
    /// </summary>
    public enum LingoTokenType
    {
        Eof,
        Number,
        String,
        Identifier,
        Symbol,
        LeftParen,
        RightParen,
        Comma,
        Dot,
        Colon,
        Semicolon,
        Equals,
        Plus,
        Minus,
        Asterisk,
        Slash,
        LessThan,
        GreaterThan,
        Not,
        And,
        Or,
        If,
        Then,
        Else,
        End,
        Repeat,
        While,
        For,
        In,
        To,
        With,
        Exit,
        Next,
        Global,
        On,
        Tell,
        Of,
        Property,
        Put,
        Into,
        The,
        Me,
        My,
        Return,
        Function,
        Handler,
        Until,
        Forever,
        Times
    }
    /// <summary>
    /// Represents a token in the Lingo source code.
    /// </summary>
    public readonly struct LingoToken
    {
        /// <summary>
        /// The type of token (identifier, number, string, etc.).
        /// </summary>
        public LingoTokenType Type { get; }

        /// <summary>
        /// The raw text from the source code that this token represents.
        /// </summary>
        public string Lexeme { get; }

        /// <summary>
        /// Line number where the token appears.
        /// </summary>
        public int Line { get; }

        public LingoToken(LingoTokenType type, string lexeme, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
        }

        public override string ToString() => $"{Type}: \"{Lexeme}\" (line {Line})";
    }
    public class Tokenizer
    {
        private readonly string _source;
        private int _start = 0;
        private int _line = 1;
        private int _position;
        private static readonly Dictionary<string, LingoTokenType> _keywords = new(StringComparer.OrdinalIgnoreCase)
            {
                { "if", LingoTokenType.If },
                { "then", LingoTokenType.Then },
                { "else", LingoTokenType.Else },
                { "end", LingoTokenType.End },
                { "repeat", LingoTokenType.Repeat },
                { "while", LingoTokenType.While },
                { "for", LingoTokenType.For },
                { "in", LingoTokenType.In },
                { "to", LingoTokenType.To },
                { "with", LingoTokenType.With },
                { "exit", LingoTokenType.Exit },
                { "next", LingoTokenType.Next },
                { "global", LingoTokenType.Global },
                { "on", LingoTokenType.On },
                { "tell", LingoTokenType.Tell },
                { "of", LingoTokenType.Of },
                { "property", LingoTokenType.Property },
                { "put", LingoTokenType.Put },
                { "into", LingoTokenType.Into },
                { "the", LingoTokenType.The },
                { "me", LingoTokenType.Me },
                { "my", LingoTokenType.My },
                { "return", LingoTokenType.Return },
                { "function", LingoTokenType.Function },
                { "handler", LingoTokenType.Handler },
                { "and", LingoTokenType.And },
                { "or", LingoTokenType.Or },
                { "not", LingoTokenType.Not },
                { "loop", LingoTokenType.Repeat },
                { "forever", LingoTokenType.Forever }, 
                { "times", LingoTokenType.Times }
            };

        public Tokenizer(string source)
        {
            _source = source;
            _position = 0;
        }

        public bool End => _position >= _source.Length;

        public char Current => !End ? _source[_position] : '\0';

        public bool Eof { get; internal set; }

        public char Advance() => _source[_position++];
        private char Peek() => IsAtEnd() ? '\0' : _source[_position];
        private char PeekNext() => _position + 1 >= _source.Length ? '\0' : _source[_position + 1];
        private bool IsAtEnd() => _position >= _source.Length;
        public char Peek(int offset = 1)
        {
            int pos = _position + offset;
            return pos < _source.Length ? _source[pos] : '\0';
        }
        public LingoToken NextToken()
        {
            SkipWhitespace();

            _start = _position;

            if (IsAtEnd())
                return new LingoToken(LingoTokenType.Eof, string.Empty, _line);

            char c = Advance();

            if (char.IsDigit(c)) return Number();
            if (char.IsLetter(c) || c == '_') return Identifier();

            return c switch
            {
                '"' => String(),
                '(' => MakeToken(LingoTokenType.LeftParen),
                ')' => MakeToken(LingoTokenType.RightParen),
                ',' => MakeToken(LingoTokenType.Comma),
                '.' => MakeToken(LingoTokenType.Dot),
                ':' => MakeToken(LingoTokenType.Colon),
                ';' => MakeToken(LingoTokenType.Semicolon),
                '+' => MakeToken(LingoTokenType.Plus),
                '-' => MakeToken(LingoTokenType.Minus),
                '*' => MakeToken(LingoTokenType.Asterisk),
                '/' => MakeToken(LingoTokenType.Slash),
                '=' => MakeToken(LingoTokenType.Equals),
                '<' => MakeToken(LingoTokenType.LessThan),
                '>' => MakeToken(LingoTokenType.GreaterThan),
                _ => MakeToken(LingoTokenType.Symbol)
            };
        }
        private void SkipWhitespace()
        {
            while (!IsAtEnd())
            {
                char c = Peek();
                switch (c)
                {
                    case ' ':
                    case '\r':
                    case '\t':
                        Advance();
                        break;
                    case '\n':
                        _line++;
                        Advance();
                        break;
                    case '-' when PeekNext() == '-':
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                        break;
                    default:
                        return;
                }
            }
        }
        private LingoToken MakeToken(LingoTokenType type) => new(type, _source[_start.._position], _line);

        public string ReadWhile(Func<char, bool> predicate)
        {
            int start = _position;
            while (!End && predicate(Current))
            {
                Advance();
            }
            return _source[start.._position];
        }

        public string ReadIdentifier()
        {
            return ReadWhile(c => char.IsLetterOrDigit(c) || c == '_');
        }

        public string ReadStringLiteral()
        {
            if (Current != '\"') return string.Empty;
            Advance(); // Skip opening quote
            int start = _position;
            while (!End && Current != '\"')
            {
                Advance();
            }
            string result = _source[start.._position];
            if (Current == '\"') Advance(); // Skip closing quote
            return result;
        }

        public string ReadSymbol()
        {
            if (Current != '#') return string.Empty;
            Advance();
            return "#" + ReadIdentifier();
        }

        public bool Match(char expected)
        {
            if (Current == expected)
            {
                Advance();
                return true;
            }
            return false;
        }

        public bool Match(string expected)
        {
            if (_source[_position..].StartsWith(expected, StringComparison.OrdinalIgnoreCase))
            {
                _position += expected.Length;
                return true;
            }
            return false;
        }

        public void SkipLine()
        {
            while (!End && Current != '\n')
                Advance();
            if (Current == '\n') Advance();
        }

       
        private LingoToken Number()
        {
            // Hexadecimal check: starts with $
            if (Peek() == '$')
            {
                Advance(); // consume '$'
                while (!IsAtEnd() && IsHexDigit(Peek()))
                    Advance();

                string hexText = _source[_start.._position];
                return new LingoToken(LingoTokenType.Number, hexText, _line);
            }

            // Decimal number (possibly with fraction)
            while (!IsAtEnd() && char.IsDigit(Peek()))
                Advance();

            if (Peek() == '.' && char.IsDigit(PeekNext()))
            {
                Advance(); // consume '.'
                while (!IsAtEnd() && char.IsDigit(Peek()))
                    Advance();
            }

            string text = _source[_start.._position];
            return new LingoToken(LingoTokenType.Number, text, _line);
        }

        private bool IsHexDigit(char c) =>
            char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');




        private LingoToken Identifier()
        {
            while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
                Advance();

            string text = _source[_start.._position];
            var type = _keywords.TryGetValue(text.ToLowerInvariant(), out var keywordType)
                ? keywordType
                : LingoTokenType.Identifier;

            return new LingoToken(type, text, _line);
        }

        private LingoToken String()
        {
            var builder = new StringBuilder();

            while (!IsAtEnd())
            {
                char c = Advance();

                if (c == '"')
                    break;

                if (c == '\\')
                {
                    if (IsAtEnd()) break;

                    char next = Advance();
                    builder.Append(next switch
                    {
                        '"' => '"',
                        '\\' => '\\',
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        _ => next // unknown escape, preserve raw
                    });
                }
                else
                {
                    if (c == '\n') _line++;
                    builder.Append(c);
                }
            }

            return new LingoToken(LingoTokenType.String, builder.ToString(), _line);
        }



    }
}
