using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using FluentAssertions;
using Xunit;

namespace Blingo.PacMan.Tests;

public sealed class BlPacManGhostBehaviorTests
{
    private static readonly MethodInfo CanMoveMethod = typeof(BlPacManGhostBehavior)
        .GetMethod("CanMove", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("Unable to access CanMove method for testing.");

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
}
