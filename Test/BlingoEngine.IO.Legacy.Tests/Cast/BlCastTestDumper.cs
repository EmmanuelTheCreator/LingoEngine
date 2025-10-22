using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using System.IO;
using System.Linq;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Cast
{
    public class BlCastTestDumper
    {
        //[Fact]
        public void GenerateCastDumps()
        {
            var root = TestContextHarness.GetAssetPath("");
            var files = Directory.EnumerateFiles(root, "*.cst", SearchOption.AllDirectories)
                                 .Concat(Directory.EnumerateFiles(root, "*.dir", SearchOption.AllDirectories));

            foreach (var file in files)
            {
                DumpCastTokens(file);
            }
        }

        [Fact]
        public void DumpSingleCastFile()
        {
            var files = new string[] {
                //TestContextHarness.GetTextAssetPath("3D_Extruder/Text_Hallo.cst"),
                //TestContextHarness.GetTextAssetPath("3D_Extruder/Text_Hallo_3DExtruder_Values.cst"),
                //TestContextHarness.GetTextAssetPath("3D_Extruder/Text_Hallo_Display_3DMode.cst"),
                //TestContextHarness.GetTextAssetPath("MemberTests/Text_AntiAlias_LargerThan_19pt.cst"),
                //TestContextHarness.GetTextAssetPath("MemberTests/Text_Kerning_LargerThan_19pt.cst"),
                //TestContextHarness.GetTextAssetPath("MemberTests/Text_DTS_On.cst"),
                //TestContextHarness.GetTextAssetPath("FontSize/Text_Single_Line_Multi_Style3_lh13.cst"),
                //TestContextHarness.GetTextAssetPath("MemberTypes/MemberButton.cst"),
                //TestContextHarness.GetTextAssetPath("MemberTypes/MemberShape.cst"),
                //TestContextHarness.GetTextAssetPath("Styles/Text_Flags_BIUSubSup.cst"),
               // TestContextHarness.GetTextAssetPath("MemberTypes/ImgCast.cst"),
                //TestContextHarness.GetTextAssetPath("MemberTypes/MemberImagePainted.cst"),
                TestContextHarness.GetTextAssetPath("MemberTypes/MemberImage_Gif.cst"),
                //TestContextHarness.GetTextAssetPath("MemberTypes/MemberImage_jpg_32bit.cst"),
                //TestContextHarness.GetTextAssetPath("MemberTypes/MemberImage_jpg_24bit.cst"),
            };
            foreach (var file in files) 
                DumpCastTokens(file);
        }

        private static void DumpCastTokens(string file)
        {
            using var harness = TestContextHarness.Open(file);
            var outPath = Path.Combine(Path.GetDirectoryName(file)!,
               $"{Path.GetFileNameWithoutExtension(file)}.CASt.txt");
            //if (File.Exists(outPath))
            //    return;
            harness.ReadResources();
            var ctx = harness.Context;
            if (!File.Exists(outPath))
                BlLegacyCastTokenDumper.DumpCastTokens(ctx, outPath);
        


            var reader = new BlLegacyCastReader(ctx);
            var libs = reader.Read();
            Assert.NotEmpty(libs);

            var first = libs[0];
            foreach (var slot in first.MemberSlots)
            {
                if (!ctx.Resources.TryGetEntry(slot.ResourceId, out var entry)) continue;
                var bytes = entry.ReadClassicPayload(new BlClassicPayloadLoader(ctx));
                var info = bytes.ToArray(); 
                var tokenizer = new BlLegacyCastTokenizer();
                var tokens = tokenizer.TokenizeInfo(info);
                Assert.NotNull(tokens);
                break;
            }
        }
    }
}
