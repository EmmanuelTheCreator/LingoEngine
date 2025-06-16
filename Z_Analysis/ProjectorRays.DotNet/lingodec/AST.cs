using ProjectorRays.Common;
using System.Collections.Generic;

namespace ProjectorRays.LingoDec;

public class Datum
{
    public DatumType Type;
    public int I;
    public double F;
    public string S = string.Empty;
    public List<Datum> L = new();

    public Datum() { Type = DatumType.kDatumVoid; }
    public Datum(int val) { Type = DatumType.kDatumInt; I = val; }
    public Datum(double val) { Type = DatumType.kDatumFloat; F = val; }
    public Datum(DatumType t, string val) { Type = t; S = val; }

    public int ToInt()
    {
        return Type switch
        {
            DatumType.kDatumInt => I,
            DatumType.kDatumFloat => (int)F,
            _ => 0
        };
    }

    public void WriteScriptText(CodeWriter code, bool dot, bool sum)
    {
        switch (Type)
        {
            case DatumType.kDatumVoid:
                code.Write("VOID");
                break;
            case DatumType.kDatumSymbol:
                code.Write("#" + S);
                break;
            case DatumType.kDatumVarRef:
                code.Write(S);
                break;
            case DatumType.kDatumString:
                if (S.Length == 0)
                    code.Write("EMPTY");
                else
                    code.Write('"' + S + '"');
                break;
            case DatumType.kDatumInt:
                code.Write(I.ToString());
                break;
            case DatumType.kDatumFloat:
                code.Write(Util.FloatToString(F));
                break;
            default:
                code.Write("LIST");
                break;
        }
    }
}

public abstract class AstNode
{
    /// <summary>
    /// Write a textual representation of this node. This is a very minimal
    /// pretty printer used by <see cref="Script.WriteScriptText"/> and does not
    /// attempt to cover all Lingo syntax features.
    /// </summary>
    public virtual void WriteScriptText(CodeWriter code) { }
}

public class BlockNode : AstNode
{
    public List<AstNode> Statements { get; } = new();

    public override void WriteScriptText(CodeWriter code)
    {
        foreach (var stmt in Statements)
        {
            stmt.WriteScriptText(code);
            code.WriteLine();
        }
    }
}

public class LiteralNode : AstNode
{
    public Datum Value;
    public LiteralNode(Datum value) => Value = value;

    public override void WriteScriptText(CodeWriter code) =>
        Value.WriteScriptText(code, false, false);
}

public class VarNode : AstNode
{
    public string Name;
    public VarNode(string name) => Name = name;

    public override void WriteScriptText(CodeWriter code) => code.Write(Name);
}

public class BinaryOpNode : AstNode
{
    public OpCode Op;
    public AstNode Left;
    public AstNode Right;
    public BinaryOpNode(OpCode op, AstNode left, AstNode right)
    {
        Op = op;
        Left = left;
        Right = right;
    }

    public override void WriteScriptText(CodeWriter code)
    {
        Left.WriteScriptText(code);
        code.Write(" ");
        code.Write(StandardNames.GetName(StandardNames.BinaryOpNames, (uint)Op));
        code.Write(" ");
        Right.WriteScriptText(code);
    }
}

public class UnaryOpNode : AstNode
{
    public OpCode Op;
    public AstNode Operand;
    public UnaryOpNode(OpCode op, AstNode operand)
    {
        Op = op;
        Operand = operand;
    }

    public override void WriteScriptText(CodeWriter code)
    {
        string opStr = Op == OpCode.kOpNot ? "not" : "-";
        code.Write(opStr + " ");
        Operand.WriteScriptText(code);
    }
}

public class AssignmentNode : AstNode
{
    public VarNode Target;
    public AstNode Value;
    public AssignmentNode(VarNode target, AstNode value)
    {
        Target = target;
        Value = value;
    }

    public override void WriteScriptText(CodeWriter code)
    {
        Target.WriteScriptText(code);
        code.Write(" = ");
        Value.WriteScriptText(code);
    }
}

public class ArgListNode : AstNode
{
    public List<AstNode> Args;
    public bool NoReturn;
    public ArgListNode(List<AstNode> args, bool noReturn)
    {
        Args = args;
        NoReturn = noReturn;
    }

    public override void WriteScriptText(CodeWriter code)
    {
        for (int i = 0; i < Args.Count; i++)
        {
            if (i > 0) code.Write(", ");
            Args[i].WriteScriptText(code);
        }
    }
}

public class CallNode : AstNode
{
    public string Name;
    public List<AstNode> Args;
    public bool Statement;
    public CallNode(string name, List<AstNode> args, bool statement)
    {
        Name = name;
        Args = args;
        Statement = statement;
    }

    public override void WriteScriptText(CodeWriter code)
    {
        code.Write(Name);
        code.Write("(");
        for (int i = 0; i < Args.Count; i++)
        {
            if (i > 0) code.Write(", ");
            Args[i].WriteScriptText(code);
        }
        code.Write(")");
    }
}

public class ReturnNode : AstNode
{
    public AstNode? Value;
    public ReturnNode(AstNode? value) => Value = value;

    public override void WriteScriptText(CodeWriter code)
    {
        code.Write("return");
        if (Value != null)
        {
            code.Write(" ");
            Value.WriteScriptText(code);
        }
    }
}

public class UnknownOpNode : AstNode
{
    public Bytecode Bytecode;
    public UnknownOpNode(Bytecode bc) => Bytecode = bc;

    public override void WriteScriptText(CodeWriter code)
    {
        code.Write($"/* unknown {Bytecode.Opcode} */");
    }
}

