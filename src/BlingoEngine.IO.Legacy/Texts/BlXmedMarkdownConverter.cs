using System.Text;
using BlingoEngine.IO.Legacy.Core;

namespace BlingoEngine.IO.Legacy.Texts;

public static class BlXmedMarkdownConverter
{
    public static string ToCustomMarkdown(XmedDocument doc)
    {
        var sb = new StringBuilder();

        if (doc.Styles.Count > 0)
        {
            var align = doc.Styles[0].Alignment switch
            {
                XmedAlignment.Left => "left",
                XmedAlignment.Right => "right",
                _ => "center"
            };
            sb.Append("{{ALIGN:").Append(align).Append("}}");
        }

        string? currentFont = null;
        int? currentSize = 0;
        BlLegacyColor currentColor = default;
        bool hasColor = false;

        foreach (var run in doc.Runs)
        {
            if (string.IsNullOrEmpty(run.Text))
                continue;

            if (!string.IsNullOrEmpty(run.FontName) && run.FontName != currentFont)
            {
                sb.Append("{{FONT-FAMILY:").Append(run.FontName).Append("}}");
                currentFont = run.FontName;
            }

            if (run.FontSize > 0 && run.FontSize != currentSize)
            {
                sb.Append("{{FONT-SIZE:").Append(run.FontSize).Append("}}");
                currentSize = run.FontSize;
            }

            if ((!hasColor) || run.ForeColor.R != currentColor.R || run.ForeColor.G != currentColor.G || run.ForeColor.B != currentColor.B)
            {
                sb.Append("{{COLOR:").Append(run.ForeColor.ToHex()).Append("}}");
                currentColor = run.ForeColor;
                hasColor = true;
            }

            var text = EscapeMarkdown(run.Text);

            if (run.Bold) sb.Append("**");
            if (run.Italic) sb.Append("*");
            if (run.Underline) sb.Append("__");
            sb.Append(text);
            if (run.Underline) sb.Append("__");
            if (run.Italic) sb.Append("*");
            if (run.Bold) sb.Append("**");
        }

        return sb.ToString();
    }

    private static string EscapeMarkdown(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace("*", "\\*")
            .Replace("_", "\\_")
            .Replace("{", "\\{")
            .Replace("}", "\\}");
    }
}


