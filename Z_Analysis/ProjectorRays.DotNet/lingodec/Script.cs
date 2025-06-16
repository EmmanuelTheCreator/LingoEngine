using ProjectorRays.Common;
using System.Collections.Generic;

namespace ProjectorRays.LingoDec;

public class Script
{
    public uint TotalLength;
    public uint TotalLength2;
    public ushort HeaderLength;
    public ushort ScriptNumber;
    public short Unk20;
    public short ParentNumber;

    public uint ScriptFlags;
    public short Unk42;
    public int CastID;
    public short FactoryNameID;
    public ushort HandlerVectorsCount;
    public uint HandlerVectorsOffset;
    public uint HandlerVectorsSize;
    public ushort PropertiesCount;
    public uint PropertiesOffset;
    public ushort GlobalsCount;
    public uint GlobalsOffset;
    public ushort HandlersCount;
    public uint HandlersOffset;
    public ushort LiteralsCount;
    public uint LiteralsOffset;
    public uint LiteralsDataCount;
    public uint LiteralsDataOffset;

    public List<short> PropertyNameIDs = new();
    public List<short> GlobalNameIDs = new();

    public string FactoryName = string.Empty;
    public List<string> PropertyNames = new();
    public List<string> GlobalNames = new();
    public List<Handler> Handlers = new();
    public List<LiteralStore> Literals = new();
    public List<Script> Factories = new();

    public uint Version;
    public ScriptContext? Context;

    public Script(uint version)
    {
        Version = version;
    }

    public void Read(ReadStream stream)
    {
        stream.Endianness = Endianness.BigEndian;
        stream.Seek(8);
        TotalLength = stream.ReadUint32();
        TotalLength2 = stream.ReadUint32();
        HeaderLength = stream.ReadUint16();
        ScriptNumber = stream.ReadUint16();
        Unk20 = stream.ReadInt16();
        ParentNumber = stream.ReadInt16();
        stream.Seek(38);
        ScriptFlags = stream.ReadUint32();
        Unk42 = stream.ReadInt16();
        CastID = stream.ReadInt32();
        FactoryNameID = stream.ReadInt16();
        HandlerVectorsCount = stream.ReadUint16();
        HandlerVectorsOffset = stream.ReadUint32();
        HandlerVectorsSize = stream.ReadUint32();
        PropertiesCount = stream.ReadUint16();
        PropertiesOffset = stream.ReadUint32();
        GlobalsCount = stream.ReadUint16();
        GlobalsOffset = stream.ReadUint32();
        HandlersCount = stream.ReadUint16();
        HandlersOffset = stream.ReadUint32();
        LiteralsCount = stream.ReadUint16();
        LiteralsOffset = stream.ReadUint32();
        LiteralsDataCount = stream.ReadUint32();
        LiteralsDataOffset = stream.ReadUint32();

        PropertyNameIDs = ReadVarnamesTable(stream, PropertiesCount, PropertiesOffset);
        GlobalNameIDs = ReadVarnamesTable(stream, GlobalsCount, GlobalsOffset);

        Handlers.Clear();
        for (int i = 0; i < HandlersCount; i++)
            Handlers.Add(new Handler(this));
        if ((ScriptFlags & (uint)ScriptFlag.kScriptFlagEventScript) != 0 && HandlersCount > 0)
            Handlers[0].IsGenericEvent = true;

        stream.Seek((int)HandlersOffset);
        foreach (var h in Handlers)
            h.ReadRecord(stream);
        foreach (var h in Handlers)
            h.ReadData(stream);

        stream.Seek((int)LiteralsOffset);
        Literals.Clear();
        for (int i = 0; i < LiteralsCount; i++)
        {
            var lit = new LiteralStore();
            lit.ReadRecord(stream, (int)Version);
            Literals.Add(lit);
        }
        foreach (var lit in Literals)
            lit.ReadData(stream, LiteralsDataOffset);
    }

    public List<short> ReadVarnamesTable(ReadStream stream, ushort count, uint offset)
    {
        stream.Seek((int)offset);
        var ids = new List<short>();
        for (int i = 0; i < count; i++)
            ids.Add((short)stream.ReadInt16());
        return ids;
    }

    public bool ValidName(int id) => Context != null && Context.ValidName(id);
    public string GetName(int id) => Context != null ? Context.GetName(id) : $"UNK_{id}";

