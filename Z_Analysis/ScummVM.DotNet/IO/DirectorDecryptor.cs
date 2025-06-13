namespace Director.IO
{

 
    public static class DirectorDecryptor
    {
        // XOR key table from ScummVM
        private static readonly byte[] s_key = new byte[]
        {
    0x69, 0x97, 0x11, 0xC0, 0x7D, 0x9F, 0xF4, 0xA9,
    0xCB, 0x0E, 0x4F, 0xE4, 0xE2, 0xA5, 0x59, 0x13
        };

        /// <summary>
        /// Decrypts a buffer in-place using Director's XOR algorithm.
        /// </summary>
        public static void Decrypt(byte[] buffer)
        {
            byte k = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] ^= s_key[k];
                k = (byte)((k + buffer[i]) & 0xF);
            }
        }

        /// <summary>
        /// Wraps a Read-only byte stream with decryption for Director encrypted resources.
        /// </summary>
        public static MemoryStream WrapWithDecryption(Stream encryptedStream)
        {
            var ms = new MemoryStream();
            encryptedStream.CopyTo(ms);
            byte[] data = ms.ToArray();
            Decrypt(data);
            return new MemoryStream(data, writable: false);
        }

    }

    


}
