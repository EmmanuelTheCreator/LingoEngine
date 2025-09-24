using System.Collections.Generic;
using System.Linq;
using BlingoEngine.Events;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Tests.Fakes;

namespace BlingoEngine.Tests;

public class MovieEventOrderTests
{
    [Fact]
    public void AdvanceFrame_RaisesFrameLifecycleEventsInManualOrder()
    {
        var timeline = new List<string>();
        var mediator = new BlingoEventMediator();
        var frameHandler = new RecordingFrameHandler(timeline);
        mediator.Subscribe(frameHandler);
        mediator.SubscribeStepFrame(frameHandler);

        var harness = FakeBlingoMovieBuilder.Create(mediator, timeline);
        PrivateFieldSetter.SetField(harness.Movie, "_isPlaying", true);

        harness.Movie.AdvanceFrame();
        harness.Movie.OnIdle(1f / 60f);
        harness.Movie.AdvanceFrame();

        var expected = new[]
        {
            "beginSprite",
            "stepFrame",
            "prepareFrame",
            "enterFrame",
            "idleFrame",
            "exitFrame",
            "endSprite"
        };

        Assert.True(timeline.Count >= expected.Length, "timeline missing expected callbacks");
        Assert.Equal(expected, timeline.Take(expected.Length));
    }
}

public class TransitionEventOrderTests
{
    [Fact]
    public void AdvanceFrame_RaisesTransitionLifecycleAroundFrameHandlers()
    {
        var timeline = new List<string>();
        var mediator = new BlingoEventMediator();
        var frameHandler = new RecordingFrameHandler(timeline);
        mediator.Subscribe(frameHandler);
        mediator.SubscribeStepFrame(frameHandler);

        var harness = FakeBlingoMovieBuilder.Create(mediator, timeline, options =>
        {
            options.RecordTransitionLifecycle = true;
            options.TransitionActivationFrame = 1;
        });

        harness.TransitionPlayer.StartResult = false;
        PrivateFieldSetter.SetField(harness.Movie, "_isPlaying", true);

        harness.Movie.AdvanceFrame();
        harness.Movie.OnIdle(1f / 60f);
        harness.Movie.AdvanceFrame();

        var expected = new[]
        {
            "beginSprite",
            "transition.beginSprite",
            "stepFrame",
            "prepareFrame",
            "enterFrame",
            "idleFrame",
            "exitFrame",
            "endSprite",
            "transition.endSprite"
        };

        Assert.True(timeline.Count >= expected.Length, "timeline missing expected callbacks");
        Assert.Equal(expected, timeline.Take(expected.Length));
        Assert.Equal(1, harness.TransitionPlayer.StartCallCount);
    }
}

internal sealed class RecordingFrameHandler : IHasStepFrameEvent, IHasPrepareFrameEvent,
    IHasEnterFrameEvent, IHasIdleFrameEvent, IHasExitFrameEvent
{
    private readonly List<string> _timeline;

    internal RecordingFrameHandler(List<string> timeline) => _timeline = timeline;

    public void StepFrame() => _timeline.Add("stepFrame");

    public void PrepareFrame() => _timeline.Add("prepareFrame");

    public void EnterFrame() => _timeline.Add("enterFrame");

    public void IdleFrame() => _timeline.Add("idleFrame");

    public void ExitFrame() => _timeline.Add("exitFrame");
}
