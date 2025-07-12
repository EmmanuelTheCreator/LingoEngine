"""Simple command line interface for ProjectorRays.Python."""

import sys
from .common.json_writer import JSONWriter
from .common.stream import ReadStream, Endianness
from .director.rays_director_file import RaysDirectorFile


def main() -> None:
    if len(sys.argv) < 2 or sys.argv[1] in {"-h", "--help"}:
        print("Usage: projector <input> [--dump-json OUTPUT]")
        return

    input_path = sys.argv[1]
    json_output = None
    args = sys.argv[2:]
    i = 0
    while i < len(args):
        if args[i] == "--dump-json" and i + 1 < len(args):
            json_output = args[i + 1]
            i += 2
        else:
            i += 1

    with open(input_path, "rb") as f:
        data = f.read()

    stream = ReadStream(data, Endianness.BIG)
    director = RaysDirectorFile()
    if not director.read(stream):
        print("Failed to read Director file")
        return

    if json_output is not None:
        writer = JSONWriter()
        writer.start_object()
        writer.write_key("version")
        writer.write_val(director.version)
        writer.write_key("casts")
        writer.write_val(len(director.casts))
        writer.end_object()
        with open(json_output, "w", encoding="utf-8") as out:
            out.write(writer.to_string())

    print(f"Read Director file version {director.version}")


if __name__ == "__main__":
    main()
