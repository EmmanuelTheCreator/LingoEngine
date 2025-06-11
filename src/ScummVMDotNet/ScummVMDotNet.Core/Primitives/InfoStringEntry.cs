using System.Text;

namespace Director.Primitives
{
    public class InfoStringEntry
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public int Length => Data.Length;

        public string ReadString(bool nullTerminated = true)
        {
            if (nullTerminated)
            {
                int i = Array.IndexOf(Data, (byte)0);
                return Encoding.UTF8.GetString(Data, 0, i >= 0 ? i : Data.Length);
            }
            else
            {
                return Encoding.UTF8.GetString(Data);
            }
        }
    }
}


