using System;
using System.Collections.Generic;

namespace BlingoEngine.Legacy.Lingo.Syntax;

/// <summary>
/// Converts Lingo source text into a rich stream of tokens annotated with trivia and positions.
/// </summary>
public sealed class BlLingoTokenizer
{
    /// <summary>
    /// Tokenizes the supplied source text.
    /// </summary>
    public IReadOnlyList<BlSyntaxToken> Tokenize(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var scanner = new Scanner(source);
        scanner.Scan();
        return scanner.Tokens;
    }

    /// <summary>
    /// Implements the actual scanning logic over the source text.
    /// </summary>
    private sealed class Scanner
    {
        private static readonly string[] s_multiCharacterOperators =
        {
            "<=",
            ">=",
            "<>",
            "&&",
        };

        private static readonly HashSet<char> s_operatorStarts = new()
        {
            '+',
            '-',
            '*',
            '/',
            '^',
            '=',
            '<',
            '>',
            '&',
        };

        private readonly string _text;
        private readonly List<BlSyntaxToken> _tokens = new();
        private int _position;
        private int _line;
        private int _column;

        internal Scanner(string text)
        {
            _text = text;
        }

        public IReadOnlyList<BlSyntaxToken> Tokens => _tokens;

        /// <summary>
        /// Performs a full scan of the text, emitting tokens and an end-of-file marker.
        /// </summary>
        public void Scan()
        {
            var leadingTrivia = ReadLeadingTrivia();

            while (!IsAtEnd)
            {
                var token = ReadToken(leadingTrivia);
                _tokens.Add(token);
                leadingTrivia = ReadLeadingTrivia();
            }

            var span = new BlTextSpan(_position, 0);
            var position = new BlLinePosition(_line, _column);
            var lineSpan = new BlLinePositionSpan(position, position);
            _tokens.Add(new BlSyntaxToken(
                BlSyntaxKind.EndOfFileToken,
                string.Empty,
                string.Empty,
                span,
                lineSpan,
                leadingTrivia,
                Array.Empty<BlSyntaxTrivia>()));
        }

        /// <summary>
        /// Reads a single token along with its trailing trivia.
        /// </summary>
        private BlSyntaxToken ReadToken(IReadOnlyList<BlSyntaxTrivia> leadingTrivia)
        {
            var start = _position;
            var startLine = _line;
            var startColumn = _column;
            var current = Peek();

            if (current == '#')
            {
                Advance();
                while (IsIdentifierPart(Peek()))
                {
                    Advance();
                }

                var text = _text.Substring(start, _position - start);
                var kind = text.Length > 1 ? BlSyntaxKind.SymbolToken : BlSyntaxKind.HashToken;
                var trailing = ReadTrailingTrivia();
                return CreateToken(kind, text, start, startLine, startColumn, leadingTrivia, trailing);
            }

            if (IsIdentifierStart(current))
            {
                Advance();
                while (IsIdentifierPart(Peek()))
                {
                    Advance();
                }

                var text = _text.Substring(start, _position - start);
                var kind = BlLingoKeywordFacts.IsKeyword(text)
                    ? BlSyntaxKind.KeywordToken
                    : BlSyntaxKind.IdentifierToken;
                var trailing = ReadTrailingTrivia();
                return CreateToken(kind, text, start, startLine, startColumn, leadingTrivia, trailing);
            }

            if (char.IsDigit(current) || (current == '.' && char.IsDigit(Peek(1))))
            {
                ReadNumber(current);
                var text = _text.Substring(start, _position - start);
                var trailing = ReadTrailingTrivia();
                return CreateToken(BlSyntaxKind.NumberToken, text, start, startLine, startColumn, leadingTrivia, trailing);
            }

            if (current == '"')
            {
                ReadStringLiteral();
                var text = _text.Substring(start, _position - start);
                var trailing = ReadTrailingTrivia();
                return CreateToken(BlSyntaxKind.StringLiteralToken, text, start, startLine, startColumn, leadingTrivia, trailing);
            }

            if (IsOperatorStart(current))
            {
                var text = ReadOperatorText();
                var trailing = ReadTrailingTrivia();
                return CreateToken(BlSyntaxKind.OperatorToken, text, start, startLine, startColumn, leadingTrivia, trailing);
            }

            if (TryGetPunctuationKind(current, out var punctuationKind))
            {
                Advance();
                var text = _text.Substring(start, _position - start);
                var trailing = ReadTrailingTrivia();
                return CreateToken(punctuationKind, text, start, startLine, startColumn, leadingTrivia, trailing);
            }

            Advance();
            var unknownText = _text.Substring(start, _position - start);
            var unknownTrailing = ReadTrailingTrivia();
            return CreateToken(BlSyntaxKind.UnknownToken, unknownText, start, startLine, startColumn, leadingTrivia, unknownTrailing);
        }

