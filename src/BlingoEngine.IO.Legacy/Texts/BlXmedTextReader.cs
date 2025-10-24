using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Texts.Data;
using BlingoEngine.IO.Legacy.Texts.Data.Pre10;
using BlingoEngine.IO.Legacy.Tools;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.IO.Legacy.Texts
{

    /// <summary>XMED reader.</summary>
    public sealed class BlXmedTextReader
    {
        private const int _defaultDirectorVersion = 13;
        private const int _legacyRichTextMaxVersion = 10;
        
        private ILogger _logger;

        public BlXmedTextReader( ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>Read from byte[] (modern v13 default).</summary>
        public XmedDocument Read(byte[] buffer)
        {
            return Read(buffer, _defaultDirectorVersion);
        }

        /// <summary>Read from byte[] with explicit Director version.</summary>
        public XmedDocument Read(byte[] buffer, int directorVersion)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            using var ms = new MemoryStream(buffer, writable: false);
            return Read(ms, directorVersion);
        }
        /// <summary>Read from stream (modern v13 default).</summary>
        public XmedDocument Read(Stream stream)
        {
            return Read(stream, _defaultDirectorVersion);
        }

        /// <summary>Read from stream with explicit Director version.</summary>
        public XmedDocument Read(Stream stream, int directorVersion)
        {
            ArgumentNullException.ThrowIfNull(stream);
            var buf = ReadAllBytes(stream);
            return ShouldUseLegacyRichText(directorVersion) ? ReadLegacyRichText(buf, directorVersion) : ReadModernXmed(buf, directorVersion);
        }

        private static bool ShouldUseLegacyRichText(int directorVersion) =>
            directorVersion > 0 && directorVersion <= _legacyRichTextMaxVersion;

        /// <summary>Modern XMED (v13+) parse.</summary>
        private XmedDocument ReadModernXmed(byte[] buffer, int directorVersion)
        {
            var tokenizer = new BlXmedTokenizer();
            var (tokens, lastNumbers) = tokenizer.Tokenize(buffer);
            var parser = new BlXmedTokenParser(_logger, buffer, tokens, lastNumbers);
            var doc = parser.Parse(directorVersion);
            doc.FillParagraphTexts();
            return doc;
        }



        #region Read legacy


        /// <summary>Legacy (≤10) rich text parse.</summary>
        private XmedDocument ReadLegacyRichText(byte[] buffer, int directorVersion)
        {
            if (buffer.Length < 34) throw new InvalidDataException("Rich text header too small.");

            using var memory = new MemoryStream(buffer, writable: false);
            var reader = new BlStreamReader(memory) { Endianness = BlEndianness.BigEndian };

            var meta = new XmedRichTextMetadata
            {
                InitialRect = ReadLegacyRect(reader),
                BoundingRect = ReadLegacyRect(reader),
                AntialiasFlag = reader.ReadByte(),
                CropFlags = reader.ReadByte(),
                ScrollPosition = reader.ReadUInt16(),
                AntialiasFontSize = reader.ReadUInt16(),
                DisplayHeight = reader.ReadUInt16()
            };

            _ = reader.ReadByte(); // pad
            var foreR = reader.ReadByte();
            var foreG = reader.ReadByte();
            var foreB = reader.ReadByte();
            meta.ForegroundColor = new BlLegacyColor(foreR, foreG, foreB);

            var bgR = (byte)(reader.ReadUInt16() >> 8);
            var bgG = (byte)(reader.ReadUInt16() >> 8);
            var bgB = (byte)(reader.ReadUInt16() >> 8);
            meta.BackgroundColor = new BlLegacyColor(bgR, bgG, bgB);

            return new XmedDocument { DirectorVersion = directorVersion, RichText = meta };
        }

        private static XmedRect ReadLegacyRect(BlStreamReader reader)
        {
            return new XmedRect
            {
                Top = reader.ReadInt16(),
                Left = reader.ReadInt16(),
                Bottom = reader.ReadInt16(),
                Right = reader.ReadInt16()
            };
        }
        #endregion


        private static byte[] ReadAllBytes(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (!stream.CanSeek)
            {
                using var memory = new MemoryStream();
                stream.CopyTo(memory);
                memory.Position = 0;
                var reader = new BlStreamReader(memory);
                var len = (int)reader.Length;
                var buffer = new byte[len];
                reader.ReadExactly(buffer);
                return buffer;
            }

            var sreader = new BlStreamReader(stream);
            long saved = sreader.Position;
            sreader.Position = 0;
            var size = (int)sreader.Length;
            var result = new byte[size];
            sreader.ReadExactly(result);
            sreader.Position = saved;
            return result;
        }



    }
}
