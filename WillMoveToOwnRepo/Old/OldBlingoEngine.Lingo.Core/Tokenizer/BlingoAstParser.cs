using System;
using System.Collections.Generic;

namespace OldBlingoEngine.Lingo.Core.Tokenizer
{
    /// <summary>
    /// Parses a Lingo script source string into an abstract syntax tree (AST).
    /// </summary>
    public class BlingoAstParser
    {
        private BlingoTokenizer _tokenizer = null!;
        private BlingoToken _currentToken;

        public BlingoNode Parse(string source)
        {
            _tokenizer = new BlingoTokenizer(source);
            AdvanceToken();
            var block = new BlingoBlockNode();
            while (_currentToken.Type != BlingoTokenType.Eof)
            {
                if (_currentToken.Type == BlingoTokenType.On)
                    block.Children.Add(ParseHandler());
                else if (_currentToken.Type == BlingoTokenType.Comment)
                {
                    block.Children.Add(new BlingoCommentNode { Text = _currentToken.Lexeme });
                    AdvanceToken();
                }
                else
                    block.Children.Add(ParseStatement());
            }
            return block;
        }
        private void AdvanceToken() => _currentToken = _tokenizer.NextToken();
        private bool Match(BlingoTokenType type)
        {
            if (_currentToken.Type != type) return false;
            AdvanceToken();
            return true;
        }

        private BlingoToken Expect(BlingoTokenType type)
        {
            if (_currentToken.Type != type)
                throw new Exception($"Expected token {type}, but got {_currentToken.Type} at line {_currentToken.Line}");

            var token = _currentToken;
            AdvanceToken();
            return token;
        }


        private BlingoNode ParseHandler()
        {
            Expect(BlingoTokenType.On);
            var nameTok = Expect(BlingoTokenType.Identifier);
            int declLine = nameTok.Line;
            var args = new List<string>();
            while (_currentToken.Line == declLine && _currentToken.Type != BlingoTokenType.Eof)
            {
                if (_currentToken.Type == BlingoTokenType.Identifier || _currentToken.Type == BlingoTokenType.Me)
                    args.Add(_currentToken.Lexeme);
                AdvanceToken();
            }
            var body = new BlingoBlockNode();
            while (_currentToken.Type != BlingoTokenType.End && _currentToken.Type != BlingoTokenType.Eof)
                body.Children.Add(ParseStatement());
            if (_currentToken.Type == BlingoTokenType.End)
                AdvanceToken();
            return new BlingoHandlerNode
            {
                Handler = new BlingoHandler { Name = nameTok.Lexeme, ArgumentNames = args },
                Block = body
            };
        }

        private BlingoNode ParseStatement()
        {
            switch (_currentToken.Type)
            {
                case BlingoTokenType.Comment:
                    var text = _currentToken.Lexeme;
                    AdvanceToken();
                    return new BlingoCommentNode { Text = text };
                case BlingoTokenType.Identifier:
                case BlingoTokenType.Me:
                    return ParseCallOrAssignment();
                case BlingoTokenType.Global:
                case BlingoTokenType.Property:
                case BlingoTokenType.Instance:
                    return ParseDeclarationStatement(_currentToken.Type);
                case BlingoTokenType.If:
                case BlingoTokenType.Put:
                case BlingoTokenType.Exit:
                case BlingoTokenType.Next:
                case BlingoTokenType.Repeat:
                case BlingoTokenType.Return:
                case BlingoTokenType.Case:
                    return ParseKeywordStatement();
                default:
                    AdvanceToken();
                    return new BlingoErrorNode();
            }
        }


