using ProjectorRays.Common;
using System.Collections.Generic;

namespace ProjectorRays.LingoDec;

public static class StandardNames
{
    public static readonly Dictionary<uint, string> OpcodeNames = new()
    {
        // single-byte
        { (uint)OpCode.kOpRet, "ret" },
        { (uint)OpCode.kOpRetFactory, "retfactory" },
        { (uint)OpCode.kOpPushZero, "pushzero" },
        { (uint)OpCode.kOpMul, "mul" },
        { (uint)OpCode.kOpAdd, "add" },
        { (uint)OpCode.kOpSub, "sub" },
        { (uint)OpCode.kOpDiv, "div" },
        { (uint)OpCode.kOpMod, "mod" },
        { (uint)OpCode.kOpInv, "inv" },
        { (uint)OpCode.kOpJoinStr, "joinstr" },
        { (uint)OpCode.kOpJoinPadStr, "joinpadstr" },
        { (uint)OpCode.kOpLt, "lt" },
        { (uint)OpCode.kOpLtEq, "lteq" },
        { (uint)OpCode.kOpNtEq, "nteq" },
        { (uint)OpCode.kOpEq, "eq" },
        { (uint)OpCode.kOpGt, "gt" },
        { (uint)OpCode.kOpGtEq, "gteq" },
        { (uint)OpCode.kOpAnd, "and" },
        { (uint)OpCode.kOpOr, "or" },
        { (uint)OpCode.kOpNot, "not" },
        { (uint)OpCode.kOpContainsStr, "containsstr" },
        { (uint)OpCode.kOpContains0Str, "contains0str" },
        { (uint)OpCode.kOpGetChunk, "getchunk" },
        { (uint)OpCode.kOpHiliteChunk, "hilitechunk" },
        { (uint)OpCode.kOpOntoSpr, "ontospr" },
        { (uint)OpCode.kOpIntoSpr, "intospr" },
        { (uint)OpCode.kOpGetField, "getfield" },
        { (uint)OpCode.kOpStartTell, "starttell" },
        { (uint)OpCode.kOpEndTell, "endtell" },
        { (uint)OpCode.kOpPushList, "pushlist" },
        { (uint)OpCode.kOpPushPropList, "pushproplist" },
        { (uint)OpCode.kOpSwap, "swap" },

        // multi-byte
        { (uint)OpCode.kOpPushInt8, "pushint8" },
        { (uint)OpCode.kOpPushArgListNoRet, "pusharglistnoret" },
        { (uint)OpCode.kOpPushArgList, "pusharglist" },
        { (uint)OpCode.kOpPushCons, "pushcons" },
        { (uint)OpCode.kOpPushSymb, "pushsymb" },
        { (uint)OpCode.kOpPushVarRef, "pushvarref" },
        { (uint)OpCode.kOpGetGlobal2, "getglobal2" },
        { (uint)OpCode.kOpGetGlobal, "getglobal" },
        { (uint)OpCode.kOpGetProp, "getprop" },
        { (uint)OpCode.kOpGetParam, "getparam" },
        { (uint)OpCode.kOpGetLocal, "getlocal" },
        { (uint)OpCode.kOpSetGlobal2, "setglobal2" },
        { (uint)OpCode.kOpSetGlobal, "setglobal" },
        { (uint)OpCode.kOpSetProp, "setprop" },
        { (uint)OpCode.kOpSetParam, "setparam" },
        { (uint)OpCode.kOpSetLocal, "setlocal" },
        { (uint)OpCode.kOpJmp, "jmp" },
        { (uint)OpCode.kOpEndRepeat, "endrepeat" },
        { (uint)OpCode.kOpJmpIfZ, "jmpifz" },
        { (uint)OpCode.kOpLocalCall, "localcall" },
        { (uint)OpCode.kOpExtCall, "extcall" },
        { (uint)OpCode.kOpObjCallV4, "objcallv4" },
        { (uint)OpCode.kOpPut, "put" },
        { (uint)OpCode.kOpPutChunk, "putchunk" },
        { (uint)OpCode.kOpDeleteChunk, "deletechunk" },
        { (uint)OpCode.kOpGet, "get" },
        { (uint)OpCode.kOpSet, "set" },
        { (uint)OpCode.kOpGetMovieProp, "getmovieprop" },
        { (uint)OpCode.kOpSetMovieProp, "setmovieprop" },
        { (uint)OpCode.kOpGetObjProp, "getobjprop" },
        { (uint)OpCode.kOpSetObjProp, "setobjprop" },
        { (uint)OpCode.kOpTellCall, "tellcall" },
        { (uint)OpCode.kOpPeek, "peek" },
        { (uint)OpCode.kOpPop, "pop" },
        { (uint)OpCode.kOpTheBuiltin, "thebuiltin" },
        { (uint)OpCode.kOpObjCall, "objcall" },
        { (uint)OpCode.kOpPushChunkVarRef, "pushchunkvarref" },
        { (uint)OpCode.kOpPushInt16, "pushint16" },
        { (uint)OpCode.kOpPushInt32, "pushint32" },
        { (uint)OpCode.kOpGetChainedProp, "getchainedprop" },
        { (uint)OpCode.kOpPushFloat32, "pushfloat32" },
        { (uint)OpCode.kOpGetTopLevelProp, "gettoplevelprop" },
        { (uint)OpCode.kOpNewObj, "newobj" }
    };

