using System;
using System.Threading;
using System.Threading.Tasks;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetTerminal.Datas;
using BlingoEngine.Net.RNetTerminal.Tests.Fakes;
using Xunit;

namespace BlingoEngine.Net.RNetTerminal.Tests;

public class RNetTerminalConnectionTests
{
    [Fact]
    public async Task ConnectAsync_ProcessesSpriteDeltas()
    {
        var delta = new SpriteDeltaDto(
            Frame: 2,
            SpriteNum: 7,
            BeginFrame: 2,
            Z: 3,
            CastLibNum: 4,
            MemberNum: 5,
            LocH: 200,
            LocV: 300,
            Width: 150,
            Height: 220,
            Rotation: 15,
            Skew: 5,
            Blend: 90,
            Ink: 2);

        var project = CreateProject(delta);
        var movieState = new MovieStateDto(delta.Frame, 30, false);
        var fakeClient = new FakeRNetClient(project, movieState);
        var store = TerminalDataStore.Instance;

        await using var connection = new RNetTerminalConnection(store, _ => fakeClient, action => action());

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await connection.ConnectAsync(new RNetTerminalConnectionOptions(1234, RNetTerminalTransport.Http), cts.Token);
        await fakeClient.WaitForDeltaEnumerationAsync(cts.Token);

        var spriteUpdated = new TaskCompletionSource<Blingo2DSpriteDTO>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Handler(Blingo2DSpriteDTO dto)
        {
            if (dto.SpriteNum == delta.SpriteNum)
            {
                spriteUpdated.TrySetResult(dto);
            }
        }

        store.SpriteChanged += Handler;
        try
        {
            fakeClient.PublishDelta(delta);
            var updated = await spriteUpdated.Task.WaitAsync(cts.Token);

            Assert.Equal(delta.BeginFrame, updated.BeginFrame);
            Assert.Equal(delta.LocH, updated.LocH);
            Assert.Equal(delta.LocV, updated.LocV);
            Assert.Equal(delta.Width, updated.Width);
            Assert.Equal(delta.Height, updated.Height);
            Assert.Equal(delta.Blend, updated.Blend);
            Assert.Equal(delta.Ink, updated.Ink);
        }
        finally
        {
            store.SpriteChanged -= Handler;
        }
    }

    private static BlingoProjectDTO CreateProject(SpriteDeltaDto delta)
    {
        var sprite = new Blingo2DSpriteDTO
        {
            Name = "Sprite",
            SpriteNum = delta.SpriteNum,
            BeginFrame = 1,
            EndFrame = 1,
            LocH = 0,
            LocV = 0,
            LocZ = delta.Z,
            Width = delta.Width,
            Height = delta.Height,
            Rotation = 0,
            Skew = 0,
            Blend = delta.Blend,
            Ink = delta.Ink,
            Member = new BlingoMemberRefDTO(delta.MemberNum, delta.CastLibNum)
        };

        var cast = new BlingoCastDTO
        {
            Name = "Cast",
            Number = delta.CastLibNum,
            Members =
            {
                new BlingoMemberDTO
                {
                    Name = "Member",
                    CastLibNum = delta.CastLibNum,
                    NumberInCast = delta.MemberNum,
                    Type = BlingoMemberTypeDTO.Bitmap,
                    Width = delta.Width,
                    Height = delta.Height,
                    RegPoint = new BlingoPointDTO { X = 0, Y = 0 }
                }
            }
        };

        var movie = new BlingoMovieDTO
        {
            Name = "Movie",
            Number = 1,
            Tempo = 30,
            FrameCount = 120,
            MaxSpriteChannelCount = 120,
            Sprite2Ds = { sprite },
            Casts = { cast }
        };

        return new BlingoProjectDTO
        {
            Stage = new BlingoStageDTO { Width = 640, Height = 480 },
            Movies = { movie }
        };
    }
}
