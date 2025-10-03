using System.Collections.Generic;

namespace OldBlingoEngine.Lingo.Core.Tokenizer
{
    public abstract class BlingoNode
    {
        public BlingoNode? Parent { get; set; }
        public abstract void Accept(IBlingoAstVisitor visitor);
    }

    public class BlingoHandlerNode : BlingoNode
    {
        public BlingoHandler Handler { get; set; }
        public BlingoBlockNode Block { get; set; }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoErrorNode : BlingoNode
    {
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class BlingoExitRepeatIfStmtNode : BlingoNode
    {
        public BlingoNode Condition { get; }

        public BlingoExitRepeatIfStmtNode(BlingoNode condition)
        {
            Condition = condition;
        }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class BlingoExitStmtNode : BlingoNode
    {
        public BlingoExitStmtNode()
        {
        }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class BlingoNextStmtNode : BlingoNode
    {
        public BlingoNextStmtNode()
        {
        }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoReturnStmtNode : BlingoNode
    {
        public BlingoReturnStmtNode(BlingoNode? value)
        {
            Value = value;
        }

        public BlingoNode? Value { get; }

        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class BlingoNextRepeatIfStmtNode : BlingoNode
    {
        public BlingoNode Condition { get; }

        public BlingoNextRepeatIfStmtNode(BlingoNode condition)
        {
            Condition = condition;
        }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class BlingoCommentNode : BlingoNode
    {
        public string Text { get; set; } = string.Empty;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoNewObjNode : BlingoNode
    {
        public string ObjType { get; set; } = string.Empty;
        public BlingoNode ObjArgs { get; set; }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoLiteralNode : BlingoNode
    {
        public BlingoDatum Value { get; set; }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }


    public class BlingoIfStmtNode : BlingoNode
    {
        public BlingoIfStmtNode(BlingoNode condition, BlingoNode thenBlock, BlingoNode? elseBlock)
        {
            Condition = condition;
            ThenBlock = thenBlock;
            ElseBlock = elseBlock;
        }

        public bool HasElse => ElseBlock != null;
        public BlingoNode Condition { get; set; }
        public BlingoNode ThenBlock { get; }
        public BlingoNode? ElseBlock { get; }

        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoIfElseStmtNode : BlingoNode
    {
        public BlingoIfElseStmtNode(BlingoNode condition, BlingoBlockNode thenBlock, BlingoBlockNode elseBlock)
        {
            Condition = condition;
            ThenBlock = thenBlock;
            ElseBlock = elseBlock;
        }

        public BlingoNode Condition { get; }
        public BlingoBlockNode ThenBlock { get; }
        public BlingoBlockNode ElseBlock { get; }

        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoEndCaseNode : BlingoNode
    {
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoObjCallNode : BlingoNode
    {
        public BlingoLiteralNode Name { get; set; }
        public BlingoDatumNode ArgList { get; set; }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoPutStmtNode : BlingoNode
    {
        public BlingoPutStmtNode(BlingoNode value, BlingoNode? target)
        {
            Value = value;
            Target = target;
            Variable = target;
        }

        public BlingoNode Value { get; set; }
        public BlingoPutType Type { get; set; }
        public BlingoNode? Variable { get; set; }
        public BlingoNode? Target { get; }

        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoGlobalDeclStmtNode : BlingoNode
    {
        public List<string> Names { get; } = new();
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoPropertyDeclStmtNode : BlingoNode
    {
        public List<string> Names { get; } = new();
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoInstanceDeclStmtNode : BlingoNode
    {
        public List<string> Names { get; } = new();
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoTheExprNode : BlingoNode
    {
        public string Prop { get; set; } = string.Empty;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoBinaryOpNode : BlingoNode
    {
        public BlingoNode Left { get; set; }
        public BlingoNode Right { get; set; }
        public BlingoBinaryOpcode Opcode { get; set; }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    #region Switch case

    public class BlingoCaseStmtNode : BlingoNode
    {
        public BlingoNode Value { get; set; }
        public BlingoNode? FirstLabel { get; set; }
        public BlingoOtherwiseNode? Otherwise { get; set; }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class BlingoCaseLabelNode : BlingoNode
    {
        public BlingoNode Value { get; set; }
        public BlingoNode? Block { get; set; }

        public BlingoCaseLabelNode? NextOr { get; set; }
        public BlingoCaseLabelNode? NextLabel { get; set; }

        public BlingoCaseStmtNode? Parent { get; set; }

        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    #endregion


    public class BlingoTellStmtNode : BlingoNode
    {
        public BlingoNode Window { get; set; }
        public BlingoBlockNode Block { get; set; }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoWhenStmtNode : BlingoNode
    {
        public int Event { get; set; }
        public string Script { get; set; } = string.Empty;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoOtherwiseNode : BlingoNode
    {
        public BlingoBlockNode Block { get; set; }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoBlockNode : BlingoNode
    {
        public List<BlingoNode> Children { get; set; } = new();
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoVarNode : BlingoNode
    {
        public string VarName { get; set; } = string.Empty;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoDatumNode : BlingoNode
    {
        public BlingoDatumNode(BlingoDatum datum)
        {
            Datum = datum;
        }

        public BlingoDatum Value { get; set; }
        public BlingoDatum Datum { get; }

        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }



    public class BlingoChunkExprNode : BlingoNode
    {
        public BlingoNode Expr { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoInverseOpNode : BlingoNode
    {
        public BlingoNode Expr { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoObjCallV4Node : BlingoNode
    {
        public BlingoNode Object { get; set; } = null!;
        public BlingoLiteralNode Name { get; set; } = null!;
        public BlingoDatumNode ArgList { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoMemberExprNode : BlingoNode
    {
        public BlingoNode Expr { get; set; } = null!;
        public BlingoNode? CastLib { get; set; }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoObjPropExprNode : BlingoNode
    {
        public BlingoNode Object { get; set; } = null!;
        public BlingoNode Property { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoPlayCmdStmtNode : BlingoNode
    {
        public BlingoNode Command { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoThePropExprNode : BlingoNode
    {
        public BlingoNode Property { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoMenuPropExprNode : BlingoNode
    {
        public BlingoNode Menu { get; set; } = null!;
        public BlingoNode Property { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoSoundCmdStmtNode : BlingoNode
    {
        public BlingoNode Command { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoSoundPropExprNode : BlingoNode
    {
        public BlingoNode Sound { get; set; } = null!;
        public BlingoNode Property { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoCursorStmtNode : BlingoNode
    {
        public BlingoNode Value { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoGoToStmtNode : BlingoNode
    {
        public BlingoNode Target { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoAssignmentStmtNode : BlingoNode
    {
        public BlingoNode Target { get; set; } = null!;
        public BlingoNode Value { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    /// <summary>
    /// Represents a sendSprite command.
    /// </summary>
    public class BlingoSendSpriteStmtNode : BlingoNode
    {
        public BlingoNode Sprite { get; set; } = null!;
        public BlingoNode Message { get; set; } = null!;
        public BlingoNode? Arguments { get; set; }
        public string? TargetType { get; set; }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    /// <summary>
    /// Represents a sendSprite expression that returns a value.
    /// </summary>
    public class BlingoSendSpriteExprNode : BlingoNode
    {
        public BlingoNode Sprite { get; set; } = null!;
        public BlingoNode Message { get; set; } = null!;
        public BlingoNode? Arguments { get; set; }
        public string? TargetType { get; set; }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoExitRepeatStmtNode : BlingoNode
    {
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoNextRepeatStmtNode : BlingoNode
    {
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoRangeExprNode : BlingoNode
    {
        public BlingoNode Start { get; set; } = null!;
        public BlingoNode End { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoObjBracketExprNode : BlingoNode
    {
        public BlingoNode Object { get; set; } = null!;
        public BlingoNode Index { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoSpritePropExprNode : BlingoNode
    {
        public BlingoNode Sprite { get; set; } = null!;
        public BlingoNode Property { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoChunkDeleteStmtNode : BlingoNode
    {
        public BlingoNode Chunk { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoChunkHiliteStmtNode : BlingoNode
    {
        public BlingoNode Chunk { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoRepeatWhileStmtNode : BlingoNode
    {
        public BlingoRepeatWhileStmtNode(BlingoNode condition, BlingoBlockNode body)
        {
            Condition = condition;
            Body = body;
        }

        public BlingoNode Condition { get; set; } = null!;
        public BlingoNode Body { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class BlingoRepeatUntilStmtNode : BlingoNode
    {
        public BlingoNode Condition { get; }
        public BlingoBlockNode Body { get; }

        public BlingoRepeatUntilStmtNode(BlingoNode condition, BlingoBlockNode body)
        {
            Condition = condition;
            Body = body;
        }

        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class BlingoRepeatWithStmtNode : BlingoNode
    {
        public string Variable { get; }
        public BlingoNode Start { get; }
        public BlingoNode End { get; }
        public BlingoBlockNode Body { get; }

        public BlingoRepeatWithStmtNode(string variable, BlingoNode start, BlingoNode end, BlingoBlockNode body)
        {
            Variable = variable;
            Start = start;
            End = end;
            Body = body;
        }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class BlingoRepeatTimesStmtNode : BlingoNode
    {
        public BlingoNode Count { get; }
        public BlingoBlockNode Body { get; }

        public BlingoRepeatTimesStmtNode(BlingoNode count, BlingoBlockNode body)
        {
            Count = count;
            Body = body;
        }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class BlingoRepeatForeverStmtNode : BlingoNode
    {
        public BlingoBlockNode Body { get; }

        public BlingoRepeatForeverStmtNode(BlingoBlockNode body)
        {
            Body = body;
        }
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }
    public class BlingoMenuItemPropExprNode : BlingoNode
    {
        public BlingoNode MenuItem { get; set; } = null!;
        public BlingoNode Property { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoObjPropIndexExprNode : BlingoNode
    {
        public BlingoNode Object { get; set; } = null!;
        public BlingoNode PropertyIndex { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoRepeatWithInStmtNode : BlingoNode
    {
        public string Variable { get; set; } = string.Empty;
        public BlingoNode List { get; set; } = null!;
        public BlingoNode Body { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoRepeatWithToStmtNode : BlingoNode
    {
        public string Variable { get; set; } = string.Empty;
        public BlingoNode Start { get; set; } = null!;
        public BlingoNode End { get; set; } = null!;
        public BlingoNode Body { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoSpriteWithinExprNode : BlingoNode
    {
        public BlingoNode SpriteA { get; set; } = null!;
        public BlingoNode SpriteB { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoLastStringChunkExprNode : BlingoNode
    {
        public BlingoNode Source { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoSpriteIntersectsExprNode : BlingoNode
    {
        public BlingoNode SpriteA { get; set; } = null!;
        public BlingoNode SpriteB { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoStringChunkCountExprNode : BlingoNode
    {
        public BlingoNode Source { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoNotOpNode : BlingoNode
    {
        public BlingoNode Expr { get; set; } = null!;
        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlingoCallNode : BlingoNode
    {
        public BlingoNode Callee { get; set; } = null!;
        public BlingoNode Arguments { get; set; } = null!;
        public string Name { get; set; } = "";
        public string? TargetType { get; set; }

        public override void Accept(IBlingoAstVisitor visitor) => visitor.Visit(this);
    }


}



