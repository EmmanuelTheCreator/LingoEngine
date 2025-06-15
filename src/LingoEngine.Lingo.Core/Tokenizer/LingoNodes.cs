
namespace LingoEngine.Lingo.Core.Tokenizer
{
    public abstract class LingoNode
    {
        public LingoNode? Parent { get; set; }
        public abstract void Accept(ILingoAstVisitor visitor);
    }

    public class LingoHandlerNode : LingoNode
    {
        public LingoHandler Handler { get; set; }
        public LingoBlockNode Block { get; set; }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoErrorNode : LingoNode
    {
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class LingoExitRepeatIfStmtNode : LingoNode
    {
        public LingoNode Condition { get; }

        public LingoExitRepeatIfStmtNode(LingoNode condition)
        {
            Condition = condition;
        }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class LingoExitStmtNode : LingoNode
    {
        public LingoExitStmtNode()
        {
        }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class LingoNextStmtNode : LingoNode
    {
        public LingoNextStmtNode()
        {
        }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoReturnStmtNode : LingoNode
    {
        public LingoReturnStmtNode(LingoNode? value)
        {
            Value = value;
        }

        public LingoNode? Value { get; }

        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class LingoNextRepeatIfStmtNode : LingoNode
    {
        public LingoNode Condition { get; }

        public LingoNextRepeatIfStmtNode(LingoNode condition)
        {
            Condition = condition;
        }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class LingoCommentNode : LingoNode
    {
        public string Text { get; set; } = string.Empty;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoNewObjNode : LingoNode
    {
        public string ObjType { get; set; } = string.Empty;
        public LingoNode ObjArgs { get; set; }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoLiteralNode : LingoNode
    {
        public LingoDatum Value { get; set; }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }


    public class LingoIfStmtNode : LingoNode
    {
        public LingoIfStmtNode(LingoNode condition, LingoNode thenBlock, LingoNode? elseBlock)
        {
            Condition = condition;
            ThenBlock = thenBlock;
            ElseBlock = elseBlock;
        }

        public bool HasElse => ElseBlock != null;
        public LingoNode Condition { get; set; }
        public LingoNode ThenBlock { get; }
        public LingoNode? ElseBlock { get; }

        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoIfElseStmtNode : LingoNode
    {
        public LingoIfElseStmtNode(LingoNode condition, LingoBlockNode thenBlock, LingoBlockNode elseBlock)
        {
            Condition = condition;
            ThenBlock = thenBlock;
            ElseBlock = elseBlock;
        }

        public LingoNode Condition { get; }
        public LingoBlockNode ThenBlock { get; }
        public LingoBlockNode ElseBlock { get; }

        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoEndCaseNode : LingoNode
    {
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoObjCallNode : LingoNode
    {
        public LingoLiteralNode Name { get; set; }
        public LingoDatumNode ArgList { get; set; }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoPutStmtNode : LingoNode
    {
        public LingoPutStmtNode(LingoNode value, LingoNode target)
        {
            Value = value;
            Target = target;
        }

        public LingoNode Value { get; set; }
        public LingoPutType Type { get; set; }
        public LingoNode Variable { get; set; }
        public LingoNode Target { get; }

        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoGlobalDeclStmtNode : LingoNode
    {
        public List<string> Names { get; } = new();
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoPropertyDeclStmtNode : LingoNode
    {
        public List<string> Names { get; } = new();
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoInstanceDeclStmtNode : LingoNode
    {
        public List<string> Names { get; } = new();
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoTheExprNode : LingoNode
    {
        public string Prop { get; set; } = string.Empty;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoBinaryOpNode : LingoNode
    {
        public LingoNode Left { get; set; }
        public LingoNode Right { get; set; }
        public LingoBinaryOpcode Opcode { get; set; }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    #region Switch case

    public class LingoCaseStmtNode : LingoNode
    {
        public LingoNode Value { get; set; }
        public LingoNode? FirstLabel { get; set; }
        public LingoOtherwiseNode? Otherwise { get; set; }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class LingoCaseLabelNode : LingoNode
    {
        public LingoNode Value { get; set; }
        public LingoNode? Block { get; set; }

        public LingoCaseLabelNode? NextOr { get; set; }
        public LingoCaseLabelNode? NextLabel { get; set; }

        public LingoCaseStmtNode? Parent { get; set; }

        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    #endregion


    public class LingoTellStmtNode : LingoNode
    {
        public LingoNode Window { get; set; }
        public LingoBlockNode Block { get; set; }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoWhenStmtNode : LingoNode
    {
        public int Event { get; set; }
        public string Script { get; set; } = string.Empty;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoOtherwiseNode : LingoNode
    {
        public LingoBlockNode Block { get; set; }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoBlockNode : LingoNode
    {
        public List<LingoNode> Children { get; set; } = new();
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoVarNode : LingoNode
    {
        public string VarName { get; set; } = string.Empty;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoDatumNode : LingoNode
    {
        public LingoDatumNode(LingoDatum datum)
        {
            Datum = datum;
        }

        public LingoDatum Value { get; set; }
        public LingoDatum Datum { get; }

        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }



    public class LingoChunkExprNode : LingoNode
    {
        public LingoNode Expr { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoInverseOpNode : LingoNode
    {
        public LingoNode Expr { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoObjCallV4Node : LingoNode
    {
        public LingoNode Object { get; set; } = null!;
        public LingoLiteralNode Name { get; set; } = null!;
        public LingoDatumNode ArgList { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoMemberExprNode : LingoNode
    {
        public LingoNode Expr { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoObjPropExprNode : LingoNode
    {
        public LingoNode Object { get; set; } = null!;
        public LingoNode Property { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoPlayCmdStmtNode : LingoNode
    {
        public LingoNode Command { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoThePropExprNode : LingoNode
    {
        public LingoNode Property { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoMenuPropExprNode : LingoNode
    {
        public LingoNode Menu { get; set; } = null!;
        public LingoNode Property { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoSoundCmdStmtNode : LingoNode
    {
        public LingoNode Command { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoSoundPropExprNode : LingoNode
    {
        public LingoNode Sound { get; set; } = null!;
        public LingoNode Property { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoAssignmentStmtNode : LingoNode
    {
        public LingoNode Target { get; set; } = null!;
        public LingoNode Value { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoExitRepeatStmtNode : LingoNode
    {
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoNextRepeatStmtNode : LingoNode
    {
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoObjBracketExprNode : LingoNode
    {
        public LingoNode Object { get; set; } = null!;
        public LingoNode Index { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoSpritePropExprNode : LingoNode
    {
        public LingoNode Sprite { get; set; } = null!;
        public LingoNode Property { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoChunkDeleteStmtNode : LingoNode
    {
        public LingoNode Chunk { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoChunkHiliteStmtNode : LingoNode
    {
        public LingoNode Chunk { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoRepeatWhileStmtNode : LingoNode
    {
        public LingoRepeatWhileStmtNode(LingoNode condition, LingoBlockNode body)
        {
            Condition = condition;
            Body = body;
        }

        public LingoNode Condition { get; set; } = null!;
        public LingoNode Body { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class LingoRepeatUntilStmtNode : LingoNode
    {
        public LingoNode Condition { get; }
        public LingoBlockNode Body { get; }

        public LingoRepeatUntilStmtNode(LingoNode condition, LingoBlockNode body)
        {
            Condition = condition;
            Body = body;
        }

        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class LingoRepeatWithStmtNode : LingoNode
    {
        public string Variable { get; }
        public LingoNode Start { get; }
        public LingoNode End { get; }
        public LingoBlockNode Body { get; }

        public LingoRepeatWithStmtNode(string variable, LingoNode start, LingoNode end, LingoBlockNode body)
        {
            Variable = variable;
            Start = start;
            End = end;
            Body = body;
        }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class LingoRepeatTimesStmtNode : LingoNode
    {
        public LingoNode Count { get; }
        public LingoBlockNode Body { get; }

        public LingoRepeatTimesStmtNode(LingoNode count, LingoBlockNode body)
        {
            Count = count;
            Body = body;
        }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class LingoRepeatForeverStmtNode : LingoNode
    {
        public LingoBlockNode Body { get; }

        public LingoRepeatForeverStmtNode(LingoBlockNode body)
        {
            Body = body;
        }
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class LingoMenuItemPropExprNode : LingoNode
    {
        public LingoNode MenuItem { get; set; } = null!;
        public LingoNode Property { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoObjPropIndexExprNode : LingoNode
    {
        public LingoNode Object { get; set; } = null!;
        public LingoNode PropertyIndex { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoRepeatWithInStmtNode : LingoNode
    {
        public string Variable { get; set; } = string.Empty;
        public LingoNode List { get; set; } = null!;
        public LingoNode Body { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoRepeatWithToStmtNode : LingoNode
    {
        public string Variable { get; set; } = string.Empty;
        public LingoNode Start { get; set; } = null!;
        public LingoNode End { get; set; } = null!;
        public LingoNode Body { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoSpriteWithinExprNode : LingoNode
    {
        public LingoNode SpriteA { get; set; } = null!;
        public LingoNode SpriteB { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoLastStringChunkExprNode : LingoNode
    {
        public LingoNode Source { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoSpriteIntersectsExprNode : LingoNode
    {
        public LingoNode SpriteA { get; set; } = null!;
        public LingoNode SpriteB { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoStringChunkCountExprNode : LingoNode
    {
        public LingoNode Source { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoNotOpNode : LingoNode
    {
        public LingoNode Expr { get; set; } = null!;
        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class LingoCallNode : LingoNode
    {
        public LingoNode Callee { get; set; } = null!;
        public LingoDatumNode Arguments { get; set; } = null!;
        public string Name { get; set; } = "";

        public override void Accept(ILingoAstVisitor visitor) => visitor.Visit(this);
    }


}


