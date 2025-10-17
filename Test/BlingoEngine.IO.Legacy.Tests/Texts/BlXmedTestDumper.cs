using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Texts;
using BlingoEngine.IO.Legacy.Texts.Data;
using BlingoEngine.IO.Legacy.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Texts
{
    public class BlXmedTestDumper
    {
        [Fact]
        public void GenerateXmedText()
        {

            List<string> folders =
            [
                //"Texts_Fields",
                //"Texts_Fields/Tests2",
                //"Texts_Fields/MemberTests",
                "Texts_Fields/Paragraphs",
            ];
            foreach (var folder in folders)
            {
                var cstFiles = TestContextHarness.GetAllFilesFromFolder(folder, "*.cst");
                foreach (var item in cstFiles)
                {
                    var texts = TestContextHarness.LoadTexts(item);
                    if (texts.Count > 0)
                    {
                        foreach (var textItem in texts)
                        {

                            if (textItem.Format == BlLegacyTextFormatKind.Xmed)
                            {
                                var path = TestContextHarness.GetAssetPath(folder + "/" + Path.GetFileNameWithoutExtension(item) + "_" + textItem.ResourceId + ".xmed.txt");
                                if (File.Exists(path))
                                  continue;

                                File.WriteAllText(path, textItem.Bytes.ToHexString());
                                var pathBin = TestContextHarness.GetAssetPath(folder + "/" + Path.GetFileNameWithoutExtension(item) + "_" + textItem.ResourceId + ".xmed.bin");
                                File.WriteAllBytes(pathBin, textItem.Bytes);
                                var pathLog = TestContextHarness.GetAssetPath(folder + "/" + Path.GetFileNameWithoutExtension(item) + "_" + textItem.ResourceId + ".xmedlog.txt");
                                var tokens = BlXmedTokenizer.Tokenize(textItem.Bytes).Tokens;
                                var log = BlXmedTokenizer.DumpTokensUltraCompact(tokens);
                                File.WriteAllText(pathLog, log);
                            }
                        }
                    }
                }
            }

        }
        [Fact]
        public void DumpLongLog()
        {
            var fileName = "MemberTests/Text_Multi_Style_Size_Color_13.xmed.bin";
            //var fileName = "Text_Hallo_col_blue1_13.xmed.bin";
            //var fileName = "Text_Hallo_col_bordeau_13.xmed.bin";
            //var fileName = "Text_Hallo_col_green_13.xmed.bin";
            //var fileName = "Text_Hallo_col_lightgreen_13.xmed.bin";
            //var fileName = "Text_Hallo_col_orange_13.xmed.bin";
            //var fileName = "Text_Hallo_col_pink_13.xmed.bin";
            //var fileName = "Text_Hallo_col_yellow_13.xmed.bin";
            var item = TestContextHarness.GetAssetPath($"Texts_Fields/{fileName}");
            var log = TestContextHarness.XmedDumpCompactLogGrouped(fileName);
            var resultFileName = item.Replace(".xmed.bin", ".xmedglog.txt");
            File.WriteAllText(resultFileName, log);
        }
        [Fact]
        public void DumpLog()
        {
            var fileName = "MemberTests/Text_Multi_Style_Size_Color_13.xmed.bin";
            //var fileName = "Text_Hallo_col_blue1_13.xmed.bin";
            //var fileName = "Text_Hallo_col_bordeau_13.xmed.bin";
            //var fileName = "Text_Hallo_col_green_13.xmed.bin";
            //var fileName = "Text_Hallo_col_lightgreen_13.xmed.bin";
            //var fileName = "Text_Hallo_col_orange_13.xmed.bin";
            //var fileName = "Text_Hallo_col_pink_13.xmed.bin";
            //var fileName = "Text_Hallo_col_yellow_13.xmed.bin";
            var item = TestContextHarness.GetAssetPath($"Texts_Fields/{fileName}");
            var log = TestContextHarness.XmedDumpCompactLog(fileName);
            var resultFileName = item.Replace(".xmed.bin", ".xmedlog.txt");
            File.WriteAllText(resultFileName, log);
        }


        static readonly Regex RxComp = new(@"^C([1234])\(([0-9A-Fa-f]{2})\)$", RegexOptions.Compiled);
        static readonly Regex RxKV = new(@"^(00\(\d+\)|\d{2}:[0-9A-Fa-f\-]+)$", RegexOptions.Compiled);

        [Fact]
        public void GenerateXmedComponents()
        {
            var root = TestContextHarness.GetAssetPath($"Texts_Fields/");
            var files = Directory.EnumerateFiles(root, "*.xmedlog.txt", SearchOption.AllDirectories);
            var counts = new Dictionary<string, int>();

            foreach (var f in files)
                foreach (var sig in ExtractCompositions(File.ReadLines(f)))
                    counts[sig] = counts.TryGetValue(sig, out var c) ? c + 1 : 1;

            var lines = counts.OrderByDescending(kv => kv.Value)
                              .Select(kv => $"{kv.Value}\t{kv.Key}")
                              .ToList();
            File.WriteAllLines(Path.Combine(root, "Compositions","xmed_compositions.txt"), lines, Encoding.UTF8);
        }

        // Replace ExtractCompositions (method)
        static IEnumerable<string> ExtractCompositions(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                var toks = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < toks.Length; i++)
                {
                    var m = RxComp.Match(toks[i]);
                    if (!m.Success) continue;

                    var cType = m.Groups[1].Value;          // "1","2","3","4"
                    var sub = m.Groups[2].Value.ToUpper(); // "00".."FF"

                    var sig = new List<string> { $"C{cType}({sub})" };
                    int j = i + 1, depth = 1;

                    for (; j < toks.Length && depth > 0; j++)
                    {
                        var t = toks[j];

                        if (t.StartsWith("C1(") || t.StartsWith("C2(") || t.StartsWith("C3(")) { depth++; sig.Add("C*"); continue; }
                        if (t.StartsWith("<81") || t.StartsWith("B_81")) { sig.Add("81"); continue; }
                        if (t.StartsWith("<82") || t.StartsWith("B_82")) { sig.Add("82"); depth--; continue; }
                        if (t.StartsWith("00(")) { sig.Add("00"); continue; }
                        if (t.StartsWith("01:")) { sig.Add("01"); continue; }
                        if (t.StartsWith("02:")) { sig.Add("02"); continue; }
                        if (t.StartsWith("03:")) { sig.Add("03"); continue; }
                        if (t is "true" or "false") { sig.Add("B"); continue; }
                        sig.Add("X");
                    }

                    yield return string.Join(" ", sig);
                    i = Math.Max(i, j - 1);
                }
            }
        }


     
       
    }
}
