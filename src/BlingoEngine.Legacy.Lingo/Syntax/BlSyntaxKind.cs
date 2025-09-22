namespace BlingoEngine.Legacy.Lingo.Syntax;

/// <summary>
/// Enumerates the different kinds of tokens and trivia recognized by the tokenizer.
/// </summary>
public enum BlSyntaxKind
{
    EndOfFileToken,
    UnknownToken,

    IdentifierToken,
    KeywordToken,
    NumberToken,
    StringLiteralToken,
    SymbolToken,
    OperatorToken,

    HashToken,

    LeftParenthesisToken,
    RightParenthesisToken,
    LeftBraceToken,
    RightBraceToken,
    LeftBracketToken,
    RightBracketToken,
    CommaToken,
    ColonToken,
    SemicolonToken,
    PeriodToken,
    QuestionToken,

    WhitespaceTrivia,
    NewLineTrivia,
    LineContinuationTrivia,
    CommentTrivia,
}
