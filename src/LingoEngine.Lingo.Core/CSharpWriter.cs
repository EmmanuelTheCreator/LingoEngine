using System.Text;
using LingoEngine.Lingo.Core.Tokenizer;

namespace LingoEngine.Lingo.Core;

/// <summary>
/// Writes a small subset of Lingo AST to C# using the visitor pattern.
/// Only a handful of nodes are supported for now.
/// </summary>
public class CSharpWriter : ILingoAstVisitor
{
    private readonly StringBuilder _sb = new();

    /// <summary>Converts the given AST node to C#.</summary>
    public static string Write(LingoNode node)
    {
        var writer = new CSharpWriter();
        node.Accept(writer);
        return writer._sb.ToString();
    }

    /// <summary>Fallback helper for token sequences.</summary>
    public static string WriteTokens(IEnumerable<LingoToken> tokens)
    {
        var sb = new StringBuilder();
        foreach (var tok in tokens)
        {
            switch (tok.Type)
            {
                case LingoTokenType.Put:
                    sb.Append("// put\n");
                    break;
                case LingoTokenType.Into:
                    sb.Append("// into\n");
                    break;
                default:
                    sb.Append(tok.Lexeme + " ");
                    break;
            }
        }
        return sb.ToString();
    }

    private void Append(string text) => _sb.Append(text);
    private void AppendLine(string text) => _sb.AppendLine(text);
    private void AppendLine() => _sb.AppendLine();

    private static string DatumToCSharp(LingoDatum datum)
    {
        return datum.Type switch
        {
            LingoDatum.DatumType.Integer or LingoDatum.DatumType.Float => datum.AsString(),
            LingoDatum.DatumType.String => $"\"{datum.AsString()}\"",
            LingoDatum.DatumType.Symbol or LingoDatum.DatumType.VarRef => datum.AsString(),
            _ => datum.AsString()
        };
    }

    private void Unsupported(LingoNode node)
    {
        // Unhandled nodes are ignored for now
    }

    public void Visit(LingoHandlerNode node) => node.Block.Accept(this);

    public void Visit(LingoErrorNode node) => AppendLine("// error");

    public void Visit(LingoCommentNode node)
    {
        Append("// ");
        Append(node.Text);
        AppendLine();
    }

    public void Visit(LingoNewObjNode node)
    {
        Append("new ");
        Append(node.ObjType);
        Append("(");
        node.ObjArgs.Accept(this);
        Append(")");
    }

    public void Visit(LingoLiteralNode node) => Append(node.Value.ToString());

    public void Visit(LingoIfStmtNode node)
    {
        Append("if (");
        node.Condition.Accept(this);
        AppendLine(")");
        AppendLine("{");
        node.ThenBlock.Accept(this);
        AppendLine("}");
        if (node.HasElse)
        {
            AppendLine("else");
            AppendLine("{");
            node.ElseBlock!.Accept(this);
            AppendLine("}");
        }
    }

    public void Visit(LingoIfElseStmtNode node)
    {
        Append("if (");
        node.Condition.Accept(this);
        AppendLine(")");
        AppendLine("{");
        node.ThenBlock.Accept(this);
        AppendLine("}");
        AppendLine("else");
        AppendLine("{");
        node.ElseBlock.Accept(this);
        AppendLine("}");
    }

    public void Visit(LingoEndCaseNode node) => AppendLine("// end case");

    public void Visit(LingoObjCallNode node)
    {
        Append(node.Name.Value.AsString());
        Append("(");
        node.ArgList.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoPutStmtNode node)
    {
        node.Target.Accept(this);
        Append(" = ");
        node.Value.Accept(this);
        AppendLine(";");
    }

    public void Visit(LingoTheExprNode node)
    {
        var propLower = node.Prop.ToLowerInvariant();
        if (propLower.StartsWith("mouse"))
        {
            Append("_Mouse.");
            Append(char.ToUpperInvariant(node.Prop[0]) + node.Prop.Substring(1));
        }
        else if (propLower == "actorlist")
        {
            Append("_Movie.ActorList");
        }
        else
        {
            Append($"/* the {node.Prop} */");
        }
    }

    public void Visit(LingoBinaryOpNode node)
    {
        bool needsParensLeft = node.Left is LingoBinaryOpNode;
        bool needsParensRight = node.Right is LingoBinaryOpNode;

        if (needsParensLeft) Append("(");
        node.Left.Accept(this);
        if (needsParensLeft) Append(")");

        Append(" ");
        Append(BinaryOpcodeToString(node.Opcode));
        Append(" ");

        if (needsParensRight) Append("(");
        node.Right.Accept(this);
        if (needsParensRight) Append(")");
    }

