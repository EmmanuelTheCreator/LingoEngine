using System.Linq;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Tests.Fakes;
using FluentAssertions;

namespace AbstUI.Tests;

public class AbstImagePainterGridTests
{
    [Fact]
    public void Render_LineCrossingMultipleColumns_RendersEachTile()
    {
        var painter = new TestGridPainter(64, 64, tileSize: 16);
        painter.DrawLine(new APoint(0, 8), new APoint(40, 8), AColors.White);

        painter.Render();

        painter.RenderPasses.Should().HaveCount(3);
        painter.RenderPasses
            .Select(pass => (X: pass.OffsetX / painter.TileSize, Y: pass.OffsetY / painter.TileSize))
            .Should().BeEquivalentTo(new[] { (0, 0), (1, 0), (2, 0) });

        foreach (var pass in painter.RenderPasses)
        {
            pass.Texture.IsTile.Should().BeTrue();
            pass.Texture.Lines.Should().HaveCount(1);
            var record = pass.Texture.Lines[0];
            record.LocalStart.Should().Be(new APoint(0 - pass.OffsetX, 8 - pass.OffsetY));
            record.LocalEnd.Should().Be(new APoint(40 - pass.OffsetX, 8 - pass.OffsetY));
        }
    }

    [Fact]
    public void Render_LineCrossingMultipleRows_RendersEachTile()
    {
        var painter = new TestGridPainter(64, 64, tileSize: 16);
        painter.DrawLine(new APoint(8, 0), new APoint(8, 40), AColors.White);

        painter.Render();

        painter.RenderPasses.Should().HaveCount(3);
        painter.RenderPasses
            .Select(pass => (X: pass.OffsetX / painter.TileSize, Y: pass.OffsetY / painter.TileSize))
            .Should().BeEquivalentTo(new[] { (0, 0), (0, 1), (0, 2) });

        foreach (var pass in painter.RenderPasses)
        {
            pass.Texture.IsTile.Should().BeTrue();
            pass.Texture.Lines.Should().HaveCount(1);
            var record = pass.Texture.Lines[0];
            record.LocalStart.Should().Be(new APoint(8 - pass.OffsetX, 0 - pass.OffsetY));
            record.LocalEnd.Should().Be(new APoint(8 - pass.OffsetX, 40 - pass.OffsetY));
        }
    }
}
