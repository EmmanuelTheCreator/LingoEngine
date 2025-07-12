from dataclasses import dataclass, field
from typing import Dict

from .rays_script import RaysScript


@dataclass
class ScriptContextMapEntry:
    section_id: int = 0


@dataclass
class ScriptContext:
    scripts: Dict[int, RaysScript] = field(default_factory=dict)

    def parse_scripts(self) -> None:
        for sc in self.scripts.values():
            sc.parse()