        /// <summary>
        /// Reads the textual representation of an operator, honoring multi-character combinations.
        /// </summary>
        private string ReadOperatorText()
        {
            foreach (var op in s_multiCharacterOperators)
            {
                if (Matches(op))
                {
                    for (var i = 0; i < op.Length; i++)
                    {
                        Advance();
                    }

                    return op;
                }
            }

            var current = Peek();
            Advance();
            return current == '\0' ? string.Empty : current.ToString();
        }

        /// <summary>
        /// Attempts to map punctuation characters to their token kind.
        /// </summary>
        private static bool TryGetPunctuationKind(char character, out BlSyntaxKind kind)
        {
            kind = character switch
            {
                '(' => BlSyntaxKind.LeftParenthesisToken,
                ')' => BlSyntaxKind.RightParenthesisToken,
                '{' => BlSyntaxKind.LeftBraceToken,
                '}' => BlSyntaxKind.RightBraceToken,
                '[' => BlSyntaxKind.LeftBracketToken,
                ']' => BlSyntaxKind.RightBracketToken,
                ',' => BlSyntaxKind.CommaToken,
                ':' => BlSyntaxKind.ColonToken,
                ';' => BlSyntaxKind.SemicolonToken,
                '.' => BlSyntaxKind.PeriodToken,
                '?' => BlSyntaxKind.QuestionToken,
                _ => BlSyntaxKind.UnknownToken,
            };

            return kind != BlSyntaxKind.UnknownToken;
        }

        /// <summary>
        /// Consumes trivia that precedes the next token, including comments and whitespace.
        /// </summary>
        private IReadOnlyList<BlSyntaxTrivia> ReadLeadingTrivia()
        {
            if (IsAtEnd)
            {
                return Array.Empty<BlSyntaxTrivia>();
            }

            List<BlSyntaxTrivia>? trivias = null;

            while (!IsAtEnd)
            {
                var current = Peek();
                if (current == '\r' || current == '\n')
                {
                    trivias ??= new List<BlSyntaxTrivia>();
                    var start = _position;
                    var startLine = _line;
                    var startColumn = _column;
                    var text = ReadNewLine();
                    trivias.Add(CreateTrivia(BlSyntaxKind.NewLineTrivia, text, start, startLine, startColumn));
                    continue;
                }

                if (IsLineContinuationStart())
                {
                    trivias ??= new List<BlSyntaxTrivia>();
                    var start = _position;
                    var startLine = _line;
                    var startColumn = _column;
                    var text = ReadLineContinuation();
                    trivias.Add(CreateTrivia(BlSyntaxKind.LineContinuationTrivia, text, start, startLine, startColumn));
                    continue;
                }

                if (IsLineCommentStart(current))
                {
                    trivias ??= new List<BlSyntaxTrivia>();
                    var start = _position;
                    var startLine = _line;
                    var startColumn = _column;
                    ReadLineComment();
                    var text = _text.Substring(start, _position - start);
                    trivias.Add(CreateTrivia(BlSyntaxKind.CommentTrivia, text, start, startLine, startColumn));
                    continue;
                }

                if (char.IsWhiteSpace(current))
                {
                    trivias ??= new List<BlSyntaxTrivia>();
                    var start = _position;
                    var startLine = _line;
                    var startColumn = _column;
                    ReadWhitespace();
                    var text = _text.Substring(start, _position - start);
                    trivias.Add(CreateTrivia(BlSyntaxKind.WhitespaceTrivia, text, start, startLine, startColumn));
                    continue;
                }

                break;
            }

            return trivias is null ? Array.Empty<BlSyntaxTrivia>() : trivias;
        }

