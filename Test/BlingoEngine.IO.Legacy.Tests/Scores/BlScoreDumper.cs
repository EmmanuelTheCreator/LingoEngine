using BlingoEngine.IO.Legacy.Scores;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Texts;
using BlingoEngine.IO.Legacy.Texts.Data;
using BlingoEngine.IO.Legacy.Tools;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Scores
{
    public class BlScoreDumper
    {
        [Fact]
        public void GenerateVmscDump()
        {

            List<string> folders =
            [
                //"Texts_Fields",
                //"Texts_Fields/Tests2",
                //"Texts_Fields/Colors",
                //"Texts_Fields/MemberTests",
                //"Texts_Fields/Paragraphs",
                //"Texts_Fields/Content",
                //"KeyFrames/SingleSprite",
                "KeyFrames",
            ];
            foreach (var folder in folders)
            {
                var cstFiles = TestContextHarness.GetAllFilesFromFolder(folder, "*.dir");
                foreach (var item in cstFiles)
                {
                    using var harness = TestContextHarness.Open(item);
                    harness.ReadResources();
                    var scoreReader = new BlLegacyScoreReader(harness.Context);
                    var path = TestContextHarness.GetAssetPath(folder + "/" + Path.GetFileNameWithoutExtension(item) + ".vmsc.txt");
                    if (File.Exists(path))
                        continue;
                    var scoreBytes = scoreReader.ReadVMSW();
                    if (scoreBytes == null) continue;
                    File.WriteAllText(path, scoreBytes.ToHexString());
                    var pathBin = TestContextHarness.GetAssetPath(folder + "/" + Path.GetFileNameWithoutExtension(item) + ".vmsc.bin");
                    File.WriteAllBytes(pathBin, scoreBytes);
                    var pathLog = TestContextHarness.GetAssetPath(folder + "/" + Path.GetFileNameWithoutExtension(item) +".vmsclog.txt");
                    //var tokens = BlXmedTokenizer.Tokenize(textItem.Bytes).Tokens;
                    //var log = BlXmedTokenizer.DumpTokensUltraCompact(tokens);
                    //File.WriteAllText(pathLog, log);
                }
            }

        }
    }
}
