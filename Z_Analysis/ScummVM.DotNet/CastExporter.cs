using System;
using System.IO;
using Director.IO;
using Director.Members;

namespace Director.Tools
{
    /// <summary>
    /// Utility class that exports raw cast member data from a Director file.
    /// The implementation is intentionally simple and mirrors the behaviour
    /// of the Python script 'director-files-extract_shock.py'. Each cast member
    /// found in the CASt resource is written to a separate file.
    /// </summary>
    public static class CastExporter
    {
        /// <summary>
        /// Export cast member data from <paramref name="filePath"/> into
        /// <paramref name="outputDirectory"/>.
        /// </summary>
        public static void Export(string filePath, string outputDirectory)
        {
            var loader = new ArchiveFileLoader();
            var data = loader.ReadFile(filePath);
            if (data.CASt == null)
            {
                Console.WriteLine("No CASt resource found.");
                return;
            }

            // Use Export/[castName]/ as root folder
            var castName = Path.GetFileNameWithoutExtension(filePath);
            var outDir = Path.Combine(outputDirectory, castName);
            Directory.CreateDirectory(outDir);

            var csvPath = Path.Combine(outDir, "Members.csv");
            using var csv = new StreamWriter(csvPath);
            csv.WriteLine("Number,Type,Name,Registration Point,Filename");

            foreach (var member in data.CASt.MembersData)
            {
                CastType castType = Enum.IsDefined(typeof(CastType), member.Type)
                    ? (CastType)member.Type : CastType.Any;
                string typeName = castType != CastType.Any
                    ? castType.ToString()
                    : $"type{member.Type}";

                string fileName;
                string path;

                switch (castType)
                {
                    case CastType.Script:
                        fileName = $"{member.Id:D4}_{typeName}.ls";
                        path = Path.Combine(outDir, fileName);
                        File.WriteAllText(path, System.Text.Encoding.UTF8.GetString(member.Data));
                        break;

                    case CastType.Text:
                    case CastType.RichText:
                        fileName = $"{member.Id:D4}_{typeName}.rtf";
                        path = Path.Combine(outDir, fileName);
                        string rtf = BuildRtf(member.Data);
                        File.WriteAllText(path, rtf);
                        break;

                    case CastType.Bitmap:
                    case CastType.Paint:
                        bool hasPngSig = member.Data.Length > 4 && member.Data[0] == 0x89 && member.Data[1] == 0x50 && member.Data[2] == 0x4E && member.Data[3] == 0x47;
                        bool hasBmpSig = member.Data.Length > 2 && member.Data[0] == (byte)'B' && member.Data[1] == (byte)'M';
                        bool hasAlpha = false;
                        if (!hasPngSig && member.Data.Length % 4 == 0)
                        {
                            for (int i = 3; i < member.Data.Length; i += 4)
                            {
                                if (member.Data[i] != 0xFF)
                                {
                                    hasAlpha = true;
                                    break;
                                }
                            }
                        }

                        string ext = hasPngSig || hasAlpha ? ".png" : ".bmp";
                        fileName = $"{member.Id:D4}_{typeName}{ext}";
                        path = Path.Combine(outDir, fileName);
                        File.WriteAllBytes(path, member.Data);
                        break;

                    default:
                        fileName = $"{member.Id:D4}_{typeName}.bin";
                        path = Path.Combine(outDir, fileName);
                        File.WriteAllBytes(path, member.Data);
                        break;
                }

                csv.WriteLine($"{member.Id},{typeName},,\"(0, 0)\",{fileName}");
            }
        }

        private static string BuildRtf(byte[] data)
        {
            try
            {
                using var ms = new MemoryStream(data);
                using var stream = new SeekableReadStreamEndian(ms, true);
                var dummyCast = new Cast();
                var textMember = new TextCastMember(dummyCast, 0, stream);

                var fore = textMember.ForeColor.ToHex();
                var back = textMember.BackColor.ToHex();
                var fontSize = textMember.FontSize * 2;
                string text = textMember.Text.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}").Replace("\n", "\\par ");

                var sb = new System.Text.StringBuilder();
                sb.Append("{\\rtf1\\ansi{\\fonttbl{\\f0 " + (textMember.FontName ?? "") + ";}}\n");
                sb.Append("{\\colortbl ;" + fore + ";" + back + ";}\n");
                sb.Append("\\f0 \\fs" + fontSize + " ");
                sb.Append(text);
                sb.Append("}");
                return sb.ToString();
            }
            catch
            {
                // Fallback: treat as plain text
                return System.Text.Encoding.UTF8.GetString(data);
            }
        }
    }
}
