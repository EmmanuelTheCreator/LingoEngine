using ProjectorRays.Common;
using System.Collections.Generic;
using System.Text;

namespace ProjectorRays.LingoDec;

public class Handler
{
    public short NameID;
    public ushort VectorPos;
    public uint CompiledLen;
    public uint CompiledOffset;
    public ushort ArgumentCount;
    public uint ArgumentOffset;
    public ushort LocalsCount;
    public uint LocalsOffset;
    public ushort GlobalsCount;
    public uint GlobalsOffset;
    public uint Unknown1;
    public ushort Unknown2;
    public ushort LineCount;
    public uint LineOffset;
    public uint StackHeight;

    public List<short> ArgumentNameIDs = new();
    public List<short> LocalNameIDs = new();
    public List<short> GlobalNameIDs = new();

    public Script Script;
    public List<Bytecode> BytecodeArray = new();
    public Dictionary<uint, int> BytecodePosMap = new();
    public List<string> ArgumentNames = new();
    public List<string> LocalNames = new();
    public List<string> GlobalNames = new();
    public string Name = string.Empty;

    public bool IsGenericEvent = false;
    public BlockNode? Ast;


    public Handler(Script script)
    {
        Script = script;
    }

    public void ReadRecord(ReadStream stream)
    {
        NameID = (short)stream.ReadInt16();
        VectorPos = stream.ReadUint16();
        CompiledLen = stream.ReadUint32();
        CompiledOffset = stream.ReadUint32();
        ArgumentCount = stream.ReadUint16();
        ArgumentOffset = stream.ReadUint32();
        LocalsCount = stream.ReadUint16();
        LocalsOffset = stream.ReadUint32();
        GlobalsCount = stream.ReadUint16();
        GlobalsOffset = stream.ReadUint32();
        Unknown1 = stream.ReadUint32();
        Unknown2 = stream.ReadUint16();
        LineCount = stream.ReadUint16();
        LineOffset = stream.ReadUint32();
        if (Script.Version >= 850)
            StackHeight = stream.ReadUint32();
    }

    public void ReadData(ReadStream stream)
    {
        stream.Seek((int)CompiledOffset);
        while (stream.Pos < CompiledOffset + CompiledLen)
        {
            uint pos = (uint)(stream.Pos - CompiledOffset);
            byte op = stream.ReadUint8();
            OpCode opcode = op >= 0x40 ? (OpCode)(0x40 + op % 0x40) : (OpCode)op;
            int obj = 0;
            if (op >= 0xc0)
            {
                obj = stream.ReadInt32();
            }
            else if (op >= 0x80)
            {
                if (opcode == OpCode.kOpPushInt16 || opcode == OpCode.kOpPushInt8)
                    obj = stream.ReadInt16();
                else
                    obj = stream.ReadUint16();
            }
            else if (op >= 0x40)
            {
                if (opcode == OpCode.kOpPushInt8)
                    obj = stream.ReadInt8();
                else
                    obj = stream.ReadUint8();
            }
            Bytecode bytecode = new(op, obj, pos);
            BytecodeArray.Add(bytecode);
            BytecodePosMap[pos] = BytecodeArray.Count - 1;
        }

        ArgumentNameIDs = ReadVarnamesTable(stream, ArgumentCount, ArgumentOffset);
        LocalNameIDs = ReadVarnamesTable(stream, LocalsCount, LocalsOffset);
        GlobalNameIDs = ReadVarnamesTable(stream, GlobalsCount, GlobalsOffset);
    }