    public void SetContext(ScriptContext ctx)
    {
        Context = ctx;
        if (FactoryNameID != -1)
            FactoryName = GetName(FactoryNameID);
        foreach (var id in PropertyNameIDs)
        {
            if (IsFactory() && GetName(id) == "me")
                continue;
            if (ValidName(id))
                PropertyNames.Add(GetName(id));
        }
        foreach (var id in GlobalNameIDs)
        {
            if (ValidName(id))
                GlobalNames.Add(GetName(id));
        }
        foreach (var h in Handlers)
            h.ReadNames();
    }

    public void WriteVarDeclarations(CodeWriter code)
    {
        if (!IsFactory())
        {
            if (PropertyNames.Count > 0)
            {
                code.Write("property ");
                for (int i = 0; i < PropertyNames.Count; i++)
                {
                    if (i > 0)
                        code.Write(", ");
                    code.Write(PropertyNames[i]);
                }
                code.WriteLine();
            }
        }
        if (GlobalNames.Count > 0)
        {
            code.Write("global ");
            for (int i = 0; i < GlobalNames.Count; i++)
            {
                if (i > 0)
                    code.Write(", ");
                code.Write(GlobalNames[i]);
            }
            code.WriteLine();
        }
    }

    public void WriteBytecodeText(CodeWriter code, bool dotSyntax)
    {
        int orig = code.Size;
        WriteVarDeclarations(code);
        if (IsFactory())
        {
            if (code.Size != orig)
                code.WriteLine();
            code.Write("factory ");
            code.WriteLine(FactoryName);
        }
        for (int i = 0; i < Handlers.Count; i++)
        {
            if ((!IsFactory() || i > 0) && code.Size != orig)
                code.WriteLine();
            Handlers[i].WriteBytecodeText(code, dotSyntax);
        }
        foreach (var f in Factories)
        {
            if (code.Size != orig)
                code.WriteLine();
            f.WriteBytecodeText(code, dotSyntax);
        }
    }

    /// <summary>
    /// Write the decompiled source code of this script using the parsed AST.
    /// Only a very small subset of the original syntax is supported.
    /// </summary>
    public void WriteScriptText(CodeWriter code, bool dotSyntax)
    {
        int orig = code.Size;
        WriteVarDeclarations(code);
        if (IsFactory())
        {
            if (code.Size != orig)
                code.WriteLine();
            code.Write("factory ");
            code.WriteLine(FactoryName);
        }
        for (int i = 0; i < Handlers.Count; i++)
        {
            if ((!IsFactory() || i > 0) && code.Size != orig)
                code.WriteLine();
            Handlers[i].WriteScriptText(code);
        }
        foreach (var f in Factories)
        {
            if (code.Size != orig)
                code.WriteLine();
            f.WriteScriptText(code, dotSyntax);
        }
    }

    /// <summary>Return the decompiled script using platform line endings.</summary>
    public string ScriptText(string lineEnding, bool dotSyntax)
    {
        var cw = new CodeWriter(lineEnding);
        WriteScriptText(cw, dotSyntax);
        return cw.ToString();
    }

    /// <summary>Return the bytecode listing using platform line endings.</summary>
    public string BytecodeText(string lineEnding, bool dotSyntax)
    {
        var cw = new CodeWriter(lineEnding);
        WriteBytecodeText(cw, dotSyntax);
        return cw.ToString();
    }

    public bool IsFactory() => (ScriptFlags & (uint)ScriptFlag.kScriptFlagFactoryDef) != 0;

    /// <summary>
    /// Parse all handlers in this script, building an AST for each one. This
    /// step is optional and only required when the caller wants to decompile the
    /// script back to Lingo source.
    /// </summary>
    public void Parse()
    {
        foreach (var h in Handlers)
            h.Parse();
    }
}

public class LiteralStore
{
    public LiteralType Type;
    public uint Offset;
    public Datum? Value;

    public void ReadRecord(ReadStream stream, int version)
    {
        if (version >= 500)
            Type = (LiteralType)stream.ReadUint32();
        else
            Type = (LiteralType)stream.ReadUint16();
        Offset = stream.ReadUint32();
    }

    public void ReadData(ReadStream stream, uint start)
    {
        if (Type == LiteralType.kLiteralInt)
        {
            Value = new Datum((int)Offset);
        }
        else
        {
            stream.Seek((int)(start + Offset));
            uint len = stream.ReadUint32();
            if (Type == LiteralType.kLiteralString)
            {
                Value = new Datum(DatumType.kDatumString, stream.ReadString((int)len - 1));
            }
            else if (Type == LiteralType.kLiteralFloat)
            {
                double val = 0.0;
                if (len == 8)
                    val = stream.ReadDouble();
                else if (len == 10)
                    val = stream.ReadAppleFloat80();
                Value = new Datum(val);
            }
            else
            {
                Value = new Datum();
            }
        }
    }
}
