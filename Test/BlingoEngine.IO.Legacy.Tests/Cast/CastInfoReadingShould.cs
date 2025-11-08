using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Cast
{
    public class CastInfoReadingShould
    {
        [Fact]
        public void ReadCinf()
        {
            var file = "Casts/My External Cast.cst";
            //var file = "Casts/MyMultiCastsMovie.dir";
            using var harness = TestContextHarness.Open(file);
            harness.ReadResources();
            var ctx = harness.Context;
            var reader = new BlLegacyCastReader(ctx);
            reader.Read();
        }
        [Fact]
        public void ReadMultipleCasts()
        {

            //var file = "Casts/OneCast.dir";
            //var file = "Casts/Cast_LoadAfterFrameOne.dir";
            //var file = "Casts/MyMultiCastsMovie2.dir";
            var file = "Casts/MyMulti_4_CastsMovie.dir";
            using var harness = TestContextHarness.Open(file);
            harness.ReadResources();
            var ctx = harness.Context;
            var reader = new BlLegacyCastReader(ctx);
            reader.Read();
        }
        [Fact]
        public void ReadCommentsAndUser()
        {
            var file = "Casts/ModifiedMember.dir";
            using var harness = TestContextHarness.Open(file);
            harness.ReadResources();
            var ctx = harness.Context;
            var reader = new BlLegacyCastReader(ctx);
            var casts = reader.Read();
            var cast = casts.First();
            var member1 = cast.MemberSlots[0];
            var member2 = cast.MemberSlots[1];
            member1.Member.Comment.Should().Contain("Shape comment");
            member2.Member.Comment.Should().Contain("My Comment");
            member1.Member.ModifiedBy.Should().Contain("MyUserName");
            member2.Member.ModifiedBy.Should().Contain("MyUserName");
        }
        [Fact]
        public void ReadShapes()
        {
            //var file = "Shapes/DirWith_8_Shapes.dir";
            //var file = "Shapes/Shape_Triangle.cst";
            //var file = "Shapes/Shape_Two_Triangles.cst";
            var file = "Shapes/Shape_Triangle_Scale72.cst";
            //var file = "Shapes/Shape_Triangle_gradient_radial.cst";
            //var file = "Shapes/Shape_Triangle_Stroke_3.cst";
            using var harness = TestContextHarness.Open(file);
            harness.ReadResources();
            var ctx = harness.Context;
            var reader = new BlLegacyCastReader(ctx);
            var casts = reader.Read();
            var cast = casts.First();
           
        }
    }
}
