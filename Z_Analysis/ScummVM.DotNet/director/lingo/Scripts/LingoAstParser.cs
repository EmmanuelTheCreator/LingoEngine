
namespace Director.Scripts
{
    /// <summary>
    /// Parses a Lingo script source string into an abstract syntax tree (AST).
    /// </summary>
    public class LingoAstParser
    {
        private Tokenizer _tokenizer = null!;
        private LingoToken _currentToken;

        public Node Parse(string source)
        {
            _tokenizer = new Tokenizer(source);
            AdvanceToken();
            // Placeholder for now
            return ParseHandlerBody();
        }
        private void AdvanceToken() => _currentToken = _tokenizer.NextToken();
        private bool Match(LingoTokenType type)
        {
            if (_currentToken.Type != type) return false;
            AdvanceToken();
            return true;
        }

        private LingoToken Expect(LingoTokenType type)
        {
            if (_currentToken.Type != type)
                throw new Exception($"Expected token {type}, but got {_currentToken.Type} at line {_currentToken.Line}");

            var token = _currentToken;
            AdvanceToken();
            return token;
        }


        private Node ParseHandlerBody()
        {
            var block = new BlockNode();

            while (_currentToken.Type != LingoTokenType.Eof)
            {
                switch (_currentToken.Type)
                {
                    case LingoTokenType.Identifier:
                        block.Children.Add(ParseCallOrAssignment());
                        break;

                    case LingoTokenType.If:
                    case LingoTokenType.Put:
                    case LingoTokenType.Exit:
                    case LingoTokenType.Next:
                        block.Children.Add(ParseKeywordStatement());
                        break;

                    default:
                        AdvanceToken(); // skip unrecognized
                        break;
                }
            }

            return block;
        }


        private Node ParseCallOrAssignment()
        {
            var ident = Expect(LingoTokenType.Identifier);

            if (Match(LingoTokenType.Equals))
            {
                var value = ParseExpression();
                return new AssignmentStmtNode
                {
                    Target = new DatumNode(new Datum(ident.Lexeme, isSymbol: true)),
                    Value = value
                };
            }

            return new CallNode { Name = ident.Lexeme };
        }


        private Node ParseKeywordStatement()
        {
            var keywordToken = _currentToken;
            AdvanceToken(); // consume the keyword

            switch (keywordToken.Type)
            {
                case LingoTokenType.Exit:
                    if (_currentToken.Type == LingoTokenType.Repeat)
                    {
                        AdvanceToken(); // consume 'repeat'

                        if (_currentToken.Type == LingoTokenType.If)
                        {
                            AdvanceToken(); // consume 'if'
                            var condition = ParseExpression();
                            return new ExitRepeatIfStmtNode(condition);
                        }

                        return new ExitRepeatStmtNode();
                    }
                    return new ExitStmtNode();

                case LingoTokenType.Next:
                    if (_currentToken.Type == LingoTokenType.Repeat)
                    {
                        AdvanceToken(); // consume 'repeat'

                        if (_currentToken.Type == LingoTokenType.If)
                        {
                            AdvanceToken(); // consume 'if'
                            var condition = ParseExpression();
                            return new NextRepeatIfStmtNode(condition);
                        }

                        return new NextRepeatStmtNode();
                    }

                    return new NextStmtNode(); // fallback, if you want to allow generic "next"

                case LingoTokenType.Put:
                    return ParsePutStatement();

                case LingoTokenType.If:
                    return ParseIfStatement();

                default:
                    return new DatumNode(new Datum(keywordToken.Lexeme));
            }
        }

        private Node ParseExpression()
        {
            switch (_currentToken.Type)
            {
                case LingoTokenType.Number:
                case LingoTokenType.String:
                    var literal = new DatumNode(new Datum(_currentToken.Lexeme));
                    AdvanceToken();
                    return literal;

                case LingoTokenType.Identifier:
                    var ident = new DatumNode(new Datum(_currentToken.Lexeme, isSymbol: true));
                    AdvanceToken();
                    return ident;

                default:
                    // fallback: unknown expressions are treated as raw datum
                    var fallback = new DatumNode(new Datum(_currentToken.Lexeme));
                    AdvanceToken();
                    return fallback;
            }
        }

        private Node ParsePutStatement()
        {
            // At this point, the "put" keyword has already been consumed

            // Parse the value expression (what to put)
            var value = ParseExpression();

            // Expect and consume the "into" keyword
            Expect(LingoTokenType.Into);

            // Parse the destination expression (where to put)
            var target = ParseExpression();

            return new PutStmtNode(value, target);
        }


        private Node ParseIfStatement()
        {
            // "if" already consumed

            var condition = ParseExpression();
            Expect(LingoTokenType.Then);

            var thenBlock = ParseBlock();

            Node? elseBlock = null;
            if (_currentToken.Type == LingoTokenType.Else)
            {
                AdvanceToken(); // consume 'else'
                elseBlock = ParseBlock();
            }

            Expect(LingoTokenType.End);
            Expect(LingoTokenType.If);

            return new IfStmtNode(condition, thenBlock, elseBlock);
        }

        private BlockNode ParseBlock()
        {
            var block = new BlockNode();

            while (_currentToken.Type != LingoTokenType.End &&
                   _currentToken.Type != LingoTokenType.Else &&
                   _currentToken.Type != LingoTokenType.Eof)
            {
                switch (_currentToken.Type)
                {
                    case LingoTokenType.Identifier:
                        block.Children.Add(ParseCallOrAssignment());
                        break;

                    case LingoTokenType.If:
                    case LingoTokenType.Put:
                    case LingoTokenType.Exit:
                    case LingoTokenType.Next:
                        block.Children.Add(ParseKeywordStatement());
                        break;

                    default:
                        AdvanceToken(); // Skip unknown token
                        break;
                }
            }

            return block;
        }

        private Node ParseRepeatStatement()
        {
            // "repeat" already consumed

            if (_currentToken.Type == LingoTokenType.While)
            {
                AdvanceToken();
                var condition = ParseExpression();
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new RepeatWhileStmtNode(condition, body);
            }
            else if (_currentToken.Type == LingoTokenType.Until)
            {
                AdvanceToken();
                var condition = ParseExpression();
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new RepeatUntilStmtNode(condition, body);
            }
            else if (_currentToken.Type == LingoTokenType.With)
            {
                AdvanceToken();
                var varToken = Expect(LingoTokenType.Identifier);
                Expect(LingoTokenType.Equals);
                var start = ParseExpression();
                Expect(LingoTokenType.To);
                var end = ParseExpression();
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new RepeatWithStmtNode(varToken.Lexeme, start, end, body);
            }
            else if (_currentToken.Type == LingoTokenType.Number)
            {
                // repeat <number> times
                var countExpr = ParseExpression();
                Expect(LingoTokenType.Times);
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new RepeatTimesStmtNode(countExpr, body);
            }
            else
            {
                // repeat, repeat forever, or loop
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new RepeatForeverStmtNode(body);
            }
        }

    }



}



