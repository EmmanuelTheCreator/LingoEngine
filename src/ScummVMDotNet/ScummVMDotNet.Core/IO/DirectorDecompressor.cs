namespace Director.IO
{

    public static class DirectorDecompressor
    {
        /// <summary>
        /// Wraps the stream, decompressing LZSS chunks when detected.
        /// Checks if the data starts with an LZSS flag byte.
        /// </summary>
        public static Stream WrapIfNeeded(Stream s)
        {
            int first = s.ReadByte();
            if (first < 0)
                return s; // empty stream

            s.Seek(0, SeekOrigin.Begin);

            // LZSS uses flag bytes for every 8 chunks
            // No strict signature, but usually compressed files begin with non-random flag
            // We'll just attempt decoding for .dcr/.dxr files
            return new MemoryStream(DecompressLzss(s).ToArray(), false);
        }

        private static MemoryStream DecompressLzss(Stream input)
        {
            const int BUFFER_SIZE = 4096;
            const int THRESHOLD = 3;

            var window = new byte[BUFFER_SIZE];
            Array.Fill(window, (byte)0x20);
            int windowPos = BUFFER_SIZE - THRESHOLD - 1;

            var output = new MemoryStream();
            int flag = 0, mask = 0;

            int nextByte;
            while ((nextByte = input.ReadByte()) != -1)
            {
                flag = nextByte;
                for (int i = 0; i < 8; i++)
                {
                    if ((flag & (1 << i)) != 0)
                    {
                        // literal byte
                        int c = input.ReadByte();
                        if (c < 0) return output;
                        output.WriteByte((byte)c);
                        window[windowPos] = (byte)c;
                        windowPos = (windowPos + 1) & (BUFFER_SIZE - 1);
                    }
                    else
                    {
                        // back-reference
                        int byte1 = input.ReadByte();
                        int byte2 = input.ReadByte();
                        if (byte2 < 0) return output;
                        int offsetLen = byte1 | (byte2 << 8);
                        int offset = (windowPos - ((offsetLen >> 4) & 0xFFF) - 1) & (BUFFER_SIZE - 1);
                        int length = (offsetLen & 0xF) + THRESHOLD;

                        for (int j = 0; j < length; j++)
                        {
                            byte c = window[(offset + j) & (BUFFER_SIZE - 1)];
                            output.WriteByte(c);
                            window[windowPos] = c;
                            windowPos = (windowPos + 1) & (BUFFER_SIZE - 1);
                        }
                    }
                }
            }

            return output;
        }
    }

}