    public static readonly Dictionary<uint, string> BinaryOpNames = new()
    {
        { (uint)OpCode.kOpMul, "*" },
        { (uint)OpCode.kOpAdd, "+" },
        { (uint)OpCode.kOpSub, "-" },
        { (uint)OpCode.kOpDiv, "/" },
        { (uint)OpCode.kOpMod, "mod" },
        { (uint)OpCode.kOpJoinStr, "&" },
        { (uint)OpCode.kOpJoinPadStr, "&&" },
        { (uint)OpCode.kOpLt, "<" },
        { (uint)OpCode.kOpLtEq, "<=" },
        { (uint)OpCode.kOpNtEq, "<>" },
        { (uint)OpCode.kOpEq, "=" },
        { (uint)OpCode.kOpGt, ">" },
        { (uint)OpCode.kOpGtEq, ">=" },
        { (uint)OpCode.kOpAnd, "and" },
        { (uint)OpCode.kOpOr, "or" },
        { (uint)OpCode.kOpContainsStr, "contains" },
        { (uint)OpCode.kOpContains0Str, "starts" }
    };

    public static readonly Dictionary<uint, string> ChunkTypeNames = new()
    {
        { (uint)ChunkExprType.kChunkChar, "char" },
        { (uint)ChunkExprType.kChunkWord, "word" },
        { (uint)ChunkExprType.kChunkItem, "item" },
        { (uint)ChunkExprType.kChunkLine, "line" }
    };

    public static readonly Dictionary<uint, string> PutTypeNames = new()
    {
        { (uint)PutType.kPutInto, "into" },
        { (uint)PutType.kPutAfter, "after" },
        { (uint)PutType.kPutBefore, "before" }
    };

    public static string GetOpcodeName(byte id)
    {
        uint u = id >= 0x40 ? (uint)(0x40 + id % 0x40) : id;
        return OpcodeNames.TryGetValue(u, out var name)
            ? name
            : $"unk{RaysUtil.ByteToString(id)}";
    }

    public static string GetName(Dictionary<uint, string> map, uint id)
        => map.TryGetValue(id, out var name) ? name : "ERROR";
}

public class ScriptNames
{
    public int Unknown0;
    public int Unknown1;
    public uint Len1;
    public uint Len2;
    public ushort NamesOffset;
    public ushort NamesCount;
    public List<string> Names = new();
    public readonly uint Version;

    public ScriptNames(uint version) => Version = version;

    public void Read(ReadStream stream)
    {
        stream.Endianness = Endianness.BigEndian;
        Unknown0 = stream.ReadInt32();
        Unknown1 = stream.ReadInt32();
        Len1 = stream.ReadUint32();
        Len2 = stream.ReadUint32();
        NamesOffset = stream.ReadUint16();
        NamesCount = stream.ReadUint16();

        stream.Seek(NamesOffset);
        Names.Clear();
        for (int i = 0; i < NamesCount; i++)
        {
            int len = stream.ReadUint8();
            Names.Add(stream.ReadString(len));
        }
    }

    public bool ValidName(int id) => id >= 0 && id < Names.Count;

    public string GetName(int id) => ValidName(id) ? Names[id] : $"UNKNOWN_NAME_{id}";
}