    /// <summary>
    /// Convert the bytecode stored in <see cref="BytecodeArray"/> into a very
    /// small abstract syntax tree. Only a subset of instructions are
    /// recognized; unknown opcodes result in <see cref="UnknownOpNode"/> entries.
    /// The parser uses a simple stack machine mirroring the original bytecode
    /// evaluation order.
    /// </summary>
    public void Parse()
    {
        Ast = new BlockNode();
        var stack = new Stack<AstNode>();
        foreach (var bc in BytecodeArray)
        {
            switch (bc.Opcode)
            {
                case OpCode.kOpPushZero:
                    stack.Push(new LiteralNode(new Datum(0)));
                    break;
                case OpCode.kOpPushInt8:
                case OpCode.kOpPushInt16:
                case OpCode.kOpPushInt32:
                    stack.Push(new LiteralNode(new Datum(bc.Obj)));
                    break;
                case OpCode.kOpPushFloat32:
                    stack.Push(new LiteralNode(new Datum(BitConverter.Int32BitsToSingle(bc.Obj))));
                    break;
                case OpCode.kOpPushSymb:
                    stack.Push(new LiteralNode(new Datum(DatumType.kDatumSymbol, GetName(bc.Obj))));
                    break;
                case OpCode.kOpPushVarRef:
                    stack.Push(new LiteralNode(new Datum(DatumType.kDatumVarRef, GetName(bc.Obj))));
                    break;
                case OpCode.kOpGetGlobal:
                case OpCode.kOpGetGlobal2:
                    stack.Push(new VarNode(GetName(bc.Obj)));
                    break;
                case OpCode.kOpGetParam:
                    stack.Push(new VarNode(GetArgumentName(bc.Obj / VariableMultiplier())));
                    break;
                case OpCode.kOpGetLocal:
                    stack.Push(new VarNode(GetLocalName(bc.Obj / VariableMultiplier())));
                    break;
                case OpCode.kOpMul:
                case OpCode.kOpAdd:
                case OpCode.kOpSub:
                case OpCode.kOpDiv:
                case OpCode.kOpMod:
                case OpCode.kOpJoinStr:
                case OpCode.kOpJoinPadStr:
                case OpCode.kOpLt:
                case OpCode.kOpLtEq:
                case OpCode.kOpNtEq:
                case OpCode.kOpEq:
                case OpCode.kOpGt:
                case OpCode.kOpGtEq:
                case OpCode.kOpAnd:
                case OpCode.kOpOr:
                case OpCode.kOpContainsStr:
                case OpCode.kOpContains0Str:
                    if (stack.Count >= 2) { var right = stack.Pop(); var left = stack.Pop(); stack.Push(new BinaryOpNode(bc.Opcode, left, right)); } else { Ast.Statements.Add(new UnknownOpNode(bc)); }
                    break;
                case OpCode.kOpInv:
                case OpCode.kOpNot:
                    if (stack.Count >= 1) { var opnd = stack.Pop(); stack.Push(new UnaryOpNode(bc.Opcode, opnd)); } else { Ast.Statements.Add(new UnknownOpNode(bc)); }
                    break;
                case OpCode.kOpPushArgList:
                case OpCode.kOpPushArgListNoRet:
                    { int n = bc.Obj; var list = new List<AstNode>(); for (int i=0;i<n;i++) list.Insert(0, stack.Pop()); stack.Push(new ArgListNode(list, bc.Opcode == OpCode.kOpPushArgListNoRet)); }
                    break;
                case OpCode.kOpLocalCall:
                    { var args = stack.Pop() as ArgListNode ?? new ArgListNode(new List<AstNode>(), false); string name = bc.Obj >=0 && bc.Obj < Script.Handlers.Count ? Script.Handlers[bc.Obj].Name :  $"handler_{bc.Obj}"; var call = new CallNode(name, args.Args, args.NoReturn); if (args.NoReturn) Ast.Statements.Add(call); else stack.Push(call); }
                    break;
                case OpCode.kOpExtCall:
                    { var args = stack.Pop() as ArgListNode ?? new ArgListNode(new List<AstNode>(), false); var call = new CallNode(GetName(bc.Obj), args.Args, args.NoReturn); if (args.NoReturn) Ast.Statements.Add(call); else stack.Push(call); }
                    break;
                case OpCode.kOpSetGlobal:
                case OpCode.kOpSetGlobal2:
                    if (stack.Count > 0) { var val = stack.Pop(); Ast.Statements.Add(new AssignmentNode(new VarNode(GetName(bc.Obj)), val)); }
                    break;
                case OpCode.kOpSetParam:
                    if (stack.Count > 0) { var val = stack.Pop(); Ast.Statements.Add(new AssignmentNode(new VarNode(GetArgumentName(bc.Obj / VariableMultiplier())), val)); }
                    break;
                case OpCode.kOpSetLocal:
                    if (stack.Count > 0) { var val = stack.Pop(); Ast.Statements.Add(new AssignmentNode(new VarNode(GetLocalName(bc.Obj / VariableMultiplier())), val)); }
                    break;
                case OpCode.kOpRet:
                case OpCode.kOpRetFactory:
                    Ast.Statements.Add(new ReturnNode(stack.Count > 0 ? stack.Pop() : null));
                    break;
                case OpCode.kOpPop:
                    if (stack.Count > 0) stack.Pop();
                    break;
                default:
                    Ast.Statements.Add(new UnknownOpNode(bc)); stack.Clear();
                    break;
            }
        }
    }

    public List<short> ReadVarnamesTable(ReadStream stream, ushort count, uint offset)
    {
        stream.Seek((int)offset);
        var ids = new List<short>();
        for (int i = 0; i < count; i++)
            ids.Add((short)stream.ReadInt16());
        return ids;
    }

