using System.Text;
using static Director.Scripts.Datum;

namespace Director.Scripts
{
    public interface IAstVisitor
    {
        void Visit(HandlerNode node);
        void Visit(ErrorNode node);
        void Visit(CommentNode node);
        void Visit(NewObjNode node);
        void Visit(LiteralNode node);
        void Visit(IfStmtNode node);
        void Visit(IfElseStmtNode node);
        void Visit(EndCaseNode node);
        void Visit(ObjCallNode node);
        void Visit(PutStmtNode node);
        void Visit(TheExprNode node);
        void Visit(BinaryOpNode node);
        void Visit(CaseStmtNode node);
        void Visit(ExitStmtNode node);
        void Visit(ReturnStmtNode node);
        void Visit(TellStmtNode node);
        void Visit(WhenStmtNode node);
        void Visit(OtherwiseNode node);
        void Visit(CaseLabelNode node);
        void Visit(ChunkExprNode node);
        void Visit(InverseOpNode node);
        void Visit(ObjCallV4Node node);
        void Visit(MemberExprNode node);
        void Visit(ObjPropExprNode node);
        void Visit(PlayCmdStmtNode node);
        void Visit(ThePropExprNode node);
        void Visit(MenuPropExprNode node);
        void Visit(SoundCmdStmtNode node);
        void Visit(SoundPropExprNode node);
        void Visit(AssignmentStmtNode node);
        void Visit(ExitRepeatStmtNode node);
        void Visit(NextRepeatStmtNode node);
        void Visit(ObjBracketExprNode node);
        void Visit(SpritePropExprNode node);
        void Visit(ChunkDeleteStmtNode node);
        void Visit(ChunkHiliteStmtNode node);
        void Visit(GlobalDeclStmtNode node);
        void Visit(PropertyDeclStmtNode node);
        void Visit(InstanceDeclStmtNode node);
        void Visit(RepeatWhileStmtNode node);
        void Visit(MenuItemPropExprNode node);
        void Visit(ObjPropIndexExprNode node);
        void Visit(RepeatWithInStmtNode node);
        void Visit(RepeatWithToStmtNode node);
        void Visit(SpriteWithinExprNode node);
        void Visit(LastStringChunkExprNode node);
        void Visit(SpriteIntersectsExprNode node);
        void Visit(StringChunkCountExprNode node);
        void Visit(NotOpNode node);
        void Visit(CallNode node);
        void Visit(VarNode node);
        void Visit(BlockNode node);
        void Visit(DatumNode datumNode);
        void Visit(RepeatWithStmtNode repeatWithStmtNode);
        void Visit(RepeatUntilStmtNode repeatUntilStmtNode);
        void Visit(RepeatForeverStmtNode repeatForeverStmtNode);
        void Visit(RepeatTimesStmtNode repeatTimesStmtNode);
        void Visit(ExitRepeatIfStmtNode exitRepeatIfStmtNode);
        void Visit(NextRepeatIfStmtNode nextRepeatIfStmtNode);
        void Visit(NextStmtNode nextStmtNode);
    }
    public class CodeWriterVisitor : IAstVisitor
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
        public CodeWriterVisitor(bool dot, bool sum, string lineEnding = "\n")
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
        public void Visit(ErrorNode node) => Write("ERROR");

        public void Visit(CommentNode node)
        {
            Write("-- ");
            Write(node.Text);
        }

        public void Visit(LiteralNode node) => Write(node.Value);

        public void Visit(TheExprNode node)
        {
            Write("the ");
            Write(node.Prop);
        }

        public void Visit(ExitStmtNode node) => Write("exit");
        public void Visit(ReturnStmtNode node)
        {
            Write("return");
            if (node.Value != null)
            {
                Write(" ");
                node.Value.Accept(this);
            }
        }
        public void Visit(VarNode node) => Write(node.VarName);

        private void Write(Datum datum)
        {
            switch (datum.Type)
            {
                case Datum.DatumType.Void:
                    Write("VOID");
                    break;

                case Datum.DatumType.Symbol:
                    Write("#" + datum.AsSymbol());
                    break;

                case Datum.DatumType.VarRef:
                    Write(datum.AsString());
                    break;

                case Datum.DatumType.String:
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

                case Datum.DatumType.Integer:
                    Write(datum.AsInt().ToString());
                    break;

                case Datum.DatumType.Float:
                    Write(datum.AsFloat().ToString("G"));
                    break;

                case Datum.DatumType.List:
                case Datum.DatumType.ArgList:
                case Datum.DatumType.ArgListNoRet:
                    {
                        if (datum.Type == Datum.DatumType.List)
                            Write("[");

                        if (datum.Value is List<Node> list)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (i > 0) Write(", ");
                                list[i].Accept(this);
                            }
                        }

                        if (datum.Type == Datum.DatumType.List)
                            Write("]");
                        break;
                    }

