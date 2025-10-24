using System.Text;
using System.Collections.Generic;
using OldBlingoEngine.Lingo.Core.Tokenizer;

namespace OldBlingoEngine.Lingo.Core
{
    public class BlingoCodeWriterVisitor : IBlingoAstVisitor
    {
        private readonly bool _dot;
        private readonly bool _sum;
        private int _indent;
        private bool _indentWritten;
        private int _lineWidth;
        private string _indentation = "\t";
        private string _lineEnding = "\n";
        private readonly StringBuilder _builder = new();
        public int Length => _builder.Length;
        public string Result => _builder.ToString();
        public BlingoCodeWriterVisitor(bool dot, bool sum, string lineEnding = "\n")
        {
            _dot = dot;
            _sum = sum;
            _indent = 0;
            _indentWritten = false;
            _lineWidth = 0;
            _lineEnding = lineEnding;
        }

        public void Write(string text)
        {
            WriteIndentation();
            _builder.Append(text);
            _lineWidth += text.Length;
        }
        public void WriteLine()
        {
            _builder.Append(_lineEnding);
            _lineWidth = 0;
            _indentWritten = false;
        }
        public void WriteLine(string line)
        {
            WriteIndentation();
            _builder.Append(line);
            _builder.Append(_lineEnding);
            _lineWidth = 0;
            _indentWritten = false;
        }
        public void Write(char c)
        {
            WriteIndentation();
            _builder.Append(c);
            _lineWidth++;
        }


        private void WriteIndentation()
        {
            if (_indentWritten)
                return;

            for (int i = 0; i < _indent; i++)
            {
                _builder.Append(_indentation);
            }

            _indentWritten = true;
            _lineWidth = _indent * _indentation.Length;
        }









        private void Indent()
        {
            _indent++;
        }

        private void Unindent()
        {
            if (_indent > 0)
                _indent--;
        }
        public void Visit(BlingoErrorNode node) => Write("ERROR");

        public void Visit(BlingoCommentNode node)
        {
            Write("-- ");
            Write(node.Text);
        }

        public void Visit(BlingoLiteralNode node) => Write(node.Value);

        public void Visit(BlingoTheExprNode node)
        {
            Write("the ");
            Write(node.Prop);
        }

        public void Visit(BlingoExitStmtNode node) => Write("exit");
        public void Visit(BlingoReturnStmtNode node)
        {
            Write("return");
            if (node.Value != null)
            {
                Write(" ");
                node.Value.Accept(this);
            }
        }
        public void Visit(BlingoVarNode node) => Write(node.VarName);

        private void Write(BlingoDatum datum)
        {
            switch (datum.Type)
            {
                case BlingoDatum.DatumType.Void:
                    Write("VOID");
                    break;

                case BlingoDatum.DatumType.Symbol:
                    Write("#" + datum.AsSymbol());
                    break;

                case BlingoDatum.DatumType.VarRef:
                    Write(datum.AsString());
                    break;

                case BlingoDatum.DatumType.String:
                    var str = datum.AsString();
                    if (string.IsNullOrEmpty(str))
                    {
                        Write("EMPTY");
                    }
                    else if (str.Length == 1)
                    {
                        WriteSingleCharLiteral(str[0]);
                    }
                    else
                    {
                        Write('"' + str + '"');
                    }
                    break;

                case BlingoDatum.DatumType.Integer:
                    Write(datum.AsInt().ToString());
                    break;

                case BlingoDatum.DatumType.Float:
                    Write(datum.AsFloat().ToString("G"));
                    break;

                case BlingoDatum.DatumType.List:
                case BlingoDatum.DatumType.ArgList:
                case BlingoDatum.DatumType.ArgListNoRet:
                    {
                        if (datum.Type == BlingoDatum.DatumType.List)
                            Write("[");

                        if (datum.Value is List<BlingoNode> list)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (i > 0) Write(", ");
                                list[i].Accept(this);
                            }
                        }

                        if (datum.Type == BlingoDatum.DatumType.List)
                            Write("]");
                        break;
                    }