        private BlingoNode ParseCallOrAssignment()
        {
            if (_currentToken.Type == BlingoTokenType.Identifier &&
                _currentToken.Lexeme.Equals("cursor", StringComparison.OrdinalIgnoreCase))
            {
                AdvanceToken();
                var value = ParseExpression();
                return new BlingoCursorStmtNode { Value = value };
            }

            if (_currentToken.Type == BlingoTokenType.Identifier &&
                _currentToken.Lexeme.Equals("go", StringComparison.OrdinalIgnoreCase))
            {
                AdvanceToken();
                Match(BlingoTokenType.To);
                var target = ParseExpression();
                return new BlingoGoToStmtNode { Target = target };
            }

            if (_currentToken.Type == BlingoTokenType.Identifier &&
                _currentToken.Lexeme.Equals("sendSprite", StringComparison.OrdinalIgnoreCase))
            {
                AdvanceToken();
                var hasParen = Match(BlingoTokenType.LeftParen);
                var sprite = ParseExpression();
                Expect(BlingoTokenType.Comma);
                var message = ParseExpression();
                var args = new List<BlingoNode>();
                while (Match(BlingoTokenType.Comma))
                {
                    args.Add(ParseExpression());
                }
                if (hasParen)
                    Expect(BlingoTokenType.RightParen);
                BlingoNode? argList = null;
                if (args.Count > 0)
                    argList = new BlingoDatumNode(new BlingoDatum(args, BlingoDatum.DatumType.ArgList));
                return new BlingoSendSpriteStmtNode { Sprite = sprite, Message = message, Arguments = argList };
            }
            var expr = ParseExpression(false);
            if (_currentToken.Type == BlingoTokenType.The && expr is BlingoVarNode methodVar)
            {
                AdvanceToken();
                var propTok = Expect(BlingoTokenType.Identifier);
                BlingoNode objExpr = new BlingoTheExprNode { Prop = propTok.Lexeme };
                while (Match(BlingoTokenType.Dot))
                {
                    var pTok = Expect(BlingoTokenType.Identifier);
                    objExpr = new BlingoObjPropExprNode
                    {
                        Object = objExpr,
                        Property = new BlingoVarNode { VarName = pTok.Lexeme }
                    };
                }
                Expect(BlingoTokenType.LeftParen);
                var argExpr = ParseExpression();
                Expect(BlingoTokenType.RightParen);
                return new BlingoCallNode
                {
                    Callee = new BlingoObjPropExprNode
                    {
                        Object = objExpr,
                        Property = new BlingoVarNode { VarName = methodVar.VarName }
                    },
                    Arguments = argExpr
                };
            }
            if (Match(BlingoTokenType.Equals))
            {
                var value = ParseExpression();
                return new BlingoAssignmentStmtNode { Target = expr, Value = value };
            }

            if (expr is BlingoVarNode v)
            {
                if (_currentToken.Type != BlingoTokenType.End &&
                    _currentToken.Type != BlingoTokenType.Else &&
                    _currentToken.Type != BlingoTokenType.Eof &&
                    _currentToken.Type != BlingoTokenType.Return)
                {
                    var args = new List<BlingoNode>
                    {
                        ParseExpression()
                    };
                    while (Match(BlingoTokenType.Comma))
                    {
                        args.Add(ParseExpression());
                    }
                    var argList = new BlingoDatumNode(new BlingoDatum(args, BlingoDatum.DatumType.ArgList));
                    return new BlingoCallNode { Callee = v, Arguments = argList };
                }

                return new BlingoCallNode { Name = v.VarName };
            }

            return expr;
        }