    private static string BinaryOpcodeToString(LingoBinaryOpcode opcode)
    {
        return opcode switch
        {
            LingoBinaryOpcode.Add => "+",
            LingoBinaryOpcode.Subtract => "-",
            LingoBinaryOpcode.Multiply => "*",
            LingoBinaryOpcode.Divide => "/",
            LingoBinaryOpcode.Modulo => "%",
            LingoBinaryOpcode.And => "&&",
            LingoBinaryOpcode.Or => "||",
            LingoBinaryOpcode.Concat => "+",
            LingoBinaryOpcode.Equals => "==",
            LingoBinaryOpcode.NotEquals => "!=",
            LingoBinaryOpcode.GreaterThan => ">",
            LingoBinaryOpcode.GreaterOrEqual => ">=",
            LingoBinaryOpcode.LessThan => "<",
            LingoBinaryOpcode.LessOrEqual => "<=",
            _ => opcode.ToString()
        };
    }

    public void Visit(LingoCaseStmtNode node)
    {
        Append("switch (");
        node.Value.Accept(this);
        AppendLine(")");
        AppendLine("{");
        var label = node.FirstLabel as LingoCaseLabelNode;
        while (label != null)
        {
            Append("case ");
            label.Value.Accept(this);
            AppendLine(":");
            label.Block?.Accept(this);
            label = label.NextLabel;
        }
        if (node.Otherwise != null)
        {
            AppendLine("default:");
            node.Otherwise.Accept(this);
        }
        AppendLine("}");
    }

    public void Visit(LingoExitStmtNode node) => AppendLine("return;");

    public void Visit(LingoReturnStmtNode node)
    {
        Append("return");
        if (node.Value != null)
        {
            Append(" ");
            node.Value.Accept(this);
        }
        AppendLine(";");
    }

    public void Visit(LingoTellStmtNode node)
    {
        AppendLine("// tell block");
        node.Block.Accept(this);
    }

    public void Visit(LingoWhenStmtNode node) => AppendLine("// when statement");

    public void Visit(LingoOtherwiseNode node) => node.Block.Accept(this);

    public void Visit(LingoCaseLabelNode node)
    {
        Append("case ");
        node.Value.Accept(this);
        AppendLine(":");
        node.Block?.Accept(this);
    }

    public void Visit(LingoChunkExprNode node) => node.Expr.Accept(this);

    public void Visit(LingoInverseOpNode node)
    {
        Append("!(");
        node.Expr.Accept(this);
        Append(")");
    }

    public void Visit(LingoObjCallV4Node node)
    {
        node.Object.Accept(this);
        Append(".");
        Append(node.Name.Value.AsString());
        Append("(");
        node.ArgList.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoMemberExprNode node)
    {
        Append("Member(");
        node.Expr.Accept(this);
        Append(")");
    }

    public void Visit(LingoObjPropExprNode node)
    {
        node.Object.Accept(this);
        Append(".");
        node.Property.Accept(this);
    }

    public void Visit(LingoPlayCmdStmtNode node)
    {
        Append("Play(");
        node.Command.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoThePropExprNode node)
    {
        Append("TheProp(");
        node.Property.Accept(this);
        Append(")");
    }

    public void Visit(LingoMenuPropExprNode node)
    {
        Append("MenuProp(");
        node.Menu.Accept(this);
        Append(", ");
        node.Property.Accept(this);
        Append(")");
    }

    public void Visit(LingoSoundCmdStmtNode node)
    {
        Append("Sound(");
        node.Command.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoSoundPropExprNode node)
    {
        Append("Sound(");
        node.Sound.Accept(this);
        Append(").");
        node.Property.Accept(this);
    }

    public void Visit(LingoAssignmentStmtNode node)
    {
        node.Target.Accept(this);
        Append(" = ");
        node.Value.Accept(this);
        AppendLine(";");
    }

