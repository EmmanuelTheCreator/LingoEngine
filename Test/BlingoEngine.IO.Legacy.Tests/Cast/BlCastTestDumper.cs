using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Tools;
using System;
using System.IO;
using System.Linq;
using System.Text;
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
                //DumpCastTokens(file);
            }
        }

        [Fact]
        public void DumpSingleCastFile()
        {
            var files = new (string FileName, string Type)[] {
                //(TestContextHarness.GetTextAssetPath("3D_Extruder/Text_Hallo.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("3D_Extruder/Text_Hallo_3DExtruder_Values.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("3D_Extruder/Text_Hallo_Display_3DMode.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTests/Text_AntiAlias_LargerThan_19pt.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTests/Text_AntiAlias_AllText.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTests/Text_Kerning_LargerThan_19pt.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTests/Text_DTS_On.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTests/Text_AntiAlias_None.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTests/Text_Default.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTests/Text_Editable_On.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTests/Text_Framing_AdjustToFit_IsDefault.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTests/Text_Framing_Fixed.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTests/Text_Framing_Scrolling.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTests/Text_Kerning_AllText.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTests/Text_Kerning_LargerThan_19pt.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTests/Text_Kerning_None.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("FontSize/Text_Single_Line_Multi_Style3_lh13.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("Styles/Text_Flags_BIUSubSup.cst"),"Text"),
                //(TestContextHarness.GetTextAssetPath("MemberTypes/MemberButton.cst"),"Flash"),
                //(TestContextHarness.GetTextAssetPath("MemberTypes/MemberShape.cst"),"Shape"),
                //(TestContextHarness.GetTextAssetPath("MemberTypes/ImgCast.cst"),"Bitmap"),
                //(TestContextHarness.GetTextAssetPath("MemberTypes/MemberImagePainted.cst"),"Bitmap"),
                //(TestContextHarness.GetTextAssetPath("MemberTypes/MemberImage_Gif.cst"),"Bitmap"),
                //(TestContextHarness.GetTextAssetPath("MemberTypes/MemberImage_jpg_32bit.cst"),"Bitmap"),
                //(TestContextHarness.GetTextAssetPath("MemberTypes/MemberImage_jpg_24bit.cst"),"Bitmap"),
                //(TestContextHarness.GetTextAssetPath("MemberTypes/MemberImage_jpg_32bit.cst"),"Bitmap"),
                //(TestContextHarness.GetTextAssetPath("MemberTypes/MemberImage_jpg_32bit_loc_Out1.cst"),"Bitmap"),
                //(TestContextHarness.GetTextAssetPath("MemberTypes/MemberImage_jpg_32bit_loc_Out2.cst"),"Bitmap"),
                (TestContextHarness.GetTextAssetPath("MemberTypes/MemberImagePainted2.cst"),"Bitmap"),
                (TestContextHarness.GetTextAssetPath("MemberTypes/MemberImagePainted3.cst"),"Bitmap"),
            };
            var sb = new StringBuilder();
            foreach (var file in files)
                DumpCastTokens(sb, file);

            var result = sb.ToString();
        }

        private static void DumpCastTokens(StringBuilder sb, (string FileName, string Type) fileData)
        {
            var file = fileData.FileName;
            using var harness = TestContextHarness.Open(file);
            var outPath = Path.Combine(Path.GetDirectoryName(file)!,$"{Path.GetFileNameWithoutExtension(file)}.CASt.txt");
            var outPath2 = Path.Combine(Path.GetDirectoryName(file)!,$"{Path.GetFileNameWithoutExtension(file)}.CASt.bin");
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
                if (!File.Exists(outPath2))
                    File.WriteAllBytes(outPath2,info);
                var tokenizer = new BlLegacyCastItemReader();
                (var tokens, var member) = tokenizer.ReadItem(fileData.FileName,info);
                var text1 = tokenizer.TokenListToStringX(tokens); // for debug
                var fn = Path.GetFileName(file);
                var name = $"{fn} - {member.MemberTypeString} - {member.Name} - {member.MediaContentType} - {member.Blob?.ToHexString()} ({member.Created.Value:dd/MM:yyyy HH:mm:ss},{member.Modified.Value:dd/MM:yyyy HH:mm:ss})";
                sb.AppendLine(name);
                sb.AppendLine(new string('-', name.Length));
                sb.AppendLine(text1);
                sb.AppendLine("--------------------------------------------------");

                Assert.NotNull(tokens);
                break;
            }
        }
    }
}
