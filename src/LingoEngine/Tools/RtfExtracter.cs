using LingoEngine.Primitives;
using System.Text;
using System.Text.RegularExpressions;

namespace LingoEngine.Tools
{
    public static class RtfExtracter
    {
        public record RtfBasicInfo
        {
            public string? FontName { get; set; } = "";
            public int Size{ get; set; }
            public LingoColor? Color{ get; set; }
            public string Text { get; set; } = "";
        }
        public static RtfBasicInfo? Parse(string rtfContent)
        {
            string? fullText = null;
            // Match all inner blocks with font/color and text (non-nested, single-line format)
            var blockMatches = Regex.Matches(
                rtfContent,
                @"{[^{}]*\\f(?<f>\d+)[^{}]*?(?:\\fs(?<fs>\d+))?[^{}]*?\\cf(?<cf>\d+)[^{}]*?\s(?<text>[^\{\}\\]+)}",
                RegexOptions.Singleline
            );
            //var sb = new StringBuilder();
            Match match;
            // If no match, fall back to a more flexible version that handles multi-line and \plain
            if (blockMatches.Count == 0)
            {
                blockMatches = Regex.Matches(
    rtfContent,
    @"{[^{}]*?(?:\\plain)?[^{}]*?\\f(?<f>\d+)[^{}]*?(?:\\fs(?<fs>\d+))?[^{}]*?\\cf(?<cf>\d+)[^{}]*?(?<text>(?:\\.|[^{}\\])+)}",
    RegexOptions.Singleline
);

                if (blockMatches.Count == 0)
                    return null;
                // match = blockMatches[0]; // use the first.
                //foreach (Match match1 in blockMatches)
                //{
                //    sb.AppendLine("FontIndex: " + match1.Groups["f"].Value);
                //    sb.AppendLine("FontSize: " + match1.Groups["fs"].Value);
                //    sb.AppendLine("ColorIndex: " + match1.Groups["cf"].Value);
                //    sb.AppendLine("Text: " + match1.Groups["text"].Value);
                //    sb.AppendLine("----------");
                //}
                fullText = string.Join(Environment.NewLine, blockMatches.Select(m => m.Groups["text"].Value));
            }
           // var ressss = sb.ToString();
            ///else
                match = blockMatches[^1]; // Use last/innermost block

            // Font index from block
            int fontIndex = int.Parse(match.Groups["f"].Value);

            // Font size from block or fallback to global fs
            int fontSizeHalfPoints;
            if (match.Groups["fs"].Success)
            {
                fontSizeHalfPoints = int.Parse(match.Groups["fs"].Value);
            }
            else
            {
                var outerFsMatch = Regex.Match(rtfContent, @"\\fs(?<fs>\d+)");
                fontSizeHalfPoints = outerFsMatch.Success
                    ? int.Parse(outerFsMatch.Groups["fs"].Value)
                    : 24; // default fallback
            }

            // Color index
            int colorIndex = int.Parse(match.Groups["cf"].Value);
            string textContent = fullText?? match.Groups["text"].Value.Trim();

            // --- Font Name Resolution ---
            var fontEntries = Regex.Matches(rtfContent, @"{\\f(?<index>\d+)[^;]*?([^\\;]+);}")
                 .Cast<Match>()
                 .ToDictionary(
                     m => int.Parse(m.Groups["index"].Value),
                     m => {
                         var raw = m.Groups[1].Value.Trim();
                         // Remove known RTF font family prefixes if present (e.g., fnil, fswiss, fmodern, etc.)
                         var cleaned = Regex.Replace(raw, @"^(fnil|fswiss|fmodern|fdecor|fscript|ftech|fbidi)\s*", "", RegexOptions.IgnoreCase);
                         return cleaned;
                     }
                 );

            fontEntries.TryGetValue(fontIndex, out var fontName);

            // --- Color Resolution ---
            var colorTableMatch = Regex.Match(rtfContent, @"\\colortbl(?<colortbl>[^}]+)}");
            var colorEntries = Regex.Matches(colorTableMatch.Groups["colortbl"].Value, @"\\red(?<r>\d+)\\green(?<g>\d+)\\blue(?<b>\d+);");

            LingoColor? colorL = null;
            if (colorIndex - 1 >= 0 && colorIndex - 1 < colorEntries.Count)
            {
                var colorEntry = colorEntries[colorIndex - 1];
                var r = byte.Parse(colorEntry.Groups["r"].Value);
                var g = byte.Parse(colorEntry.Groups["g"].Value);
                var b = byte.Parse(colorEntry.Groups["b"].Value);
                colorL = new LingoColor(-1, r, g, b);
            }

            // --- Return result ---
            float pointSize = fontSizeHalfPoints / 2f;


            var result = new RtfBasicInfo
            {
                FontName =  !string.IsNullOrWhiteSpace(fontName)? fontName.TrimEnd('*').Trim(): null,
                Color = colorL,
                Size = Convert.ToInt32(pointSize),
                Text = textContent
            };
            return result;
        }

    }
}
