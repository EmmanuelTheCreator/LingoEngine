using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace BlingoEngine.IO.Legacy.Texts
{

    /// <summary>Represents one 20-byte XMED directory entry (Type, Offset, Count, StyleId).</summary>
    public readonly record struct XmedHeaderRecord(
        string Type,   // "FFFF", "0002", etc.
        int Offset,    // parsed as hex
        int Count,     // parsed as hex
        int StyleId,   // parsed as hex
        uint RawValue) // full 32-bit composite (for sentinel interpretation)
    {
        /// <summary>High 16-bit word of the composite (table index).</summary>
        public int TableIndex => (int)(RawValue >> 16);

        /// <summary>Low 16-bit word of the composite (entry index).</summary>
        public int EntryIndex => (int)(RawValue & 0xFFFF);

        public bool IsRoot => Type == "FFFF";
    }


    /// <summary>
    /// Low-level XMED byte walker with control-byte intelligence 
    /// </summary>
    public sealed class XMEDByteReader
    {
        /// <summary>Control bytes.</summary>
        public const byte VAL = 0x01;      // Perhaps start/value (ASCII follows)
        public const byte NUM = 0x02;      // Perhaps numeric token (ASCII digits/hex follow)
        public const byte NUM_SOMETHING = 0x03;
        public const byte SEP = 0x81;      // Perhaps continuation / next sub-field in same property
        public const byte BND = 0x82;      // Perhaps boundary/end-of-value to next property
        public const byte RUN = 0xC1;      // Perhaps style/run opcode prefix
        public const byte REND = 0xC2;     // Perhaps end of run/segment

        private readonly byte[] _buf;
        /// <summary>Current absolute offset into buffer.</summary>
        public int Position { get; private set; }
        // Update all usages of _buf to use _buf.AsSpan() or _buf.AsSpan(Position, length) as appropriate.

        // Update constructor and usages:
        public int Length => _buf.Length;
        /// <summary>True when the cursor reached end of buffer.</summary>
        public bool EOF => (uint)Position >= (uint)_buf.Length;

        public XMEDByteReader(byte[] buffer) => _buf = buffer;

        /// <summary>Advance by <paramref name="count"/> bytes if possible.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Skip(int count) => Position = Math.Min(Length, Position + Math.Max(0, count));

        /// <summary>Move the cursor backwards by <paramref name="count"/> bytes.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rewind(int count)
        {
            if (count <= 0) return;
            int target = Position - count;
            if (target < 0) target = 0;
            Position = target;
        }

        // Update methods to use AsSpan() for slicing:
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public byte Peek(int rel = 0)
        {
            int i = Position + rel;
            var number = (uint)i < (uint)_buf.Length ? _buf[i] : (byte)0x00;
            return number;
        }

        /// <summary>Read next byte (0 if beyond end) and advance the cursor by 1.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            if (EOF) return 0;
            return _buf[Position++];
        }
        public byte[] ReadBytes(int count)
        {
            if (count <= 0 || EOF) return Array.Empty<byte>();
            int available = Math.Min(count, Length - Position);
            var result = new byte[available];
            Array.Copy(_buf, Position, result, 0, available);
            Position += available;
            return result;
        }

        /// <summary>True if <paramref name="b"/> is printable ASCII (0x20..0x7E).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiPrintable(byte b) => b >= 0x20 && b <= 0x7E;

        /// <summary>True if <paramref name="b"/> is ASCII hex nibble ('0'..'9','A'..'F').</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiHex(byte b) =>
            (b >= (byte)'0' && b <= (byte)'9') || (b >= (byte)'A' && b <= (byte)'F');

        /// <summary>True if this byte is considered a control marker (non-printable or high bit set).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsControl(byte b) => b < 0x20 || b >= 0x80;

        /// <summary>
        /// Read a <b>numeric token</b> that starts with 0x02 and is composed of ASCII digits/hex (optional leading '-').
        /// Returns false if not on a numeric token. Method name: <c>TryReadNumericToken</c>.
        /// </summary>
        public bool TryReadNumericToken(out int value, byte? header = NUM)
        {
            value = 0;
            if (header != null)
            {
                if (Peek() != header) return false;
                // consume 0x02
                Skip(1);
            }
            // optional minus
            bool neg = false;
            if (Peek() == (byte)'-') { neg = true; Skip(1); }

            // collect sequence of ASCII hex/digits
            int start = Position;
            while (!EOF && (IsAsciiHex(Peek()) || (Peek() >= (byte)'0' && Peek() <= (byte)'9')))
                Skip(1);

            if (Position == start) return false; // no digits

            var span = _buf.AsSpan(start, Position - start);
            var s = Encoding.ASCII.GetString(span);
            value = int.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            if (neg) value = -value;
            return true;
        }

        /// <summary>
        /// Read a <b>literal value token</b> that starts with 0x01 and captures following ASCII bytes until a control byte.
        /// Returns false if not on a value token. Method name: <c>TryReadValueAscii</c>.
        /// </summary>
        public bool TryReadValueAscii(out ReadOnlySpan<byte> ascii)
        {
            ascii = default;
            if (Peek() != VAL) return false;
            Skip(1);
            int start = Position;
            while (!EOF && IsAsciiPrintable(Peek()))
                Skip(1);
            int len = Position - start;
            ascii = len > 0 ? _buf.AsSpan(start, len) : default;
            return len > 0;
        }

        /// <summary>
        /// Expect a <b>C1 opcode</b> at cursor and return its low byte (style/alignment toggle).
        /// Returns false if current byte is not 0xC1. Method name: <c>TryReadC1Opcode</c>.
        /// </summary>
        public bool TryReadC1Opcode(out byte opcode)
        {
            opcode = 0;
            if (Peek() != RUN) return false;
            Skip(1);
            opcode = ReadByte();
            return true;
        }

        /// <summary>
        /// Expect a <b>C2</b> run terminator (returns the following byte when present). Method: <c>TryReadC2Tail</c>.
        /// </summary>
        public bool TryReadC2Tail(out byte tail)
        {
            tail = 0;
            if (Peek() != REND) return false;
            Skip(1);
            tail = ReadByte(); // commonly small code (e.g., 0x07, 0x0A, …)
            return true;
        }

        /// <summary>
        /// Skip any number of <b>boundary/continuation</b> bytes (81/82) and return how many were skipped.
        /// Method name: <c>SkipSeparators</c>.
        /// </summary>
        public int SkipSeparators()
        {
            int n = 0;
            while (!EOF)
            {
                var b = Peek();
                if (b == SEP || b == BND) { Skip(1); n++; continue; }
                break;
            }
            return n;
        }
        public bool TryReadAsciiNumbers(out List<int> numbers, byte? preFixByte = NUM_SOMETHING)
        {
            numbers = new List<int>();

            if (preFixByte != null) { 
                // must start with 0x03
                if (Peek() != preFixByte)
                    return false;
            }
            Skip(1);

            while (TryReadAsciiHexInt16(out var number))
                numbers.Add(number);
            return true;

        }
        public bool TryReadAsciiHexInt16(out int value)
        {
            value = 0;
            if (!TryReadAsciiHexByte(out var hi)) return false;
            var span = _buf.AsSpan(start, len);
            var s = Encoding.ASCII.GetString(span);

            // Detect hex if contains A–F, otherwise decimal
            bool hasHex = false;
            int offset = 0;
            if (s.StartsWith("-", StringComparison.Ordinal))
            {
                offset = 1;
            }

            for (int i = offset; i < s.Length; i++)
            {
                char c = s[i];
                if ((c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'))
                {
                    hasHex = true;
                    break;
                }
            }

            if (hasHex)
            {
                var spanToParse = s.AsSpan(offset);
                int parsed = int.Parse(spanToParse, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                value = offset == 1 ? -parsed : parsed;
            }
            else
            {
                value = int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }

            return true;
        /// </summary>
        public bool TryReadAsciiInt(out int value)
        {
            value = 0;

            // must start with 0x01
            if (Peek() != VAL)
                return false;

            Skip(1); // consume 0x01

            int start = Position;
            while (!EOF && !IsControl(Peek()))
                Skip(1);

            int len = Position - start;
            if (len <= 0)
                return false;

            var span = _buf.AsSpan(start, len);
            var s = Encoding.ASCII.GetString(span);

            // Detect hex if contains A–F, otherwise decimal
                value = int.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            return true;
        }

        /// <summary>
        /// Read an ASCII-HEX byte (e.g., '4','6' → 0x46). Method: <c>TryReadAsciiHexByte</c>.
        /// </summary>
        public bool TryReadAsciiHexByte(out byte value)
        {
            value = 0;
            int a = Peek(), b = Peek(1);
            if (!IsAsciiHex((byte)a) || !IsAsciiHex((byte)b)) return false;
            Span<byte> tmp = stackalloc byte[2];
            tmp[0] = (byte)a; tmp[1] = (byte)b;
            var s = Encoding.ASCII.GetString(tmp);
            value = byte.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            Skip(2);
            return true;
        }

        /// <summary>
        /// Read a color triple written as <c>VAL/SEP</c>-delimited ASCII-HEX pairs per channel (R,G,B).
        /// Handles optional empty channels via consecutive 0x81. Returns false if not on a value token.
        /// Method name: <c>TryReadRgb</c>.
        /// </summary>
        public bool TryReadRgb(out byte r, out byte g, out byte b)
        {
            r = g = b = 0;
            if (Peek() != VAL && Peek() != SEP) return false;

            // Channel reader: expects either VAL or SEP then ASCII hex (2 bytes) then "00" pair possibly follows.
            bool ReadChan(out byte v)
            {
                v = 0;
                if (Peek() == SEP) Skip(1);           // channel continuation allowed
                if (Peek() == VAL) Skip(1);           // value marker
                // Read first hex pair as intensity.
                if (!TryReadAsciiHexByte(out v)) return false;

                // Consume optional trailing "00" (30 30) if present; not needed for the 8-bit value we expose.
                if (Peek() == (byte)'3' && Peek(1) == (byte)'0' && Peek(2) == (byte)'3' && Peek(3) == (byte)'0')
                    Skip(4);
                return true;
            }

            // Allow initial chain of separators before first value
            SkipSeparators();

            // R
            if (Peek() == SEP) Skip(1);
            if (Peek() != VAL) return false;
            _ = ReadChan(out r);

            // G (may be implicit zero via consecutive SEP)
            if (Peek() == SEP || Peek() == VAL)
            {
                byte gv;
                if (ReadChan(out gv)) g = gv;
            }

            // B (same)
            if (Peek() == SEP || Peek() == VAL)
            {
                byte bv;
                if (ReadChan(out bv)) b = bv;
            }

            return true;
        }

        /// <summary>
        /// Apply a C1 style/alignment opcode to the given descriptor.
        /// Method name: <c>ApplyC1Opcode</c>.
        /// </summary>
        public static void ApplyC1Opcode(byte opcode, ref XmedStyleDescriptor style)
        {
            // Style bits (confirmed): 0x01 Bold, 0x02 Italic, 0x04 Underline, 0x08 Strike, 0x10 Sub, 0x20 Super, 0x40 Tab/Outline?, 0x80 Editable (header only)
            switch (opcode)
            {
                // Underline toggle region examples often carry small literal "01 31 01 30" around; we flip the bit explicitly.
                case 0x1C: // underline on
                    style.Underline = true; break;
                case 0x20: // underline off / baseline align bucket used in left/right, keep underline as-is if alignment only
                    // do nothing for underline directly; handled via alignment map below
                    break;

                // Strikeout/sub/super (paired sites)
                case 0x13: style.Strikeout = true; break;
                case 0x11: style.Subscript = true; style.Superscript = false; break;
                case 0x12: style.Superscript = true; style.Subscript = false; break;

                // Clear sub/super when baseline opcodes encountered (0x1E bucket); conservative:
                case 0x1E:
                    style.Subscript = false; style.Superscript = false; break;
            }
        }

        /// <summary>
        /// Map alignment from the low 2 bits of an alignment byte to <see cref="XmedAlignment"/>.
        /// Method: <c>DecodeAlignment</c>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmedAlignment DecodeAlignment(byte alignFlags)
        {
            return (alignFlags & 0x03) switch
            {
                0x01 => XmedAlignment.Right,
                0x02 => XmedAlignment.Left,
                0x03 => XmedAlignment.Justify,
                _ => XmedAlignment.Center
            };
        }

        /// <summary>
        /// Decode style flags bitfield (0x001C) into the descriptor.
        /// Method: <c>ApplyStyleFlagsBits</c>.
        /// </summary>
        public static void ApplyStyleFlagsBits(byte flags, ref XmedStyleDescriptor style)
        {
            style.Bold = (flags & 0x01) != 0;
            style.Italic = (flags & 0x02) != 0;
            style.Underline = (flags & 0x04) != 0;
            style.Strikeout = (flags & 0x08) != 0;
            style.Subscript = (flags & 0x10) != 0;
            style.Superscript = (flags & 0x20) != 0;
            style.TabbedField = (flags & 0x40) != 0;
            style.EditableField = (flags & 0x80) != 0;
        }

        /// <summary>
        /// Decode alignment/layout bitfield (0x001D) into the descriptor.
        /// Method: <c>ApplyAlignmentBits</c>.
        /// </summary>
        public static void ApplyAlignmentBits(byte flags, ref XmedStyleDescriptor style)
        {
            style.WrapOff = (flags & 0x08) != 0;
            style.HasTabs = (flags & 0x10) != 0;
            style.AlignmentFromFlags = DecodeAlignment(flags);
            style.Alignment = style.AlignmentFromFlags;
        }

        /// <summary>
        /// Scan forward to the next control boundary (81/82/C1/C2) and return the span of printable ASCII between cursor and boundary.
        /// Cursor ends at the boundary byte. Method: <c>ReadAsciiUntilBoundary</c>.
        /// </summary>
        public ReadOnlySpan<byte> ReadAsciiUntilBoundary()
        {
            int start = Position;
            while (!EOF)
            {
                var b = Peek();

            // Fallback: single chunk parse
            if (int.TryParse(Encoding.ASCII.GetString(a), NumberStyles.Integer, CultureInfo.InvariantCulture, out param))
            {
                return true;
            }

        /// <summary>
        /// Reads one 20-byte ASCII header record (e.g., "FFFF0000000600040001").
        /// Returns true if a valid record was read.
        /// </summary>
        public bool TryReadHeaderRecord(out XmedHeaderRecord rec)
        {
            rec = default;

            // need 20 ASCII bytes minimum
            if (EOF || Position + 20 > _buf.Length)
                return false;

            // verify ASCII-printable
            for (int i = 0; i < 20; i++)
                if (!IsAsciiPrintable(_buf[Position + i]))
                    return false;

            var s = Encoding.ASCII.GetString(_buf, Position, 20);
            Position += 20;

            // split the ASCII block
            string type = s[..4];
            string offStr = s.Substring(4, 8);
            string cntStr = s.Substring(12, 4);
            string styStr = s.Substring(16, 4);

            // parse all as hex
            int offset = int.Parse(offStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            int count = int.Parse(cntStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            int style = int.Parse(styStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            // compose sentinel (Count|StyleId)
            uint raw = (uint)((count << 16) | (style & 0xFFFF));

            rec = new XmedHeaderRecord(type, offset, count, style, raw);
            return true;
        }

        // XMEDByteReader method (unified): TryReadHeaderOrInlineRecord
            if (!EOF && Position + 20 <= _buf.Length)
            {
                bool allAscii = true;
                for (int i = 0; i < 20; i++)
                {
                    byte b = _buf[Position + i];
                    if (b < 0x20 || b > 0x7E) { allAscii = false; break; }
                }

                if (allAscii)
                {
                    var s = Encoding.ASCII.GetString(_buf, Position, 20);
                    // quick sanity: first 4 must be hex
                    bool hex4 =
                        s.Length >= 4 &&
                        int.TryParse(s.AsSpan(0, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _);

                    if (hex4)
                    {
                        string type = s[..4];
                        string offStr = s.Substring(4, 8);
                        string cntStr = s.Substring(12, 4);
                        string styStr = s.Substring(16, 4);

                        if (int.TryParse(offStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int offset) &&
                            int.TryParse(cntStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int count) &&
                            int.TryParse(styStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int style))
                        {
                            Position += 20;
                            uint raw2 = (uint)((count << 16) | (style & 0xFFFF));
                            rec = new XmedHeaderRecord(type, offset, count, style, raw2);
                            return true;
                        }
                    }
                }
            }

            // 2) Fallback to inline numeric sentinel: 0x02 "40001" (or 0x01 "40001")
            if (EOF) return false;

            byte prefix = Peek();
            if (prefix != NUM && prefix != VAL) return false; // must be 0x02 or 0x01
            Skip(1);

            int start = Position;
            while (!EOF && !IsControl(Peek())) Skip(1);
            int len = Position - start;
            if (len <= 0) return false;

            // parse as HEX (always hex)
            var span = _buf.AsSpan(start, len);
            // optional leading '-' not expected here; ignore if present
            bool neg = span.Length > 0 && span[0] == (byte)'-';
            if (neg) span = span.Slice(1);
            if (span.Length == 0) return false;

            uint raw = 0;
            for (int i = 0; i < span.Length; i++)
            {
                byte c = span[i];
                int d =
                    (c >= (byte)'0' && c <= (byte)'9') ? c - '0' :
                    (c >= (byte)'A' && c <= (byte)'F') ? c - 'A' + 10 :
                    (c >= (byte)'a' && c <= (byte)'f') ? c - 'a' + 10 : -1;
                if (d < 0) return false;
                unchecked { raw = (raw << 4) | (uint)d; }
            }

            // split into count/style like 0x00040001 → count=0x0004, style=0x0001
            int countHi = (int)(raw >> 16);
            int styleLo = (int)(raw & 0xFFFF);


            Position += 20;

            // split the ASCII block
            string type = s[..4];
            string offStr = s.Substring(4, 8);
            string cntStr = s.Substring(12, 4);
            string styStr = s.Substring(16, 4);

            // parse all as hex
            int offset = int.Parse(offStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            int count = int.Parse(cntStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            int style = int.Parse(styStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            // compose sentinel (Count|StyleId)
            uint raw = (uint)((count << 16) | (style & 0xFFFF));

            rec = new XmedHeaderRecord(type, offset, count, style, raw);
            return true;
        }

        // XMEDByteReader method (unified): TryReadHeaderOrInlineRecord
        public bool TryReadHeaderOrInlineRecord(out XmedHeaderRecord rec)
        {
            rec = default;

            // 1) Try 20-byte header card: "FFFF0000000600040001"
            if (!EOF && Position + 20 <= _buf.Length)
            {
                bool allAscii = true;
                for (int i = 0; i < 20; i++)
                {
                    byte b = _buf[Position + i];
                    if (b < 0x20 || b > 0x7E) { allAscii = false; break; }
                }

                if (allAscii)
                {
                    var s = Encoding.ASCII.GetString(_buf, Position, 20);
                    // quick sanity: first 4 must be hex
                    bool hex4 =
                        s.Length >= 4 &&
                        int.TryParse(s.AsSpan(0, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _);


                    if (hex4)
                    {
                        string type = s[..4];
                        string offStr = s.Substring(4, 8);
                        string cntStr = s.Substring(12, 4);
                        string styStr = s.Substring(16, 4);

                        if (int.TryParse(offStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int offset) &&
                            int.TryParse(cntStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int count) &&
                            int.TryParse(styStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int style))
                        {
                            Position += 20;
                            uint raw2 = (uint)((count << 16) | (style & 0xFFFF));
                            rec = new XmedHeaderRecord(type, offset, count, style, raw2);
                            return true;
                        }
                    }
                }
            }

            // 2) Fallback to inline numeric sentinel: 0x02 "40001" (or 0x01 "40001")
            if (EOF) return false;

            byte prefix = Peek();
            if (prefix != NUM && prefix != VAL) return false; // must be 0x02 or 0x01
            Skip(1);

            int start = Position;
            while (!EOF && !IsControl(Peek())) Skip(1);
            rec = new XmedHeaderRecord("INLINE", 0, countHi, styleLo, raw);
            return true;
        }

        public bool TryReadBlockContent(int limit, out ReadOnlySpan<byte> content, out byte closingTail)
        {
            content = default;
            closingTail = 0;

            if (Position >= limit)
            {
                return false;
            }

            int start = Position;
            int depth = 1;

            while (!EOF && Position < limit)
            {
                byte b = Peek();
                if (b == RUN)
                {
                    Skip(1);
                    if (!EOF)
                    {
                        Skip(1);
                        depth++;
                    }

                    continue;
                }

                if (b == REND)
                {
                    Skip(1);
                    if (!EOF)
                    {
                        byte tail = ReadByte();
                        depth--;
                        if (depth == 0)
                        {
                            closingTail = tail;
                            int end = Position - 2;
                            if (end < start)
                            {
                                end = start;
                            }

                            content = _buf.AsSpan(start, end - start);
                            return true;
                        }
                    }

                    continue;
                }

                Skip(1);
            }

            Position = start;
            return false;
        }

        public bool TryReadAsciiDigits(int limit, out ReadOnlySpan<byte> digits)
        {
            digits = default;
            if (EOF || Position >= limit)
            {
                return false;
            }

            byte current = Peek();
            if (current < (byte)'0' || current > (byte)'9')
            {
                return false;
            }

            int start = Position;
            while (!EOF && Position < limit)
            {
                byte b = Peek();
                if (b >= (byte)'0' && b <= (byte)'9')
                {
                    Skip(1);
                    continue;
                }

                break;
            }

            int length = Position - start;
            digits = _buf.AsSpan(start, length);
            return length > 0;
        }

        public bool TryReadAsciiHexPair(out (int high, int low) pair)
            uint raw = 0;
            for (int i = 0; i < span.Length; i++)
            {
                byte c = span[i];
                int d =
                    (c >= (byte)'0' && c <= (byte)'9') ? c - '0' :
                    (c >= (byte)'A' && c <= (byte)'F') ? c - 'A' + 10 :
                    (c >= (byte)'a' && c <= (byte)'f') ? c - 'a' + 10 : -1;
                if (d < 0) return false;
                unchecked { raw = (raw << 4) | (uint)d; }
            }

            // split into count/style like 0x00040001 → count=0x0004, style=0x0001
            int countHi = (int)(raw >> 16);
            int styleLo = (int)(raw & 0xFFFF);

            rec = new XmedHeaderRecord("INLINE", 0, countHi, styleLo, raw);
            return true;
        }
        /// <summary>
        /// Reads the next numeric token expected to contain two 16-bit ASCII-hex numbers concatenated
        /// (e.g. "480048" → (0x0048, 0x0048)).
        /// Returns false if parsing fails.
        /// </summary>
        public bool TryReadAsciiHexPair(out (int high, int low) pair)
        {
            pair = default;

            if (!TryReadNumericToken(out var value))
                return false;

            // When Director encodes "480048", high = 0x0048, low = 0x0048
            int high = (value >> 16) & 0xFFFF;
            int low = value & 0xFFFF;

            pair = (high, low);
            return true;
        }
    }
}