        private BlingoNode ParseKeywordStatement()
        {
            var keywordToken = _currentToken;
            AdvanceToken(); // consume the keyword

            switch (keywordToken.Type)
            {
                case BlingoTokenType.Exit:
                    if (_currentToken.Type == BlingoTokenType.Repeat)
                    {
                        AdvanceToken(); // consume 'repeat'

                        if (_currentToken.Type == BlingoTokenType.If)
                        {
                            AdvanceToken(); // consume 'if'
                            var condition = ParseExpression();
                            return new BlingoExitRepeatIfStmtNode(condition);
                        }

                        return new BlingoExitRepeatStmtNode();
                    }
                    return new BlingoExitStmtNode();

                case BlingoTokenType.Next:
                    if (_currentToken.Type == BlingoTokenType.Repeat)
                    {
                        AdvanceToken(); // consume 'repeat'

                        if (_currentToken.Type == BlingoTokenType.If)
                        {
                            AdvanceToken(); // consume 'if'
                            var condition = ParseExpression();
                            return new BlingoNextRepeatIfStmtNode(condition);
                        }

                        return new BlingoNextRepeatStmtNode();
                    }

                    return new BlingoNextStmtNode(); // fallback, if you want to allow generic "next"

                case BlingoTokenType.Put:
                    return ParsePutStatement();

                case BlingoTokenType.If:
                    return ParseIfStatement();

                case BlingoTokenType.Repeat:
                    return ParseRepeatStatement();

                case BlingoTokenType.Return:
                    BlingoNode? retValue = null;
                    if (_currentToken.Type != BlingoTokenType.End &&
                        _currentToken.Type != BlingoTokenType.Eof &&
                        _currentToken.Line == keywordToken.Line)
                    {
                        retValue = ParseExpression();
                    }
                    return new BlingoReturnStmtNode(retValue);

                case BlingoTokenType.Case:
                    return ParseCaseStatement();

                default:
                    return new BlingoDatumNode(new BlingoDatum(keywordToken.Lexeme));
            }
        }

        private BlingoNode ParseDeclarationStatement(BlingoTokenType keyword)
        {
            AdvanceToken(); // consume keyword
            var names = new List<string>();
            do
            {
                var ident = Expect(BlingoTokenType.Identifier);
                names.Add(ident.Lexeme);
            } while (Match(BlingoTokenType.Comma));

            switch (keyword)
            {
                case BlingoTokenType.Global:
                    var g = new BlingoGlobalDeclStmtNode();
                    g.Names.AddRange(names);
                    return g;
                case BlingoTokenType.Property:
                    var p = new BlingoPropertyDeclStmtNode();
                    p.Names.AddRange(names);
                    return p;
                case BlingoTokenType.Instance:
                    var i = new BlingoInstanceDeclStmtNode();
                    i.Names.AddRange(names);
                    return i;
                default:
                    return new BlingoErrorNode();
            }
        }

