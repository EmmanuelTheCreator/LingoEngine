using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetTerminal;
using BlingoEngine.Net.RNetTerminal.Datas;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.Net.RNetTerminal.Views;
using BlingoEngine.IO.Data.DTO.Sprites;

namespace BlingoEngine.Net.RNetTerminal.Tests;

public class TerminalDataStoreTests
{
    [Fact]
    public void ApplySpriteDelta_RemoteMove_RepositionsSpriteAndSelection()
    {
        var store = TerminalDataStore.Instance;
        store.LoadTestData();

        var sprite = store.GetSprites().First(s => s.Member is not null);
        var originalBegin = sprite.BeginFrame;
        var originalEnd = sprite.EndFrame;
        var originalApplyLocalChanges = store.ApplyLocalChanges;
        store.SelectSprite(new SpriteRef(sprite.SpriteNum, originalBegin));

        var deltaFrames = Math.Min(5, Math.Max(1, store.FrameCount - originalBegin - 1));
        if (deltaFrames == 0)
        {
            deltaFrames = 1;
        }

        SpriteRef? requestedSprite = null;
        int? requestedBegin = null;
        int? requestedEnd = null;

        void Handler(SpriteRef s, int begin, int end)
        {
            requestedSprite = s;
            requestedBegin = begin;
            requestedEnd = end;
        }

        store.ApplyLocalChanges = false;
        store.SpriteMoveRequested += Handler;

        try
        {
            store.MoveSprite(new SpriteRef(sprite.SpriteNum, originalBegin), deltaFrames);
        }
        finally
        {
            store.SpriteMoveRequested -= Handler;
        }

        sprite.BeginFrame.Should().Be(originalBegin);
        sprite.EndFrame.Should().Be(originalEnd);

        requestedSprite.Should().NotBeNull();
        requestedSprite!.Value.SpriteNum.Should().Be(sprite.SpriteNum);
        requestedSprite.Value.BeginFrame.Should().Be(originalBegin);
        requestedBegin.Should().NotBeNull();
        requestedEnd.Should().NotBeNull();

        var delta = new SpriteDeltaDto(
            requestedBegin!.Value,
            sprite.SpriteNum,
            requestedBegin.Value,
            sprite.LocZ,
            sprite.Member!.CastLibNum,
            sprite.Member.MemberNum,
            (int)Math.Round(sprite.LocH),
            (int)Math.Round(sprite.LocV),
            (int)Math.Round(sprite.Width),
            (int)Math.Round(sprite.Height),
            (int)Math.Round(sprite.Rotation),
            (int)Math.Round(sprite.Skew),
            (int)Math.Round(sprite.Blend),
            sprite.Ink);

        store.ApplySpriteDelta(delta);

        sprite.BeginFrame.Should().Be(requestedBegin.Value);
        sprite.EndFrame.Should().Be(requestedEnd.Value);
        store.FindSprite(new SpriteRef(sprite.SpriteNum, requestedBegin.Value)).Should().BeSameAs(sprite);

        var selection = store.GetSelectedSprite();
        selection.Should().NotBeNull();
        selection!.Value.SpriteNum.Should().Be(sprite.SpriteNum);
        selection.Value.BeginFrame.Should().Be(requestedBegin.Value);

        store.ApplyLocalChanges = originalApplyLocalChanges;
        store.LoadTestData();
    }

    [Fact]
    public void PropertyHasChanged_RemoteMemberUpdate_WaitsForHostAndAppliesDelta()
    {
        var store = TerminalDataStore.Instance;
        store.LoadTestData();

        var originalApplyLocalChanges = store.ApplyLocalChanges;
        var sprite = store.GetSprites().First(s => s.Member is not null);
        var member = store.FindMember(sprite.Member!.CastLibNum, sprite.Member.MemberNum)!;
        var originalName = member.Name;
        var updatedName = originalName + "_remote";

        store.ApplyLocalChanges = false;

        var changeCount = 0;
        BlingoMemberDTO? changedMember = null;

        void Handler(BlingoMemberDTO m)
        {
            changeCount++;
            changedMember = m;
        }

        store.MemberChanged += Handler;

        try
        {
            store.PropertyHasChanged(PropertyTarget.Member, "Name", updatedName, member);

            member.Name.Should().Be(originalName);
            changeCount.Should().Be(0);

            store.ApplyMemberProperty(new RNetMemberPropertyDto(member.CastLibNum, member.NumberInCast, "Name", updatedName));

            member.Name.Should().Be(updatedName);
            changeCount.Should().Be(1);
            changedMember.Should().NotBeNull();
            changedMember!.Name.Should().Be(updatedName);
            changedMember.CastLibNum.Should().Be(member.CastLibNum);
            changedMember.NumberInCast.Should().Be(member.NumberInCast);
        }
        finally
        {
            store.MemberChanged -= Handler;
            store.ApplyLocalChanges = originalApplyLocalChanges;
            store.LoadTestData();
        }
    }

