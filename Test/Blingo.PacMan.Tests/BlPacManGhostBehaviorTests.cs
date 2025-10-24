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
using Blingo.PacMan.Core.Sprites.ParentScripts;
using Blingo.PacMan.Tests.Fakes;
using Blingo.PacMan.Tests.TestUtilities;
using FluentAssertions;
using Xunit;

using static Blingo.PacMan.Tests.TestUtilities.PrivateFieldAccessor;

namespace Blingo.PacMan.Tests;

public sealed class PMGhostBehaviorTests
{
    private static readonly MethodInfo CanGoMethod = typeof(PMGhostBehavior)
        .GetMethod("CanGo", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("Unable to access CanGo method for testing.");

    private static readonly MethodInfo DetermineNextDirectionMethod = typeof(PMGhostBehavior)
        .GetMethod("GetNextDirection", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("Unable to access GetNextDirection method for testing.");

    [Fact]
    public void CanMove_allows_dead_ghosts_to_enter_house_through_doorway()
    {
        var globals = PrepareGlobalsWithMap();
        var map = globals.Map;
        var houseCenter = map.HouseCenter ?? throw new InvalidOperationException("House center tile was not found.");
        var doorway = houseCenter.GetUp() ?? throw new InvalidOperationException("House doorway tile was not found.");
        var outside = doorway.GetUp()?.GetUp() ?? throw new InvalidOperationException("Outside doorway tile was not found.");

        var character = CreateCharacter(map, outside, PMDirection.Down);
        typeof(PMCharacter).GetProperty("Mode")!.SetValue(character, GhostMode.Dead);
        var behavior = CreateBehavior(globals, character);

        var canMove = (bool)CanGoMethod.Invoke(behavior, new object[] { PMDirection.Down, outside })!;

        canMove.Should().BeTrue();
    }

    [Fact]
    public void CanMove_prevents_reentry_into_house_from_outside()
    {
        var globals = PrepareGlobalsWithMap();
        var map = globals.Map;
        var houseCenter = map.HouseCenter ?? throw new InvalidOperationException("House center tile was not found.");
        var doorway = houseCenter.GetUp() ?? throw new InvalidOperationException("House doorway tile was not found.");
        var outside = doorway.GetUp()?.GetUp() ?? throw new InvalidOperationException("Outside doorway tile was not found.");

        var character = CreateCharacter(map, outside, PMDirection.Down);
        var behavior = CreateBehavior(globals, character);

        var canMove = (bool)CanGoMethod.Invoke(behavior, new object[] { PMDirection.Down, outside })!;

        canMove.Should().BeFalse();
    }

    [Fact]
    public void DetermineNextDirection_returns_none_when_forward_is_blocked()
    {
        var layout = new[]
        {
            "===",
            "=.=",
            "=.=",
            "===",
        };

        var globals = PrepareGlobalsWithMap(layout);
        var map = globals.Map;
        var tile = map.GetTile(1, 1) ?? throw new InvalidOperationException();
        var character = CreateCharacter(map, tile, PMDirection.Up);
        var ghost = CreateBehavior(globals, character);
        ghost.GhostName = MrGhost.Blinky;

        SetField(ghost, "_moveDir", PMDirection.Up);
        SetField(ghost, "_scatterTarget", tile);

        var direction = (PMDirection)DetermineNextDirectionMethod.Invoke(ghost, new object[] { tile })!;

        direction.Should().Be(PMDirection.None);
    }

    [Fact]
    public void OnExitMode_resets_house_navigation_state()
    {
        var globals = PrepareGlobalsWithMap();
        var map = globals.Map;
        var house = map.HouseCenter ?? throw new InvalidOperationException("House center tile was not found.");
        var character = CreateCharacter(map, house, PMDirection.Up);
        character.Mode = GhostMode.House;
        var ghost = CreateBehavior(globals, character);
        ghost.GhostName = MrGhost.Blinky;
        ghost.SetGlobalMode(GhostMode.Scatter);

        SetField(ghost, "_moveDir", PMDirection.Up);
        SetField(ghost, "_nextDir", PMDirection.Up);
        SetField(ghost, "_houseTimer", new PMTimer(10));

        ghost.OnExitMode();

        typeof(PMGhostBehavior)
            .GetField("_houseTimer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(ghost)
            .Should()
            .BeNull();
        GetField<PMDirection>(ghost, "_moveDir").Should().Be(PMDirection.Left);
        GetField<PMDirection>(ghost, "_nextDir").Should().Be(PMDirection.Left);
        character.Direction.Should().Be(PMDirection.Left);
        character.Mode.Should().Be(GhostMode.Scatter);
        character.EffectiveSpeed.Should().Be(GetField<float>(ghost, "_baseSpeed"));
    }

    [Fact]
    public void Configure_applies_start_outside_flag()
    {
        var globals = new GlobalVars();
        var ghost = CreateBehavior(globals);
        ghost.GhostName = MrGhost.Blinky;
        var settings = new GhostSettings(80f, 60f, new CruiseElroySettings(0, 0f, 0, 0f), 50f, TimeSpan.FromSeconds(6), 4);

        ghost.Configure(settings, true, 0);

        GetField<bool>(ghost, "_startOutsideHouse").Should().BeTrue();

        ghost.Configure(settings, false, 120);

        GetField<bool>(ghost, "_startOutsideHouse").Should().BeFalse();
        GetField<GhostSettings>(ghost, "_settings").Should().BeSameAs(settings);
    }

    [Fact]
    public void Blinky_targets_pacman_tile_directly()
    {
        var globals = PrepareGlobalsWithMap();
        var map = globals.Map;
        var pacTile = map.GetTile(14, 23) ?? throw new InvalidOperationException();
        var direction = PMDirection.Left;

        var character = CreateCharacter(map, pacTile, direction);

        var target = PMGhostLogic.GetChaseTargetTile(globals.GhostManager, MrGhost.Blinky, character, pacTile, direction, pacTile);

        target.Should().BeSameAs(pacTile);
    }

    [Fact]
    public void Pinky_targets_four_tiles_ahead()
    {
        var globals = PrepareGlobalsWithMap();
        var map = globals.Map;
        var pacTile = map.GetTile(10, 10) ?? throw new InvalidOperationException();
        var direction = PMDirection.Right;

        var character = CreateCharacter(map, pacTile, direction);

        var expected = pacTile;
        for (var i = 0; i < 4; i++)
            expected = expected?.Get(direction);

        var target = PMGhostLogic.GetChaseTargetTile(globals.GhostManager, MrGhost.Pinky, character, pacTile, direction, pacTile);

        target.Should().BeSameAs(expected ?? pacTile);
    }

    [Fact]
    public void Inky_reflects_vector_relative_to_blinky()
    {
        var globals = PrepareGlobalsWithMap();
        var map = globals.Map;
        var pacTile = map.GetTile(15, 20) ?? throw new InvalidOperationException();
        var direction = PMDirection.Up;

        var character = CreateCharacter(map, pacTile, direction);

        var ahead = pacTile.Get(direction)?.Get(direction) ?? pacTile;
        var blinkyTile = map.GetTile(10, 18) ?? throw new InvalidOperationException();

        var blinky = CreateBehavior(globals, CreateCharacter(map, blinkyTile, PMDirection.Left));
        blinky.GhostName = MrGhost.Blinky;
        RegisterGhost(globals.GhostManager, blinky);

        var target = PMGhostLogic.GetChaseTargetTile(globals.GhostManager, MrGhost.Inky, character, pacTile, direction, pacTile);

        var offsetCol = (ahead?.Column ?? pacTile.Column) - blinkyTile.Column;
        var offsetRow = (ahead?.Row ?? pacTile.Row) - blinkyTile.Row;
        var expected = map.GetTile((ahead?.Column ?? pacTile.Column) + offsetCol, (ahead?.Row ?? pacTile.Row) + offsetRow);

        target.Should().BeSameAs(expected);
    }

    [Fact]
    public void Sue_switches_between_scatter_and_chase_targets()
    {
        var globals = PrepareGlobalsWithMap();
        var map = globals.Map;
        var pacTile = map.GetTile(14, 23) ?? throw new InvalidOperationException();
        var scatter = map.GetTile(0, map.Height - 1) ?? throw new InvalidOperationException();
        var direction = PMDirection.Left;

        var character = CreateCharacter(map, pacTile, direction);

        var farTile = map.GetTile(0, 0) ?? throw new InvalidOperationException();
        SetCharacterPosition(character, farTile);
        var farTarget = PMGhostLogic.GetChaseTargetTile(globals.GhostManager, MrGhost.Sue, character, pacTile, direction, scatter);
        farTarget.Should().BeSameAs(pacTile);

        var closeTile = pacTile.Get(direction) ?? pacTile;
        SetCharacterPosition(character, closeTile);
        var closeTarget = PMGhostLogic.GetChaseTargetTile(globals.GhostManager, MrGhost.Sue, character, pacTile, direction, scatter);
        closeTarget.Should().BeSameAs(scatter);
    }

    private static GlobalVars PrepareGlobalsWithMap(IEnumerable<string>? layout = null)
    {
        var globals = new GlobalVars();
        var map = new PMMap(layout ?? Maps.Map1);
        typeof(PMLevelManager)
            .GetField("_map", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(globals.LevelManager, map);
        return globals;
    }

    private static PMCharacter CreateCharacter(PMMap map, PMTile tile, PMDirection direction)
    {
        var character = (PMCharacter)RuntimeHelpers.GetUninitializedObject(typeof(PMCharacter));
        var type = typeof(PMCharacter);

        type.GetField("_map", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(character, map);
        SetCharacterPosition(character, tile);
        type.GetProperty("Direction")!.SetValue(character, direction);
        type.GetProperty("Mode")!.SetValue(character, GhostMode.Scatter);

        return character;
    }

    private static void SetCharacterPosition(PMCharacter character, PMTile tile)
    {
        var type = typeof(PMCharacter);
        type.GetField("_x", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(character, tile.X);
        type.GetField("_y", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(character, tile.Y);
        type.GetField("_lastTile", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(character, tile);
    }

    private static void RegisterGhost(PMGhostManager manager, PMGhostBehavior ghost)
    {
        var field = typeof(PMGhostManager)
            .GetField("_ghosts", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to access ghost collection.");
        var list = (List<PMGhostBehavior>)field.GetValue(manager)!;
        if (!list.Contains(ghost))
            list.Add(ghost);
    }

    private static PMGhostBehavior CreateBehavior(GlobalVars? globals = null)
        => CreateBehavior(globals, null);

    private static PMGhostBehavior CreateBehavior(GlobalVars? globals, PMCharacter? character)
    {
        var behavior = (PMGhostBehavior)RuntimeHelpers.GetUninitializedObject(typeof(PMGhostBehavior));
        var type = typeof(PMGhostBehavior);

        globals ??= new GlobalVars();

        type.GetField("_globals", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(behavior, globals);
        type.GetField("_random", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(behavior, new DeterministicRandomSource());
        type.GetField("_globalMode", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(behavior, GhostMode.Scatter);
        if (character is not null)
            type.GetField("_character", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(behavior, character);

        return behavior;
    }
}
