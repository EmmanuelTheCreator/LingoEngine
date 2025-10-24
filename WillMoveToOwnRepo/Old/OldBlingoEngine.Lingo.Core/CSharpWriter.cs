using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using OldBlingoEngine.Lingo.Core.Tokenizer;
using OldBlingoEngine.Lingo.Core;

namespace BlingoEngine.Lingo.Core;

/// <summary>
/// Writes a small subset of Lingo AST to C# using the visitor pattern.
/// Only a handful of nodes are supported for now.
/// </summary>
public partial class CSharpWriter : IBlingoAstVisitor
{
    private readonly StringBuilder _sb = new();
    private readonly string _methodAccessModifier;
    private readonly IReadOnlyDictionary<string, BlingoScriptType> _scriptTypes;
    private readonly IReadOnlyDictionary<string, MethodSignature>? _methodSignatures;
    private readonly BlingoToCSharpConverterSettings _settings;
    private string? _currentHandlerName;
    private int _indent;
    private bool _atLineStart = true;

    public CSharpWriter(
        string methodAccessModifier = "public",
        IReadOnlyDictionary<string, BlingoScriptType>? scriptTypes = null,
        IReadOnlyDictionary<string, MethodSignature>? methodSignatures = null,
        BlingoToCSharpConverterSettings? settings = null)
    {
        _methodAccessModifier = methodAccessModifier;
        _scriptTypes = scriptTypes ?? new Dictionary<string, BlingoScriptType>();
        _methodSignatures = methodSignatures;
        _settings = settings ?? new BlingoToCSharpConverterSettings();
    }

    /// <summary>Converts the given AST node to C#.</summary>
    public static string Write(
        BlingoNode node,
        string methodAccessModifier = "public",
        IReadOnlyDictionary<string, BlingoScriptType>? scriptTypes = null,
        IReadOnlyDictionary<string, MethodSignature>? methodSignatures = null,
        BlingoToCSharpConverterSettings? settings = null)
    {
        var writer = new CSharpWriter(methodAccessModifier, scriptTypes, methodSignatures, settings);
        node.Accept(writer);
        return writer._sb.ToString();
    }

    /// <summary>Fallback helper for token sequences.</summary>
    public static string WriteTokens(IEnumerable<BlingoToken> tokens)
    {
        var sb = new StringBuilder();
        foreach (var tok in tokens)
        {
            switch (tok.Type)
            {
                case BlingoTokenType.Put:
                    sb.Append("// put\n");
                    break;
                case BlingoTokenType.Into:
                    sb.Append("// into\n");
                    break;
                default:
                    sb.Append(tok.Lexeme + " ");
                    break;
            }
        }
        return sb.ToString();
    }

    private void WriteIndent()
    {
        if (_atLineStart)
        {
            for (int i = 0; i < _indent; i++)
                _sb.Append("    ");
            _atLineStart = false;
        }
    }

    private void Append(string text)
    {
        if (text.Length == 0) return;
        WriteIndent();
        _sb.Append(text);
    }

    private void AppendLine(string text)
    {
        WriteIndent();
        _sb.AppendLine(text);
        _atLineStart = true;
    }

    private void AppendLine()
    {
        _sb.AppendLine();
        _atLineStart = true;
    }

    private void Indent() => _indent++;
    private void Unindent() { if (_indent > 0) _indent--; }

    private void Unsupported(BlingoNode node)
    {
        // Unhandled nodes are ignored for now
    }

    public void Visit(BlingoErrorNode node) => AppendLine("// error");

    public void Visit(BlingoCommentNode node)
    {
        Append("// ");
        Append(node.Text);
        AppendLine();
    }

    public void Visit(BlingoNewObjNode node)
    {
        Append("new ");
        Append(node.ObjType);
        Append("(");
        node.ObjArgs.Accept(this);
        Append(")");
    }

    public void Visit(BlingoLiteralNode node)
    {
        if (node.Value.Type == BlingoDatum.DatumType.Void)
            Append("null");
        else
            Append(node.Value.ToString());
    }

