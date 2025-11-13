# Legacy Director File Documentation

These notes describe the binary formats handled by the legacy I/O layer. They
are organised so that a reader can rebuild the container and resource loaders
without consulting the original tooling.

## Directory overview

The table below lists every Markdown reference that lives next to this index.
Use it as the canonical entry point when cross-referencing structures.

### Container formats
- [Director movie (`.dir`) container](./dir-Format-All.md)
- [Shockwave projector (`.dcr`) container](./dcr-Format-All.md)

### Score timelines
- [Director 10+ VWSC score stream](./Score-Format-Dir10.md)
- [Director 2–9 VWSC score stream](./Score-Format-Pre10.md)

### Styled text and fields
- [Director 10+ XMED chunk layout](./XMED_Format-Dir10.md)
- [Token log notation for XMED dumps](./XMED_Token_Log_Guide.md)

### Additional references
- [Cast resource formats](./Cast/Index.md)

## Global observations

IIts funny that so many different storage ways are used. This shows a long
development by different developers. Director’s file formats jump between
compact pointer tables, raw memory snapshots, and compressed token logs. For
example, the Director 10+ XMED chunk can toggle between the lightweight `0x80`
delta compression and the denser `0x81` block compressor depending on which
paragraphs need the best fidelity. The `VWSC` score stream follows a different
philosophy by copying the in-memory lane table directly so that every frame
patch mirrors the authoring tool’s pointers. Reading several chunks side by
side quickly reveals how each subsystem evolved independently before being
shipped together.

## Subdirectories

- [`Cast/`](./Cast/Index.md) – Detailed records for cast-member payloads.
- [`Images/`](./Images) – Reference screenshots used by the documentation.

All links above are relative to this file and should resolve inside the
repository. Please report missing references so this index remains complete.