                case Datum.DatumType.PropList:
                    {
                        Write("[");
                        if (datum.Value is List<Node> list)
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
        private static bool HasSpaces(Node node, bool dotSyntax)
        {
            // Currently only handles BinaryOpNode precedence as in original logic.
            return node is BinaryOpNode;
        }


        public void Visit(HandlerNode node)
        {
            // Implementation can be expanded as needed
            node.Block?.Accept(this);
        }

        public void Visit(NewObjNode node)
        {
            Write("new ");
            Write(node.ObjType);
            Write("(");
            node.ObjArgs?.Accept(this);
            Write(")");
        }

        public void Visit(IfStmtNode node)
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

        public void Visit(IfElseStmtNode node)
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

        public void Visit(EndCaseNode node)
        {
            Write("end case");
        }

        public void Visit(ObjCallNode node)
        {
            Write(node.Name.Value?.AsString() ?? "unknown");
            Write("(");
            node.ArgList.Accept(this);
            Write(")");
        }


        public void Visit(PutStmtNode node)
        {
            Write("put ");
            node.Value.Accept(this);
            Write(" ");
            Write(node.Type.ToString());
            Write(" ");
            node.Variable.Accept(this);
        }

        public void Visit(BinaryOpNode node)
        {
            bool needsParensLeft = node.Left is BinaryOpNode;
            bool needsParensRight = node.Right is BinaryOpNode;

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

        public void Visit(CaseStmtNode node)
        {
            Write("case ");
            node.Value.Accept(this);
            WriteLine(" of");
            Indent();

            var currentLabel = node.FirstLabel as CaseLabelNode;
            while (currentLabel != null)
            {
                currentLabel.Accept(this);
                currentLabel = currentLabel.NextLabel;
            }

            node.Otherwise?.Accept(this);

            Unindent();
            Write("end case");
        }
        public void Visit(CaseLabelNode node)
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


        public void Visit(TellStmtNode node)
        {
            Write("tell ");
            node.Window.Accept(this);
            WriteLine();
            node.Block.Accept(this);
            Write("end tell");
        }

        public void Visit(WhenStmtNode node)
        {
            Write("when ");
            Write(node.Event);
            Write(" then");
            WriteLine();
            Write(node.Script);
        }

        public void Visit(OtherwiseNode node)
        {
            WriteLine("otherwise:");
            node.Block.Accept(this);
        }

        public void Visit(BlockNode node)
        {
            Indent();
            foreach (var child in node.Children)
            {
                child.Accept(this);
                WriteLine();
            }
            Unindent();
        }

        public void Visit(DatumNode datumNode)
        {
            Write(datumNode.Value);
        }





        public void Visit(ChunkExprNode node)
        {
            node.Expr.Accept(this);
        }

        public void Visit(InverseOpNode node)
        {
            Write("not ");
            node.Expr.Accept(this);
        }

        public void Visit(ObjCallV4Node node)
        {
            node.Object.Accept(this);
            Write(".");
            Write(node.Name.Value.AsString());
            Write("(");
            node.ArgList.Accept(this);
            Write(")");
        }

        public void Visit(MemberExprNode node)
        {
            Write("member ");
            node.Expr.Accept(this);
        }

        public void Visit(ObjPropExprNode node)
        {
            node.Object.Accept(this);
            Write(".");
            node.Property.Accept(this);
        }

        public void Visit(PlayCmdStmtNode node)
        {
            Write("play ");
            node.Command.Accept(this);
        }

        public void Visit(ThePropExprNode node)
        {
            Write("the ");
            node.Property.Accept(this);
        }

        public void Visit(MenuPropExprNode node)
        {
            Write("the ");
            node.Property.Accept(this);
            Write(" of ");
            node.Menu.Accept(this);
        }

        public void Visit(SoundCmdStmtNode node)
        {
            Write("sound ");
            node.Command.Accept(this);
        }

        public void Visit(SoundPropExprNode node)
        {
            Write("the ");
            node.Property.Accept(this);
            Write(" of sound ");
            node.Sound.Accept(this);
        }

        public void Visit(AssignmentStmtNode node)
        {
            node.Target.Accept(this);
            Write(" = ");
            node.Value.Accept(this);
            WriteLine();
        }

        public void Visit(ExitRepeatStmtNode node)
        {
            WriteLine("exit repeat");
        }

        public void Visit(NextRepeatStmtNode node)
        {
            WriteLine("next repeat");
        }

        public void Visit(ObjBracketExprNode node)
        {
            node.Object.Accept(this);
            Write("[");
            node.Index.Accept(this);
            Write("]");
        }

        public void Visit(SpritePropExprNode node)
        {
            Write("sprite ");
            node.Sprite.Accept(this);
            Write(".");
            node.Property.Accept(this);
        }

        public void Visit(ChunkDeleteStmtNode node)
        {
            Write("delete ");
            node.Chunk.Accept(this);
        }

        public void Visit(ChunkHiliteStmtNode node)
        {
            Write("hilite ");
            node.Chunk.Accept(this);
        }

        public void Visit(GlobalDeclStmtNode node)
        {
            Write("global ");
            Write(string.Join(", ", node.Names));
        }

        public void Visit(PropertyDeclStmtNode node)
        {
            Write("property ");
            Write(string.Join(", ", node.Names));
        }

        public void Visit(InstanceDeclStmtNode node)
        {
            Write("instance ");
            Write(string.Join(", ", node.Names));
        }

        public void Visit(RepeatWhileStmtNode node)
        {
            Write("repeat while ");
            node.Condition.Accept(this);
            WriteLine();
            Indent();
            node.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }

        public void Visit(MenuItemPropExprNode node)
        {
            Write("menuItem ");
            node.MenuItem.Accept(this);
            Write(".");
            node.Property.Accept(this);
        }

        public void Visit(ObjPropIndexExprNode node)
        {
            node.Object.Accept(this);
            Write(".prop[");
            node.PropertyIndex.Accept(this);
            Write("]");
        }

        public void Visit(RepeatWithInStmtNode node)
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

        public void Visit(RepeatWithToStmtNode node)
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

        public void Visit(SpriteWithinExprNode node)
        {
            Write("sprite ");
            node.SpriteA.Accept(this);
            Write(" within ");
            node.SpriteB.Accept(this);
        }

        public void Visit(LastStringChunkExprNode node)
        {
            Write("the last chunk of ");
            node.Source.Accept(this);
        }

        public void Visit(SpriteIntersectsExprNode node)
        {
            Write("sprite ");
            node.SpriteA.Accept(this);
            Write(" intersects ");
            node.SpriteB.Accept(this);
        }

        public void Visit(StringChunkCountExprNode node)
        {
            Write("the number of chunks in ");
            node.Source.Accept(this);
        }

        public void Visit(NotOpNode node)
        {
            Write("not ");
            node.Expr.Accept(this);
        }

        public void Visit(CallNode node)
        {
            node.Callee.Accept(this);
            Write("(");
            node.Arguments.Accept(this);
            Write(")");
        }

        public void Visit(RepeatWithStmtNode repeatWithStmtNode)
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


        public void Visit(RepeatUntilStmtNode repeatUntilStmtNode)
        {
            Write("repeat until ");
            repeatUntilStmtNode.Condition.Accept(this);
            WriteLine();
            Indent();
            repeatUntilStmtNode.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }


        public void Visit(RepeatForeverStmtNode repeatForeverStmtNode)
        {
            WriteLine("repeat");
            Indent();
            repeatForeverStmtNode.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }


        public void Visit(RepeatTimesStmtNode repeatTimesStmtNode)
        {
            Write("repeat ");
            repeatTimesStmtNode.Count.Accept(this);
            WriteLine(" times");
            Indent();
            repeatTimesStmtNode.Body.Accept(this);
            Unindent();
            WriteLine("end repeat");
        }


        public void Visit(ExitRepeatIfStmtNode exitRepeatIfStmtNode)
        {
            Write("exit repeat if ");
            exitRepeatIfStmtNode.Condition.Accept(this);
            WriteLine();
        }


        public void Visit(NextRepeatIfStmtNode nextRepeatIfStmtNode)
        {
            Write("next repeat if ");
            nextRepeatIfStmtNode.Condition.Accept(this);
            WriteLine();
        }


        public void Visit(NextStmtNode nextStmtNode)
        {
            WriteLine("next");
        }

    }

}




