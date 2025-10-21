using System;
using System.Collections.Generic;
using System.Linq;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Texts.Data;
using Microsoft.Extensions.Logging;

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
                ReadBaseValues(styleGroup, descriptor, styleId);
                ReadColors(styleGroup, descriptor);
                ReadFlags(styleGroup, descriptor);
                styleId++;
            }

            _ = GetOrCreateStyle(0);
        }

        public void FinalizeStyles(XmedDocument document)
        {
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
            document.RunMap.Clear();
            document.Runs.Clear();

            if (document.TextLength <= 0)
                return;

            IReadOnlyList<XmedSliceBuilder.Slice> effectiveSlices = slices ?? Array.Empty<XmedSliceBuilder.Slice>();
            if (effectiveSlices.Count == 0)
                effectiveSlices = new[] { new XmedSliceBuilder.Slice(0, document.TextLength, 0, document.Text) };

            foreach (var slice in effectiveSlices)
            {
                if (slice.Length <= 0)
                    continue;

                int styleId = Math.Max(0, slice.Value);
                var descriptor = GetOrCreateStyle(styleId);
                string runText = slice.Text ?? string.Empty;

                document.RunMap.Add(new XmedRunMapEntry(0, 0,
                    (ushort)Math.Clamp(slice.Length, 0, ushort.MaxValue),
                    0,
                    (ushort)Math.Clamp(styleId, 0, ushort.MaxValue),
                    slice.Start));

                document.Runs.Add(new XmedTextRun
                {
                    Start = slice.Start,
                    Length = slice.Length,
                    Text = runText,
                    FontName = descriptor.FontName,
                    FontSize = descriptor.FontSize,
                    Bold = descriptor.Bold,
                    Italic = descriptor.Italic,
                    Underline = descriptor.Underline,
                    ForeColor = descriptor.Color,
                    BackgroundColor = descriptor.BackgroundColor
                });
            }

            if (document.Runs.Count == 0)
            {
                var baseStyle = GetOrCreateStyle(0);
                document.RunMap.Add(new XmedRunMapEntry(0, 0,
                    (ushort)Math.Clamp(document.TextLength, 0, ushort.MaxValue),
                    0,
                    0,
                    0));

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
                    ForeColor = baseStyle.Color,
                    BackgroundColor = baseStyle.BackgroundColor
                });
            }
        }

        private void ReadBaseValues(XmedTokenGroup styleGroup, XmedStyleDescriptor descriptor, int styleId)
        {
            if (styleGroup.Items.Count == 0)
                return;

            if (styleGroup.Items[0] is not XmedTokenGroup baseGroup)
                return;

            int parentId = baseGroup.ReadNumeric(0);
            if (parentId >= 0 && parentId != styleId && parentId < 256)
                _styleParents[styleId] = parentId;

            int fontSize = baseGroup.ReadNumeric(3);
            if (fontSize > 0)
                descriptor.FontSize = fontSize;
        }

        private void ReadColors(XmedTokenGroup styleGroup, XmedStyleDescriptor descriptor)
        {
            if (TryReadColors(styleGroup.Items.ElementAtOrDefault(2) as XmedTokenGroup, out var foreground, out var background, out var hasBackground))
            {
                descriptor.Color = foreground;
                if (hasBackground)
                {
                    descriptor.BackgroundColor = background;
                    descriptor.HasBackgroundColor = true;
                }
                return;
            }

            if (TryReadColors(styleGroup.Items.ElementAtOrDefault(0) as XmedTokenGroup, out foreground, out background, out hasBackground))
            {
                descriptor.Color = foreground;
                if (hasBackground)
                {
                    descriptor.BackgroundColor = background;
                    descriptor.HasBackgroundColor = true;
                }
            }
        }

        private void ReadFlags(XmedTokenGroup styleGroup, XmedStyleDescriptor descriptor)
        {
            foreach (var container in styleGroup.Items.OfType<XmedTokenGroup>())
            {
                foreach (var c2 in container.Items.OfType<XmedTokenGroup>())
                {
                    if (c2.TypeValue != 0x07)
                        continue;

                    bool bold = c2.ReadNumeric(0) != 0;
                    bool italic = c2.ReadNumeric(1) != 0;
                    bool underline = c2.ReadNumeric(2) != 0;
                    descriptor.ApplyStyleFlag(XmedStyleDescriptor.XmedStyleFlags.Bold, bold);
                    descriptor.ApplyStyleFlag(XmedStyleDescriptor.XmedStyleFlags.Italic, italic);
                    descriptor.ApplyStyleFlag(XmedStyleDescriptor.XmedStyleFlags.Underline, underline);
                    return;
                }
            }
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

        private static bool TryReadColors(XmedTokenGroup? group, out BlLegacyColor foreground, out BlLegacyColor background, out bool hasBackground)
        {
            foreground = default;
            background = default;
            hasBackground = false;
            if (group == null)
                return false;

            int itemCount = group.Items.Count;
            if (itemCount < 3)
                return false;

            byte fr = NormalizeColor(group.ReadNumeric(0));
            byte fg = NormalizeColor(group.ReadNumeric(1));
            byte fb = NormalizeColor(group.ReadNumeric(2));
            foreground = new BlLegacyColor(fr, fg, fb);

            if (itemCount > 6)
            {
                byte br = NormalizeColor(group.ReadNumeric(4));
                byte bg = NormalizeColor(group.ReadNumeric(5));
                byte bb = NormalizeColor(group.ReadNumeric(6));
                background = new BlLegacyColor(br, bg, bb);
                hasBackground = true;
            }

            return true;
        }

        private static byte NormalizeColor(int value)
        {
            if (value <= 0)
                return 0;
            int component = value >> 8;
            return (byte)Math.Clamp(component, 0, 255);
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

        public bool TryGetStyle(int styleId, out XmedStyleDescriptor? descriptor)
        {
            if (_stylesById.TryGetValue(styleId, out var existing))
            {
                descriptor = existing;
                return true;
            }

            descriptor = null;
            return false;
        }
    }
}
