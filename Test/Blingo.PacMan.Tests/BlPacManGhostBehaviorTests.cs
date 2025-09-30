using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Engine;
using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Settings;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using Blingo.PacMan.Tests.Fakes;
using Blingo.PacMan.Tests.TestUtilities;
using FluentAssertions;
using Xunit;

using static Blingo.PacMan.Tests.TestUtilities.PrivateFieldAccessor;

namespace Blingo.PacMan.Tests;

public sealed class BlPacManGhostBehaviorTests
{
    private static readonly MethodInfo CanMoveMethod = typeof(BlPacManGhostBehavior)
        .GetMethod("CanMove", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("Unable to access CanMove method for testing.");

    private static readonly MethodInfo DetermineNextDirectionMethod = typeof(BlPacManGhostBehavior)
        .GetMethod("DetermineNextDirection", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("Unable to access DetermineNextDirection method for testing.");

    [Fact]
    public void CanMove_allows_ghosts_to_leave_house_through_doorway()
    {
        var map = new Map(Maps.Map1);
        var houseCenter = map.HouseCenter ?? throw new InvalidOperationException("House center tile was not found.");
        var doorway = houseCenter.GetUp() ?? throw new InvalidOperationException("House doorway tile was not found.");

        var behavior = (BlPacManGhostBehavior)RuntimeHelpers.GetUninitializedObject(typeof(BlPacManGhostBehavior));

        var canMove = (bool)CanMoveMethod.Invoke(behavior, new object[] { BlPacManDirection.Up, doorway })!;

        canMove.Should().BeTrue();
    }

    [Fact]
    public void CanMove_prevents_reentry_into_house_from_outside()
    {
        var map = new Map(Maps.Map1);
        var houseCenter = map.HouseCenter ?? throw new InvalidOperationException("House center tile was not found.");
        var doorway = houseCenter.GetUp() ?? throw new InvalidOperationException("House doorway tile was not found.");
        var outside = doorway.GetUp()?.GetUp() ?? throw new InvalidOperationException("Outside doorway tile was not found.");

        var behavior = (BlPacManGhostBehavior)RuntimeHelpers.GetUninitializedObject(typeof(BlPacManGhostBehavior));

        var canMove = (bool)CanMoveMethod.Invoke(behavior, new object[] { BlPacManDirection.Down, outside })!;

        canMove.Should().BeFalse();
    }

    [Fact]
    public void DetermineNextDirection_reverses_when_forward_is_blocked()
    {
        var layout = new[]
        {
            "===",
            "=.=",
            "=.=",
            "===",
        };

        var globals = PrepareGlobalsWithMap(layout);
        var ghost = CreateBehavior(globals);
        SetField(ghost, "_mode", GhostMode.Scatter);

        var deadEnd = globals.Map.GetTile(1, 1) ?? throw new InvalidOperationException();

        SetField(ghost, "_dir", BlPacManDirection.Up);
        SetField(ghost, "_scatterTarget", deadEnd);

        var direction = (BlPacManDirection)DetermineNextDirectionMethod.Invoke(ghost, new object[] { deadEnd })!;

        direction.Should().Be(BlPacManDirection.Down);
    }

    [Fact]
    public void ExitCurrentMode_marks_house_exit()
    {
        var globals = new GlobalVars();
        var ghost = CreateBehavior(globals);

        SetField(ghost, "_mode", GhostMode.House);
        SetField(ghost, "_globalMode", GhostMode.Scatter);
        SetField(ghost, "_hasLeftHouse", false);

        ghost.ExitCurrentMode();

        GetField<bool>(ghost, "_hasLeftHouse").Should().BeTrue();
        GetField<GhostMode>(ghost, "_mode").Should().Be(GhostMode.Scatter);
    }

    [Fact]
    public void Configure_keeps_blinky_outside_and_resets_release_delay()
    {
        var globals = new GlobalVars();
        var ghost = CreateBehavior(globals);
        ghost.GhostName = MrGhost.Blinky;
        var settings = new GhostSettings(80f, 60f, new CruiseElroySettings(0, 0f, 0, 0f), 50f, TimeSpan.FromSeconds(6), 4);

        ghost.Configure(settings, true, 0);

        GetField<bool>(ghost, "_startOutsideHouse").Should().BeTrue();

        ghost.Configure(settings, true, 120);

        GetField<bool>(ghost, "_startOutsideHouse").Should().BeTrue();
        GetField<int>(ghost, "_initialHouseReleaseFrames").Should().Be(120);
        GetField<int>(ghost, "_houseReleaseFrames").Should().Be(120);
    }

    [Fact]
    public void Blinky_targets_pacman_tile_directly()
    {
        var globals = PrepareGlobalsWithMap();
        var ghost = CreateBehavior(globals);
        ghost.GhostName = MrGhost.Blinky;

        var pacTile = globals.Map.GetTile(14, 23) ?? throw new InvalidOperationException();
        globals.State.UpdatePacManPosition(new BlPacManPositionEventData(pacTile.CenterX, pacTile.CenterY, pacTile, BlPacManDirection.Left));

        var target = ghost.GetChaseTargetTile();

        target.Should().BeSameAs(pacTile);
    }

    [Fact]
    public void Pinky_targets_four_tiles_ahead()
    {
        var globals = PrepareGlobalsWithMap();
        var ghost = CreateBehavior(globals);
        ghost.GhostName = MrGhost.Pinky;

        var pacTile = globals.Map.GetTile(10, 10) ?? throw new InvalidOperationException();
        var direction = BlPacManDirection.Right;
        globals.State.UpdatePacManPosition(new BlPacManPositionEventData(pacTile.CenterX, pacTile.CenterY, pacTile, direction));

        var expected = pacTile;
        for (var i = 0; i < 4; i++)
        {
            expected = expected?.Get(direction);
        }

        var target = ghost.GetChaseTargetTile();

        target.Should().BeSameAs(expected ?? pacTile);
    }

    [Fact]
    public void Inky_reflects_vector_relative_to_blinky()
    {
        var globals = PrepareGlobalsWithMap();
        var ghost = CreateBehavior(globals);
        ghost.GhostName = MrGhost.Inky;

        var pacTile = globals.Map.GetTile(15, 20) ?? throw new InvalidOperationException();
        var ahead = pacTile.Get(BlPacManDirection.Up)?.Get(BlPacManDirection.Up) ?? pacTile;
        var blinkyTile = globals.Map.GetTile(10, 18) ?? throw new InvalidOperationException();

        var target = ghost.GetChaseTargetTile(pacTile, BlPacManDirection.Up, blinkyTile);

        var offsetCol = (ahead?.Column ?? pacTile.Column) - blinkyTile.Column;
        var offsetRow = (ahead?.Row ?? pacTile.Row) - blinkyTile.Row;
        var expected = globals.Map.GetTile((ahead?.Column ?? pacTile.Column) + offsetCol, (ahead?.Row ?? pacTile.Row) + offsetRow);

        target.Should().BeSameAs(expected);
    }

    [Fact]
    public void Clyde_switches_between_scatter_and_chase_targets()
    {
        var globals = PrepareGlobalsWithMap();
        var ghost = CreateBehavior(globals);
        ghost.GhostName = MrGhost.Clyde;

        var pacTile = globals.Map.GetTile(14, 23) ?? throw new InvalidOperationException();
        var scatter = globals.Map.GetTile(0, globals.Map.Height - 1) ?? throw new InvalidOperationException();
        SetField(ghost, "_scatterTarget", scatter);

        var farTile = globals.Map.GetTile(0, 0) ?? throw new InvalidOperationException();
        var farTarget = ghost.GetChaseTargetTile(pacTile, BlPacManDirection.Left, null, farTile);
        farTarget.Should().BeSameAs(pacTile);

        var closeTile = pacTile.Get(BlPacManDirection.Left) ?? pacTile;
        var closeTarget = ghost.GetChaseTargetTile(pacTile, BlPacManDirection.Left, null, closeTile);
        closeTarget.Should().BeSameAs(scatter);
    }

    private static GlobalVars PrepareGlobalsWithMap(IEnumerable<string>? layout = null)
    {
        var globals = new GlobalVars();
        var map = new Map(layout ?? Maps.Map1);
        var managerField = typeof(BlLevelManager).GetField("_map", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
        managerField.SetValue(globals.LevelManager, map);
        return globals;
    }

    private static BlPacManGhostBehavior CreateBehavior(GlobalVars? globals = null)
    {
        var behavior = (BlPacManGhostBehavior)RuntimeHelpers.GetUninitializedObject(typeof(BlPacManGhostBehavior));
        var type = typeof(BlPacManGhostBehavior);

        globals ??= new GlobalVars();

        type.GetField("_globals", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(behavior, globals);
        type.GetField("_random", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(behavior, new DeterministicRandomSource());
        type.GetField("_globalMode", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(behavior, GhostMode.Scatter);
        type.GetField("_mode", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(behavior, GhostMode.House);

        return behavior;
    }
}
