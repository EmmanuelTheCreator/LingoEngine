using System.Linq;
using BlingoEngine.Legacy.Lingo.Syntax;
using FluentAssertions;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class BlLingoTokenizerTests
{
    private readonly BlLingoTokenizer _tokenizer = new();

    [Fact]
    public void Tokenize_GlobalAssignment_ProducesExpectedTokens()
    {
        const string script = """
global gCounter

on mouseDown
  gCounter = 10 + 20
end
""";

        var tokens = _tokenizer.Tokenize(script);

        tokens.Select(token => token.Kind).Should().Equal(
            BlSyntaxKind.KeywordToken,
            BlSyntaxKind.IdentifierToken,
            BlSyntaxKind.KeywordToken,
            BlSyntaxKind.IdentifierToken,
            BlSyntaxKind.IdentifierToken,
            BlSyntaxKind.OperatorToken,
            BlSyntaxKind.NumberToken,
            BlSyntaxKind.OperatorToken,
            BlSyntaxKind.NumberToken,
            BlSyntaxKind.KeywordToken,
            BlSyntaxKind.EndOfFileToken);

        tokens[0].ValueText.Should().Be("global");
        tokens[1].ValueText.Should().Be("gCounter");
        tokens[3].ValueText.Should().Be("mouseDown");
        tokens[4].ValueText.Should().Be("gCounter");
        tokens[6].ValueText.Should().Be("10");
        tokens[8].ValueText.Should().Be("20");
    }

    [Fact]
    public void Tokenize_StringAndSymbol_ProvidesDecodedValueText()
    {
        const string script = "put \"Hello\" & #world";

        var tokens = _tokenizer.Tokenize(script);

        tokens.Select(token => token.Kind).Should().Equal(
            BlSyntaxKind.KeywordToken,
            BlSyntaxKind.StringLiteralToken,
            BlSyntaxKind.OperatorToken,
            BlSyntaxKind.SymbolToken,
            BlSyntaxKind.EndOfFileToken);

        tokens[1].ValueText.Should().Be("Hello");
        tokens[3].ValueText.Should().Be("world");
    }

    [Fact]
    public void Tokenize_PreservesCommentsAndTracksLineInformation()
    {
        const string script = "on mouseDown\n  -- greet the visitor\n  put \"hi\"\nend";

        var tokens = _tokenizer.Tokenize(script);

        tokens.Select(token => token.Kind).Should().Equal(
            BlSyntaxKind.KeywordToken,
            BlSyntaxKind.IdentifierToken,
            BlSyntaxKind.KeywordToken,
            BlSyntaxKind.StringLiteralToken,
            BlSyntaxKind.KeywordToken,
            BlSyntaxKind.EndOfFileToken);

        var putToken = tokens[2];
        putToken.ValueText.Should().Be("put");
        putToken.LineSpan.Start.Line.Should().Be(2);
        putToken.LineSpan.Start.Character.Should().Be(2);
        putToken.LeadingTrivia.Single(trivia => trivia.Kind == BlSyntaxKind.CommentTrivia)
            .ValueText.Should().Be("greet the visitor");

        var stringToken = tokens[3];
        stringToken.ValueText.Should().Be("hi");
        stringToken.LineSpan.Start.Line.Should().Be(2);
        stringToken.LineSpan.Start.Character.Should().Be(6);
    }

    [Fact]
    public void Tokenize_ProducesCommentTriviaForInlineComment()
    {
        const string script = "put 1 -- a comment\nput 2";

        var tokens = _tokenizer.Tokenize(script);

        tokens.Select(token => token.Kind).Should().Equal(
            BlSyntaxKind.KeywordToken,
            BlSyntaxKind.NumberToken,
            BlSyntaxKind.KeywordToken,
            BlSyntaxKind.NumberToken,
            BlSyntaxKind.EndOfFileToken);

        var numberToken = tokens[1];
        numberToken.TrailingTrivia.Single(trivia => trivia.Kind == BlSyntaxKind.CommentTrivia)
            .ValueText.Should().Be("a comment");
    }

    [Fact]
    public void Tokenize_HandlesLineContinuationCharacter()
    {
        const string script = "put \"Hello\" & \u00AC\n\"World\"";

        var tokens = _tokenizer.Tokenize(script);

        tokens.Select(token => token.Kind).Should().Equal(
            BlSyntaxKind.KeywordToken,
            BlSyntaxKind.StringLiteralToken,
            BlSyntaxKind.OperatorToken,
            BlSyntaxKind.StringLiteralToken,
            BlSyntaxKind.EndOfFileToken);

        var operatorToken = tokens[2];
        operatorToken.TrailingTrivia.Single(trivia => trivia.Kind == BlSyntaxKind.LineContinuationTrivia)
            .Text.Should().Contain("\u00AC");

        tokens[3].LeadingTrivia.Should().BeEmpty();
    }
}
