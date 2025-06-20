using LingoEngine.Lingo.Core.Tokenizer;
using System.Text;
using System.Collections.Generic;

namespace LingoEngine.Lingo.Core
{
    public class LingoCodeWriterVisitor : ILingoAstVisitor
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
        public LingoCodeWriterVisitor(bool dot, bool sum, string lineEnding = "\n")
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
        public void Visit(LingoErrorNode node) => Write("ERROR");

        public void Visit(LingoCommentNode node)
        {
            Write("-- ");
            Write(node.Text);
        }

        public void Visit(LingoLiteralNode node) => Write(node.Value);

        public void Visit(LingoTheExprNode node)
        {
            Write("the ");
            Write(node.Prop);
        }

        public void Visit(LingoExitStmtNode node) => Write("exit");
        public void Visit(LingoReturnStmtNode node)
        {
            Write("return");
            if (node.Value != null)
            {
                Write(" ");
                node.Value.Accept(this);
            }
        }
        public void Visit(LingoVarNode node) => Write(node.VarName);

        private void Write(LingoDatum datum)
        {
            switch (datum.Type)
            {
                case LingoDatum.DatumType.Void:
                    Write("VOID");
                    break;

                case LingoDatum.DatumType.Symbol:
                    Write("#" + datum.AsSymbol());
                    break;

                case LingoDatum.DatumType.VarRef:
                    Write(datum.AsString());
                    break;

                case LingoDatum.DatumType.String:
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

                case LingoDatum.DatumType.Integer:
                    Write(datum.AsInt().ToString());
                    break;

                case LingoDatum.DatumType.Float:
                    Write(datum.AsFloat().ToString("G"));
                    break;

                case LingoDatum.DatumType.List:
                case LingoDatum.DatumType.ArgList:
                case LingoDatum.DatumType.ArgListNoRet:
                    {
                        if (datum.Type == LingoDatum.DatumType.List)
                            Write("[");

                        if (datum.Value is List<LingoNode> list)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (i > 0) Write(", ");
                                list[i].Accept(this);
                            }
                        }

                        if (datum.Type == LingoDatum.DatumType.List)
                            Write("]");
                        break;
                    }

