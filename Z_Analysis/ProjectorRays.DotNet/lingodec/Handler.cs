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
