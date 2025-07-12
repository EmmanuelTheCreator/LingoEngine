from dataclasses import dataclass, field
from typing import List

from .rays_handler import RaysHandler


@dataclass
class RaysScript:
    handlers: List[RaysHandler] = field(default_factory=list)
    factories: List['RaysScript'] = field(default_factory=list)

    def parse(self) -> None:
        for h in self.handlers:
            h.parse()
        for f in self.factories:
            f.parse()
