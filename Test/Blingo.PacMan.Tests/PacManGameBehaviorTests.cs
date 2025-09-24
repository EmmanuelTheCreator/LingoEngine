using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Blingo.PacMan.Core;
using Blingo.PacMan.Core.Models;
using Blingo.PacMan.Core.Sprites.Behaviors;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Events;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Inputs;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sounds;
using BlingoEngine.Stages;
using FluentAssertions;
using Xunit;

namespace Blingo.PacMan.Tests;

public sealed class PacManGameBehaviorTests
{
    [Fact]
    public void Ghosts_follow_model_mode_sequence()
    {
        var clock = new TestClock { FrameRate = 60 };
        var model = new GameModel(new InMemoryGameModelRepository(), clock);
        var bonuses = new TestBonusesModel();
        var behavior = CreateBehavior(model, bonuses);
        behavior.BeginSprite();

        var ghostA = new RecordingGhost();
        var ghostB = new RecordingGhost();
        behavior.RegisterGhost(ghostA);
        behavior.RegisterGhost(ghostB);

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

        model.UpdateMode();
        AdvanceAndUpdate(clock, model, 7);
        AdvanceAndUpdate(clock, model, 20);
        AdvanceAndUpdate(clock, model, 7);
        AdvanceAndUpdate(clock, model, 20);
        AdvanceAndUpdate(clock, model, 5);
        AdvanceAndUpdate(clock, model, 20);
        AdvanceAndUpdate(clock, model, 5);

        ghostA.Modes.Should().Equal(expected);
        ghostB.Modes.Should().Equal(expected);
    }

    private static void AdvanceAndUpdate(TestClock clock, GameModel model, int seconds)
    {
        clock.AdvanceSeconds(seconds);
        model.UpdateMode();
    }

    private static PacManGameBehavior CreateBehavior(IGameModel model, IBonusesModel bonuses)
    {
        var behavior = (PacManGameBehavior)RuntimeHelpers.GetUninitializedObject(typeof(PacManGameBehavior));
        SetField(behavior, "_model", model);
        SetField(behavior, "_bonusesModel", bonuses);
        SetField(behavior, "_ghosts", new List<IGhostModeController>());
        return behavior;
    }

    private sealed class RecordingGhost : IGhostModeController
    {
        public List<GhostMode> Modes { get; } = new();

        public void SetMode(GhostMode? mode)
        {
            if (mode is GhostMode value)
            {
                Modes.Add(value);
            }
        }
    }

    private sealed class InMemoryGameModelRepository : IGameModelRepository
    {
        public PacManSaveData? Load() => null;

        public void Save(PacManSaveData data)
        {
        }
    }

    private sealed class TestClock : IBlingoClock
    {
        public int FrameRate { get; set; }

        public int TickCount { get; private set; }

        public int EngineTickCount { get; private set; }

        public void AdvanceSeconds(int seconds)
        {
            var ticks = seconds * FrameRate;
            EngineTickCount += ticks;
            TickCount += ticks;
        }

        public void Reset()
        {
            TickCount = 0;
            EngineTickCount = 0;
        }

        public void Subscribe(IBlingoClockListener listener)
        {
        }

        public void Unsubscribe(IBlingoClockListener listener)
        {
        }
    }

    private sealed class TestBonusesModel : IBonusesModel
    {
        private int _level;

        public int Level
        {
            get => _level;
            set
            {
                if (_level == value)
                {
                    return;
                }

                _level = value;
                LevelChanged?.Invoke(value);
            }
        }

        public event Action<int>? LevelChanged;
    }

    private static void SetField(object instance, string fieldName, object? value)
    {
        var type = instance.GetType();
        while (type is not null)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(instance, value);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException($"Field '{fieldName}' could not be located on type '{instance.GetType()}'");
    }
}