        /// <summary>
        /// Consumes trivia that trails a token until a newline or end of input is reached.
        /// </summary>
        private IReadOnlyList<BlSyntaxTrivia> ReadTrailingTrivia()
        {
            if (IsAtEnd)
            {
                return Array.Empty<BlSyntaxTrivia>();
            }

            List<BlSyntaxTrivia>? trivias = null;

            while (!IsAtEnd)
            {
                var current = Peek();
                if (current == '\r' || current == '\n')
                {
                    break;
                }

                if (IsLineContinuationStart())
                {
                    trivias ??= new List<BlSyntaxTrivia>();
                    var start = _position;
                    var startLine = _line;
                    var startColumn = _column;
                    var text = ReadLineContinuation();
                    trivias.Add(CreateTrivia(BlSyntaxKind.LineContinuationTrivia, text, start, startLine, startColumn));
                    break;
                }

                if (IsLineCommentStart(current))
                {
                    trivias ??= new List<BlSyntaxTrivia>();
                    var start = _position;
                    var startLine = _line;
                    var startColumn = _column;
                    ReadLineComment();
                    var text = _text.Substring(start, _position - start);
                    trivias.Add(CreateTrivia(BlSyntaxKind.CommentTrivia, text, start, startLine, startColumn));
                    continue;
                }

                if (char.IsWhiteSpace(current))
                {
                    trivias ??= new List<BlSyntaxTrivia>();
                    var start = _position;
                    var startLine = _line;
                    var startColumn = _column;
                    ReadWhitespace();
                    var text = _text.Substring(start, _position - start);
                    trivias.Add(CreateTrivia(BlSyntaxKind.WhitespaceTrivia, text, start, startLine, startColumn));
                    continue;
                }

                break;
            }

            return trivias is null ? Array.Empty<BlSyntaxTrivia>() : trivias;
        }

        private void ReadWhitespace()
        {
            while (!IsAtEnd)
            {
                var current = Peek();
                if (current == '\r' || current == '\n')
                {
                    break;
                }

                if (!char.IsWhiteSpace(current))
                {
                    break;
                }

                Advance();
            }
        }

        private void ReadLineComment()
        {
            Advance();
            Advance();

            while (!IsAtEnd)
            {
                var current = Peek();
                if (current == '\r' || current == '\n')
                {
                    break;
                }

                Advance();
            }
        }

        private string ReadNewLine()
        {
            var start = _position;
            Advance();
            return _text.Substring(start, _position - start);
        }

        private string ReadLineContinuation()
        {
            var start = _position;
            Advance();

            while (!IsAtEnd)
            {
                var current = Peek();
                if (current == ' ' || current == '\t')
                {
                    Advance();
                    continue;
                }

                break;
            }

            if (Peek() is '\r' or '\n')
            {
                Advance();
            }

            while (!IsAtEnd)
            {
                var current = Peek();
                if (current == ' ' || current == '\t')
                {
                    Advance();
                    continue;
                }

                break;
            }

            return _text.Substring(start, _position - start);
        }

        private bool IsLineCommentStart(char current) => current == '-' && Peek(1) == '-';

        private bool IsLineContinuationStart()
        {
            var current = Peek();
            if (!IsLineContinuationChar(current))
            {
                return false;
            }

            var offset = 1;
            while (true)
            {
                var lookahead = Peek(offset);
                if (lookahead == '\0')
                {
                    return false;
                }

                if (lookahead == '\r' || lookahead == '\n')
                {
                    return true;
                }

                if (lookahead == ' ' || lookahead == '\t')
                {
                    offset++;
                    continue;
                }

                return false;
            }
        }

        private static bool IsLineContinuationChar(char character) => character == '\u00AC' || character == '\\';

        private BlSyntaxTrivia CreateTrivia(BlSyntaxKind kind, string text, int start, int startLine, int startColumn)
        {
            var span = new BlTextSpan(start, _position - start);
            var startPosition = new BlLinePosition(startLine, startColumn);
            var endPosition = new BlLinePosition(_line, _column);
            var lineSpan = new BlLinePositionSpan(startPosition, endPosition);
            var valueText = GetTriviaValueText(kind, text);
            return new BlSyntaxTrivia(kind, text, valueText, span, lineSpan);
        }

        private static string GetTriviaValueText(BlSyntaxKind kind, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return kind switch
            {
                BlSyntaxKind.CommentTrivia => GetCommentValueText(text),
                _ => string.Empty,
            };
        }

