using System;
using System.Collections.Generic;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetPipe.Tests.Fakes;
using BlingoEngine.Net.RNetPipeServer;

namespace BlingoEngine.Net.RNetPipe.Tests;

public class RNetPipePublisherTests
{
    [Fact]
    public void TryPublish_PushesImmediateMessagesToBus()
    {
        var bus = new TestPipeBus();
        var publisher = new RNetPipePublisher(bus);

        var frame = new StageFrameDto(320, 240, 7, new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc), new byte[] { 1, 2, 3 });
        publisher.TryPublishFrame(frame);
        Assert.True(bus.Frames.Reader.TryRead(out var actualFrame));
        Assert.Equal(frame.Width, actualFrame.Width);
        Assert.Equal(frame.Height, actualFrame.Height);
        Assert.Equal(frame.FrameId, actualFrame.FrameId);
        Assert.Equal(frame.TimestampUtc, actualFrame.TimestampUtc);
        Assert.Equal(frame.Argb32, actualFrame.Argb32);

        var keyframe = new KeyframeDto(12, 3, 1, "Blend", "80");
        publisher.TryPublishKeyframe(keyframe);
        Assert.True(bus.Keyframes.Reader.TryRead(out var actualKeyframe));
        Assert.Equal(keyframe, actualKeyframe);

        var filmLoop = new FilmLoopDto(1, 2, 5, 15, true);
        publisher.TryPublishFilmLoop(filmLoop);
        Assert.True(bus.FilmLoops.Reader.TryRead(out var actualFilmLoop));
        Assert.Equal(filmLoop, actualFilmLoop);

        var sound = new SoundEventDto(9, 4, 6, true);
        publisher.TryPublishSound(sound);
        Assert.True(bus.Sounds.Reader.TryRead(out var actualSound));
        Assert.Equal(sound, actualSound);

        var tempo = new TempoDto(18, 144);
        publisher.TryPublishTempo(tempo);
        Assert.True(bus.Tempos.Reader.TryRead(out var actualTempo));
        Assert.Equal(tempo, actualTempo);

        var palette = new ColorPaletteDto(21, new byte[] { 10, 20, 30, 40 });
        publisher.TryPublishColorPalette(palette);
        Assert.True(bus.ColorPalettes.Reader.TryRead(out var actualPalette));
        Assert.Equal(palette.Frame, actualPalette.Frame);
        Assert.Equal(palette.Argb32, actualPalette.Argb32);

        var script = new FrameScriptDto(24, "go to frame 30");
        publisher.TryPublishFrameScript(script);
        Assert.True(bus.FrameScripts.Reader.TryRead(out var actualScript));
        Assert.Equal(script, actualScript);

        var transition = new TransitionDto(27, "iris", 8);
        publisher.TryPublishTransition(transition);
        Assert.True(bus.Transitions.Reader.TryRead(out var actualTransition));
        Assert.Equal(transition, actualTransition);

        var textStyle = new TextStyleDto(2, 6, 0, 4, "FontStyle", "Italic");
        publisher.TryPublishTextStyle(textStyle);
        Assert.True(bus.TextStyles.Reader.TryRead(out var actualTextStyle));
        Assert.Equal(textStyle, actualTextStyle);

        var spriteEvent = new RNetSpriteCollectionEventDto("Sprite2D", RNetSpriteCollectionEventType.Cleared, 0, 0, null);
        publisher.TryPublishSpriteCollectionEvent(spriteEvent);
        Assert.True(bus.SpriteCollectionEvents.Reader.TryRead(out var actualSpriteEvent));
        Assert.Equal(spriteEvent, actualSpriteEvent);
    }

    [Fact]
    public void TryPublish_SendsMessagesAndDrainsCommands()
    {
        var bus = new TestPipeBus();
        var publisher = new RNetPipePublisher(bus);

        var delta = new SpriteDeltaDto(3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
        publisher.TryPublishDelta(delta);
        Assert.True(bus.Deltas.Reader.TryRead(out var actualDelta));
        Assert.Equal(delta, actualDelta);

        var memberProperty = new RNetMemberPropertyDto(5, 9, "Score", "1000");
        publisher.TryPublishMemberProperty(memberProperty);
        Assert.True(bus.MemberProperties.Reader.TryRead(out var actualMember));
        Assert.Equal(memberProperty, actualMember);

        var movieProperty = new RNetMoviePropertyDto("Title", "TestMovie");
        publisher.TryPublishMovieProperty(movieProperty);
        Assert.True(bus.MovieProperties.Reader.TryRead(out var actualMovie));
        Assert.Equal(movieProperty, actualMovie);

        var stageProperty = new RNetStagePropertyDto("Background", "Black");
        publisher.TryPublishStageProperty(stageProperty);
        Assert.True(bus.StageProperties.Reader.TryRead(out var actualStage));
        Assert.Equal(stageProperty, actualStage);

        publisher.FlushQueuedProperties();

        Assert.False(bus.Deltas.Reader.TryRead(out _));
        Assert.False(bus.MemberProperties.Reader.TryRead(out _));
        Assert.False(bus.MovieProperties.Reader.TryRead(out _));
        Assert.False(bus.StageProperties.Reader.TryRead(out _));

        var commands = new List<IRNetCommand>
        {
            new GoToFrameCmd(12),
            new ResumeCmd()
        };

        foreach (var cmd in commands)
        {
            Assert.True(bus.Commands.Writer.TryWrite(cmd));
        }

        var drained = new List<IRNetCommand>();
        Assert.True(publisher.TryDrainCommands(drained.Add));
        Assert.Equal(commands, drained);
        Assert.False(publisher.TryDrainCommands(_ => throw new InvalidOperationException("should not drain")));
    }
}

