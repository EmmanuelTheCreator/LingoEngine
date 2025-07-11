from .rays_enums import OpCode, ChunkExprType, PutType

class StandardNames:
    OpcodeNames = {
        OpCode.kOpRet: 'ret',
        OpCode.kOpRetFactory: 'retfactory',
        OpCode.kOpPushZero: 'pushzero',
        OpCode.kOpMul: 'mul',
        OpCode.kOpAdd: 'add',
        OpCode.kOpSub: 'sub',
        OpCode.kOpDiv: 'div',
        OpCode.kOpMod: 'mod',
        OpCode.kOpInv: 'inv',
        OpCode.kOpJoinStr: 'joinstr',
        OpCode.kOpJoinPadStr: 'joinpadstr',
        OpCode.kOpLt: 'lt',
        OpCode.kOpLtEq: 'lteq',
        OpCode.kOpNtEq: 'nteq',
        OpCode.kOpEq: 'eq',
        OpCode.kOpGt: 'gt',
        OpCode.kOpGtEq: 'gteq',
        OpCode.kOpAnd: 'and',
        OpCode.kOpOr: 'or',
        OpCode.kOpNot: 'not',
        OpCode.kOpContainsStr: 'containsstr',
        OpCode.kOpContains0Str: 'contains0str',
    }

    BinaryOpNames = {
        OpCode.kOpMul: '*',
        OpCode.kOpAdd: '+',
        OpCode.kOpSub: '-',
        OpCode.kOpDiv: '/',
        OpCode.kOpMod: 'mod',
        OpCode.kOpJoinStr: '&',
        OpCode.kOpJoinPadStr: '&&',
        OpCode.kOpLt: '<',
        OpCode.kOpLtEq: '<=',
        OpCode.kOpNtEq: '<>',
        OpCode.kOpEq: '=',
        OpCode.kOpGt: '>',
        OpCode.kOpGtEq: '>=',
        OpCode.kOpAnd: 'and',
        OpCode.kOpOr: 'or',
        OpCode.kOpContainsStr: 'contains',
        OpCode.kOpContains0Str: 'starts',
    }

    ChunkTypeNames = {
        ChunkExprType.kChunkChar: 'char',
        ChunkExprType.kChunkWord: 'word',
        ChunkExprType.kChunkItem: 'item',
        ChunkExprType.kChunkLine: 'line',
    }

    PutTypeNames = {
        PutType.kPutInto: 'into',
        PutType.kPutAfter: 'after',
        PutType.kPutBefore: 'before',
    }

    @staticmethod
    def get_opcode_name(op: int) -> str:
        if op >= 0x40:
            u = 0x40 + op % 0x40
            return StandardNames.OpcodeNames.get(OpCode(u), f"unk{op:02X}")
        return StandardNames.OpcodeNames.get(OpCode(op), f"unk{op:02X}")

    @staticmethod
    def get_name(map_: dict, id_: int) -> str:
        return map_.get(id_, 'ERROR')
