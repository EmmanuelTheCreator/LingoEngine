from dataclasses import dataclass, field
from typing import List

from .rays_enums import OpCode
from .rays_ast import BlockNode


@dataclass
class Bytecode:
    opcode: OpCode
    obj: int
    pos: int


@dataclass
class RaysHandler:
    name: str = ''
    bytecode_array: List[Bytecode] = field(default_factory=list)
    ast: BlockNode | None = None

    def parse(self) -> None:
        self.ast = BlockNode()
