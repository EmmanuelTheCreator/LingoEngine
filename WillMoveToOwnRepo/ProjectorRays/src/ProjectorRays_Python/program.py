"""Simple command line interface for ``ProjectorRays_Python``."""

from __future__ import annotations

import os
import sys

if __package__ is None or __package__ == "":
    # Allow running the script directly without installing the package
    sys.path.append(os.path.dirname(__file__))

from .common.stream import ReadStream, Endianness
from .director.rays_director_file import RaysDirectorFile


if __name__ == "__main__":
    import logging

    logging.basicConfig(level=logging.INFO)
    logger = logging.getLogger("projector")

    with open("ChannelHow_4.dir", "rb") as f:
        stream = ReadStream(f.read())
        director = RaysDirectorFile(logger)
        director.read(stream)