                case LingoDatum.DatumType.PropList:
                    {
                        Write("[");
                        if (datum.Value is List<LingoNode> list)
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
                case '\r': Write("RETURN"); break;
                case '"': Write("QUOTE"); break;
                default: Write('"' + c.ToString() + '"'); break;
            }
        }
        private static bool HasSpaces(LingoNode node, bool dotSyntax)
        {
            // Currently only handles LingoBinaryOpNode precedence as in original logic.
            return node is LingoBinaryOpNode;
        }


        public void Visit(LingoHandlerNode node)
        {
            // Implementation can be expanded as needed
            node.Block?.Accept(this);
        }

        public void Visit(LingoNewObjNode node)
        {
            Write("new ");
            Write(node.ObjType);
            Write("(");
            node.ObjArgs?.Accept(this);
            Write(")");
        }

        public void Visit(LingoIfStmtNode node)
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

        public void Visit(LingoIfElseStmtNode node)
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

        public void Visit(LingoEndCaseNode node)
        {
            Write("end case");
        }

        public void Visit(LingoObjCallNode node)
        {
            Write(node.Name.Value?.AsString() ?? "unknown");
            Write("(");
            node.ArgList.Accept(this);
            Write(")");
        }


        public void Visit(LingoPutStmtNode node)
        {
            Write("put ");
            node.Value.Accept(this);
            Write(" ");
            Write(node.Type.ToString());
            Write(" ");
            node.Variable.Accept(this);
        }

        public void Visit(LingoBinaryOpNode node)
        {
            bool needsParensLeft = node.Left is LingoBinaryOpNode;
            bool needsParensRight = node.Right is LingoBinaryOpNode;

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

        public void Visit(LingoCaseStmtNode node)
        {
            Write("case ");
            node.Value.Accept(this);
            WriteLine(" of");
            Indent();

            var currentLabel = node.FirstLabel as LingoCaseLabelNode;
            while (currentLabel != null)
            {
                currentLabel.Accept(this);
                currentLabel = currentLabel.NextLabel;
            }

            node.Otherwise?.Accept(this);

            Unindent();
            Write("end case");
        }
        public void Visit(LingoCaseLabelNode node)
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


        public void Visit(LingoTellStmtNode node)
        {
            Write("tell ");
            node.Window.Accept(this);
            WriteLine();
            node.Block.Accept(this);
            Write("end tell");
        }

        public void Visit(LingoWhenStmtNode node)
        {
            Write("when ");
            Write(node.Event);
            Write(" then");
            WriteLine();
            Write(node.Script);
        }

        public void Visit(LingoOtherwiseNode node)
        {
            WriteLine("otherwise:");
            node.Block.Accept(this);
        }

        public void Visit(LingoBlockNode node)
        {
            Indent();
            foreach (var child in node.Children)
            {
                child.Accept(this);
                WriteLine();
            }
            Unindent();
        }

        public void Visit(LingoDatumNode datumNode)
        {
            Write(datumNode.Value);
        }





        public void Visit(LingoChunkExprNode node)
        {
            node.Expr.Accept(this);
        }

        public void Visit(LingoInverseOpNode node)
        {
            Write("not ");
            node.Expr.Accept(this);
        }

        public void Visit(LingoObjCallV4Node node)
        {
            node.Object.Accept(this);
            Write(".");
            Write(node.Name.Value.AsString());
            Write("(");
            node.ArgList.Accept(this);
            Write(")");
        }

        public void Visit(LingoMemberExprNode node)
        {
            Write("member ");
            node.Expr.Accept(this);
        }

        public void Visit(LingoObjPropExprNode node)
        {
            node.Object.Accept(this);
            Write(".");
            node.Property.Accept(this);
        }

        public void Visit(LingoPlayCmdStmtNode node)
        {
            Write("play ");
            node.Command.Accept(this);
        }

        public void Visit(LingoThePropExprNode node)
        {
            Write("the ");
            node.Property.Accept(this);
        }

        public void Visit(LingoMenuPropExprNode node)
        {
            Write("the ");
            node.Property.Accept(this);
            Write(" of ");
            node.Menu.Accept(this);
        }

        public void Visit(LingoSoundCmdStmtNode node)
        {
            Write("sound ");
            node.Command.Accept(this);
        }

        public void Visit(LingoSoundPropExprNode node)
        {
            Write("the ");
            node.Property.Accept(this);
            Write(" of sound ");
            node.Sound.Accept(this);
        }

        public void Visit(LingoAssignmentStmtNode node)
        {
            node.Target.Accept(this);
            Write(" = ");
            node.Value.Accept(this);
            WriteLine();
        }

        public void Visit(LingoSendSpriteStmtNode node)
        {
            Write("sendSprite ");
            node.Sprite.Accept(this);
            Write(", ");
            node.Message.Accept(this);
        }

        public void Visit(LingoExitRepeatStmtNode node)
        {
            WriteLine("exit repeat");
        }

        public void Visit(LingoNextRepeatStmtNode node)
        {
            WriteLine("next repeat");
        }

        public void Visit(LingoObjBracketExprNode node)
        {
            node.Object.Accept(this);
            Write("[");
            node.Index.Accept(this);
            Write("]");
        }

        public void Visit(LingoSpritePropExprNode node)
        {
            Write("sprite ");
            node.Sprite.Accept(this);
            Write(".");
            node.Property.Accept(this);
        }

        public void Visit(LingoChunkDeleteStmtNode node)
        {
            Write("delete ");
            node.Chunk.Accept(this);
        }

        public void Visit(LingoChunkHiliteStmtNode node)
        {
            Write("hilite ");
            node.Chunk.Accept(this);
        }

        public void Visit(LingoGlobalDeclStmtNode node)
        {
            Write("global ");
            Write(string.Join(", ", node.Names));
        }

        public void Visit(LingoPropertyDeclStmtNode node)
        {
            Write("property ");
            Write(string.Join(", ", node.Names));
        }

        public void Visit(LingoInstanceDeclStmtNode node)
        {
            Write("instance ");
            Write(string.Join(", ", node.Names));
        }

        public void Visit(LingoRepeatWhileStmtNode node)
        {
            Write("repeat while ");
            node.Condition.Accept(this);
            WriteLine();
            Indent();
            node.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }

        public void Visit(LingoMenuItemPropExprNode node)
        {
            Write("menuItem ");
            node.MenuItem.Accept(this);
            Write(".");
            node.Property.Accept(this);
        }

        public void Visit(LingoObjPropIndexExprNode node)
        {
            node.Object.Accept(this);
            Write(".prop[");
            node.PropertyIndex.Accept(this);
            Write("]");
        }

        public void Visit(LingoRepeatWithInStmtNode node)
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

        public void Visit(LingoRepeatWithToStmtNode node)
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

        public void Visit(LingoSpriteWithinExprNode node)
        {
            Write("sprite ");
            node.SpriteA.Accept(this);
            Write(" within ");
            node.SpriteB.Accept(this);
        }

        public void Visit(LingoLastStringChunkExprNode node)
        {
            Write("the last chunk of ");
            node.Source.Accept(this);
        }

        public void Visit(LingoSpriteIntersectsExprNode node)
        {
            Write("sprite ");
            node.SpriteA.Accept(this);
            Write(" intersects ");
            node.SpriteB.Accept(this);
        }

        public void Visit(LingoStringChunkCountExprNode node)
        {
            Write("the number of chunks in ");
            node.Source.Accept(this);
        }

        public void Visit(LingoNotOpNode node)
        {
            Write("not ");
            node.Expr.Accept(this);
        }

        public void Visit(LingoCallNode node)
        {
            node.Callee.Accept(this);
            Write("(");
            node.Arguments.Accept(this);
            Write(")");
        }

        public void Visit(LingoRepeatWithStmtNode repeatWithStmtNode)
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


        public void Visit(LingoRepeatUntilStmtNode repeatUntilStmtNode)
        {
            Write("repeat until ");
            repeatUntilStmtNode.Condition.Accept(this);
            WriteLine();
            Indent();
            repeatUntilStmtNode.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }


        public void Visit(LingoRepeatForeverStmtNode repeatForeverStmtNode)
        {
            WriteLine("repeat");
            Indent();
            repeatForeverStmtNode.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }


        public void Visit(LingoRepeatTimesStmtNode repeatTimesStmtNode)
        {
            Write("repeat ");
            repeatTimesStmtNode.Count.Accept(this);
            WriteLine(" times");
            Indent();
            repeatTimesStmtNode.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }


        public void Visit(LingoExitRepeatIfStmtNode exitRepeatIfStmtNode)
        {
            Write("exit repeat if ");
            exitRepeatIfStmtNode.Condition.Accept(this);
            WriteLine();
        }


        public void Visit(LingoNextRepeatIfStmtNode nextRepeatIfStmtNode)
        {
            Write("next repeat if ");
            nextRepeatIfStmtNode.Condition.Accept(this);
            WriteLine();
        }


        public void Visit(LingoNextStmtNode nextStmtNode)
        {
            WriteLine("next");
        }

    }

}




