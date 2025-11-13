using System.Buffers.Binary;
using System.Text;

namespace BlingoEngine.IO.Legacy.Texts
{
    public static class XmedExtensions
    {
     
        /// <summary>
        /// Converts the supplied <c>STXT</c> payload into a UTF-8 string while tolerating truncated or
        /// padded buffers that appear in early projector versions.
        /// </summary>
        /// <param name="data">Raw bytes copied from the <c>STXT</c> resource.</param>
        /// <returns>The decoded string with trailing null characters trimmed.</returns>
        public static string DecodeSTXT(this ReadOnlySpan<byte> data)
        {
            if (data.Length >= 2)
            {
                var declaredLength = BinaryPrimitives.ReadUInt16BigEndian(data);
                if (declaredLength > 0 && declaredLength <= data.Length - 2)
                    return Encoding.UTF8.GetString(data.Slice(2, declaredLength)).TrimEnd('\0');
            }

            return Encoding.UTF8.GetString(data).TrimEnd('\0');
        }
       
    }
}
