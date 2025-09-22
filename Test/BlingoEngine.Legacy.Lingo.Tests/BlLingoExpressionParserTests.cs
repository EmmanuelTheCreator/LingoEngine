using BlingoEngine.Legacy.Lingo.Syntax;
using BlingoEngine.Legacy.Lingo.Syntax.Expressions;
using FluentAssertions;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class BlLingoExpressionParserTests
{
    private readonly BlLingoTokenizer _tokenizer = new();
    private readonly BlLingoExpressionParser _parser = new();

    [Fact]
    public void ParseExpression_RespectsMultiplicativePrecedence()
    {
        const string source = "1 + 2 * 3";

        var tokens = _tokenizer.Tokenize(source);
        var expression = _parser.ParseExpression(tokens);

        var addition = expression.Should().BeOfType<BlBinaryExpression>().Subject;
        addition.OperatorKind.Should().Be(BlBinaryOperatorKind.Add);

        var leftLiteral = addition.Left.Should().BeOfType<BlLiteralExpression>().Subject;
        leftLiteral.Kind.Should().Be(BlExpressionKind.NumberLiteral);
        leftLiteral.LiteralToken.ValueText.Should().Be("1");

        var right = addition.Right.Should().BeOfType<BlBinaryExpression>().Subject;
        right.OperatorKind.Should().Be(BlBinaryOperatorKind.Multiply);

        right.Left.Should().BeOfType<BlLiteralExpression>().Which.LiteralToken.ValueText.Should().Be("2");
        right.Right.Should().BeOfType<BlLiteralExpression>().Which.LiteralToken.ValueText.Should().Be("3");
    }

    [Fact]
    public void ParseExpression_HonorsParentheses()
    {
        const string source = "1 * (2 + 3)";

        var tokens = _tokenizer.Tokenize(source);
        var expression = _parser.ParseExpression(tokens);

        var multiply = expression.Should().BeOfType<BlBinaryExpression>().Subject;
        multiply.OperatorKind.Should().Be(BlBinaryOperatorKind.Multiply);

        var right = multiply.Right.Should().BeOfType<BlGroupingExpression>().Subject;
        var groupedAddition = right.InnerExpression.Should().BeOfType<BlBinaryExpression>().Subject;
        groupedAddition.OperatorKind.Should().Be(BlBinaryOperatorKind.Add);

        groupedAddition.Left.Should().BeOfType<BlLiteralExpression>().Which.LiteralToken.ValueText.Should().Be("2");
        groupedAddition.Right.Should().BeOfType<BlLiteralExpression>().Which.LiteralToken.ValueText.Should().Be("3");
    }

    [Fact]
    public void ParseExpression_ParsesUnaryOperators()
    {
        const string source = "-5 + not flag";

        var tokens = _tokenizer.Tokenize(source);
        var expression = _parser.ParseExpression(tokens);

        var addition = expression.Should().BeOfType<BlBinaryExpression>().Subject;
        addition.OperatorKind.Should().Be(BlBinaryOperatorKind.Add);

        var negative = addition.Left.Should().BeOfType<BlUnaryExpression>().Subject;
        negative.OperatorKind.Should().Be(BlUnaryOperatorKind.Negative);
        negative.Operand.Should().BeOfType<BlLiteralExpression>().Which.LiteralToken.ValueText.Should().Be("5");

        var notExpression = addition.Right.Should().BeOfType<BlUnaryExpression>().Subject;
        notExpression.OperatorKind.Should().Be(BlUnaryOperatorKind.LogicalNot);
        notExpression.Operand.Should().BeOfType<BlIdentifierExpression>().Which.IdentifierToken.ValueText.Should().Be("flag");
    }

    [Fact]
    public void ParseExpression_TreatsExponentAsRightAssociative()
    {
        const string source = "2 ^ 3 ^ 2";

        var tokens = _tokenizer.Tokenize(source);
        var expression = _parser.ParseExpression(tokens);

        var outerPower = expression.Should().BeOfType<BlBinaryExpression>().Subject;
        outerPower.OperatorKind.Should().Be(BlBinaryOperatorKind.Power);

        outerPower.Left.Should().BeOfType<BlLiteralExpression>().Which.LiteralToken.ValueText.Should().Be("2");

        var innerPower = outerPower.Right.Should().BeOfType<BlBinaryExpression>().Subject;
        innerPower.OperatorKind.Should().Be(BlBinaryOperatorKind.Power);

        innerPower.Left.Should().BeOfType<BlLiteralExpression>().Which.LiteralToken.ValueText.Should().Be("3");
        innerPower.Right.Should().BeOfType<BlLiteralExpression>().Which.LiteralToken.ValueText.Should().Be("2");
    }

    [Fact]
    public void ParseExpression_RecognizesStringConcatenation()
    {
        const string source = "\"hello\" & \"world\"";

        var tokens = _tokenizer.Tokenize(source);
        var expression = _parser.ParseExpression(tokens);

        var concatenation = expression.Should().BeOfType<BlBinaryExpression>().Subject;
        concatenation.OperatorKind.Should().Be(BlBinaryOperatorKind.Concatenate);

        var leftLiteral = concatenation.Left.Should().BeOfType<BlLiteralExpression>().Subject;
        leftLiteral.Kind.Should().Be(BlExpressionKind.StringLiteral);
        leftLiteral.LiteralToken.ValueText.Should().Be("hello");

        var rightLiteral = concatenation.Right.Should().BeOfType<BlLiteralExpression>().Subject;
        rightLiteral.Kind.Should().Be(BlExpressionKind.StringLiteral);
        rightLiteral.LiteralToken.ValueText.Should().Be("world");
    }

    [Fact]
    public void ParseExpression_RecognizesStringConcatenationWithSpace()
    {
        const string source = "\"hello\" && \"world\"";

        var tokens = _tokenizer.Tokenize(source);
        var expression = _parser.ParseExpression(tokens);

        var concatenation = expression.Should().BeOfType<BlBinaryExpression>().Subject;
        concatenation.OperatorKind.Should().Be(BlBinaryOperatorKind.ConcatenateWithSpace);

        var leftLiteral = concatenation.Left.Should().BeOfType<BlLiteralExpression>().Subject;
        leftLiteral.Kind.Should().Be(BlExpressionKind.StringLiteral);
        leftLiteral.LiteralToken.ValueText.Should().Be("hello");

        var rightLiteral = concatenation.Right.Should().BeOfType<BlLiteralExpression>().Subject;
        rightLiteral.Kind.Should().Be(BlExpressionKind.StringLiteral);
        rightLiteral.LiteralToken.ValueText.Should().Be("world");
    }
}
