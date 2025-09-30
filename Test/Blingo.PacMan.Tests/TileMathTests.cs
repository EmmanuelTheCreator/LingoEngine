using Blingo.PacMan.Core.Game;
using FluentAssertions;
using Xunit;

namespace Blingo.PacMan.Tests;

public sealed class TileMathTests
{
    [Theory]
    [InlineData(-10f, 480, 0f)]
    [InlineData(-0.1f, 320, 0f)]
    public void ClampVerticalPosition_prevents_negative_values(float y, int mapHeight, float expected)
    {
        TileMath.ClampVerticalPosition(y, mapHeight).Should().Be(expected);
    }

    [Theory]
    [InlineData(100f, 480)]
    [InlineData(0f, 320)]
    [InlineData(479f, 480)]
    public void ClampVerticalPosition_preserves_in_bounds_values(float y, int mapHeight)
    {
        TileMath.ClampVerticalPosition(y, mapHeight).Should().Be(y);
    }

    [Theory]
    [InlineData(480f, 480, 479f)]
    [InlineData(960f, 480, 479f)]
    [InlineData(1000f, 1, 0f)]
    public void ClampVerticalPosition_caps_at_bottom_edge(float y, int mapHeight, float expected)
    {
        TileMath.ClampVerticalPosition(y, mapHeight).Should().Be(expected);
    }

    [Fact]
    public void ClampVerticalPosition_returns_original_when_height_is_not_positive()
    {
        TileMath.ClampVerticalPosition(25f, 0).Should().Be(25f);
        TileMath.ClampVerticalPosition(25f, -10).Should().Be(25f);
    }
}
