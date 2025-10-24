using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using AbstUI.Primitives;
using BlingoEngine.Events;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using FluentAssertions;
using static BlingoEngine.Tests.Fakes.ReflectionTestHelper;

namespace BlingoEngine.Tests.Sprites;

public sealed class BlingoSprite2DMemberRectTests
{
    [Fact]
    public void RegPointDefaultsToMemberWhenNoMemberRect()
    {
        var sprite = CreateSprite();
        var member = CreateMember(new APoint(10, 20));

        sprite.SetMember(member);

        sprite.RegPoint.Should().Be(member.RegPoint);
    }

    [Fact]
    public void SetMemberRectOverridesRegPointWhenRectIsProvided()
    {
        var sprite = CreateSprite();
        var member = CreateMember(new APoint(10, 20));
        sprite.SetMember(member);

        var customRect = new ARect(1, 2, 11, 12);
        var customRegPoint = new APoint(3, 4);

        sprite.SetMemberRect(customRect, customRegPoint);

        sprite.MemberSourceRect.Should().Be(customRect);
        sprite.RegPoint.Should().Be(customRegPoint);
    }

    [Fact]
    public void SetMemberRectDefaultsRegPointToTopLeft()
    {
        var sprite = CreateSprite();
        var member = CreateMember(new APoint(10, 20));
        sprite.SetMember(member);

        var rect = ARect.New(4, 6, 12, 14);

        sprite.SetMemberRect(rect);

        sprite.MemberSourceRect.Should().Be(rect);
        sprite.RegPoint.Should().Be(new APoint(0f, 0f));
    }

    [Fact]
    public void ClearingMemberRectRestoresMembersRegPoint()
    {
        var sprite = CreateSprite();
        var member = CreateMember(new APoint(5, 6));
        sprite.SetMember(member);

        sprite.SetMemberRect(new ARect(0, 0, 8, 8), new APoint(1, 1));

        sprite.MemberSourceRect = null;

        sprite.RegPoint.Should().Be(member.RegPoint);
    }

    private static BlingoSprite2D CreateSprite()
    {
        var sprite = (BlingoSprite2D)FormatterServices.GetUninitializedObject(typeof(BlingoSprite2D));
        SetPrivateField(sprite, "_eventMediator", new BlingoEventMediator());
        SetPrivateField(sprite, "_movie", (BlingoMovie)FormatterServices.GetUninitializedObject(typeof(BlingoMovie)));
        SetPrivateField(sprite, "_spritesHolder", new DummySpritesPlayer());
        SetPrivateField(sprite, "_frameworkFactory", null!);
        SetPrivateField(sprite, "_environment", null!);
        SetPrivateField(sprite, "_behaviors", new List<BlingoSpriteBehavior>());
        SetPrivateField(sprite, "_regPoint", default(APoint));
        SetPrivateField(sprite, "_memberSourceRect", null);
        SetPrivateField(sprite, "_memberSourceRectChanged", false);
        SetAutoProperty(sprite, nameof(BlingoSprite2D.ScriptInstanceList), new List<string>());
        SetPrivateField(sprite, "_spriteActors", new List<object>());
        return sprite;
    }

    private static BlingoMember CreateMember(APoint regPoint)
    {
        var member = (BlingoMember)FormatterServices.GetUninitializedObject(typeof(BlingoMember));
        SetPrivateField(member, "_linkedMemberRefUsers", new List<IMemberRefUser>());
        SetPrivateField(member, "_frameworkMember", new DummyFrameworkMember());
        SetPrivateField(member, "_regPoint", regPoint);
        SetPrivateField(member, "_width", 64);
        SetPrivateField(member, "_height", 64);
        SetAutoProperty(member, nameof(BlingoMember.Type), BlingoMemberType.Bitmap);

        return member;
    }

    private sealed class DummyFrameworkMember : IBlingoFrameworkMember
    {
        public bool IsLoaded => true;
        public void CopyToClipboard() { }
        public void Erase() { }
        public void ImportFileInto() { }
        public void PasteClipboardInto() { }
        public void Preload() { }
        public Task PreloadAsync() => Task.CompletedTask;
        public void ReleaseFromSprite(BlingoSprite2D blingoSprite) { }
        public void Unload() { }
        public bool IsPixelTransparent(int x, int y) => false;
    }

    private sealed class DummySpritesPlayer : IBlingoSpritesPlayer
    {
        public int CurrentFrame => 0;
        public int GetMaxLocZ() => 0;
    }
}
