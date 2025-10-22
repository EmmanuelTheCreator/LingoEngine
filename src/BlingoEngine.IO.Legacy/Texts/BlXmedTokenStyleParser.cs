using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Texts.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class BlXmedTokenStyleParser
    {
        private readonly XmedDocument _document;
        private readonly Dictionary<int, XmedStyleDescriptor> _stylesById = new();
        private readonly Dictionary<int, int> _styleParents = new();

        public BlXmedTokenStyleParser(ILogger logger, XmedDocument document)
        {
            _ = logger;
            _document = document;
        }

        public void Reset()
        {
            _stylesById.Clear();
            _styleParents.Clear();
        }

        public void LoadStyles(XmedTokenGroup? stylesGroup)
        {
            Reset();

            if (stylesGroup == null)
                return;

            int styleId = 0;
            foreach (var item in stylesGroup.Items)
            {
                if (item is not XmedTokenGroup styleGroup)
                {
                    styleId++;
                    continue;
                }

                var descriptor = GetOrCreateStyle(styleId);
                ReadBaseValues0(styleGroup, descriptor, styleId);
                ReadColors(styleGroup, descriptor);
                ReadBaseValues3(styleGroup, descriptor);
                
                styleId++;
            }

            _ = GetOrCreateStyle(0);
        }

        public void FinalizeStyles(XmedDocument document)
        {
            foreach (var descriptor in _stylesById.Values)
                ApplyFont(descriptor, document.Fonts);

            foreach (var styleId in _styleParents.Keys.ToList())
                ApplyParent(styleId, new HashSet<int>());

            document.Styles.Clear();
            foreach (var descriptor in _stylesById.Values.OrderBy(s => s.StyleId))
                document.Styles.Add(descriptor);

            if (document.Styles.Count == 0)
                document.Styles.Add(GetOrCreateStyle(0));
        }

        public void BuildRuns(XmedDocument document, IReadOnlyList<XmedSliceBuilder.Slice> slices)
        {
            document.Runs.Clear();


            foreach (var slice in slices)
            {
                if (slice.Text!.Length <= 0)
                    continue;

                int styleId = Math.Max(0, slice.Value);
                var descriptor = GetOrCreateStyle(styleId);
                string runText = slice.Text ?? string.Empty;


                document.Runs.Add(new XmedTextRun
                {
                    Start = slice.Start,
                    Length = runText.Length,
                    Text = runText,
                    FontName = descriptor.FontName,
                    FontSize = descriptor.FontSize,
                    Bold = descriptor.Bold,
                    Italic = descriptor.Italic,
                    Underline = descriptor.Underline,
                    ForeColor = descriptor.ForegroundColor,
                    BackgroundColor = descriptor.BackgroundColor
                });
            }

            if (document.Runs.Count == 0)
            {
                var baseStyle = GetOrCreateStyle(0);

                document.Runs.Add(new XmedTextRun
                {
                    Start = 0,
                    Length = document.TextLength,
                    Text = document.Text,
                    FontName = baseStyle.FontName,
                    FontSize = baseStyle.FontSize,
                    Bold = baseStyle.Bold,
                    Italic = baseStyle.Italic,
                    Underline = baseStyle.Underline,
                    ForeColor = baseStyle.ForegroundColor,
                    BackgroundColor = baseStyle.BackgroundColor
                });
            }
        }

        private void ReadBaseValues0(XmedTokenGroup styleGroup, XmedStyleDescriptor descriptor, int styleId)
        {
            int parentId = styleGroup.ReadNumeric(1);
            descriptor.FontDescent = styleGroup.ReadNumeric(4);
            descriptor.FontAscendent = styleGroup.ReadNumeric(5);

            int fontSlot = styleGroup.ReadNumeric(31);
            if (fontSlot > 0)
                descriptor.FontTableIndex = fontSlot;

            if (parentId >= 0 && parentId != styleId && parentId < 256)
                _styleParents[styleId] = parentId;
        }

        private void ReadBaseValues3(XmedTokenGroup styleGroup, XmedStyleDescriptor descriptor)
        {
            int rawFontSize = styleGroup.ReadNumeric(19);
            int fontSize = rawFontSize >> 16;
            if (fontSize > 0)
                descriptor.FontSize = fontSize;

            ReadFlags(styleGroup, descriptor);
        }

        private void ReadFlags(XmedTokenGroup styleGroup, XmedStyleDescriptor descriptor)
        {
            bool bold = styleGroup.ReadNumeric(40) != 0;
            bool italic = styleGroup.ReadNumeric(41) != 0;
            bool underline = styleGroup.ReadNumeric(42) != 0;

            descriptor.ApplyStyleFlag(XmedStyleDescriptor.XmedStyleFlags.Bold, bold);
            descriptor.ApplyStyleFlag(XmedStyleDescriptor.XmedStyleFlags.Italic, italic);
            descriptor.ApplyStyleFlag(XmedStyleDescriptor.XmedStyleFlags.Underline, underline);
        }

        private void ReadColors(XmedTokenGroup styleGroup, XmedStyleDescriptor descriptor)
        {
            descriptor.ForegroundColor = ReadColor(styleGroup, 10);
            descriptor.BackgroundColor = ReadColor(styleGroup, 14);
        }

        private static void ApplyFont(XmedStyleDescriptor descriptor, IReadOnlyList<XmedFontDescriptor> fonts)
        {
            if (descriptor.FontTableIndex is not int fontSlot || fontSlot <= 0)
                return;

            int index = fontSlot - 1;
            if ((uint)index >= fonts.Count)
                return;

            var font = fonts[index];
            string family = font.FamilyName ?? string.Empty;
            string style = font.StyleName ?? string.Empty;

            descriptor.FontName = string.IsNullOrWhiteSpace(style)
                ? family
                : string.Concat(family, " ", style).Trim();
        }

        private static BlLegacyColor ReadColor(XmedTokenGroup group, int offset)
        {
            byte fr = NormalizeColor(group.ReadNumeric(offset + 0));
            byte fg = NormalizeColor(group.ReadNumeric(offset + 1));
            byte fb = NormalizeColor(group.ReadNumeric(offset + 2));
            byte fa = NormalizeColor(group.ReadNumeric(offset + 3));

            return fa > 0
                ? new BlLegacyColor(fr, fg, fb, fa)
                : new BlLegacyColor(fr, fg, fb);
        }

        private static byte NormalizeColor(int value)
        {
            if (value <= 0)
                return 0;

            if (value <= 0xFF)
                return (byte)Math.Clamp(value, 0, 255);

            if (value <= 0xFFFF)
                return (byte)Math.Clamp(value >> 8, 0, 255);

            return (byte)Math.Clamp(value >> 16, 0, 255);
        }


        private void ApplyParent(int styleId, HashSet<int> visited)
        {
            if (!visited.Add(styleId))
                return;

            if (!_styleParents.TryGetValue(styleId, out var parentId))
                return;

            if (!_stylesById.TryGetValue(styleId, out var child) || !_stylesById.TryGetValue(parentId, out var parent))
                return;

            ApplyParent(parentId, visited);
            parent.ApplyStyleInheritanceToChild(child);
        }

       

        public XmedStyleDescriptor GetOrCreateStyle(int styleId)
        {
            if (!_stylesById.TryGetValue(styleId, out var descriptor))
            {
                descriptor = new XmedStyleDescriptor { StyleId = styleId };
                _stylesById[styleId] = descriptor;
            }

            return descriptor;
        }
    }
}
