from dataclasses import dataclass, field
from typing import List, Optional

from .rays_enums import DatumType, OpCode
from ..common.rays_code_writer import RaysCodeWriter


@dataclass
class Datum:
    type: DatumType = DatumType.kDatumVoid
    i: int = 0
    f: float = 0.0
    s: str = ''
    l: List['Datum'] = field(default_factory=list)

    def to_int(self) -> int:
        if self.type == DatumType.kDatumInt:
            return self.i
        if self.type == DatumType.kDatumFloat:
            return int(self.f)
        return 0

    def write_script_text(self, code: RaysCodeWriter, dot: bool, sum_: bool):
        if self.type == DatumType.kDatumVoid:
            code.write('VOID')
        elif self.type == DatumType.kDatumSymbol:
            code.write('#' + self.s)
        elif self.type == DatumType.kDatumVarRef:
            code.write(self.s)
        elif self.type == DatumType.kDatumString:
            code.write('"' + self.s + '"' if self.s else 'EMPTY')
        elif self.type == DatumType.kDatumInt:
            code.write(str(self.i))
        elif self.type == DatumType.kDatumFloat:
            code.write(str(self.f))
        else:
            code.write('LIST')


class AstNode:
    def write_script_text(self, code: RaysCodeWriter) -> None:
        """Write a textual representation of this node to ``code``.

        Base nodes have no default rendering; subclasses should override this
        method to output their specific syntax.
        """
        # Intentionally empty -- concrete nodes implement their own logic.
        return


@dataclass
class BlockNode(AstNode):
    statements: List[AstNode] = field(default_factory=list)

    def write_script_text(self, code: RaysCodeWriter) -> None:
        for stmt in self.statements:
            stmt.write_script_text(code)
            code.write_line()


@dataclass
class LiteralNode(AstNode):
    value: Datum

    def write_script_text(self, code: RaysCodeWriter) -> None:
        self.value.write_script_text(code, False, False)


@dataclass
class VarNode(AstNode):
    name: str

    def write_script_text(self, code: RaysCodeWriter) -> None:
        code.write(self.name)


@dataclass
class BinaryOpNode(AstNode):
    op: OpCode
    left: AstNode
    right: AstNode

    def write_script_text(self, code: RaysCodeWriter) -> None:
        self.left.write_script_text(code)
        code.write(' ' + self.op.name + ' ')
        self.right.write_script_text(code)


@dataclass
class UnaryOpNode(AstNode):
    op: OpCode
    operand: AstNode

    def write_script_text(self, code: RaysCodeWriter) -> None:
        code.write(self.op.name + ' ')
        self.operand.write_script_text(code)


@dataclass
class AssignmentNode(AstNode):
    target: VarNode
    value: AstNode

    def write_script_text(self, code: RaysCodeWriter) -> None:
        self.target.write_script_text(code)
        code.write(' = ')
        self.value.write_script_text(code)


@dataclass
class ArgListNode(AstNode):
    args: List[AstNode] = field(default_factory=list)
    no_return: bool = False

    def write_script_text(self, code: RaysCodeWriter) -> None:
        for i, a in enumerate(self.args):
            if i > 0:
                code.write(', ')
            a.write_script_text(code)


@dataclass
class CallNode(AstNode):
    name: str
    args: List[AstNode] = field(default_factory=list)
    statement: bool = False

    def write_script_text(self, code: RaysCodeWriter) -> None:
        code.write(self.name + '(')
        for i, a in enumerate(self.args):
            if i > 0:
                code.write(', ')
            a.write_script_text(code)
        code.write(')')


@dataclass
class ReturnNode(AstNode):
    value: Optional[AstNode] = None

    def write_script_text(self, code: RaysCodeWriter) -> None:
        code.write('return')
        if self.value:
            code.write(' ')
            self.value.write_script_text(code)


@dataclass
class UnknownOpNode(AstNode):
    bytecode: 'Bytecode'

    def write_script_text(self, code: RaysCodeWriter) -> None:
        code.write(f"/* unknown {self.bytecode.opcode} */")
