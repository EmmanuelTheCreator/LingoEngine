using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using System.IO;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Cast
{
    public class BlLegacyCastTokenizerTests
    {
        [Fact]
        public void ReadTextMember_reads_3d_extruder_values()
        {
            var path = TestContextHarness.GetTextAssetPath("3D_Extruder/Text_Hallo_3DExtruder_Values.CASt.bin");
            var bytes = File.ReadAllBytes(path);
            var tokenizer = new BlLegacyCastTokenizer();

            var member = tokenizer.ReadTextMember(bytes);

            Assert.Equal("text", member.Type);
            Assert.Equal(0x1B0, member.SpecificDataLength);
            Assert.False(member.IsEditable);
            Assert.Equal(BlLegacyTextFraming.Fixed, member.Framing);
            Assert.True(member.IsAntialiasEnabled);
            Assert.Equal(0x0E, member.AntialiasMode);
            Assert.Equal(0x1E, member.KerningLargerThanPointSize);
            Assert.True(member.IsKerningEnabled);
            Assert.Equal(0x0E, member.KerningMode);
            Assert.Equal("3TEX", member.ShaderTag);
            Assert.Equal(0x164, member.ShaderDataLength);
            Assert.Equal(4, member.FaceFlags);
            Assert.True(member.IsBevelEnabled);
            Assert.Equal(2.80, member.BevelAmount, 2);
            Assert.Equal(BlCastTextBevelEdge.Miter, member.BevelEdge);
            Assert.Equal(2, member.Smoothness);
            Assert.Equal(BlCastTextDirectionalLight.TopLeft, member.LightSetting);
            Assert.Equal(BlCastTextShaderTexture.Default, member.ShaderTexture);
            Assert.Equal(53, member.Reflectivity);
            Assert.Equal(0x99663300u, member.DirectionalColor.Raw);
            Assert.Equal(0x99336600u, member.AmbientColor.Raw);
            Assert.Equal(0x00996600u, member.BackgroundColor.Raw);
            Assert.Equal(83.13, member.TunnelDepth, 2);
            Assert.Equal(12f, member.CameraPosition.X, 3);
            Assert.Equal(34f, member.CameraPosition.Y, 3);
            Assert.Equal(56f, member.CameraPosition.Z, 3);
            Assert.Equal(78f, member.CameraRotation.X, 3);
            Assert.Equal(98f, member.CameraRotation.Y, 3);
            Assert.Equal(76f, member.CameraRotation.Z, 3);
            Assert.Equal("NoTexture", member.TextureName);
        }

        [Fact]
        public void ReadTextMember_reads_display_mode_values()
        {
            var path = TestContextHarness.GetTextAssetPath("3D_Extruder/Text_Hallo_Display_3DMode.CASt.bin");
            var bytes = File.ReadAllBytes(path);
            var tokenizer = new BlLegacyCastTokenizer();

            var member = tokenizer.ReadTextMember(bytes);

            Assert.Equal("text", member.Type);
            Assert.Equal(0x1B0, member.SpecificDataLength);
            Assert.False(member.IsBevelEnabled);
            Assert.Equal(50.0, member.TunnelDepth, 2);
            Assert.Equal(1.0, member.BevelAmount, 2);
            Assert.Equal(BlCastTextBevelEdge.Miter, member.BevelEdge);
            Assert.Equal(5, member.Smoothness);
            Assert.Equal(BlCastTextDirectionalLight.TopLeft, member.LightSetting);
            Assert.Equal(BlCastTextShaderTexture.Default, member.ShaderTexture);
            Assert.Equal(0xFFFFFF00u, member.DirectionalColor.Raw);
            Assert.Equal(0xFFFFFF00u, member.AmbientColor.Raw);
            Assert.Equal(0xFFFFFF00u, member.BackgroundColor.Raw);
            Assert.Equal("NoTexture", member.TextureName);
        }
    }
}