    public void Visit(BlingoIfStmtNode node)
    {
        Append("if (");
        var startCond = _sb.Length;
        node.Condition.Accept(this);
        TrimSemicolon(startCond);
        AppendLine(")");
        AppendLine("{");
        Indent();
        node.ThenBlock.Accept(this);
        Unindent();
        AppendLine("}");
        if (node.HasElse)
        {
            if (node.ElseBlock is BlingoBlockNode block && block.Children.Count == 1 && block.Children[0] is BlingoIfStmtNode nested)
            {
                Append("else ");
                nested.Accept(this);
            }
            else
            {
                AppendLine("else");
                AppendLine("{");
                Indent();
                node.ElseBlock!.Accept(this);
                Unindent();
                AppendLine("}");
            }
        }
    }

    public void Visit(BlingoIfElseStmtNode node)
    {
        Append("if (");
        var startCond = _sb.Length;
        node.Condition.Accept(this);
        TrimSemicolon(startCond);
        AppendLine(")");
        AppendLine("{");
        Indent();
        node.ThenBlock.Accept(this);
        Unindent();
        AppendLine("}");
        AppendLine("else");
        AppendLine("{");
        Indent();
        node.ElseBlock.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(BlingoEndCaseNode node) => AppendLine("// end case");

    public void Visit(BlingoObjCallNode node)
    {
        var name = node.Name.Value.AsString();
        if (name.Equals("castLib", StringComparison.OrdinalIgnoreCase))
            Append("CastLib");
        else
            Append(name);
        Append("(");
        node.ArgList.Accept(this);
        AppendLine(");");
    }

    public void Visit(BlingoPutStmtNode node)
    {
        if (node.Target == null || node.Type == BlingoPutType.Message)
        {
            Append("Put(");
            node.Value.Accept(this);
            AppendLine(");");
        }
        else
        {
            node.Target.Accept(this);
            Append(" = ");
            var start = _sb.Length;
            node.Value.Accept(this);
            TrimSemicolon(start);
            AppendLine(";");
        }
    }

    public void Visit(BlingoTheExprNode node)
    {
        var propLower = node.Prop.ToLowerInvariant();
        if (propLower.StartsWith("mouse"))
        {
            Append("_Mouse.");
            Append(char.ToUpperInvariant(node.Prop[0]) + node.Prop.Substring(1));
        }
        else if (propLower == "actorlist")
        {
            Append("_movie.ActorList");
        }
        else if (propLower == "frame")
        {
            Append("_Movie.CurrentFrame");
        }
        else if (propLower == "key")
        {
            Append("_Key.Key");
        }
        else
        {
            Append($"/* the {node.Prop} */");
        }
    }

    public void Visit(BlingoBinaryOpNode node)
    {
        if (node.Opcode == BlingoBinaryOpcode.Subtract &&
            node.Left is BlingoDatumNode dn && dn.Datum.Type == BlingoDatum.DatumType.Integer && dn.Datum.AsInt() == 0)
        {
            Append("-");
            bool needsParens = node.Right is BlingoBinaryOpNode;
            if (needsParens) Append("(");
            node.Right.Accept(this);
            if (needsParens) Append(")");
            return;
        }

        if (node.Opcode == BlingoBinaryOpcode.Contains)
        {
            bool needsParensTarget = node.Left is BlingoBinaryOpNode;
            if (needsParensTarget) Append("(");
            var startTarget = _sb.Length;
            node.Left.Accept(this);
            TrimSemicolon(startTarget);
            if (needsParensTarget) Append(")");

            Append(".Contains(");

            var startArgument = _sb.Length;
            node.Right.Accept(this);
            TrimSemicolon(startArgument);
            Append(")");
            return;
        }

        bool needsParensLeft = node.Left is BlingoBinaryOpNode;
        bool needsParensRight = node.Right is BlingoBinaryOpNode;

        if (needsParensLeft) Append("(");
        var startLeft = _sb.Length;
        node.Left.Accept(this);
        TrimSemicolon(startLeft);
        if (needsParensLeft) Append(")");

        Append(" ");
        Append(BinaryOpcodeToString(node.Opcode));
        Append(" ");

        if (needsParensRight) Append("(");
        var startRight = _sb.Length;
        node.Right.Accept(this);
        TrimSemicolon(startRight);
        if (needsParensRight) Append(")");
    }

    private static string BinaryOpcodeToString(BlingoBinaryOpcode opcode)
    {
        return opcode switch
        {
            BlingoBinaryOpcode.Add => "+",
            BlingoBinaryOpcode.Subtract => "-",
            BlingoBinaryOpcode.Multiply => "*",
            BlingoBinaryOpcode.Divide => "/",
            BlingoBinaryOpcode.Modulo => "%",
            BlingoBinaryOpcode.And => "&&",
            BlingoBinaryOpcode.Or => "||",
            BlingoBinaryOpcode.Concat => "+",
            BlingoBinaryOpcode.Equals => "==",
            BlingoBinaryOpcode.NotEquals => "!=",
            BlingoBinaryOpcode.GreaterThan => ">",
            BlingoBinaryOpcode.GreaterOrEqual => ">=",
            BlingoBinaryOpcode.LessThan => "<",
            BlingoBinaryOpcode.LessOrEqual => "<=",
            _ => opcode.ToString()
        };
    }

    public void Visit(BlingoCaseStmtNode node)
    {
        Append("switch (");
        node.Value.Accept(this);
        AppendLine(")");
        AppendLine("{");
        Indent();
        var label = node.FirstLabel as BlingoCaseLabelNode;
        while (label != null)
        {
            Append("case ");
            label.Value.Accept(this);
            AppendLine(":");
            Indent();
            label.Block?.Accept(this);
            AppendLine("break;");
            Unindent();
            label = label.NextLabel;
        }
        if (node.Otherwise != null)
        {
            AppendLine("default:");
            Indent();
            node.Otherwise.Accept(this);
            AppendLine("break;");
            Unindent();
        }
        Unindent();
        AppendLine("}");
    }

    public void Visit(BlingoExitStmtNode node) => AppendLine("return;");

    public void Visit(BlingoReturnStmtNode node)
    {
        if (node.Value is BlingoVarNode varNode &&
            varNode.VarName.Equals("me", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(_currentHandlerName, "new", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Append("return");
        if (node.Value != null)
        {
            Append(" ");
            var start = _sb.Length;
            node.Value.Accept(this);
            TrimSemicolon(start);
        }
        AppendLine(";");
    }

    public void Visit(BlingoTellStmtNode node)
    {
        AppendLine("// tell block");
        node.Block.Accept(this);
    }

    public void Visit(BlingoWhenStmtNode node) => AppendLine("// when statement");

    public void Visit(BlingoOtherwiseNode node) => node.Block.Accept(this);

    public void Visit(BlingoCaseLabelNode node)
    {
        Append("case ");
        node.Value.Accept(this);
        AppendLine(":");
        Indent();
        node.Block?.Accept(this);
        Unindent();
    }

    public void Visit(BlingoChunkExprNode node) => node.Expr.Accept(this);

    public void Visit(BlingoInverseOpNode node)
    {
        Append("!(");
        node.Expr.Accept(this);
        Append(")");
    }

    public void Visit(BlingoObjCallV4Node node)
    {
        if (node.Object is BlingoVarNode objVar &&
            objVar.VarName.Equals("me", StringComparison.OrdinalIgnoreCase))
        {
            var methodName = node.Name.Value.AsString();
            Append(char.ToUpperInvariant(methodName[0]) + methodName[1..]);
            Append("(");
            node.ArgList.Accept(this);
            AppendLine(");");
            return;
        }

        WriteObjCallV4Expr(node);
        AppendLine(";");
    }

    public void Visit(BlingoMemberExprNode node)
    {
        Append("Member(");
        node.Expr.Accept(this);
        if (node.CastLib != null)
        {
            Append(", ");
            node.CastLib.Accept(this);
        }
        Append(")");
    }

    public void Visit(BlingoPlayCmdStmtNode node)
    {
        Append("Play(");
        node.Command.Accept(this);
        AppendLine(");");
    }

    public void Visit(BlingoSoundCmdStmtNode node)
    {
        Append("Sound(");
        node.Command.Accept(this);
        AppendLine(");");
    }

    public void Visit(BlingoCursorStmtNode node)
    {
        Append("Cursor = ");
        node.Value.Accept(this);
        AppendLine(";");
    }

    public void Visit(BlingoGoToStmtNode node)
    {
        Append("_Movie.GoTo(");
        node.Target.Accept(this);
        AppendLine(");");
    }

    public void Visit(BlingoAssignmentStmtNode node)
    {
        if (node.Target is BlingoVarNode lhs && lhs.VarName.Equals("me", StringComparison.OrdinalIgnoreCase))
        {
            bool rhsIsVoid = node.Value is BlingoVarNode rhsVar && rhsVar.VarName.Equals("void", StringComparison.OrdinalIgnoreCase)
                || node.Value is BlingoDatumNode rhsDatum && rhsDatum.Datum.Type == BlingoDatum.DatumType.Void;

            if (rhsIsVoid)
                return;
        }

        node.Target.Accept(this);
        Append(" = ");
        var start = _sb.Length;
        node.Value.Accept(this);
        TrimSemicolon(start);
        AppendLine(";");
    }

    public void Visit(BlingoSendSpriteStmtNode node)
    {
        Append("SendSprite");
        string param = "sprite";
        if (!string.IsNullOrEmpty(node.TargetType))
        {
            Append("<");
            Append(node.TargetType);
            Append(">");
            param = node.TargetType.ToLowerInvariant();
        }
        Append("(");
        node.Sprite.Accept(this);
        Append($", {param} => {param}.");
        if (node.Message is BlingoDatumNode dn && dn.Datum.Type == BlingoDatum.DatumType.Symbol)
            Append(dn.Datum.AsSymbol());
        else
            node.Message.Accept(this);
        Append("(");
        node.Arguments?.Accept(this);
        Append("));");
        AppendLine();
    }

    public void Visit(BlingoSendSpriteExprNode node)
    {
        Append("SendSprite");
        string param = "sprite";
        if (!string.IsNullOrEmpty(node.TargetType))
        {
            Append("<");
            Append(node.TargetType);
            Append(">");
            param = node.TargetType.ToLowerInvariant();
        }
        Append("(");
        node.Sprite.Accept(this);
        Append($", {param} => {param}.");
        if (node.Message is BlingoDatumNode dn && dn.Datum.Type == BlingoDatum.DatumType.Symbol)
            Append(dn.Datum.AsSymbol());
        else
            node.Message.Accept(this);
        Append("(");
        node.Arguments?.Accept(this);
        Append("))");
    }

    public void Visit(BlingoExitRepeatStmtNode node) => AppendLine("break;");

    public void Visit(BlingoNextRepeatStmtNode node) => AppendLine("continue;");

    public void Visit(BlingoRangeExprNode node)
    {
        node.Start.Accept(this);
        Append("..");
        node.End.Accept(this);
    }

    public void Visit(BlingoObjBracketExprNode node)
    {
        node.Object.Accept(this);
        Append("[");
        node.Index.Accept(this);
        Append("]");
    }

    public void Visit(BlingoChunkDeleteStmtNode node)
    {
        Append("DeleteChunk(");
        node.Chunk.Accept(this);
        AppendLine(");");
    }

    public void Visit(BlingoChunkHiliteStmtNode node)
    {
        Append("Hilite(");
        node.Chunk.Accept(this);
        AppendLine(");");
    }

    public void Visit(BlingoGlobalDeclStmtNode node) { }

    public void Visit(BlingoInstanceDeclStmtNode node) { }

    public void Visit(BlingoRepeatWhileStmtNode node)
    {
        Append("while (");
        node.Condition.Accept(this);
        AppendLine(")");
        AppendLine("{");
        Indent();
        node.Body.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(BlingoRepeatWithInStmtNode node)
    {
        Append($"foreach (var {node.Variable} in ");
        node.List.Accept(this);
        AppendLine(")");
        AppendLine("{");
        Indent();
        node.Body.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(BlingoRepeatWithToStmtNode node)
    {
        Append($"for (var {node.Variable} = ");
        node.Start.Accept(this);
        Append($"; {node.Variable} <= ");
        node.End.Accept(this);
        Append($"; {node.Variable}++)");
        AppendLine();
        AppendLine("{");
        Indent();
        node.Body.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(BlingoSpriteWithinExprNode node)
    {
        Append("SpriteWithin(");
        node.SpriteA.Accept(this);
        Append(", ");
        node.SpriteB.Accept(this);
        Append(")");
    }

    public void Visit(BlingoLastStringChunkExprNode node)
    {
        Append("LastChunkOf(");
        node.Source.Accept(this);
        Append(")");
    }

    public void Visit(BlingoSpriteIntersectsExprNode node)
    {
        Append("SpriteIntersects(");
        node.SpriteA.Accept(this);
        Append(", ");
        node.SpriteB.Accept(this);
        Append(")");
    }

    public void Visit(BlingoStringChunkCountExprNode node)
    {
        Append("ChunkCount(");
        node.Source.Accept(this);
        Append(")");
    }

    public void Visit(BlingoNotOpNode node)
    {
        Append("!(");
        node.Expr.Accept(this);
        Append(")");
    }


    public void Visit(BlingoVarNode node)
    {
        if (node.VarName.Equals("me", StringComparison.OrdinalIgnoreCase))
        {
            Append("this");
        }
        else if (node.VarName.Equals("void", StringComparison.OrdinalIgnoreCase))
        {
            Append("null");
        }
        else
        {
            Append(node.VarName);
        }
    }

    public void Visit(BlingoBlockNode node)
    {
        foreach (var child in node.Children)
        {
            child.Accept(this);
        }
    }

    public void Visit(BlingoDatumNode datumNode)
    {
        Append(DatumToCSharp(datumNode.Datum));
    }

    public void Visit(BlingoRepeatWithStmtNode repeatWithStmtNode)
    {
        Append($"for (var {repeatWithStmtNode.Variable} = ");
        repeatWithStmtNode.Start.Accept(this);
        Append($"; {repeatWithStmtNode.Variable} <= ");
        repeatWithStmtNode.End.Accept(this);
        Append($"; {repeatWithStmtNode.Variable}++)");
        AppendLine();
        AppendLine("{");
        Indent();
        repeatWithStmtNode.Body.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(BlingoRepeatUntilStmtNode repeatUntilStmtNode)
    {
        AppendLine("do");
        AppendLine("{");
        Indent();
        repeatUntilStmtNode.Body.Accept(this);
        Unindent();
        Append("} while (!(");
        repeatUntilStmtNode.Condition.Accept(this);
        AppendLine("));");
    }

    public void Visit(BlingoRepeatForeverStmtNode repeatForeverStmtNode)
    {
        AppendLine("while (true)");
        AppendLine("{");
        Indent();
        repeatForeverStmtNode.Body.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(BlingoRepeatTimesStmtNode repeatTimesStmtNode)
    {
        Append("for (int i = 1; i <= ");
        repeatTimesStmtNode.Count.Accept(this);
        AppendLine("; i++)");
        AppendLine("{");
        Indent();
        repeatTimesStmtNode.Body.Accept(this);
        Unindent();
        AppendLine("}");
    }

    public void Visit(BlingoExitRepeatIfStmtNode exitRepeatIfStmtNode)
    {
        Append("if (");
        exitRepeatIfStmtNode.Condition.Accept(this);
        AppendLine(") break;");
    }

    public void Visit(BlingoNextRepeatIfStmtNode nextRepeatIfStmtNode)
    {
        Append("if (");
        nextRepeatIfStmtNode.Condition.Accept(this);
        AppendLine(") continue;");
    }

    public void Visit(BlingoNextStmtNode nextStmtNode) => AppendLine("// next");

    private void TrimSemicolon(int startLen)
    {
        if (_sb.Length - startLen >= 1 && _sb[^1] == '\n')
        {
            if (_sb.Length - startLen >= 2 && _sb[^2] == ';')
                _sb.Length -= 2;
            else if (_sb.Length - startLen >= 3 && _sb[^2] == '\r' && _sb[^3] == ';')
                _sb.Length -= 3;
            _atLineStart = false;
        }
        else if (_sb.Length - startLen >= 1 && _sb[^1] == ';')
        {
            _sb.Length -= 1;
            _atLineStart = false;
        }
    }

}

