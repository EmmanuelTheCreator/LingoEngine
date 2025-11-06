using BlingoEngine.IO.Legacy.Scores;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Texts;
using BlingoEngine.IO.Legacy.Texts.Data;
using BlingoEngine.IO.Legacy.Tools;
using System;
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
                "KeyFrames/SingleSprite",
                "KeyFrames",
            ];
            foreach (var folder in folders)
            {
                var cstFiles = TestContextHarness.GetAllFilesFromFolder(folder, "*.dir");
                foreach (var item in cstFiles)
                {

                    var path = TestContextHarness.GetAssetPath(folder + "/" + Path.GetFileNameWithoutExtension(item) + ".vmsc.txt");
                    var pathLog = TestContextHarness.GetAssetPath(folder + "/" + Path.GetFileNameWithoutExtension(item) + ".vmsclog.txt");
                    if (File.Exists(pathLog))
                        continue;

                    using var harness = TestContextHarness.Open(item);
                    harness.ReadResources();
                    var scoreReader = new BlLegacyScoreReader(harness.Context);
                   
                    var scoreBytes = scoreReader.ReadVMSW();
                    //if (scoreBytes == null) continue;
                    //File.WriteAllText(path, scoreBytes.ToHexString());
                    //var pathBin = TestContextHarness.GetAssetPath(folder + "/" + Path.GetFileNameWithoutExtension(item) + ".vmsc.bin");
                    //File.WriteAllBytes(pathBin, scoreBytes);


                    
                    scoreReader.ParseVMSC(scoreBytes);
                    var log = 
                        Path.GetFileName(item)+
                        Environment.NewLine+"--------------------------------"+Environment.NewLine+
                        scoreReader.ToLog();
                    File.WriteAllText(pathLog, log);
                }
            }

        }
    }
}
