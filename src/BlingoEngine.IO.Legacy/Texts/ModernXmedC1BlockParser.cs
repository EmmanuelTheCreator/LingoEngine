using System;
using System.Buffers.Binary;
using System.Text;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal static class ModernXmedC1BlockParser
    {
        public static bool TryParse(byte[] buffer, int directorVersion, out XmedDocument document)
        {
            document = new XmedDocument { DirectorVersion = directorVersion };
            if (buffer == null || buffer.Length == 0)
            {
                return false;
            }

            var demx = Encoding.ASCII.GetBytes("DEMX");
            int chunkStart = buffer.IndexOfSequence(0, demx);
            if (chunkStart < 0 || chunkStart + demx.Length > buffer.Length)
            {
                return false;
            }

            document.Width = buffer.ReadUInt32Safe(chunkStart + 0x18);
            document.LineSpacing = buffer.ReadUInt32Safe(chunkStart + 0x3C);
            document.TextLength = (int)Math.Min(int.MaxValue, buffer.ReadUInt32Safe(chunkStart + 0x4C));
            document.DirectorVersion = directorVersion;

            ushort fontSize = 0;
            if (chunkStart >= 0x14 && chunkStart - 0x14 + 2 <= buffer.Length)
            {
                fontSize = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(chunkStart - 0x14, 2));
            }

            if (fontSize == 0 && chunkStart + 0x42 <= buffer.Length)
            {
                fontSize = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(chunkStart + 0x40, 2));
            }

            byte styleFlags = chunkStart + 0x1C < buffer.Length ? buffer[chunkStart + 0x1C] : (byte)0;
            byte alignFlags = chunkStart + 0x1D < buffer.Length ? buffer[chunkStart + 0x1D] : (byte)0;

            var baseStyle = new XmedStyleDescriptor
            {
                FontSize = fontSize,
                AlignmentRaw = alignFlags,
                StyleFlags = styleFlags
            };

            baseStyle.ApplyStyleFlags(styleFlags);
            baseStyle.ApplyAlignmentFlags(alignFlags);
            document.Styles.Add(baseStyle);

            int bodyOffset = chunkStart + demx.Length;
            if (bodyOffset >= buffer.Length)
            {
                document.Text = string.Empty;
                return true;
            }

            int bodyLength = buffer.Length - bodyOffset;
            var textBuilder = new StringBuilder();
            var activeColor = new BlLegacyColor(baseStyle.ColorIndex, baseStyle.ColorIndex, baseStyle.ColorIndex);
            bool colorFromBlock = false;
            ushort blockFontSize = fontSize;
            var blockAlignment = baseStyle.Alignment;
            bool blockWrapOff = baseStyle.WrapOff;
            bool blockHasTabs = baseStyle.HasTabs;
            XmedStyleDescriptor? currentStyle = null;

            var processor = new ModernXmedSpanProcessor(buffer, bodyOffset, bodyLength, document, textBuilder, baseStyle,
                currentStyle, activeColor, colorFromBlock, blockFontSize, blockAlignment, blockWrapOff, blockHasTabs);

            bool parsed = processor.Process();

            currentStyle = processor.CurrentStyle;
            activeColor = processor.ActiveColor;
            colorFromBlock = processor.ColorFromBlock;
            blockFontSize = processor.BlockFontSize;
            blockAlignment = processor.BlockAlignment;
            blockWrapOff = processor.BlockWrapOff;
            blockHasTabs = processor.BlockHasTabs;

            if (!parsed && document.Runs.Count == 0 && textBuilder.Length == 0)
            {
                return false;
            }

            if (document.Runs.Count > 0 && string.IsNullOrEmpty(document.Runs[0].FontName))
            {
                EnsureInitialRunDefaults(document, baseStyle, currentStyle, activeColor, colorFromBlock, blockFontSize);
            }

            document.Text = textBuilder.ToString();
            document.TextLength = document.Text.Length;
            document.DirectorVersion = directorVersion;
            document.Runs.MergeAdjacentEqualStyleRuns();
            return true;
        }

        private static void EnsureInitialRunDefaults(XmedDocument document, XmedStyleDescriptor baseStyle,
            XmedStyleDescriptor? currentStyle, BlLegacyColor activeColor, bool colorFromBlock, ushort blockFontSize)
        {
            var run = document.Runs[0];
            if (string.IsNullOrEmpty(run.FontName))
            {
                if (currentStyle != null && !string.IsNullOrEmpty(currentStyle.FontName))
                {
                    run.FontName = currentStyle.FontName;
                }
                else if (!string.IsNullOrEmpty(baseStyle.FontName))
                {
                    run.FontName = baseStyle.FontName;
                }
            }

            run.ForeColor = currentStyle.ResolveRunColor(baseStyle, activeColor, colorFromBlock);
            run.FontSize = blockFontSize.ResolveFontSize(currentStyle, baseStyle.FontSize);
        }
    }
}
