using Director.Scripts.Lingo;

namespace Director.Scripts
{
    public abstract class Node
    {
        public Node? Parent { get; set; }
        public abstract void Accept(IAstVisitor visitor);
    }

    public class HandlerNode : Node
    {
        public Handler Handler { get; set; }
        public BlockNode Block { get; set; }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class ErrorNode : Node
    {
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
    public class ExitRepeatIfStmtNode : Node
    {
        public Node Condition { get; }

        public ExitRepeatIfStmtNode(Node condition)
        {
            Condition = condition;
        }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
    public class ExitStmtNode : Node
    {
        public ExitStmtNode()
        {
        }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
    public class NextStmtNode : Node
    {
        public NextStmtNode()
        {
        }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class ReturnStmtNode : Node
    {
        public ReturnStmtNode(Node? value)
        {
            Value = value;
        }

        public Node? Value { get; }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
    public class NextRepeatIfStmtNode : Node
    {
        public Node Condition { get; }

        public NextRepeatIfStmtNode(Node condition)
        {
            Condition = condition;
        }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
    public class CommentNode : Node
    {
        public string Text { get; set; } = string.Empty;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class NewObjNode : Node
    {
        public string ObjType { get; set; } = string.Empty;
        public Node ObjArgs { get; set; }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class LiteralNode : Node
    {
        public Datum Value { get; set; }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }


    public class IfStmtNode : Node
    {
        public IfStmtNode(Node condition, Node thenBlock, Node? elseBlock)
        {
            Condition = condition;
            ThenBlock = thenBlock;
            ElseBlock = elseBlock;
        }

        public bool HasElse => ElseBlock != null;
        public Node Condition { get; set; }
        public Node ThenBlock { get; }
        public Node? ElseBlock { get; }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class IfElseStmtNode : Node
    {
        public IfElseStmtNode(Node condition, BlockNode thenBlock, BlockNode elseBlock)
        {
            Condition = condition;
            ThenBlock = thenBlock;
            ElseBlock = elseBlock;
        }

        public Node Condition { get; }
        public BlockNode ThenBlock { get; }
        public BlockNode ElseBlock { get; }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class EndCaseNode : Node
    {
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class ObjCallNode : Node
    {
        public LiteralNode Name { get; set; }
        public DatumNode ArgList { get; set; }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class PutStmtNode : Node
    {
        public PutStmtNode(Node value, Node target)
        {
            Value = value;
            Target = target;
        }

        public Node Value { get; set; }
        public PutType Type { get; set; }
        public Node Variable { get; set; }
        public Node Target { get; }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class GlobalDeclStmtNode : Node
    {
        public List<string> Names { get; } = new();
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class PropertyDeclStmtNode : Node
    {
        public List<string> Names { get; } = new();
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class InstanceDeclStmtNode : Node
    {
        public List<string> Names { get; } = new();
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class TheExprNode : Node
    {
        public string Prop { get; set; } = string.Empty;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class BinaryOpNode : Node
    {
        public Node Left { get; set; }
        public Node Right { get; set; }
        public BinaryOpcode Opcode { get; set; }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    #region Switch case

    public class CaseStmtNode : Node
    {
        public Node Value { get; set; }
        public Node? FirstLabel { get; set; }
        public OtherwiseNode? Otherwise { get; set; }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
    public class CaseLabelNode : Node
    {
        public Node Value { get; set; }
        public Node? Block { get; set; }

        public CaseLabelNode? NextOr { get; set; }
        public CaseLabelNode? NextLabel { get; set; }

        public CaseStmtNode? Parent { get; set; }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    #endregion


    public class TellStmtNode : Node
    {
        public Node Window { get; set; }
        public BlockNode Block { get; set; }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class WhenStmtNode : Node
    {
        public int Event { get; set; }
        public string Script { get; set; } = string.Empty;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class OtherwiseNode : Node
    {
        public BlockNode Block { get; set; }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class BlockNode : Node
    {
        public List<Node> Children { get; set; } = new();
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class VarNode : Node
    {
        public string VarName { get; set; } = string.Empty;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class DatumNode : Node
    {
        public DatumNode(Datum datum)
        {
            Datum = datum;
        }

        public Datum Value { get; set; }
        public Datum Datum { get; }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    

    public class ChunkExprNode : Node
    {
        public Node Expr { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class InverseOpNode : Node
    {
        public Node Expr { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class ObjCallV4Node : Node
    {
        public Node Object { get; set; } = null!;
        public LiteralNode Name { get; set; } = null!;
        public DatumNode ArgList { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class MemberExprNode : Node
    {
        public Node Expr { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class ObjPropExprNode : Node
    {
        public Node Object { get; set; } = null!;
        public Node Property { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class PlayCmdStmtNode : Node
    {
        public Node Command { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class ThePropExprNode : Node
    {
        public Node Property { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class MenuPropExprNode : Node
    {
        public Node Menu { get; set; } = null!;
        public Node Property { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class SoundCmdStmtNode : Node
    {
        public Node Command { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class SoundPropExprNode : Node
    {
        public Node Sound { get; set; } = null!;
        public Node Property { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class AssignmentStmtNode : Node
    {
        public Node Target { get; set; } = null!;
        public Node Value { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class ExitRepeatStmtNode : Node
    {
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class NextRepeatStmtNode : Node
    {
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class ObjBracketExprNode : Node
    {
        public Node Object { get; set; } = null!;
        public Node Index { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class SpritePropExprNode : Node
    {
        public Node Sprite { get; set; } = null!;
        public Node Property { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class ChunkDeleteStmtNode : Node
    {
        public Node Chunk { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class ChunkHiliteStmtNode : Node
    {
        public Node Chunk { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class RepeatWhileStmtNode : Node
    {
        public RepeatWhileStmtNode(Node condition, BlockNode body)
        {
            Condition = condition;
            Body = body;
        }

        public Node Condition { get; set; } = null!;
        public Node Body { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
    public class RepeatUntilStmtNode : Node
    {
        public Node Condition { get; }
        public BlockNode Body { get; }

        public RepeatUntilStmtNode(Node condition, BlockNode body)
        {
            Condition = condition;
            Body = body;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
    public class RepeatWithStmtNode : Node
    {
        public string Variable { get; }
        public Node Start { get; }
        public Node End { get; }
        public BlockNode Body { get; }

        public RepeatWithStmtNode(string variable, Node start, Node end, BlockNode body)
        {
            Variable = variable;
            Start = start;
            End = end;
            Body = body;
        }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
    public class RepeatTimesStmtNode : Node
    {
        public Node Count { get; }
        public BlockNode Body { get; }

        public RepeatTimesStmtNode(Node count, BlockNode body)
        {
            Count = count;
            Body = body;
        }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
    public class RepeatForeverStmtNode : Node
    {
        public BlockNode Body { get; }

        public RepeatForeverStmtNode(BlockNode body)
        {
            Body = body;
        }
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
    public class MenuItemPropExprNode : Node
    {
        public Node MenuItem { get; set; } = null!;
        public Node Property { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class ObjPropIndexExprNode : Node
    {
        public Node Object { get; set; } = null!;
        public Node PropertyIndex { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class RepeatWithInStmtNode : Node
    {
        public string Variable { get; set; } = string.Empty;
        public Node List { get; set; } = null!;
        public Node Body { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class RepeatWithToStmtNode : Node
    {
        public string Variable { get; set; } = string.Empty;
        public Node Start { get; set; } = null!;
        public Node End { get; set; } = null!;
        public Node Body { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class SpriteWithinExprNode : Node
    {
        public Node SpriteA { get; set; } = null!;
        public Node SpriteB { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class LastStringChunkExprNode : Node
    {
        public Node Source { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class SpriteIntersectsExprNode : Node
    {
        public Node SpriteA { get; set; } = null!;
        public Node SpriteB { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class StringChunkCountExprNode : Node
    {
        public Node Source { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class NotOpNode : Node
    {
        public Node Expr { get; set; } = null!;
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }

    public class CallNode : Node
    {
        public Node Callee { get; set; } = null!;
        public DatumNode Arguments { get; set; } = null!;
        public string Name { get; set; } = "";

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }


}


