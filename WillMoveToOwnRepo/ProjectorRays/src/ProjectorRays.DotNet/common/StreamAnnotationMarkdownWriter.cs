using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectorRays.Common;

public static class StreamAnnotationMarkdownWriter
{
    public static string WriteMarkdown(RayStreamAnnotatorDecorator annotator, byte[] data)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(annotator.Name))
        {
            sb.AppendLine($"# Stream Annotations for: {annotator.Name}");
            sb.AppendLine("------------------------------------------------");
        }
        sb.AppendLine("| Offset (hex) | Bytes | ASCII | Description | Keys |");
        sb.AppendLine("|--------------|-------|--------|-------------|------|");

        var annotations = annotator.Annotations.OrderBy(a => a.Address).ToList();
        long baseOffset = annotator.StreamOffsetBase;
        long current = baseOffset;

        const int collapseThreshold = 32;
        string ascii = "";
        foreach (var annotation in annotations)
        {
            long gapStart = current;
            long gapEnd = annotation.Address;

            if (gapEnd > gapStart)
            {
                int gapLength = (int)(gapEnd - gapStart);
                int relativeOffset = (int)(gapStart - baseOffset);

                if (gapLength > collapseThreshold)
                {
                    sb.AppendLine($"| 0x{gapStart:X4} | ... | ... | *(Unknown: {gapLength} bytes skipped)* | |");
                }
                else
                {
                    string hex = string.Join(" ", data.Skip(relativeOffset).Take(gapLength).Select(_ => "??"));
                    ascii = new string('?', gapLength);
                    sb.AppendLine($"| 0x{gapStart:X4} | `{hex}` | `{ascii}` | *(Unknown bytes)* | |");
                }

                current = gapEnd;
            }

            int relativeAnnotationOffset = (int)(annotation.Address - baseOffset);
            string hexBytes = string.Join(" ", data.Skip(relativeAnnotationOffset).Take(annotation.Length).Select(b => b.ToString("X2")));

            try
            {
                ascii = new string(Encoding.ASCII
                    .GetString(data, relativeAnnotationOffset, annotation.Length)
                    .Select(c => char.IsControl(c) ? '.' : c)
                    .ToArray());
            }
            catch
            {
                ascii = new string('?', annotation.Length);
            }

            string keys = string.Join(", ", annotation.Keys.Select(k => $"{k.Key}:{k.Value}"));
            sb.AppendLine($"| 0x{annotation.Address:X4} | `{hexBytes}` | `{ascii}` | {annotation.Description} | {keys} |");

            current = annotation.Address + annotation.Length;
        }

        // Final tail gap
        long tailStart = current;
        long tailLength = baseOffset + data.Length - current;
        int tailRelative = (int)(tailStart - baseOffset);

        if (tailLength > 0)
        {
            if (tailLength > collapseThreshold)
            {
                sb.AppendLine($"| 0x{tailStart:X4} | ... | ... | *(Unknown: {tailLength} bytes skipped)* | |");
            }
            else
            {
                string hex = string.Join(" ", data.Skip(tailRelative).Take((int)tailLength).Select(_ => "??"));
                ascii = new string('?', (int)tailLength);
                sb.AppendLine($"| 0x{tailStart:X4} | `{hex}` | `{ascii}` | *(Unknown bytes)* | |");
            }
        }

        return sb.ToString();
    }

}
