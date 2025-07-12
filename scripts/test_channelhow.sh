#!/bin/sh
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PYTHON_DIR="$SCRIPT_DIR/../WillMoveToOwnRepo/ProjectorRays/src/ProjectorRays_Python"
DIR_FILE="$SCRIPT_DIR/../WillMoveToOwnRepo/ProjectorRays/Test/TestData/ChannelSearch/ChannelHow_4.dir"
if [ ! -f "$DIR_FILE" ]; then
  DIR_FILE="$SCRIPT_DIR/../WillMoveToOwnRepo/ProjectorRays/Test/TestData/Dir_With_One_Tex_Sprite_Hallo.dir"
fi
python3 "$PYTHON_DIR/program.py" "$DIR_FILE"
