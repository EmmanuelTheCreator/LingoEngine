using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectorRays.Common;

public static class StreamAnnotationMarkdownWriter
{
    public static string WriteMarkdown(StreamAnnotatorDecorator annotator, byte[] data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("| Offset (hex) | Bytes | ASCII | Description | Keys |");
        sb.AppendLine("|--------------|-------|--------|-------------|------|");

        var annotations = annotator.Annotations.OrderBy(a => a.Address).ToList();
        long current = annotator.StreamOffsetBase;
        foreach (var annotation in annotations)
        {
            if (annotation.Address > current)
            {
                int unknownLength = (int)(annotation.Address - current);
                string hex = string.Join(" ", data.Skip((int)current).Take(unknownLength).Select(_ => "??"));
                string ascii = new string('?', unknownLength);
                sb.AppendLine($"| 0x{current:X4} | `{hex}` | `{ascii}` | *(Unknown bytes)* | |");
                current = annotation.Address;
            }

            string hexBytes = string.Join(" ", data.Skip((int)annotation.Address).Take(annotation.Length).Select(b => b.ToString("X2")));
            string ascii = Encoding.ASCII.GetString(data, (int)annotation.Address, annotation.Length)
                .Select(c => char.IsControl(c) ? '.' : c).Aggregate("", (a, b) => a + b);

            string keys = string.Join(", ", annotation.Keys.Select(k => $"{k.Key}:{k.Value}"));
            sb.AppendLine($"| 0x{annotation.Address:X4} | `{hexBytes}` | `{ascii}` | {annotation.Description} | {keys} |");

            current = annotation.Address + annotation.Length;
        }

        if (current < annotator.StreamOffsetBase + data.Length)
        {
            int unknownLength = data.Length - (int)(current - annotator.StreamOffsetBase);
            string hex = string.Join(" ", data.Skip((int)current).Take(unknownLength).Select(_ => "??"));
            string ascii = new string('?', unknownLength);
            sb.AppendLine($"| 0x{current:X4} | `{hex}` | `{ascii}` | *(Unknown bytes)* | |");
        }

        return sb.ToString();
    }
}