                case BlingoDatum.DatumType.PropList:
                    {
                        Write("[");
                        if (datum.Value is List<BlingoNode> list)
                        {
                            if (list.Count == 0)
                            {
                                Write(":");
                            }
                            else
                            {
                                for (int i = 0; i < list.Count; i += 2)
                                {
                                    if (i > 0) Write(", ");
                                    list[i].Accept(this);
                                    Write(": ");
                                    list[i + 1].Accept(this);
                                }
                            }
                        }
                        Write("]");
                        break;
                    }

                default:
                    Write($"<unsupported datum type: {datum.Type}>");
                    break;
            }
        }


        private void WriteSingleCharLiteral(char c)
        {
            switch (c)
            {
                case '\x03': Write("ENTER"); break;
                case '\x08': Write("BACKSPACE"); break;
                case '\t': Write("TAB"); break;
                case '\n': Write("RETURN"); break;
                case '"': Write("QUOTE"); break;
                case ' ': Write("SPACE"); break;
                default: Write('"' + c.ToString() + '"'); break;
            }
        }
        private static bool HasSpaces(BlingoNode node, bool dotSyntax)
        {
            // Currently only handles BlingoBinaryOpNode precedence as in original logic.
            return node is BlingoBinaryOpNode;
        }


        public void Visit(BlingoHandlerNode node)
        {
            // Implementation can be expanded as needed
            node.Block?.Accept(this);
        }

        public void Visit(BlingoNewObjNode node)
        {
            Write("new ");
            Write(node.ObjType);
            Write("(");
            node.ObjArgs?.Accept(this);
            Write(")");
        }

        public void Visit(BlingoIfStmtNode node)
        {
            Write("if ");
            node.Condition.Accept(this);
            Write(" then");
            WriteLine();
            node.ThenBlock.Accept(this);
            if (node.HasElse)
            {
                WriteLine("else");
                node.ElseBlock!.Accept(this);
            }
            Write("end if");
        }

        public void Visit(BlingoIfElseStmtNode node)
        {
            Write("if ");
            node.Condition.Accept(this);
            Write(" then");
            WriteLine();
            node.ThenBlock.Accept(this);
            WriteLine("else");
            node.ElseBlock.Accept(this);
            Write("end if");
        }

        public void Visit(BlingoEndCaseNode node)
        {
            Write("end case");
        }

        public void Visit(BlingoObjCallNode node)
        {
            Write(node.Name.Value?.AsString() ?? "unknown");
            Write("(");
            node.ArgList.Accept(this);
            Write(")");
        }


        public void Visit(BlingoPutStmtNode node)
        {
            Write("put ");
            node.Value.Accept(this);
            if (node.Target != null && node.Type != BlingoPutType.Message)
            {
                Write(" ");
                Write(node.Type.ToString());
                Write(" ");
                node.Variable?.Accept(this);
            }
        }

        public void Visit(BlingoBinaryOpNode node)
        {
            bool needsParensLeft = node.Left is BlingoBinaryOpNode;
            bool needsParensRight = node.Right is BlingoBinaryOpNode;

            if (needsParensLeft) Write("(");
            node.Left.Accept(this);
            if (needsParensLeft) Write(")");

            Write(" ");
            Write(node.Opcode.ToString());
            Write(" ");

            if (needsParensRight) Write("(");
            node.Right.Accept(this);
            if (needsParensRight) Write(")");
        }

        public void Visit(BlingoCaseStmtNode node)
        {
            Write("case ");
            node.Value.Accept(this);
            WriteLine(" of");
            Indent();

            var currentLabel = node.FirstLabel as BlingoCaseLabelNode;
            while (currentLabel != null)
            {
                currentLabel.Accept(this);
                currentLabel = currentLabel.NextLabel;
            }

            node.Otherwise?.Accept(this);

            Unindent();
            Write("end case");
        }
        public void Visit(BlingoCaseLabelNode node)
        {
            bool parenValue = HasSpaces(node.Value, _dot);
            if (parenValue)
                Write("(");
            node.Value.Accept(this);
            if (parenValue)
                Write(")");

            if (node.NextOr != null)
            {
                Write(", ");
                node.NextOr.Accept(this);
            }
            else
            {
                WriteLine(":");
                node.Block?.Accept(this);
            }

            if (node.NextLabel != null)
                node.NextLabel.Accept(this);
        }


        public void Visit(BlingoTellStmtNode node)
        {
            Write("tell ");
            node.Window.Accept(this);
            WriteLine();
            node.Block.Accept(this);
            Write("end tell");
        }

        public void Visit(BlingoWhenStmtNode node)
        {
            Write("when ");
            Write(node.Event);
            Write(" then");
            WriteLine();
            Write(node.Script);
        }

        public void Visit(BlingoOtherwiseNode node)
        {
            WriteLine("otherwise:");
            node.Block.Accept(this);
        }

        public void Visit(BlingoBlockNode node)
        {
            Indent();
            foreach (var child in node.Children)
            {
                child.Accept(this);
                WriteLine();
            }
            Unindent();
        }

        public void Visit(BlingoDatumNode datumNode)
        {
            Write(datumNode.Value);
        }





        public void Visit(BlingoChunkExprNode node)
        {
            node.Expr.Accept(this);
        }

        public void Visit(BlingoInverseOpNode node)
        {
            Write("not ");
            node.Expr.Accept(this);
        }

        public void Visit(BlingoObjCallV4Node node)
        {
            node.Object.Accept(this);
            Write(".");
            Write(node.Name.Value.AsString());
            Write("(");
            node.ArgList.Accept(this);
            Write(")");
        }

        public void Visit(BlingoMemberExprNode node)
        {
            Write("member(");
            node.Expr.Accept(this);
            if (node.CastLib != null)
            {
                Write(", ");
                node.CastLib.Accept(this);
            }
            Write(")");
        }

        public void Visit(BlingoObjPropExprNode node)
        {
            node.Object.Accept(this);
            Write(".");
            node.Property.Accept(this);
        }

        public void Visit(BlingoPlayCmdStmtNode node)
        {
            Write("play ");
            node.Command.Accept(this);
        }

        public void Visit(BlingoThePropExprNode node)
        {
            Write("the ");
            node.Property.Accept(this);
        }

        public void Visit(BlingoMenuPropExprNode node)
        {
            Write("the ");
            node.Property.Accept(this);
            Write(" of ");
            node.Menu.Accept(this);
        }

        public void Visit(BlingoSoundCmdStmtNode node)
        {
            Write("sound ");
            node.Command.Accept(this);
        }

        public void Visit(BlingoSoundPropExprNode node)
        {
            Write("the ");
            node.Property.Accept(this);
            Write(" of sound ");
            node.Sound.Accept(this);
        }

        public void Visit(BlingoCursorStmtNode node)
        {
            Write("cursor ");
            node.Value.Accept(this);
            WriteLine();
        }

        public void Visit(BlingoGoToStmtNode node)
        {
            Write("go to ");
            node.Target.Accept(this);
            WriteLine();
        }

        public void Visit(BlingoAssignmentStmtNode node)
        {
            node.Target.Accept(this);
            Write(" = ");
            node.Value.Accept(this);
            WriteLine();
        }

        public void Visit(BlingoSendSpriteStmtNode node)
        {
            Write("sendSprite ");
            node.Sprite.Accept(this);
            Write(", ");
            node.Message.Accept(this);
            if (node.Arguments != null)
            {
                Write(", ");
                node.Arguments.Accept(this);
            }
            WriteLine();
        }

        public void Visit(BlingoSendSpriteExprNode node)
        {
            Write("sendSprite ");
            node.Sprite.Accept(this);
            Write(", ");
            node.Message.Accept(this);
            if (node.Arguments != null)
            {
                Write(", ");
                node.Arguments.Accept(this);
            }
        }

        public void Visit(BlingoExitRepeatStmtNode node)
        {
            WriteLine("exit repeat");
        }

        public void Visit(BlingoNextRepeatStmtNode node)
        {
            WriteLine("next repeat");
        }

        public void Visit(BlingoRangeExprNode node)
        {
            node.Start.Accept(this);
            Write("..");
            node.End.Accept(this);
        }

        public void Visit(BlingoObjBracketExprNode node)
        {
            node.Object.Accept(this);
            Write("[");
            node.Index.Accept(this);
            Write("]");
        }

        public void Visit(BlingoSpritePropExprNode node)
        {
            Write("sprite ");
            node.Sprite.Accept(this);
            Write(".");
            node.Property.Accept(this);
        }

        public void Visit(BlingoChunkDeleteStmtNode node)
        {
            Write("delete ");
            node.Chunk.Accept(this);
        }

        public void Visit(BlingoChunkHiliteStmtNode node)
        {
            Write("hilite ");
            node.Chunk.Accept(this);
        }

        public void Visit(BlingoGlobalDeclStmtNode node)
        {
            Write("global ");
            Write(string.Join(", ", node.Names));
        }

        public void Visit(BlingoPropertyDeclStmtNode node)
        {
            Write("property ");
            Write(string.Join(", ", node.Names));
        }

        public void Visit(BlingoInstanceDeclStmtNode node)
        {
            Write("instance ");
            Write(string.Join(", ", node.Names));
        }

        public void Visit(BlingoRepeatWhileStmtNode node)
        {
            Write("repeat while ");
            node.Condition.Accept(this);
            WriteLine();
            Indent();
            node.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }

        public void Visit(BlingoMenuItemPropExprNode node)
        {
            Write("menuItem ");
            node.MenuItem.Accept(this);
            Write(".");
            node.Property.Accept(this);
        }

        public void Visit(BlingoObjPropIndexExprNode node)
        {
            node.Object.Accept(this);
            Write(".prop[");
            node.PropertyIndex.Accept(this);
            Write("]");
        }

        public void Visit(BlingoRepeatWithInStmtNode node)
        {
            Write("repeat with ");
            Write(node.Variable);
            Write(" in ");
            node.List.Accept(this);
            WriteLine();
            Indent();
            node.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }

        public void Visit(BlingoRepeatWithToStmtNode node)
        {
            Write("repeat with ");
            Write(node.Variable);
            Write(" = ");
            node.Start.Accept(this);
            Write(" to ");
            node.End.Accept(this);
            WriteLine();
            Indent();
            node.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }

        public void Visit(BlingoSpriteWithinExprNode node)
        {
            Write("sprite ");
            node.SpriteA.Accept(this);
            Write(" within ");
            node.SpriteB.Accept(this);
        }

        public void Visit(BlingoLastStringChunkExprNode node)
        {
            Write("the last chunk of ");
            node.Source.Accept(this);
        }

        public void Visit(BlingoSpriteIntersectsExprNode node)
        {
            Write("sprite ");
            node.SpriteA.Accept(this);
            Write(" intersects ");
            node.SpriteB.Accept(this);
        }

        public void Visit(BlingoStringChunkCountExprNode node)
        {
            Write("the number of chunks in ");
            node.Source.Accept(this);
        }

        public void Visit(BlingoNotOpNode node)
        {
            Write("not ");
            node.Expr.Accept(this);
        }

        public void Visit(BlingoCallNode node)
        {
            node.Callee.Accept(this);
            Write("(");
            node.Arguments.Accept(this);
            Write(")");
        }

        public void Visit(BlingoRepeatWithStmtNode repeatWithStmtNode)
        {
            Write($"repeat with {repeatWithStmtNode.Variable} = ");
            repeatWithStmtNode.Start.Accept(this);
            Write(" to ");
            repeatWithStmtNode.End.Accept(this);
            WriteLine();
            Indent();
            repeatWithStmtNode.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }


        public void Visit(BlingoRepeatUntilStmtNode repeatUntilStmtNode)
        {
            Write("repeat until ");
            repeatUntilStmtNode.Condition.Accept(this);
            WriteLine();
            Indent();
            repeatUntilStmtNode.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }


        public void Visit(BlingoRepeatForeverStmtNode repeatForeverStmtNode)
        {
            WriteLine("repeat");
            Indent();
            repeatForeverStmtNode.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }


        public void Visit(BlingoRepeatTimesStmtNode repeatTimesStmtNode)
        {
            Write("repeat ");
            repeatTimesStmtNode.Count.Accept(this);
            WriteLine(" times");
            Indent();
            repeatTimesStmtNode.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }


        public void Visit(BlingoExitRepeatIfStmtNode exitRepeatIfStmtNode)
        {
            Write("exit repeat if ");
            exitRepeatIfStmtNode.Condition.Accept(this);
            WriteLine();
        }


        public void Visit(BlingoNextRepeatIfStmtNode nextRepeatIfStmtNode)
        {
            Write("next repeat if ");
            nextRepeatIfStmtNode.Condition.Accept(this);
            WriteLine();
        }


        public void Visit(BlingoNextStmtNode nextStmtNode)
        {
            WriteLine("next");
        }

    }

}





