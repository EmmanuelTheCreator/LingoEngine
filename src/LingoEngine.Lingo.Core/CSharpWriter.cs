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

    public void Visit(LingoErrorNode node) => Unsupported(node);

    public void Visit(LingoCommentNode node)
    {
        Append("// ");
        Append(node.Text);
        AppendLine();
    }

    public void Visit(LingoNewObjNode node) => Unsupported(node);

    public void Visit(LingoLiteralNode node) => Append(node.Value.ToString());

    public void Visit(LingoIfStmtNode node) => Unsupported(node);

    public void Visit(LingoIfElseStmtNode node) => Unsupported(node);

    public void Visit(LingoEndCaseNode node) => Unsupported(node);

    public void Visit(LingoObjCallNode node) => Unsupported(node);

    public void Visit(LingoPutStmtNode node)
    {
        node.Target.Accept(this);
        Append(" = ");
        node.Value.Accept(this);
        AppendLine(";");
    }

    public void Visit(LingoTheExprNode node) => Unsupported(node);

    public void Visit(LingoBinaryOpNode node) => Unsupported(node);

    public void Visit(LingoCaseStmtNode node) => Unsupported(node);

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

    public void Visit(LingoTellStmtNode node) => Unsupported(node);

    public void Visit(LingoWhenStmtNode node) => Unsupported(node);

    public void Visit(LingoOtherwiseNode node) => Unsupported(node);

    public void Visit(LingoCaseLabelNode node) => Unsupported(node);

    public void Visit(LingoChunkExprNode node) => Unsupported(node);

    public void Visit(LingoInverseOpNode node) => Unsupported(node);

    public void Visit(LingoObjCallV4Node node) => Unsupported(node);

    public void Visit(LingoMemberExprNode node) => Unsupported(node);

    public void Visit(LingoObjPropExprNode node) => Unsupported(node);

    public void Visit(LingoPlayCmdStmtNode node) => Unsupported(node);

    public void Visit(LingoThePropExprNode node) => Unsupported(node);

    public void Visit(LingoMenuPropExprNode node) => Unsupported(node);

    public void Visit(LingoSoundCmdStmtNode node) => Unsupported(node);

    public void Visit(LingoSoundPropExprNode node) => Unsupported(node);

    public void Visit(LingoAssignmentStmtNode node)
    {
        node.Target.Accept(this);
        Append(" = ");
        node.Value.Accept(this);
        AppendLine(";");
    }

    public void Visit(LingoExitRepeatStmtNode node) => Unsupported(node);

    public void Visit(LingoNextRepeatStmtNode node) => Unsupported(node);

    public void Visit(LingoObjBracketExprNode node) => Unsupported(node);

    public void Visit(LingoSpritePropExprNode node) => Unsupported(node);

    public void Visit(LingoChunkDeleteStmtNode node) => Unsupported(node);

    public void Visit(LingoChunkHiliteStmtNode node) => Unsupported(node);

    public void Visit(LingoGlobalDeclStmtNode node) => Unsupported(node);

    public void Visit(LingoPropertyDeclStmtNode node) => Unsupported(node);

    public void Visit(LingoInstanceDeclStmtNode node) => Unsupported(node);

    public void Visit(LingoRepeatWhileStmtNode node) => Unsupported(node);

    public void Visit(LingoMenuItemPropExprNode node) => Unsupported(node);

    public void Visit(LingoObjPropIndexExprNode node) => Unsupported(node);

    public void Visit(LingoRepeatWithInStmtNode node) => Unsupported(node);

    public void Visit(LingoRepeatWithToStmtNode node) => Unsupported(node);

    public void Visit(LingoSpriteWithinExprNode node) => Unsupported(node);

    public void Visit(LingoLastStringChunkExprNode node) => Unsupported(node);

    public void Visit(LingoSpriteIntersectsExprNode node) => Unsupported(node);

    public void Visit(LingoStringChunkCountExprNode node) => Unsupported(node);

    public void Visit(LingoNotOpNode node) => Unsupported(node);

    public void Visit(LingoCallNode node) => Unsupported(node);

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

    public void Visit(LingoRepeatWithStmtNode repeatWithStmtNode) => Unsupported(repeatWithStmtNode);

    public void Visit(LingoRepeatUntilStmtNode repeatUntilStmtNode) => Unsupported(repeatUntilStmtNode);

    public void Visit(LingoRepeatForeverStmtNode repeatForeverStmtNode) => Unsupported(repeatForeverStmtNode);

    public void Visit(LingoRepeatTimesStmtNode repeatTimesStmtNode) => Unsupported(repeatTimesStmtNode);

    public void Visit(LingoExitRepeatIfStmtNode exitRepeatIfStmtNode) => Unsupported(exitRepeatIfStmtNode);

    public void Visit(LingoNextRepeatIfStmtNode nextRepeatIfStmtNode) => Unsupported(nextRepeatIfStmtNode);

    public void Visit(LingoNextStmtNode nextStmtNode) => Unsupported(nextStmtNode);
}