    public void Visit(LingoSendSpriteStmtNode node)
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
        if (node.Message is LingoDatumNode dn && dn.Datum.Type == LingoDatum.DatumType.Symbol)
            Append(dn.Datum.AsSymbol());
        else
            node.Message.Accept(this);
        Append("());");
        AppendLine();
    }

    public void Visit(LingoExitRepeatStmtNode node) => AppendLine("break;");

    public void Visit(LingoNextRepeatStmtNode node) => AppendLine("continue;");

    public void Visit(LingoObjBracketExprNode node)
    {
        node.Object.Accept(this);
        Append("[");
        node.Index.Accept(this);
        Append("]");
    }

    public void Visit(LingoSpritePropExprNode node)
    {
        Append("Sprite(");
        node.Sprite.Accept(this);
        Append(").");
        node.Property.Accept(this);
    }

    public void Visit(LingoChunkDeleteStmtNode node)
    {
        Append("DeleteChunk(");
        node.Chunk.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoChunkHiliteStmtNode node)
    {
        Append("Hilite(");
        node.Chunk.Accept(this);
        AppendLine(");");
    }

    public void Visit(LingoGlobalDeclStmtNode node)
    {
        Append("var ");
        Append(string.Join(", ", node.Names));
        AppendLine(";");
    }

    public void Visit(LingoPropertyDeclStmtNode node)
    {
        Append("var ");
        Append(string.Join(", ", node.Names));
        AppendLine(";");
    }

    public void Visit(LingoInstanceDeclStmtNode node)
    {
        Append("var ");
        Append(string.Join(", ", node.Names));
        AppendLine(";");
    }

    public void Visit(LingoRepeatWhileStmtNode node)
    {
        Append("while (");
        node.Condition.Accept(this);
        AppendLine(")");
        AppendLine("{");
        node.Body.Accept(this);
        AppendLine("}");
    }

    public void Visit(LingoMenuItemPropExprNode node)
    {
        Append("menuItem(");
        node.MenuItem.Accept(this);
        Append(").");
        node.Property.Accept(this);
    }

    public void Visit(LingoObjPropIndexExprNode node)
    {
        node.Object.Accept(this);
        Append(".prop[");
        node.PropertyIndex.Accept(this);
        Append("]");
    }

    public void Visit(LingoRepeatWithInStmtNode node)
    {
        Append($"foreach (var {node.Variable} in ");
        node.List.Accept(this);
        AppendLine(")");
        AppendLine("{");
        node.Body.Accept(this);
        AppendLine("}");
    }

    public void Visit(LingoRepeatWithToStmtNode node)
    {
        Append($"for (var {node.Variable} = ");
        node.Start.Accept(this);
        Append($"; {node.Variable} <= ");
        node.End.Accept(this);
        Append($"; {node.Variable}++)");
        AppendLine();
        AppendLine("{");
        node.Body.Accept(this);
        AppendLine("}");
    }

    public void Visit(LingoSpriteWithinExprNode node)
    {
        Append("SpriteWithin(");
        node.SpriteA.Accept(this);
        Append(", ");
        node.SpriteB.Accept(this);
        Append(")");
    }

    public void Visit(LingoLastStringChunkExprNode node)
    {
        Append("LastChunkOf(");
        node.Source.Accept(this);
        Append(")");
    }

    public void Visit(LingoSpriteIntersectsExprNode node)
    {
        Append("SpriteIntersects(");
        node.SpriteA.Accept(this);
        Append(", ");
        node.SpriteB.Accept(this);
        Append(")");
    }

    public void Visit(LingoStringChunkCountExprNode node)
    {
        Append("ChunkCount(");
        node.Source.Accept(this);
        Append(")");
    }

    public void Visit(LingoNotOpNode node)
    {
        Append("!(");
        node.Expr.Accept(this);
        Append(")");
    }

    public void Visit(LingoCallNode node)
    {
        if (!string.IsNullOrEmpty(node.Name))
        {
            if (!string.IsNullOrEmpty(node.TargetType))
            {
                var param = node.TargetType.ToLowerInvariant();
                Append("CallMovieScript<");
                Append(node.TargetType);
                Append(">(");
                Append($"{param} => {param}.");
                Append(node.Name);
                Append("());");
                AppendLine();
            }
            else
            {
                AppendLine($"{node.Name}();");
            }
            return;
        }

        node.Callee.Accept(this);
        Append("(");
        node.Arguments.Accept(this);
        Append(")");
    }

    public void Visit(LingoVarNode node) => Append(node.VarName);

    public void Visit(LingoBlockNode node)
    {
        foreach (var child in node.Children)
        {
            child.Accept(this);
        }
    }

    public void Visit(LingoDatumNode datumNode)
    {
        Append(DatumToCSharp(datumNode.Datum));
    }

    public void Visit(LingoRepeatWithStmtNode repeatWithStmtNode)
    {
        Append($"for (var {repeatWithStmtNode.Variable} = ");
        repeatWithStmtNode.Start.Accept(this);
        Append($"; {repeatWithStmtNode.Variable} <= ");
        repeatWithStmtNode.End.Accept(this);
        Append($"; {repeatWithStmtNode.Variable}++)");
        AppendLine();
        AppendLine("{");
        repeatWithStmtNode.Body.Accept(this);
        AppendLine("}");
    }

    public void Visit(LingoRepeatUntilStmtNode repeatUntilStmtNode)
    {
        AppendLine("do");
        AppendLine("{");
        repeatUntilStmtNode.Body.Accept(this);
        Append("}");
        Append(" while (!(");
        repeatUntilStmtNode.Condition.Accept(this);
        AppendLine("));");
    }

    public void Visit(LingoRepeatForeverStmtNode repeatForeverStmtNode)
    {
        AppendLine("while (true)");
        AppendLine("{");
        repeatForeverStmtNode.Body.Accept(this);
        AppendLine("}");
    }

    public void Visit(LingoRepeatTimesStmtNode repeatTimesStmtNode)
    {
        Append("for (int i = 1; i <= ");
        repeatTimesStmtNode.Count.Accept(this);
        AppendLine("; i++)");
        AppendLine("{");
        repeatTimesStmtNode.Body.Accept(this);
        AppendLine("}");
    }

    public void Visit(LingoExitRepeatIfStmtNode exitRepeatIfStmtNode)
    {
        Append("if (");
        exitRepeatIfStmtNode.Condition.Accept(this);
        AppendLine(") break;");
    }

    public void Visit(LingoNextRepeatIfStmtNode nextRepeatIfStmtNode)
    {
        Append("if (");
        nextRepeatIfStmtNode.Condition.Accept(this);
        AppendLine(") continue;");
    }

    public void Visit(LingoNextStmtNode nextStmtNode) => AppendLine("// next");
}
