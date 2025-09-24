using System.Text.Json;
using BlingoEngine.Net.RNetContracts;

namespace BlingoEngine.Net.RNetPipe.Tests;

public class RNetCommandSerializationTests
{
    public static TheoryData<RNetCommand> CommandData => new()
    {
        new SetSpritePropCmd(1, 2, RNetSpriteTypeDto.Sprite2D, "LocH", "42"),
        new SetMemberPropCmd(3, 4, RNetMemberTypeDto.Bitmap, "Name", "Sprite"),
        new SetCastPropCmd(5, "Label", "Cast"),
        new GoToFrameCmd(123),
        new RewindCmd(),
        new PauseCmd(),
        new ResumeCmd(),
    };

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [Theory]
    [MemberData(nameof(CommandData))]
    public void SerializeAsRNetCommand_RoundTrips(RNetCommand command)
    {
        var json = JsonSerializer.Serialize(command, Options);
        var result = JsonSerializer.Deserialize<RNetCommand>(json, Options);

        Assert.NotNull(result);
        Assert.IsType(command.GetType(), result);
        Assert.Equal(command, result);
    }

    [Theory]
    [MemberData(nameof(CommandData))]
    public void SerializeAsObject_RoundTrips(RNetCommand command)
    {
        var json = JsonSerializer.Serialize<object>(command, Options);
        var result = JsonSerializer.Deserialize<RNetCommand>(json, Options);

        Assert.NotNull(result);
        Assert.IsType(command.GetType(), result);
        Assert.Equal(command, result);
    }
}
