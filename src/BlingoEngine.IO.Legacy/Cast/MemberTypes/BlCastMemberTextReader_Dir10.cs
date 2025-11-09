using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Tools;
using System.Numerics;

namespace BlingoEngine.IO.Legacy.Cast.MemberTypes
{
    internal class BlCastMemberTextReader_Dir10
    {
        public BlCastRawMemberText Read(byte[] specificBytes)
        {
            using var specificStream = new MemoryStream(specificBytes, writable: false);
            var reader = new BlStreamReader(specificStream)
            {
                Endianness = BlEndianness.BigEndian
            };

            if (specificBytes.Length < 8)
                throw new InvalidDataException("Specific block does not contain the text header.");

            var typeLength = reader.ReadInt32();
            var type = reader.ReadAsciiString(typeLength);
            var specificDataLength = reader.ReadInt32();

            var isEditable = ReadBoolean(reader);
            var framing = (BlRawTextFraming)reader.ReadInt32();
            var tabsEnabled = ReadBoolean(reader);
            var dtdEnabled = ReadBoolean(reader);
            reader.Skip(4);

            var isAntialiasEnabled = ReadBoolean(reader);
            var antialiasMode = reader.ReadInt32();
            var antialiasLargerThan = reader.ReadInt32();
            reader.Skip(4);

            var kerningLargerThan = reader.ReadInt32();
            reader.Skip(4);

            var isKerningEnabled = ReadBoolean(reader);
            var kerningMode = reader.ReadInt32();
            var useHyperlinkStyles = ReadBoolean(reader);

            reader.Skip(4 * 3);

            var preRenderInk = (BlRawTextPreRenderInk)reader.ReadInt32();
            var savePreRenderBitmap = ReadBoolean(reader);

            var shaderTag = reader.ReadAsciiString(4);
            var shaderDataLength = reader.ReadInt32();
            var faceFlags = reader.ReadInt32();
            var tunnelDepth = ReadFixed1616(reader);
            var isBevelEnabled = ReadBoolean(reader);
            var bevelAmount = ReadFixed1616(reader);
            var bevelEdge = (BlRawTextBevelEdge)reader.ReadInt32();
            var smoothness = reader.ReadInt32();
            var lightSetting = (BlRawTextDirectionalLight)reader.ReadInt32();
            var shaderTexture = (BlRawTextShaderTexture)reader.ReadInt32();
            var diffuseIndex = reader.ReadInt32();
            var specularIndex = reader.ReadInt32();
            var reflectivity = reader.ReadInt32();

            var directionalColor = ReadColor(reader);
            var ambientColor = ReadColor(reader);
            var backgroundColor = ReadColor(reader);

            var cameraPosition = new Vector3(ReadSingle(reader), ReadSingle(reader), ReadSingle(reader));
            var cameraDistance = ReadFixed1616(reader);
            var cameraRotation = new Vector3(ReadSingle(reader), ReadSingle(reader), ReadSingle(reader));
            var cameraFocalLength = ReadFixed1616(reader);
            var textureName = reader.ReadCString();

            return new BlCastRawMemberText(
                type,
                specificDataLength,
                isEditable,
                framing,
                tabsEnabled,
                dtdEnabled,
                isAntialiasEnabled,
                antialiasMode,
                antialiasLargerThan,
                kerningLargerThan,
                isKerningEnabled,
                kerningMode,
                useHyperlinkStyles,
                preRenderInk,
                savePreRenderBitmap,
                shaderTag,
                shaderDataLength,
                faceFlags,
                tunnelDepth,
                isBevelEnabled,
                bevelAmount,
                bevelEdge,
                smoothness,
                lightSetting,
                shaderTexture,
                diffuseIndex,
                specularIndex,
                reflectivity,
                directionalColor,
                ambientColor,
                backgroundColor,
                cameraPosition,
                cameraDistance,
                cameraRotation,
                cameraFocalLength,
                textureName);
        }

        private static bool ReadBoolean(BlStreamReader reader) => reader.ReadInt32() != 0;

        private static double ReadFixed1616(BlStreamReader reader)
        {
            var raw = reader.ReadUInt32();
            return raw / 65536d;
        }

        private static BlLegacyColor ReadColor(BlStreamReader reader)
        {
            var raw = reader.ReadUInt32();
            var r = (byte)(raw >> 24);
            var g = (byte)(raw >> 16);
            var b = (byte)(raw >> 8);

            var rgb565 = PackRgb565(r, g, b);
            return UnpackRgb565(rgb565);
        }

        private static ushort PackRgb565(byte r, byte g, byte b)
        {
            return (ushort)(((r & 0xF8) << 8) | ((g & 0xFC) << 3) | (b >> 3));
        }

        private static BlLegacyColor UnpackRgb565(ushort value)
        {
            var red = (byte)((value >> 11) & 0x1F);
            var green = (byte)((value >> 5) & 0x3F);
            var blue = (byte)(value & 0x1F);

            red = (byte)((red * 255 + 15) / 31);
            green = (byte)((green * 255 + 31) / 63);
            blue = (byte)((blue * 255 + 15) / 31);

            return new BlLegacyColor(red, green, blue);
        }

        private static float ReadSingle(BlStreamReader reader)
        {
            var raw = reader.ReadUInt32();
            return BitConverter.Int32BitsToSingle(unchecked((int)raw));
        }
    }
}
