using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using Xunit;
using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Cast.Data;

namespace BlingoEngine.IO.Legacy.Tests.Cast;

public class BlLegacyCastReaderTests
{
    [Fact]
    public void ReadDirFileWithThreeSounds_RegistersCastEntries()
    {
        using var harness = TestContextHarness.Open("Sounds/DirFileWith_3_Sounds.dir");
        harness.ReadResources();

        harness.Context.Resources.Entries.Should().Contain(entry => entry.Tag == BlTag.CasStar);

        var libraries = harness.Context.ReadCastLibraries();
        var soundLibrary = libraries.First(library => library.MemberSlots.Any(member => member.ResourceId == 4));

        soundLibrary.EntryCount.Should().Be(4);
        soundLibrary.MemberSlots.Should().HaveCount(3);
        soundLibrary.MemberSlots.Select(member => member.ResourceId).Should().BeEquivalentTo(new[] { 4, 5, 6 });
    }

    [Fact]
    public void ReadDirFileWithThreeSounds_ReadsMemberNames()
    {
        var libraries = TestContextHarness.LoadCastLibraries("Sounds/DirFileWith_3_Sounds.dir");
        var soundLibrary = libraries.First(library => library.MemberSlots.Any(member => member.ResourceId == 4));

        soundLibrary.MemberSlots.Select(member => member.Member.Name)
            .Should()
            .Contain(new[] { "level_up", "go", "blockfall_1" });

        soundLibrary.MemberSlots.Select(member => member.Member.MemberType)
            .Should()
            .AllBeEquivalentTo(BlCastRawMemberType.Sound);
    }

    [Fact]
    public void ReadFieldCast_ReadsMemberName()
    {
        var libraries = TestContextHarness.LoadCastLibraries("Texts_Fields/Field_Hallo_3lines.cst");
        var fieldLibrary = libraries.First();

        fieldLibrary.MemberSlots.Should().ContainSingle();
        var member = fieldLibrary.MemberSlots[0];

        member.Member.Name.Should().Be("My field");
        member.Member.MemberType.Should().BeOneOf(BlCastRawMemberType.Field, BlCastRawMemberType.Text);
    }

    [Fact]
    public void ReaderContext_Dispose_ClosesUnderlyingStream()
    {
        var stream = new TrackingStream();

        using (var context = new ReaderContext(stream, "mock.dir", leaveOpen: false))
        {
            context.Should().NotBeNull();
        }

        stream.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void ReaderContext_LeaveOpen_DoesNotDisposeStream()
    {
        var stream = new TrackingStream();

        using (var context = new ReaderContext(stream, "mock.dir", leaveOpen: true))
        {
            context.Should().NotBeNull();
        }

        stream.IsDisposed.Should().BeFalse();
        stream.Dispose();
    }

   
}

   

