using System;
using System.Collections.Generic;
using System.Text.Json;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.FilmLoops;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Data.DTO.Sprites;
using Xunit;

namespace BlingoEngine.Net.RNetTerminal.Tests;

public class ProjectSerializationTests
{
    [Theory]
    [MemberData(nameof(MemberRoundTripData))]
    public void DeserializeProject_PreservesMemberRuntimeType(
        Func<BlingoMemberDTO> memberFactory,
        Type expectedType,
        Action<BlingoMemberDTO>? additionalAssertions)
    {
        var member = memberFactory();
        var project = new BlingoProjectDTO
        {
            Stage = new BlingoStageDTO { Width = 320, Height = 240 },
            Movies =
            {
                new BlingoMovieDTO
                {
                    Name = "Movie",
                    Number = 1,
                    Casts =
                    {
                        new BlingoCastDTO
                        {
                            Name = "Cast",
                            Number = member.CastLibNum,
                            Members = { member }
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
        var roundTrip = JsonSerializer.Deserialize<BlingoProjectDTO>(json);

        Assert.NotNull(roundTrip);
        var movie = Assert.Single(roundTrip!.Movies);
        var cast = Assert.Single(movie.Casts);
        var actualMember = Assert.Single(cast.Members);

        Assert.IsType(expectedType, actualMember);
        additionalAssertions?.Invoke(actualMember);
    }

    public static IEnumerable<object?[]> MemberRoundTripData()
    {
        yield return new object?[]
        {
            (Func<BlingoMemberDTO>)(() => new BlingoMemberBitmapDTO
            {
                Name = "Bitmap",
                CastLibNum = 1,
                NumberInCast = 1,
                Type = BlingoMemberTypeDTO.Bitmap,
                RegPoint = new BlingoPointDTO { X = 1, Y = 2 },
                Width = 10,
                Height = 20,
                Size = 30,
                Comments = "Bitmap comment",
                FileName = "bitmap.png",
                PurgePriority = 5,
                ImageFile = "bitmap.png"
            }),
            typeof(BlingoMemberBitmapDTO),
            (Action<BlingoMemberDTO>)(member =>
            {
                var bitmap = Assert.IsType<BlingoMemberBitmapDTO>(member);
                Assert.Equal("bitmap.png", bitmap.ImageFile);
            })
        };

        yield return new object?[]
        {
            (Func<BlingoMemberDTO>)(() => new BlingoMemberTextDTO
            {
                Name = "Text",
                CastLibNum = 2,
                NumberInCast = 3,
                Type = BlingoMemberTypeDTO.Text,
                RegPoint = new BlingoPointDTO { X = 3, Y = 4 },
                Width = 40,
                Height = 50,
                Size = 60,
                Comments = "Text comment",
                FileName = "text",
                PurgePriority = 1,
                MarkDownText = "**Hello**"
            }),
            typeof(BlingoMemberTextDTO),
            (Action<BlingoMemberDTO>)(member =>
            {
                var text = Assert.IsType<BlingoMemberTextDTO>(member);
                Assert.Equal("**Hello**", text.MarkDownText);
            })
        };

        yield return new object?[]
        {
            (Func<BlingoMemberDTO>)(() => new BlingoMemberFieldDTO
            {
                Name = "Field",
                CastLibNum = 3,
                NumberInCast = 4,
                Type = BlingoMemberTypeDTO.Field,
                RegPoint = new BlingoPointDTO { X = 5, Y = 6 },
                Width = 70,
                Height = 80,
                Size = 90,
                Comments = "Field comment",
                FileName = "field",
                PurgePriority = 2,
                MarkDownText = "Field text"
            }),
            typeof(BlingoMemberFieldDTO),
            (Action<BlingoMemberDTO>)(member =>
            {
                var field = Assert.IsType<BlingoMemberFieldDTO>(member);
                Assert.Equal("Field text", field.MarkDownText);
            })
        };

        yield return new object?[]
        {
            (Func<BlingoMemberDTO>)(() => new BlingoMemberSoundDTO
            {
                Name = "Sound",
                CastLibNum = 4,
                NumberInCast = 5,
                Type = BlingoMemberTypeDTO.Sound,
                RegPoint = new BlingoPointDTO { X = 7, Y = 8 },
                Width = 15,
                Height = 25,
                Size = 35,
                Comments = "Sound comment",
                FileName = "sound",
                PurgePriority = 3,
                Stereo = true,
                Length = 1.5,
                Loop = true,
                IsLinked = true,
                LinkedFilePath = "linked.wav",
                SoundFile = "sound.wav"
            }),
            typeof(BlingoMemberSoundDTO),
            (Action<BlingoMemberDTO>)(member =>
            {
                var sound = Assert.IsType<BlingoMemberSoundDTO>(member);
                Assert.True(sound.Stereo);
                Assert.Equal(1.5, sound.Length);
                Assert.Equal("linked.wav", sound.LinkedFilePath);
                Assert.Equal("sound.wav", sound.SoundFile);
            })
        };

        yield return new object?[]
        {
            (Func<BlingoMemberDTO>)(() => new BlingoMemberShapeDTO
            {
                Name = "Shape",
                CastLibNum = 5,
                NumberInCast = 6,
                Type = BlingoMemberTypeDTO.Shape,
                RegPoint = new BlingoPointDTO { X = 9, Y = 10 },
                Width = 120,
                Height = 130,
                Size = 140,
                Comments = "Shape comment",
                FileName = "shape",
                PurgePriority = 4,
                FillColor = new BlingoColorDTO(1, 2, 3),
                StrokeColor = new BlingoColorDTO(4, 5, 6),
                StrokeWidth = 2,
                ShapeType = BlingoShapeTypeDto.Oval,
                EndColor = new BlingoColorDTO(7, 8, 9),
                Closed = true,
                Filled = true,
                AntiAlias = true,
                VertexList = { new BlingoPointDTO { X = 0, Y = 0 }, new BlingoPointDTO { X = 1, Y = 1 } }
            }),
            typeof(BlingoMemberShapeDTO),
            (Action<BlingoMemberDTO>)(member =>
            {
                var shape = Assert.IsType<BlingoMemberShapeDTO>(member);
                Assert.Equal(BlingoShapeTypeDto.Oval, shape.ShapeType);
                Assert.True(shape.Filled);
                Assert.Equal(2, shape.VertexList.Count);
            })
        };

        yield return new object?[]
        {
            (Func<BlingoMemberDTO>)(() => new BlingoMemberFilmLoopDTO
            {
                Name = "FilmLoop",
                CastLibNum = 6,
                NumberInCast = 7,
                Type = BlingoMemberTypeDTO.FilmLoop,
                RegPoint = new BlingoPointDTO { X = 11, Y = 12 },
                Width = 210,
                Height = 220,
                Size = 230,
                Comments = "FilmLoop comment",
                FileName = "filmloop",
                PurgePriority = 5,
                Framing = BlingoFilmLoopFramingDTO.Scale,
                Loop = true,
                FrameCount = 24,
                SpriteEntries =
                {
                    new BlingoFilmLoopMemberSpriteDTO
                    {
                        Name = "SpriteEntry",
                        Member = new BlingoMemberRefDTO(1, 1),
                        DisplayMember = 2,
                        SpriteNum = 3,
                        Channel = 4,
                        BeginFrame = 5,
                        EndFrame = 6,
                        Ink = 7,
                        Hilite = true,
                        Blend = 8,
                        LocH = 9,
                        LocV = 10,
                        LocZ = 11,
                        Rotation = 12,
                        Skew = 13,
                        FlipH = true,
                        FlipV = true,
                        RegPoint = new BlingoPointDTO { X = 14, Y = 15 },
                        ForeColor = new BlingoColorDTO(10, 20, 30),
                        BackColor = new BlingoColorDTO(40, 50, 60),
                        Width = 70,
                        Height = 80
                    }
                },
                SoundEntries =
                {
                    new BlingoFilmLoopSoundEntryDTO
                    {
                        Channel = 1,
                        StartFrame = 2,
                        Member = new BlingoMemberRefDTO(3, 4)
                    }
                }
            }),
            typeof(BlingoMemberFilmLoopDTO),
            (Action<BlingoMemberDTO>)(member =>
            {
                var filmLoop = Assert.IsType<BlingoMemberFilmLoopDTO>(member);
                Assert.True(filmLoop.Loop);
                Assert.Equal(24, filmLoop.FrameCount);
                Assert.Single(filmLoop.SpriteEntries);
                Assert.Single(filmLoop.SoundEntries);
            })
        };

        yield return new object?[]
        {
            (Func<BlingoMemberDTO>)(() => new BlingoSpriteBehaviorDTO
            {
                Name = "Behavior",
                CastLibNum = 7,
                NumberInCast = 8,
                Type = BlingoMemberTypeDTO.Script,
                RegPoint = new BlingoPointDTO { X = 16, Y = 17 },
                Width = 310,
                Height = 320,
                Size = 330,
                Comments = "Behavior comment",
                FileName = "behavior",
                PurgePriority = 6,
                BehaviorType = "Custom",
                UserProperties =
                {
                    new BlingoSpriteBehaviorPropertyDTO
                    {
                        Key = "speed",
                        Type = "number",
                        Value = "42"
                    }
                }
            }),
            typeof(BlingoSpriteBehaviorDTO),
            (Action<BlingoMemberDTO>)(member =>
            {
                var behavior = Assert.IsType<BlingoSpriteBehaviorDTO>(member);
                Assert.Equal("Custom", behavior.BehaviorType);
                Assert.Single(behavior.UserProperties);
            })
        };
    }
}