        private BlingoNode ParseExpression(bool allowEquals = true)
        {
            BlingoNode ParseOperand()
            {
                var operand = ParsePrimary();
                while (true)
                {
                    if (operand is BlingoVarNode sv && sv.VarName.Equals("sendSprite", StringComparison.OrdinalIgnoreCase))
                    {
                        var hasParen = Match(BlingoTokenType.LeftParen);
                        var sprite = ParseExpression();
                        Expect(BlingoTokenType.Comma);
                        var message = ParseExpression();
                        var args = new List<BlingoNode>();
                        while (Match(BlingoTokenType.Comma))
                        {
                            args.Add(ParseExpression());
                        }
                        if (hasParen)
                            Expect(BlingoTokenType.RightParen);
                        BlingoNode? argList = null;
                        if (args.Count > 0)
                            argList = new BlingoDatumNode(new BlingoDatum(args, BlingoDatum.DatumType.ArgList));
                        operand = new BlingoSendSpriteExprNode { Sprite = sprite, Message = message, Arguments = argList };
                    }
                    else if (Match(BlingoTokenType.Dot))
                    {
                        var pTok = Expect(BlingoTokenType.Identifier);
                        operand = new BlingoObjPropExprNode
                        {
                            Object = operand,
                            Property = new BlingoVarNode { VarName = pTok.Lexeme }
                        };
                    }
                    else if (Match(BlingoTokenType.LeftBracket))
                    {
                        var start = ParseExpression();
                        BlingoNode idx = start;
                        if (Match(BlingoTokenType.Range))
                        {
                            var end = ParseExpression();
                            idx = new BlingoRangeExprNode { Start = start, End = end };
                        }
                        Expect(BlingoTokenType.RightBracket);
                        operand = new BlingoObjBracketExprNode
                        {
                            Object = operand,
                            Index = idx
                        };
                    }
                    else if (Match(BlingoTokenType.LeftParen))
                    {
                        var args = new List<BlingoNode>();
                        if (_currentToken.Type != BlingoTokenType.RightParen)
                        {
                            args.Add(ParseExpression());
                            while (Match(BlingoTokenType.Comma))
                            {
                                args.Add(ParseExpression());
                            }
                        }
                        Expect(BlingoTokenType.RightParen);
                        BlingoNode argExpr;
                        if (args.Count == 0)
                            argExpr = new BlingoBlockNode();
                        else if (args.Count == 1)
                            argExpr = args[0];
                        else
                            argExpr = new BlingoDatumNode(new BlingoDatum(args, BlingoDatum.DatumType.ArgList));

                        operand = new BlingoCallNode
                        {
                            Callee = operand,
                            Arguments = argExpr
                        };
                    }
                    else
                    {
                        break;
                    }
                }

                return operand;
            }

            var expr = ParseOperand();

            while (true)
            {
                if (_currentToken.Type == BlingoTokenType.Plus)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new BlingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = BlingoBinaryOpcode.Add
                    };
                }
                else if (_currentToken.Type == BlingoTokenType.Minus)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new BlingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = BlingoBinaryOpcode.Subtract
                    };
                }
                else if (_currentToken.Type == BlingoTokenType.Asterisk)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new BlingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = BlingoBinaryOpcode.Multiply
                    };
                }
                else if (_currentToken.Type == BlingoTokenType.Slash)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new BlingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = BlingoBinaryOpcode.Divide
                    };
                }
                else if (_currentToken.Type == BlingoTokenType.Ampersand)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new BlingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = BlingoBinaryOpcode.Concat
                    };
                }
                else if (_currentToken.Type == BlingoTokenType.NotEquals)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new BlingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = BlingoBinaryOpcode.NotEquals
                    };
                }
                else if (_currentToken.Type == BlingoTokenType.GreaterThan)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new BlingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = BlingoBinaryOpcode.GreaterThan
                    };
                }
                else if (_currentToken.Type == BlingoTokenType.GreaterOrEqual)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new BlingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = BlingoBinaryOpcode.GreaterOrEqual
                    };
                }
                else if (_currentToken.Type == BlingoTokenType.LessThan)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new BlingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = BlingoBinaryOpcode.LessThan
                    };
                }
                else if (_currentToken.Type == BlingoTokenType.LessOrEqual)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new BlingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = BlingoBinaryOpcode.LessOrEqual
                    };
                }
                else if (_currentToken.Type == BlingoTokenType.And)
                {
                    AdvanceToken();
                    var right = ParseExpression();
                    expr = new BlingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = BlingoBinaryOpcode.And
                    };
                }
                else if (_currentToken.Type == BlingoTokenType.Or)
                {
                    AdvanceToken();
                    var right = ParseExpression();
                    expr = new BlingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = BlingoBinaryOpcode.Or
                    };
                }
                else if (allowEquals && _currentToken.Type == BlingoTokenType.Equals)
                {
                    AdvanceToken();
                    var right = ParseExpression(false);
                    expr = new BlingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = BlingoBinaryOpcode.Equals
                    };
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private BlingoNode ParsePrimary()
        {
            BlingoNode expr;
            switch (_currentToken.Type)
            {
                case BlingoTokenType.Number:
                    var text = _currentToken.Lexeme;
                    BlingoDatum datum;
                    if (text.StartsWith("$"))
                    {
                        if (int.TryParse(text[1..], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var hex))
                            datum = new BlingoDatum(hex);
                        else
                            datum = new BlingoDatum(text);
                    }
                    else if (text.Contains('.'))
                    {
                        if (float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f))
                            datum = new BlingoDatum(f);
                        else
                            datum = new BlingoDatum(text);
                    }
                    else if (int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var i))
                    {
                        datum = new BlingoDatum(i);
                    }
                    else
                    {
                        datum = new BlingoDatum(text);
                    }
                    AdvanceToken();
                    expr = new BlingoDatumNode(datum);
                    break;

                case BlingoTokenType.Not:
                    AdvanceToken();
                    var notExpr = ParsePrimary();
                    expr = new BlingoNotOpNode { Expr = notExpr };
                    break;

                case BlingoTokenType.Minus:
                    AdvanceToken();
                    if (_currentToken.Type == BlingoTokenType.Number)
                    {
                        var negText = "-" + _currentToken.Lexeme;
                        BlingoDatum negDatum;
                        if (negText.Contains('.'))
                        {
                            if (float.TryParse(negText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var nf))
                                negDatum = new BlingoDatum(nf);
                            else
                                negDatum = new BlingoDatum(negText);
                        }
                        else if (int.TryParse(negText, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var ni))
                        {
                            negDatum = new BlingoDatum(ni);
                        }
                        else
                        {
                            negDatum = new BlingoDatum(negText);
                        }
                        AdvanceToken();
                        expr = new BlingoDatumNode(negDatum);
                    }
                    else
                    {
                        var operand = ParsePrimary();
                        expr = new BlingoBinaryOpNode
                        {
                            Left = new BlingoDatumNode(new BlingoDatum(0)),
                            Right = operand,
                            Opcode = BlingoBinaryOpcode.Subtract
                        };
                    }
                    break;

                case BlingoTokenType.String:
                    expr = new BlingoDatumNode(new BlingoDatum(_currentToken.Lexeme));
                    AdvanceToken();
                    break;

                case BlingoTokenType.LeftBracket:
                    AdvanceToken();
                    var elements = new List<BlingoNode>();
                    var isPropList = false;
                    if (Match(BlingoTokenType.Colon))
                    {
                        Expect(BlingoTokenType.RightBracket);
                        expr = new BlingoDatumNode(new BlingoDatum(elements, BlingoDatum.DatumType.PropList));
                        break;
                    }
                    if (_currentToken.Type != BlingoTokenType.RightBracket)
                    {
                        do
                        {
                            var keyOrValue = ParseExpression();
                            if (Match(BlingoTokenType.Colon))
                            {
                                var value = ParseExpression();
                                elements.Add(keyOrValue);
                                elements.Add(value);
                                isPropList = true;
                            }
                            else
                            {
                                elements.Add(keyOrValue);
                            }
                        } while (Match(BlingoTokenType.Comma));
                    }
                    Expect(BlingoTokenType.RightBracket);
                    expr = new BlingoDatumNode(new BlingoDatum(elements, isPropList ? BlingoDatum.DatumType.PropList : BlingoDatum.DatumType.List));
                    break;

                case BlingoTokenType.Symbol:
                    expr = new BlingoDatumNode(new BlingoDatum(_currentToken.Lexeme, isSymbol: true));
                    AdvanceToken();
                    break;

                case BlingoTokenType.Me:
                    expr = new BlingoVarNode { VarName = "me" };
                    AdvanceToken();
                    break;

                case BlingoTokenType.Return:
                    expr = new BlingoDatumNode(new BlingoDatum("\n"));
                    AdvanceToken();
                    break;

                case BlingoTokenType.The:
                    AdvanceToken();
                    var propTok = Expect(BlingoTokenType.Identifier);
                    expr = new BlingoTheExprNode { Prop = propTok.Lexeme };
                    break;

                case BlingoTokenType.LeftParen:
                    AdvanceToken();
                    expr = ParseExpression();
                    Expect(BlingoTokenType.RightParen);
                    break;

                case BlingoTokenType.Identifier:
                    var name = _currentToken.Lexeme;
                    AdvanceToken();
                    var upper = name.ToUpperInvariant();
                    switch (upper)
                    {
                        case "BACKSPACE":
                            expr = new BlingoDatumNode(new BlingoDatum("\b"));
                            break;
                        case "ENTER":
                            expr = new BlingoDatumNode(new BlingoDatum("\u0003"));
                            break;
                        case "QUOTE":
                            expr = new BlingoDatumNode(new BlingoDatum("\""));
                            break;
                        case "SPACE":
                            expr = new BlingoDatumNode(new BlingoDatum(" "));
                            break;
                        case "TAB":
                            expr = new BlingoDatumNode(new BlingoDatum("\t"));
                            break;
                        default:
                            if (name.Equals("sprite", StringComparison.OrdinalIgnoreCase) && Match(BlingoTokenType.LeftParen))
                            {
                                var indexExpr = ParseSpriteIndex();
                                Expect(BlingoTokenType.RightParen);
                                if (Match(BlingoTokenType.Dot))
                                {
                                    var pTok = Expect(BlingoTokenType.Identifier);
                                    expr = new BlingoSpritePropExprNode
                                    {
                                        Sprite = indexExpr,
                                        Property = new BlingoVarNode { VarName = pTok.Lexeme }
                                    };
                                }
                                else
                                {
                                    expr = new BlingoCallNode
                                    {
                                        Callee = new BlingoVarNode { VarName = name },
                                        Arguments = indexExpr
                                    };
                                }
                            }
                            else if (name.Equals("member", StringComparison.OrdinalIgnoreCase) && Match(BlingoTokenType.LeftParen))
                            {
                                var inner = ParseExpression();
                                BlingoNode? castExpr = null;
                                if (Match(BlingoTokenType.Comma))
                                {
                                    castExpr = ParseExpression();
                                }
                                Expect(BlingoTokenType.RightParen);
                                expr = new BlingoMemberExprNode { Expr = inner, CastLib = castExpr };
                                while (Match(BlingoTokenType.Dot))
                                {
                                    var pTok = Expect(BlingoTokenType.Identifier);
                                    expr = new BlingoObjPropExprNode
                                    {
                                        Object = expr,
                                        Property = new BlingoVarNode { VarName = pTok.Lexeme }
                                    };
                                }
                            }
                            else if (name.Equals("field", StringComparison.OrdinalIgnoreCase))
                            {
                                var arg = ParseExpression();
                                expr = new BlingoObjPropExprNode
                                {
                                    Object = new BlingoMemberExprNode { Expr = arg },
                                    Property = new BlingoVarNode { VarName = "Text" }
                                };
                            }
                            else
                            {
                                expr = new BlingoVarNode { VarName = name };
                            }
                            break;
                    }
                    break;

                default:
                    AdvanceToken();
                    return new BlingoErrorNode();
            }

            return expr;
        }

        private BlingoNode ParseSpriteIndex()
        {
            // A sprite index can be any valid expression (numbers, variables,
            // property chains, etc.). The previous implementation only allowed
            // identifiers or `me`, leaving the current token untouched when a
            // number was encountered. This resulted in a parser error such as
            // "Expected token RightParen, but got Number" for expressions like
            // `sprite(263).property`.

            // Reuse the general expression parser here so that any valid Lingo
            // expression can serve as the sprite index and the token stream is
            // advanced correctly.
            return ParseExpression();
        }

        private BlingoNode ParsePutStatement()
        {
            // At this point, the "put" keyword has already been consumed

            // Parse the value expression (what to put)
            var value = ParseExpression();

            // If followed by "into" parse target; otherwise treat as message output
            if (_currentToken.Type == BlingoTokenType.Into)
            {
                var tokType = _currentToken.Type;
                AdvanceToken();
                var target = ParseExpression(false);
                var node = new BlingoPutStmtNode(value, target)
                {
                    Type = BlingoPutType.Into
                };
                return node;
            }

            return new BlingoPutStmtNode(value, null) { Type = BlingoPutType.Message };
        }


        private BlingoNode ParseIfStatement()
        {
            // "if" already consumed

            var condition = ParseExpression();
            var thenTok = Expect(BlingoTokenType.Then);

            while (_currentToken.Type == BlingoTokenType.Comment &&
                   _currentToken.Line == thenTok.Line)
            {
                AdvanceToken();
            }

            if (_currentToken.Line == thenTok.Line &&
                _currentToken.Type != BlingoTokenType.Comment)
            {
                var stmt = ParseStatement();
                var stmtBlock = new BlingoBlockNode();
                stmtBlock.Children.Add(stmt);

                BlingoNode? singleElseBlock = null;

                if (_currentToken.Type == BlingoTokenType.Else)
                {
                    var elseTok = _currentToken;
                    AdvanceToken(); // consume 'else'

                    if (_currentToken.Type == BlingoTokenType.If &&
                        _currentToken.Line == elseTok.Line)
                    {
                        singleElseBlock = ParseElseIfChain();
                    }
                    else if (_currentToken.Line == elseTok.Line &&
                             _currentToken.Type != BlingoTokenType.Comment)
                    {
                        var elseStmt = ParseStatement();
                        var singleElse = new BlingoBlockNode();
                        singleElse.Children.Add(elseStmt);
                        singleElseBlock = singleElse;
                    }
                    else
                    {
                        singleElseBlock = ParseBlock();
                    }

                    Expect(BlingoTokenType.End);
                    Expect(BlingoTokenType.If);
                }
                else if (_currentToken.Type == BlingoTokenType.End)
                {
                    Expect(BlingoTokenType.End);
                    Expect(BlingoTokenType.If);
                }
                // single-line 'if' without explicit 'end if'

                return new BlingoIfStmtNode(condition, stmtBlock, singleElseBlock);
            }

            var thenBlock = ParseBlock();

            BlingoNode? elseBlock = null;
            if (_currentToken.Type == BlingoTokenType.Else)
            {
                var elseTok = _currentToken;
                AdvanceToken(); // consume 'else'
                if (_currentToken.Type == BlingoTokenType.If && _currentToken.Line == elseTok.Line)
                {
                    elseBlock = ParseElseIfChain();
                }
                else
                {
                    elseBlock = ParseBlock();
                }
            }

            Expect(BlingoTokenType.End);
            Expect(BlingoTokenType.If);

            return new BlingoIfStmtNode(condition, thenBlock, elseBlock);
        }

        private BlingoBlockNode ParseElseIfChain()
        {
            // current token is 'if'
            AdvanceToken(); // consume 'if'
            var cond = ParseExpression();
            Expect(BlingoTokenType.Then);
            var thenBlock = ParseBlock();
            BlingoNode? elseBlock = null;
            if (_currentToken.Type == BlingoTokenType.Else)
            {
                AdvanceToken();
                if (_currentToken.Type == BlingoTokenType.If)
                    elseBlock = ParseElseIfChain();
                else
                    elseBlock = ParseBlock();
            }
            var block = new BlingoBlockNode();
            block.Children.Add(new BlingoIfStmtNode(cond, thenBlock, elseBlock));
            return block;
        }

        private BlingoBlockNode ParseBlock()
        {
            var block = new BlingoBlockNode();

            while (_currentToken.Type != BlingoTokenType.End &&
                   _currentToken.Type != BlingoTokenType.Else &&
                   _currentToken.Type != BlingoTokenType.Eof)
            {
                block.Children.Add(ParseStatement());
            }

            return block;
        }

        private BlingoNode ParseRepeatStatement()
        {
            // "repeat" already consumed

            if (_currentToken.Type == BlingoTokenType.While)
            {
                AdvanceToken();
                var condition = ParseExpression();
                var body = ParseBlock();
                Expect(BlingoTokenType.End);
                Expect(BlingoTokenType.Repeat);
                return new BlingoRepeatWhileStmtNode(condition, body);
            }
            else if (_currentToken.Type == BlingoTokenType.Until)
            {
                AdvanceToken();
                var condition = ParseExpression();
                var body = ParseBlock();
                Expect(BlingoTokenType.End);
                Expect(BlingoTokenType.Repeat);
                return new BlingoRepeatUntilStmtNode(condition, body);
            }
            else if (_currentToken.Type == BlingoTokenType.With)
            {
                AdvanceToken();
                var varToken = Expect(BlingoTokenType.Identifier);
                if (Match(BlingoTokenType.Equals))
                {
                    var start = ParseExpression();
                    Expect(BlingoTokenType.To);
                    var end = ParseExpression();
                    var body = ParseBlock();
                    Expect(BlingoTokenType.End);
                    Expect(BlingoTokenType.Repeat);
                    return new BlingoRepeatWithStmtNode(varToken.Lexeme, start, end, body);
                }
                else if (Match(BlingoTokenType.In))
                {
                    var list = ParseExpression();
                    var body = ParseBlock();
                    Expect(BlingoTokenType.End);
                    Expect(BlingoTokenType.Repeat);
                    return new BlingoRepeatWithInStmtNode
                    {
                        Variable = varToken.Lexeme,
                        List = list,
                        Body = body
                    };
                }
                else
                {
                    throw new Exception($"Expected token Equals or In, but got {_currentToken.Type} at line {_currentToken.Line}");
                }
            }
            else if (_currentToken.Type == BlingoTokenType.Number)
            {
                // repeat <number> times
                var countExpr = ParseExpression();
                Expect(BlingoTokenType.Times);
                var body = ParseBlock();
                Expect(BlingoTokenType.End);
                Expect(BlingoTokenType.Repeat);
                return new BlingoRepeatTimesStmtNode(countExpr, body);
            }
            else
            {
                // repeat, repeat forever, or loop
                var body = ParseBlock();
                Expect(BlingoTokenType.End);
                Expect(BlingoTokenType.Repeat);
                return new BlingoRepeatForeverStmtNode(body);
            }
        }

        private BlingoCaseLabelNode ParseCaseLabel()
        {
            var value = ParseExpression();
            Expect(BlingoTokenType.Colon);
            var block = new BlingoBlockNode();
            block.Children.Add(ParseStatement());
            return new BlingoCaseLabelNode { Value = value, Block = block };
        }

        private BlingoNode ParseCaseStatement()
        {
            var value = ParseExpression();
            Expect(BlingoTokenType.Of);
            var first = ParseCaseLabel();
            var current = first;
            while (_currentToken.Type != BlingoTokenType.End &&
                   _currentToken.Type != BlingoTokenType.Otherwise &&
                   _currentToken.Type != BlingoTokenType.Eof)
            {
                var nextLabel = ParseCaseLabel();
                current.NextLabel = nextLabel;
                current = nextLabel;
            }

            BlingoOtherwiseNode? otherwise = null;
            if (_currentToken.Type == BlingoTokenType.Otherwise)
            {
                AdvanceToken();
                if (_currentToken.Type == BlingoTokenType.Colon)
                    AdvanceToken();
                var block = new BlingoBlockNode();
                while (_currentToken.Type != BlingoTokenType.End &&
                       _currentToken.Type != BlingoTokenType.Eof)
                {
                    block.Children.Add(ParseStatement());
                }
                otherwise = new BlingoOtherwiseNode { Block = block };
            }

            Expect(BlingoTokenType.End);
            Expect(BlingoTokenType.Case);
            return new BlingoCaseStmtNode { Value = value, FirstLabel = first, Otherwise = otherwise };
        }

    }


}