    [Fact]
    public void PropertyHasChanged_RemoteSpriteUpdate_WaitsForHostDelta()
    {
        var store = TerminalDataStore.Instance;
        store.LoadTestData();

        var originalApplyLocalChanges = store.ApplyLocalChanges;
        var sprite = store.GetSprites().First(s => s.Member is not null);
        store.SelectSprite(new SpriteRef(sprite.SpriteNum, sprite.BeginFrame));
        var originalLocH = sprite.LocH;
        var desiredLocHInt = (int)Math.Round(sprite.LocH + 10);
        var desiredLocH = (float)desiredLocHInt;

        store.ApplyLocalChanges = false;

        var changeCount = 0;
        Blingo2DSpriteDTO? changedSprite = null;

        void Handler(Blingo2DSpriteDTO s)
        {
            if (s.SpriteNum == sprite.SpriteNum && s.BeginFrame == sprite.BeginFrame)
            {
                changeCount++;
                changedSprite = s;
            }
        }

        store.SpriteChanged += Handler;

        try
        {
            store.PropertyHasChanged(PropertyTarget.Sprite, "LocH", desiredLocH.ToString(CultureInfo.InvariantCulture));

            sprite.LocH.Should().Be(originalLocH);
            changeCount.Should().Be(0);

            var delta = new SpriteDeltaDto(
                sprite.BeginFrame,
                sprite.SpriteNum,
                sprite.BeginFrame,
                sprite.LocZ,
                sprite.Member!.CastLibNum,
                sprite.Member.MemberNum,
                desiredLocHInt,
                (int)Math.Round(sprite.LocV),
                (int)Math.Round(sprite.Width),
                (int)Math.Round(sprite.Height),
                (int)Math.Round(sprite.Rotation),
                (int)Math.Round(sprite.Skew),
                (int)Math.Round(sprite.Blend),
                sprite.Ink);

            store.ApplySpriteDelta(delta);

            sprite.LocH.Should().Be(desiredLocH);
            changeCount.Should().Be(1);
            changedSprite.Should().NotBeNull();
            changedSprite!.LocH.Should().Be(desiredLocH);
        }
        finally
        {
            store.SpriteChanged -= Handler;
            store.ApplyLocalChanges = originalApplyLocalChanges;
            store.LoadTestData();
        }
    }

    [Fact]
    public void SetFrame_UpdatesMovieStateFrameAndRaisesEvent()
    {
        var store = TerminalDataStore.Instance;
        store.LoadTestData();

        MovieStateDto? observed = null;
        void Handler(MovieStateDto state) => observed = state;

        store.MovieStateChanged += Handler;

        try
        {
            var targetFrame = store.MovieState.Frame + 5;
            store.SetFrame(targetFrame);

            store.MovieState.Frame.Should().Be(targetFrame);
            store.GetFrame().Should().Be(targetFrame);
            observed.Should().NotBeNull();
            observed!.Frame.Should().Be(targetFrame);
        }
        finally
        {
            store.MovieStateChanged -= Handler;
            store.LoadTestData();
        }
    }

    [Fact]
    public void ApplyMovieProperty_UpdatesMovieState()
    {
        var store = TerminalDataStore.Instance;
        store.LoadTestData();

        var initialTempo = store.MovieState.Tempo;
        var newTempo = initialTempo + 10;
        MovieStateDto? observed = null;

        void Handler(MovieStateDto state) => observed = state;

        store.MovieStateChanged += Handler;

        try
        {
            store.ApplyMovieProperty(new RNetMoviePropertyDto("Tempo", newTempo.ToString(CultureInfo.InvariantCulture)));
            store.ApplyMovieProperty(new RNetMoviePropertyDto("IsPlaying", "true"));
            store.ApplyMovieProperty(new RNetMoviePropertyDto("CurrentFrame", "12"));

            store.MovieState.Tempo.Should().Be(newTempo);
            store.MovieState.IsPlaying.Should().BeTrue();
            store.MovieState.Frame.Should().Be(12);
            observed.Should().NotBeNull();
        }
        finally
        {
            store.MovieStateChanged -= Handler;
            store.LoadTestData();
        }
    }
}

