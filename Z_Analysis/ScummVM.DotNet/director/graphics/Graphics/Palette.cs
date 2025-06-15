namespace Director.Graphics
{
    public class Palette
    {
        private byte[] _colors;
        private int _size;

        public Palette()
        {
            _colors = Array.Empty<byte>();
            _size = 0;
        }

        public void Resize(int newSize, bool clear = false)
        {
            var newPalette = new byte[newSize * 3]; // 3 bytes per color (RGB)
            if (!clear && _colors.Length >= newPalette.Length)
            {
                Array.Copy(_colors, newPalette, newPalette.Length);
            }
            _colors = newPalette;
            _size = newSize;
        }

        public void Set(byte[] source, int startIndex, int count)
        {
            if (_colors.Length < (count * 3))
            {
                Resize(count);
            }
            Array.Copy(source, startIndex * 3, _colors, 0, count * 3);
        }

        public byte[] Colors => _colors;
        public int Length => _size;
    }


}
