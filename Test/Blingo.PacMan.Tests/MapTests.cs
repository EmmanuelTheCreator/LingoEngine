using System;
using Blingo.PacMan.Core.Game;
using FluentAssertions;
using Xunit;

namespace Blingo.PacMan.Tests;

public sealed class MapTests
{
    private static readonly string[] SimpleLayout =
    {
        "===",
        "=.=",
        "===",
    };

    [Fact]
    public void GetTile_inPixels_returns_null_when_above_top_row()
    {
        var map = new PMMap(SimpleLayout);
        var top = map.GetTile(1, 0) ?? throw new InvalidOperationException("Top tile was not found.");

        var tile = map.GetTile(top.X, -1f, true);

        tile.Should().BeNull();
    }

    [Fact]
    public void GetTile_inPixels_wraps_negative_columns_like_reference_implementation()
    {
        var map = new PMMap(SimpleLayout);
        var rowTile = map.GetTile(1, 1) ?? throw new InvalidOperationException("Row tile was not found.");
        var expected = map.GetTile(map.Width - 1, rowTile.Row) ?? throw new InvalidOperationException("Expected tile was not found.");

        var tile = map.GetTile(-1f, rowTile.Y, true);

        tile.Should().BeSameAs(expected);
    }
}
