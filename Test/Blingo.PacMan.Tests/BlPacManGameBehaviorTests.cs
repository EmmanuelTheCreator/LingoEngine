using System;
using System.Collections.Generic;
using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Models;
using FluentAssertions;
using Xunit;

namespace Blingo.PacMan.Tests;

public sealed class BlPacManGameBehaviorTests
{
    [Fact]
    public void GameModel_follows_default_mode_sequence()
    {
        var globals = new GlobalVars();
        var model = globals.GameModel ?? throw new InvalidOperationException("Game model was not initialised.");
        var recorded = new List<GhostMode>();

        model.SubscribeModeChanged(mode =>
        {
            if (mode is GhostMode value)
            {
                recorded.Add(value);
            }
        });

        model.UpdateMode();
        AdvanceSeconds(model, 7);
        AdvanceSeconds(model, 20);
        AdvanceSeconds(model, 7);
        AdvanceSeconds(model, 20);
        AdvanceSeconds(model, 5);
        AdvanceSeconds(model, 20);
        AdvanceSeconds(model, 5);

        var expected = new List<GhostMode>
        {
            GhostMode.Scatter,
            GhostMode.Chase,
            GhostMode.Scatter,
            GhostMode.Chase,
            GhostMode.Scatter,
            GhostMode.Chase,
            GhostMode.Scatter,
            GhostMode.Chase,
        };

        recorded.Should().Equal(expected);
    }

    private static void AdvanceSeconds(GameModel model, int seconds)
    {
        if (seconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds));
        }

        var frames = seconds * 60;
        for (var i = 0; i < frames; i++)
        {
            model.UpdateMode();
        }
    }

}