    public void ReadNames()
    {
        if (!IsGenericEvent)
            Name = GetName(NameID);
        for (int i = 0; i < ArgumentNameIDs.Count; i++)
        {
            if (i == 0 && Script.IsFactory())
                continue;
            ArgumentNames.Add(GetName(ArgumentNameIDs[i]));
        }
        foreach (var id in LocalNameIDs)
        {
            if (ValidName(id))
                LocalNames.Add(GetName(id));
        }
        foreach (var id in GlobalNameIDs)
        {
            if (ValidName(id))
                GlobalNames.Add(GetName(id));
        }
    }

    public bool ValidName(int id) => Script.ValidName(id);
    public string GetName(int id) => Script.GetName(id);

    public string GetArgumentName(int id)
    {
        if (id >= 0 && id < ArgumentNameIDs.Count)
            return GetName(ArgumentNameIDs[id]);
        return $"UNKNOWN_ARG_{id}";
    }

    public string GetLocalName(int id)
    {
        if (id >= 0 && id < LocalNameIDs.Count)
            return GetName(LocalNameIDs[id]);
        return $"UNKNOWN_LOCAL_{id}";
    }
    /// <summary>
    /// Earlier Director versions store variable indices multiplied by a
    /// constant. This helper returns the appropriate multiplier so bytecode
    /// offsets can be converted back to plain indices.
    /// </summary>
    private int VariableMultiplier()
    {
        if (Script.Version >= 850) return 1;
        if (Script.Version >= 500) return 8;
        return 6;
    }


    private static string PosToString(int pos)
    {
        return $"[{pos,3}]";
    }

    public void WriteBytecodeText(CodeWriter code, bool dotSyntax)
    {
        bool isMethod = Script.IsFactory();

        if (!IsGenericEvent)
        {
            if (isMethod)
                code.Write("method ");
            else
                code.Write("on ");
            code.Write(Name);
            if (ArgumentNames.Count > 0)
            {
                code.Write(" ");
                for (int i = 0; i < ArgumentNames.Count; i++)
                {
                    if (i > 0)
                        code.Write(", ");
                    code.Write(ArgumentNames[i]);
                }
            }
            code.WriteLine();
            code.Indent();
        }
        foreach (var bc in BytecodeArray)
        {
            code.Write(PosToString((int)bc.Pos));
            code.Write(" ");
            code.Write(StandardNames.GetOpcodeName(bc.OpID));
            switch (bc.Opcode)
            {
                case OpCode.kOpJmp:
                case OpCode.kOpJmpIfZ:
                    code.Write(" ");
                    code.Write(PosToString((int)(bc.Pos + bc.Obj)));
                    break;
                case OpCode.kOpEndRepeat:
                    code.Write(" ");
                    code.Write(PosToString((int)(bc.Pos - bc.Obj)));
                    break;
                case OpCode.kOpPushFloat32:
                    code.Write(" ");
                    float f = BitConverter.Int32BitsToSingle(bc.Obj);
                    code.Write(Util.FloatToString(f));
                    break;
                default:
                    if (bc.OpID > 0x40)
                    {
                        code.Write(" ");
                        code.Write(bc.Obj.ToString());
                    }
                    break;
            }
            code.WriteLine();
        }
        if (!IsGenericEvent)
        {
            code.Unindent();
            if (!isMethod)
                code.WriteLine("end");
        }
    }

    /// <summary>
    /// Write the decompiled script for this handler using the parsed AST. This
    /// is a simplified printer that only understands the small subset of nodes
    /// produced by <see cref="Parse"/>.
    /// </summary>
    public void WriteScriptText(CodeWriter code)
    {
        bool isMethod = Script.IsFactory();

        if (!IsGenericEvent)
        {
            if (isMethod)
                code.Write("method ");
            else
                code.Write("on ");
            code.Write(Name);
            if (ArgumentNames.Count > 0)
            {
                code.Write(" ");
                for (int i = 0; i < ArgumentNames.Count; i++)
                {
                    if (i > 0) code.Write(", ");
                    code.Write(ArgumentNames[i]);
                }
            }
            code.WriteLine();
            code.Indent();
        }

        Ast?.WriteScriptText(code);

        if (!IsGenericEvent)
        {
            code.Unindent();
            if (!isMethod)
                code.WriteLine("end");
        }
    }
}

public class Bytecode
{
    public byte OpID;
    public OpCode Opcode;
    public int Obj;
    public uint Pos;
    public BytecodeTag Tag;
    public uint OwnerLoop;

    public Bytecode(byte op, int obj, uint pos)
    {
        OpID = op;
        Opcode = op >= 0x40 ? (OpCode)(0x40 + op % 0x40) : (OpCode)op;
        Obj = obj;
        Pos = pos;
        Tag = BytecodeTag.kTagNone;
        OwnerLoop = uint.MaxValue;
    }
}
