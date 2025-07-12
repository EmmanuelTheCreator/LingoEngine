"""Simple command line interface for ``ProjectorRays_Python``."""

from __future__ import annotations

import os
import sys

if __package__ is None or __package__ == "":
    # Allow running the script directly without installing the package
    PACKAGE_ROOT = os.path.dirname(__file__)
    sys.path.append(os.path.dirname(PACKAGE_ROOT))
    from ProjectorRays_Python.common.stream import ReadStream
    from ProjectorRays_Python.director.rays_director_file import RaysDirectorFile
    from ProjectorRays_Python.director.scores import RaysScoreFrameParserV2
else:
    from .common.stream import ReadStream
    from .director.rays_director_file import RaysDirectorFile
    from .director.scores import RaysScoreFrameParserV2


if __name__ == "__main__":
    import argparse
    import logging

    parser = argparse.ArgumentParser(
        description="Read a Director .dir file using ProjectorRays"
    )
    parser.add_argument(
        "dir_file",
        help="Path to the .dir file to open"
    )
    parser.add_argument(
        "--full-score",
        action="store_true",
        help="Parse score blocks and link keyframes"
    )
    args = parser.parse_args()

    logging.basicConfig(level=logging.INFO)
    logger = logging.getLogger("projector")

    RaysScoreFrameParserV2.enable_full_parsing = args.full_score

    with open(args.dir_file, "rb") as f:
        stream = ReadStream(f.read())
        director = RaysDirectorFile(logger)
        director.read(stream)

    print(f"Loaded '{args.dir_file}' successfully")

