﻿namespace LingoEngine.Lingo.Core.Tokenizer
{
    /// <summary>
    /// Parses a Lingo script source string into an abstract syntax tree (AST).
    /// </summary>
    public class LingoAstParser
    {
        private LingoTokenizer _tokenizer = null!;
        private LingoToken _currentToken;

        public LingoNode Parse(string source)
        {
            _tokenizer = new LingoTokenizer(source);
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


        private LingoNode ParseHandlerBody()
        {
            var block = new LingoBlockNode();

            while (_currentToken.Type != LingoTokenType.Eof)
            {
                switch (_currentToken.Type)
                {
                    case LingoTokenType.Identifier:
                        block.Children.Add(ParseCallOrAssignment());
                        break;
                    case LingoTokenType.On:
                        AdvanceToken();
                        if (_currentToken.Type == LingoTokenType.Identifier)
                            AdvanceToken();
                        break;

                    case LingoTokenType.If:
                    case LingoTokenType.Put:
                    case LingoTokenType.Exit:
                    case LingoTokenType.Next:
                    case LingoTokenType.Repeat:
                        block.Children.Add(ParseKeywordStatement());
                        break;

                    default:
                        AdvanceToken(); // skip unrecognized
                        break;
                }
            }

            return block;
        }


        private LingoNode ParseCallOrAssignment()
        {
            var ident = Expect(LingoTokenType.Identifier);

            if (string.Equals(ident.Lexeme, "sendSprite", System.StringComparison.OrdinalIgnoreCase))
            {
                var sprite = ParseExpression();
                if (Match(LingoTokenType.Comma))
                {
                    var message = ParseExpression();
                    return new LingoSendSpriteStmtNode { Sprite = sprite, Message = message };
                }
            }

            if (Match(LingoTokenType.Equals))
            {
                var value = ParseExpression();
                return new LingoAssignmentStmtNode
                {
                    Target = new LingoDatumNode(new LingoDatum(ident.Lexeme, isSymbol: true)),
                    Value = value
                };
            }

            return new LingoCallNode { Name = ident.Lexeme };
        }


        private LingoNode ParseKeywordStatement()
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
                            return new LingoExitRepeatIfStmtNode(condition);
                        }

                        return new LingoExitRepeatStmtNode();
                    }
                    return new LingoExitStmtNode();

                case LingoTokenType.Next:
                    if (_currentToken.Type == LingoTokenType.Repeat)
                    {
                        AdvanceToken(); // consume 'repeat'

                        if (_currentToken.Type == LingoTokenType.If)
                        {
                            AdvanceToken(); // consume 'if'
                            var condition = ParseExpression();
                            return new LingoNextRepeatIfStmtNode(condition);
                        }

                        return new LingoNextRepeatStmtNode();
                    }

                    return new LingoNextStmtNode(); // fallback, if you want to allow generic "next"

                case LingoTokenType.Put:
                    return ParsePutStatement();

                case LingoTokenType.If:
                    return ParseIfStatement();

                case LingoTokenType.Repeat:
                    return ParseRepeatStatement();

                default:
                    return new LingoDatumNode(new LingoDatum(keywordToken.Lexeme));
            }
        }

        private LingoNode ParseExpression()
        {
            switch (_currentToken.Type)
            {
                case LingoTokenType.Number:
                    var text = _currentToken.Lexeme;
                    LingoDatum datum;
                    if (text.StartsWith("$"))
                    {
                        if (int.TryParse(text[1..], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var hex))
                            datum = new LingoDatum(hex);
                        else
                            datum = new LingoDatum(text);
                    }
                    else if (text.Contains('.'))
                    {
                        if (float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f))
                            datum = new LingoDatum(f);
                        else
                            datum = new LingoDatum(text);
                    }
                    else if (int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var i))
                    {
                        datum = new LingoDatum(i);
                    }
                    else
                    {
                        datum = new LingoDatum(text);
                    }
                    AdvanceToken();
                    return new LingoDatumNode(datum);

                case LingoTokenType.String:
                    var strLiteral = new LingoDatumNode(new LingoDatum(_currentToken.Lexeme));
                    AdvanceToken();
                    return strLiteral;

                case LingoTokenType.Identifier:
                    var ident = new LingoDatumNode(new LingoDatum(_currentToken.Lexeme, isSymbol: true));
                    AdvanceToken();
                    return ident;

                default:
                    // fallback: unknown expressions are treated as raw datum
                    var fallback = new LingoDatumNode(new LingoDatum(_currentToken.Lexeme));
                    AdvanceToken();
                    return fallback;
            }
        }

        private LingoNode ParsePutStatement()
        {
            // At this point, the "put" keyword has already been consumed

            // Parse the value expression (what to put)
            var value = ParseExpression();

            // Expect and consume the "into" keyword
            Expect(LingoTokenType.Into);

            // Parse the destination expression (where to put)
            var target = ParseExpression();

            return new LingoPutStmtNode(value, target);
        }


        private LingoNode ParseIfStatement()
        {
            // "if" already consumed

            var condition = ParseExpression();
            Expect(LingoTokenType.Then);

            var thenBlock = ParseBlock();

            LingoNode? elseBlock = null;
            if (_currentToken.Type == LingoTokenType.Else)
            {
                AdvanceToken(); // consume 'else'
                elseBlock = ParseBlock();
            }

            Expect(LingoTokenType.End);
            Expect(LingoTokenType.If);

            return new LingoIfStmtNode(condition, thenBlock, elseBlock);
        }

        private LingoBlockNode ParseBlock()
        {
            var block = new LingoBlockNode();

            while (_currentToken.Type != LingoTokenType.End &&
                   _currentToken.Type != LingoTokenType.Else &&
                   _currentToken.Type != LingoTokenType.Eof)
            {
                switch (_currentToken.Type)
                {
                    case LingoTokenType.Identifier:
                        block.Children.Add(ParseCallOrAssignment());
                        break;
                    case LingoTokenType.On:
                        AdvanceToken();
                        if (_currentToken.Type == LingoTokenType.Identifier)
                            AdvanceToken();
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

        private LingoNode ParseRepeatStatement()
        {
            // "repeat" already consumed

            if (_currentToken.Type == LingoTokenType.While)
            {
                AdvanceToken();
                var condition = ParseExpression();
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new LingoRepeatWhileStmtNode(condition, body);
            }
            else if (_currentToken.Type == LingoTokenType.Until)
            {
                AdvanceToken();
                var condition = ParseExpression();
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new LingoRepeatUntilStmtNode(condition, body);
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
                return new LingoRepeatWithStmtNode(varToken.Lexeme, start, end, body);
            }
            else if (_currentToken.Type == LingoTokenType.Number)
            {
                // repeat <number> times
                var countExpr = ParseExpression();
                Expect(LingoTokenType.Times);
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new LingoRepeatTimesStmtNode(countExpr, body);
            }
            else
            {
                // repeat, repeat forever, or loop
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new LingoRepeatForeverStmtNode(body);
            }
        }

    }



}



