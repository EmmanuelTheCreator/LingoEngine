from enum import IntEnum

class OpCode(IntEnum):
    kOpRet = 0x01
    kOpRetFactory = 0x02
    kOpPushZero = 0x03
    kOpMul = 0x04
    kOpAdd = 0x05
    kOpSub = 0x06
    kOpDiv = 0x07
    kOpMod = 0x08
    kOpInv = 0x09
    kOpJoinStr = 0x0a
    kOpJoinPadStr = 0x0b
    kOpLt = 0x0c
    kOpLtEq = 0x0d
    kOpNtEq = 0x0e
    kOpEq = 0x0f
    kOpGt = 0x10
    kOpGtEq = 0x11
    kOpAnd = 0x12
    kOpOr = 0x13
    kOpNot = 0x14
    kOpContainsStr = 0x15
    kOpContains0Str = 0x16
    kOpGetChunk = 0x17
    kOpHiliteChunk = 0x18
    kOpOntoSpr = 0x19
    kOpIntoSpr = 0x1a
    kOpGetField = 0x1b
    kOpStartTell = 0x1c
    kOpEndTell = 0x1d
    kOpPushList = 0x1e
    kOpPushPropList = 0x1f
    kOpSwap = 0x21
    kOpPushInt8 = 0x41
    kOpPushArgListNoRet = 0x42
    kOpPushArgList = 0x43
    kOpPushCons = 0x44
    kOpPushSymb = 0x45
    kOpPushVarRef = 0x46
    kOpGetGlobal2 = 0x48
    kOpGetGlobal = 0x49
    kOpGetProp = 0x4a
    kOpGetParam = 0x4b
    kOpGetLocal = 0x4c
    kOpSetGlobal2 = 0x4e
    kOpSetGlobal = 0x4f
    kOpSetProp = 0x50
    kOpSetParam = 0x51
    kOpSetLocal = 0x52
    kOpJmp = 0x53
    kOpEndRepeat = 0x54
    kOpJmpIfZ = 0x55
    kOpLocalCall = 0x56
    kOpExtCall = 0x57
    kOpObjCallV4 = 0x58
    kOpPut = 0x59
    kOpPutChunk = 0x5a
    kOpDeleteChunk = 0x5b
    kOpGet = 0x5c
    kOpSet = 0x5d
    kOpGetMovieProp = 0x5f
    kOpSetMovieProp = 0x60
    kOpGetObjProp = 0x61
    kOpSetObjProp = 0x62
    kOpTellCall = 0x63
    kOpPeek = 0x64
    kOpPop = 0x65
    kOpTheBuiltin = 0x66
    kOpObjCall = 0x67
    kOpPushChunkVarRef = 0x6d
    kOpPushInt16 = 0x6e
    kOpPushInt32 = 0x6f
    kOpGetChainedProp = 0x70
    kOpPushFloat32 = 0x71
    kOpGetTopLevelProp = 0x72
    kOpNewObj = 0x73

class DatumType(IntEnum):
    kDatumVoid = 0
    kDatumSymbol = 1
    kDatumVarRef = 2
    kDatumString = 3
    kDatumInt = 4
    kDatumFloat = 5
    kDatumList = 6
    kDatumArgList = 7
    kDatumArgListNoRet = 8
    kDatumPropList = 9

class ChunkExprType(IntEnum):
    kChunkChar = 1
    kChunkWord = 2
    kChunkItem = 3
    kChunkLine = 4

class PutType(IntEnum):
    kPutInto = 1
    kPutAfter = 2
    kPutBefore = 3

class BytecodeTag(IntEnum):
    kTagNone = 0
    kTagSkip = 1
    kTagRepeatWhile = 2
    kTagRepeatWithIn = 3
    kTagRepeatWithTo = 4
    kTagRepeatWithDownTo = 5
    kTagNextRepeatTarget = 6
    kTagEndCase = 7

class ScriptFlag(IntEnum):
    kScriptFlagUnused = 1 << 0
    kScriptFlagFuncsGlobal = 1 << 1
    kScriptFlagVarsGlobal = 1 << 2
    kScriptFlagUnk3 = 1 << 3
    kScriptFlagFactoryDef = 1 << 4
    kScriptFlagUnk5 = 1 << 5
    kScriptFlagUnk6 = 1 << 6
    kScriptFlagUnk7 = 1 << 7
    kScriptFlagHasFactory = 1 << 8
    kScriptFlagEventScript = 1 << 9
    kScriptFlagEventScript2 = 1 << 10
    kScriptFlagUnkB = 1 << 11
    kScriptFlagUnkC = 1 << 12
    kScriptFlagUnkD = 1 << 13
    kScriptFlagUnkE = 1 << 14
    kScriptFlagUnkF = 1 << 15

class LiteralType(IntEnum):
    kLiteralString = 1
    kLiteralInt = 4
    kLiteralFloat = 9