        private static string GetCommentValueText(string text)
        {
            if (text.StartsWith("--", StringComparison.Ordinal))
            {
                return text.Substring(2).TrimStart();
            }

            return text.Trim();
        }

        private void ReadNumber(char firstChar)
        {
            if (firstChar == '.')
            {
                Advance();
                while (char.IsDigit(Peek()))
                {
                    Advance();
                }
            }
            else
            {
                while (char.IsDigit(Peek()))
                {
                    Advance();
                }

                if (Peek() == '.' && char.IsDigit(Peek(1)))
                {
                    Advance();
                    while (char.IsDigit(Peek()))
                    {
                        Advance();
                    }
                }
            }

            if (Peek() is 'e' or 'E')
            {
                var next = Peek(1);
                var afterNext = Peek(2);
                if (char.IsDigit(next) || ((next is '+' or '-') && char.IsDigit(afterNext)))
                {
                    Advance();
                    if (Peek() is '+' or '-')
                    {
                        Advance();
                    }

                    while (char.IsDigit(Peek()))
                    {
                        Advance();
                    }
                }
            }
        }

        private void ReadStringLiteral()
        {
            Advance();

            while (!IsAtEnd)
            {
                var current = Peek();
                if (current == '"')
                {
                    if (Peek(1) == '"')
                    {
                        Advance();
                        Advance();
                        continue;
                    }

                    Advance();
                    break;
                }

                if (current == '\r' || current == '\n')
                {
                    break;
                }

                Advance();
            }
        }

        private static bool IsIdentifierStart(char character) => char.IsLetter(character) || character == '_';

        private static bool IsIdentifierPart(char character) => char.IsLetterOrDigit(character) || character == '_';

        private static bool IsOperatorStart(char character) => s_operatorStarts.Contains(character);

        private bool Matches(string text)
        {
            if (_position + text.Length > _text.Length)
            {
                return false;
            }

            for (var i = 0; i < text.Length; i++)
            {
                if (_text[_position + i] != text[i])
                {
                    return false;
                }
            }

            return true;
        }

        private BlSyntaxToken CreateToken(
            BlSyntaxKind kind,
            string text,
            int start,
            int startLine,
            int startColumn,
            IReadOnlyList<BlSyntaxTrivia> leadingTrivia,
            IReadOnlyList<BlSyntaxTrivia> trailingTrivia)
        {
            var span = new BlTextSpan(start, _position - start);
            var startPosition = new BlLinePosition(startLine, startColumn);
            var endPosition = new BlLinePosition(_line, _column);
            var lineSpan = new BlLinePositionSpan(startPosition, endPosition);
            var valueText = GetValueText(kind, text);
            return new BlSyntaxToken(kind, text, valueText, span, lineSpan, leadingTrivia, trailingTrivia);
        }

        private static string GetValueText(BlSyntaxKind kind, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return kind switch
            {
                BlSyntaxKind.StringLiteralToken => DecodeStringLiteral(text),
                BlSyntaxKind.SymbolToken when text.Length > 0 && text[0] == '#' => text.Substring(1),
                _ => text,
            };
        }

        private static string DecodeStringLiteral(string text)
        {
            var startIndex = text.Length > 0 && text[0] == '"' ? 1 : 0;
            var length = text.Length - startIndex;
            if (length > 0 && text[^1] == '"')
            {
                length--;
            }

            if (length <= 0)
            {
                return string.Empty;
            }

            var inner = text.Substring(startIndex, length);
            var doubleQuote = new string('"', 2);
            var singleQuote = new string('"', 1);
            return inner.Replace(doubleQuote, singleQuote);
        }

        private char Peek(int offset = 0)
        {
            var index = _position + offset;
            if (index >= _text.Length)
            {
                return '\0';
            }

            return _text[index];
        }

        private bool IsAtEnd => _position >= _text.Length;

        private void Advance()
        {
            if (IsAtEnd)
            {
                return;
            }

            var current = _text[_position];
            _position++;

            if (current == '\r')
            {
                if (!IsAtEnd && _text[_position] == '\n')
                {
                    _position++;
                }

                _line++;
                _column = 0;
                return;
            }

            if (current == '\n')
            {
                _line++;
                _column = 0;
                return;
            }

            _column++;
        }
    }
}
