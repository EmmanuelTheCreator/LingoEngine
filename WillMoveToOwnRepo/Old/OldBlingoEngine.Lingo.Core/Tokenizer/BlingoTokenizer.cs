using System.Numerics;
using System.Text;
using System.Collections.Generic;

namespace OldBlingoEngine.Lingo.Core.Tokenizer
{
    /// <summary>
    /// Token types used by the Lingo tokenizer.
    /// </summary>
    public enum BlingoTokenType
    {
        Eof,
        Number,
        String,
        Identifier,
        Symbol,
        Comment,
        LeftParen,
        RightParen,
        LeftBracket,
        RightBracket,
        Comma,
        Dot,
        Range,
        Colon,
        Semicolon,
        Equals,
        Plus,
        Ampersand,
        Minus,
        Asterisk,
        Slash,
        LessThan,
        GreaterThan,
        LessOrEqual,
        GreaterOrEqual,
        NotEquals,
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
        Instance,
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
        Times,
        Case,
        Otherwise
    }
    /// <summary>
    /// Represents a token in the Lingo source code.
    /// </summary>
    public readonly struct BlingoToken
    {
        /// <summary>
        /// The type of token (identifier, number, string, etc.).
        /// </summary>
        public BlingoTokenType Type { get; }

        /// <summary>
        /// The raw text from the source code that this token represents.
        /// </summary>
        public string Lexeme { get; }

        /// <summary>
        /// Line number where the token appears.
        /// </summary>
        public int Line { get; }

        public BlingoToken(BlingoTokenType type, string lexeme, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
        }

        public override string ToString() => $"{Type}: \"{Lexeme}\" (line {Line})";
    }
    public class BlingoTokenizer
    {
        private readonly string _source;
        private int _start = 0;
        private int _line = 1;
        private int _position;
        private static readonly Dictionary<string, BlingoTokenType> _keywords = new(StringComparer.OrdinalIgnoreCase)
            {
                { "if", BlingoTokenType.If },
                { "then", BlingoTokenType.Then },
                { "else", BlingoTokenType.Else },
                { "end", BlingoTokenType.End },
                { "repeat", BlingoTokenType.Repeat },
                { "while", BlingoTokenType.While },
                { "for", BlingoTokenType.For },
                { "in", BlingoTokenType.In },
                { "to", BlingoTokenType.To },
                { "with", BlingoTokenType.With },
                { "exit", BlingoTokenType.Exit },
                { "next", BlingoTokenType.Next },
                { "global", BlingoTokenType.Global },
                { "on", BlingoTokenType.On },
                { "tell", BlingoTokenType.Tell },
                { "of", BlingoTokenType.Of },
                { "property", BlingoTokenType.Property },
                { "instance", BlingoTokenType.Instance },
                { "put", BlingoTokenType.Put },
                { "into", BlingoTokenType.Into },
                { "the", BlingoTokenType.The },
                { "me", BlingoTokenType.Me },
                { "my", BlingoTokenType.My },
                { "return", BlingoTokenType.Return },
                { "function", BlingoTokenType.Function },
                { "handler", BlingoTokenType.Handler },
                { "and", BlingoTokenType.And },
                { "or", BlingoTokenType.Or },
                { "not", BlingoTokenType.Not },
                { "loop", BlingoTokenType.Repeat },
                { "forever", BlingoTokenType.Forever },
                { "times", BlingoTokenType.Times },
                { "case", BlingoTokenType.Case },
                { "otherwise", BlingoTokenType.Otherwise }
            };

        public BlingoTokenizer(string source)
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
        public BlingoToken NextToken()
        {
            SkipWhitespace();

            _start = _position;

            if (IsAtEnd())
                return new BlingoToken(BlingoTokenType.Eof, string.Empty, _line);

            char c = Advance();

            if (char.IsDigit(c)) return Number();
            if (char.IsLetter(c) || c == '_') return Identifier();

            return c switch
            {
                '"' => String(),
                '(' => MakeToken(BlingoTokenType.LeftParen),
                ')' => MakeToken(BlingoTokenType.RightParen),
                '[' => MakeToken(BlingoTokenType.LeftBracket),
                ']' => MakeToken(BlingoTokenType.RightBracket),
                ',' => MakeToken(BlingoTokenType.Comma),
                '.' => Match('.') ? MakeToken(BlingoTokenType.Range) : MakeToken(BlingoTokenType.Dot),
                ':' => MakeToken(BlingoTokenType.Colon),
                ';' => MakeToken(BlingoTokenType.Semicolon),
                '+' => MakeToken(BlingoTokenType.Plus),
                '&' => Match('&') ? MakeToken(BlingoTokenType.Ampersand) : MakeToken(BlingoTokenType.Ampersand),
                '-' => Peek() == '-' ? Comment() : MakeToken(BlingoTokenType.Minus),
                '*' => MakeToken(BlingoTokenType.Asterisk),
                '/' => MakeToken(BlingoTokenType.Slash),
                '=' => MakeToken(BlingoTokenType.Equals),
                '<' => Match('=') ? MakeToken(BlingoTokenType.LessOrEqual) :
                    Match('>') ? MakeToken(BlingoTokenType.NotEquals) :
                    MakeToken(BlingoTokenType.LessThan),
                '>' => Match('=') ? MakeToken(BlingoTokenType.GreaterOrEqual) :
                    MakeToken(BlingoTokenType.GreaterThan),
                '#' => new BlingoToken(BlingoTokenType.Symbol, ReadIdentifier(), _line),
                _ => MakeToken(BlingoTokenType.Symbol)
            };
        }

        private BlingoToken Comment()
        {
            Advance(); // consume second '-'
            while (Peek() != '\n' && !IsAtEnd())
                Advance();
            var text = _source[(_start + 2).._position];
            return new BlingoToken(BlingoTokenType.Comment, text.Trim(), _line);
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
                    default:
                        return;
                }
            }
        }
        private BlingoToken MakeToken(BlingoTokenType type) => new(type, _source[_start.._position], _line);

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


        private BlingoToken Number()
        {
            // Hexadecimal check: starts with $
            if (Peek() == '$')
            {
                Advance(); // consume '$'
                while (!IsAtEnd() && IsHexDigit(Peek()))
                    Advance();

                string hexText = _source[_start.._position];
                return new BlingoToken(BlingoTokenType.Number, hexText, _line);
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
            return new BlingoToken(BlingoTokenType.Number, text, _line);
        }

        private bool IsHexDigit(char c) =>
            char.IsDigit(c) || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f';




        private BlingoToken Identifier()
        {
            while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
                Advance();

            string text = _source[_start.._position];
            var type = _keywords.TryGetValue(text.ToLowerInvariant(), out var keywordType)
                ? keywordType
                : BlingoTokenType.Identifier;

            return new BlingoToken(type, text, _line);
        }

        private BlingoToken String()
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

            return new BlingoToken(BlingoTokenType.String, builder.ToString(), _line);
        }



    }
}

