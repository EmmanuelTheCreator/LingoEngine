using AbstUI.Primitives;
using BlingoEngine.Animations;
using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.ColorPalettes;
using BlingoEngine.Core;
using BlingoEngine.Events;
using BlingoEngine.FilmLoops;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.FilmLoops;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.Medias;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Scripts;
using BlingoEngine.Shapes;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using BlingoEngine.Tempos;
using BlingoEngine.Texts;
using BlingoEngine.Transitions;
using System;
using System.IO;
using System.Text.Json;

namespace BlingoEngine.IO;

public interface IJsonStateRepository
{
    BlingoMovie Load(BlingoProjectDTO dto, BlingoPlayer player, string resourceDir);
    BlingoMovie Load(BlingoStageDTO stageDto, BlingoMovieDTO movieDto, BlingoPlayer player, string resourceDir);
    BlingoMovie Load(BlingoStageDTO stageDto, BlingoMovieDTO movieDto, DirFilesContainerDTO resources, BlingoPlayer player, string resourceDir);
    BlingoMovie Load(string filePath, BlingoPlayer player);

    (string JsonString, BlingoMovieDTO MovieDto) Save(string filePath, BlingoMovie movie, JsonStateRepository.MovieStoreOptions? options = null);


    BlingoProjectDTO ToProjectDto(BlingoPlayer player, JsonStateRepository.MovieStoreOptions options);

    (string JsonString, BlingoMovieDTO MovieDto) Serialize(BlingoMovie movie, JsonStateRepository.MovieStoreOptions options);
    string SerializeProject(BlingoPlayer player, JsonStateRepository.MovieStoreOptions options);
    BlingoMovieDTO Deserialize(string json);
}

public class JsonStateRepository : IJsonStateRepository
{
    private readonly BlingoBinaryRepository _binaryRepository;

    public JsonStateRepository()
        : this(new BlingoBinaryRepository())
    {
    }

    public JsonStateRepository(BlingoBinaryRepository binaryRepository)
    {
        _binaryRepository = binaryRepository;
    }

    public class MovieStoreOptions
    {
        public bool WithMedia { get; set; }
        public string TargetDirectory { get; set; } = "";
    }
    public (string JsonString, BlingoMovieDTO MovieDto) Save(string filePath, BlingoMovie movie, MovieStoreOptions? options = null)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(dir))
            dir = Directory.GetCurrentDirectory();
        if (options == null) options = new();
        options.TargetDirectory = dir;
        var jsonTuple = Serialize(movie, options);
        File.WriteAllText(filePath, jsonTuple.JsonString);
        return jsonTuple;
    }

    public string SerializeProject(BlingoPlayer player, MovieStoreOptions options)
    {
        var dto = ToProjectDto(player, options);
        var joptions = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(dto, joptions);
        return json;
    }
    public BlingoProjectDTO ToProjectDto(BlingoPlayer player, MovieStoreOptions options)
    {
        BlingoProjectDTO projectDTO = new BlingoProjectDTO
        {
            Stage = new BlingoStageDTO
            {
                Width = player.Stage.Width,
                Height = player.Stage.Height,
                BackgroundColor = player.Stage.BackgroundColor.ToDto(),
            },
        };
        if (player.ActiveMovie != null)
            projectDTO.Movies = [((BlingoMovie)player.ActiveMovie).ToDto(options)];
        return projectDTO;
    }
    public (string JsonString, BlingoMovieDTO MovieDto) Serialize(BlingoMovie movie, MovieStoreOptions options)
    {
        BlingoMovieDTO dto = movie.ToDto(options);
        var joptions = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(dto, joptions);
        return (json, dto);
    }

    public BlingoMovie Load(string filePath, BlingoPlayer player)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(dir))
            dir = Directory.GetCurrentDirectory();

        var json = File.ReadAllText(filePath);
        BlingoProjectDTO dto = DeserializeProject(json);

        return Load(dto, player, dir);
    }

    public BlingoProjectDTO DeserializeProject(string json)
    {
        return JsonSerializer.Deserialize<BlingoProjectDTO>(json) ?? throw new Exception("Invalid project file");
    }
    public BlingoMovieDTO Deserialize(string json)
    {
        return JsonSerializer.Deserialize<BlingoMovieDTO>(json) ?? throw new Exception("Invalid movie file");
    }

    public BlingoMovie Load(BlingoProjectDTO dto, BlingoPlayer player, string resourceDir)
    {
        resourceDir = EnsureResourceDirectory(resourceDir);
        var movieDto = dto.Movies.First();
        _binaryRepository.Initialize(movieDto, resourceDir);
        BuildStageFromDto(dto.Stage, player);
        return BuildMovieFromDto(movieDto, player);
    }
    public BlingoMovie Load(BlingoStageDTO stageDto, BlingoMovieDTO movieDto, BlingoPlayer player, string resourceDir)
    {
        resourceDir = EnsureResourceDirectory(resourceDir);
        _binaryRepository.Initialize(movieDto, resourceDir);

        BuildStageFromDto(stageDto, player);
        return BuildMovieFromDto(movieDto, player);
    }

    public BlingoMovie Load(BlingoStageDTO stageDto, BlingoMovieDTO movieDto, DirFilesContainerDTO resources, BlingoPlayer player, string resourceDir)
    {
        ArgumentNullException.ThrowIfNull(resources);

        resourceDir = EnsureResourceDirectory(resourceDir);

        _binaryRepository.Initialize(movieDto, resourceDir);
        _binaryRepository.PersistResources(resources);

        BuildStageFromDto(stageDto, player);
        return BuildMovieFromDto(movieDto, player);
    }

    private static void BuildStageFromDto(BlingoStageDTO dtoStage, BlingoPlayer player)
    {
        player.LoadStage((int)dtoStage.Width, (int)dtoStage.Height, FromDto(dtoStage.BackgroundColor));
    }
    private BlingoMovie BuildMovieFromDto(BlingoMovieDTO dto, BlingoPlayer player)
    {
        IBlingoEventMediator mediator = player.GetEventMediator();
        var movie = (BlingoMovie)player.NewMovie(dto.Name);
        movie.Tempo = dto.Tempo;
        movie.About = dto.About;
        movie.Copyright = dto.Copyright;
        movie.UserName = dto.UserName;
        movie.CompanyName = dto.CompanyName;

        var castMap = new Dictionary<int, BlingoCast>();
        var memberMap = new Dictionary<(int CastNum, int MemberNum), BlingoMember>();

        foreach (var castDto in dto.Casts)
        {
            var cast = (BlingoCast)movie.CastLib.AddCast(castDto.Name);
            cast.PreLoadMode = (PreLoadModeType)castDto.PreLoadMode;
            castMap[castDto.Number] = cast;

            foreach (var memDto in castDto.Members)
            {
                var reg = new APoint(memDto.RegPoint.X, memDto.RegPoint.Y);
                string fileName = memDto.FileName;
                if (memDto is BlingoMemberScriptDTO scriptDto)
                    fileName = _binaryRepository.WriteScriptFile(scriptDto);
                else if (memDto is BlingoMemberBitmapDTO bitmapDto)
                    fileName = _binaryRepository.ResolveBinaryPath(memDto, bitmapDto.ImageFile);
                else if (memDto is BlingoMemberSoundDTO soundDto)
                    fileName = _binaryRepository.ResolveBinaryPath(memDto, soundDto.SoundFile);
                else if (memDto is BlingoMemberVideoDTO videoDto)
                    fileName = _binaryRepository.ResolveBinaryPath(memDto, videoDto.LinkedFileName);
                else
                {
                    var stored = _binaryRepository.TryGetStoredPath(memDto);
                    if (!string.IsNullOrEmpty(stored))
                        fileName = stored;
                }

                var member = (BlingoMember)cast.Add((BlingoMemberType)memDto.Type, memDto.NumberInCast, memDto.Name, fileName, reg);
                member.FileName = fileName;
                member.Width = memDto.Width;
                member.Height = memDto.Height;
                member.Size = memDto.Size;
                member.Comments = memDto.Comments;
                member.PurgePriority = memDto.PurgePriority;

                if (member is BlingoMemberText txt && memDto is BlingoMemberTextDTO txtDto)
                    txt.SetTextMD(txtDto.MarkDownText);
                if (member is BlingoMemberField fld && memDto is BlingoMemberFieldDTO fldDto)
                    fld.SetTextMD(fldDto.MarkDownText);
                if (member is BlingoMemberSound sndMem && memDto is BlingoMemberSoundDTO sndDto)
                {
                    sndMem.Loop = sndDto.Loop;
                    sndMem.IsLinked = sndDto.IsLinked;
                    sndMem.LinkedFilePath = fileName;
                    sndMem.ImportedStereo = sndDto.Stereo;
                    sndMem.OriginalLength = sndDto.Length;
                    sndMem.SourceFileName = sndDto.SoundFile ?? sndDto.LinkedFilePath ?? Path.GetFileName(fileName);
                }
                if (member is BlingoMemberScript scriptMem && memDto is BlingoMemberScriptDTO scriptDto2)
                {
                    scriptMem.ScriptType = MapScriptType(scriptDto2.ScriptType);
                    scriptMem.IsJavascript = scriptDto2.IsJavascript;
                    scriptMem.LinkedFilePath = fileName;
                    scriptMem.ScriptSource = scriptDto2.Script ?? string.Empty;
                }
                if (member is BlingoMemberMedia mediaMem && memDto is BlingoMemberVideoDTO videoDto2)
                {
                    mediaMem.PlayVideo = videoDto2.PlayVideo;
                    mediaMem.PlayAudio = videoDto2.PlayAudio;
                    mediaMem.StartPause = videoDto2.StartPause;
                    mediaMem.EnableLoop = videoDto2.EnableLoop;
                    mediaMem.StartValueMs = videoDto2.StartValueMs;
                    mediaMem.VideoFps = videoDto2.VideoFps;
                    mediaMem.DurationSeconds = videoDto2.DurationSeconds;
                    mediaMem.LinkedFileName = Path.GetFileName(fileName);
                    mediaMem.LinkedFolder = Path.GetDirectoryName(fileName) ?? string.Empty;
                }
                if (member is BlingoFilmLoopMember flMem && memDto is BlingoMemberFilmLoopDTO flDto)
                {
                    flMem.Framing = (BlingoFilmLoopFraming)flDto.Framing;
                    flMem.Loop = flDto.Loop;
                    foreach (var sEntry in flDto.SpriteEntries)
                        flMem.AddSprite(BuildSprite2DVirtualFromDto(sEntry));
                    foreach (var sndEntry in flDto.SoundEntries)
                    {
                        if (TryResolveMember<BlingoMemberSound>(memberMap, sndEntry.Member, out var sndMem2) && sndMem2 != null)
                            flMem.AddSound(sndEntry.Channel, sndEntry.StartFrame, sndMem2);
                    }
                }
                if (member is BlingoMemberShape shapeMem && memDto is BlingoMemberShapeDTO shapeDto)
                {
                    ShapeDtoConverter.Apply(shapeDto, shapeMem);
                }

                memberMap[(member.CastLibNum, member.NumberInCast)] = member;
            }
        }

        foreach (var sDto in dto.Sprite2Ds)
            BuildSpriteFromDto(sDto, movie, memberMap, mediator);

        foreach (var tempoDto in dto.TempoSprites)
            BuildTempoSpriteFromDto(tempoDto, movie);

        foreach (var paletteDto in dto.ColorPaletteSprites)
            BuildColorPaletteSpriteFromDto(paletteDto, movie, memberMap);

        foreach (var transitionDto in dto.TransitionSprites)
            BuildTransitionSpriteFromDto(transitionDto, movie, memberMap);

        foreach (var soundDto in dto.SoundSprites)
            BuildSoundSpriteFromDto(soundDto, movie, memberMap);

        return movie;
    }

    private static BlingoSprite2D BuildSpriteFromDto(Blingo2DSpriteDTO sDto, BlingoMovie movie, Dictionary<(int CastNum, int MemberNum), BlingoMember> memberMap, IBlingoEventMediator eventMediator)
    {
        BlingoSprite2D sprite;

        sprite = movie.AddSprite(sDto.SpriteNum, sDto.Name, s =>
        {
            s.Lock = sDto.Lock;
            s.Visibility = sDto.Visibility;
            s.BeginFrame = sDto.BeginFrame;
            s.EndFrame = sDto.EndFrame;
        });

        var state = new BlingoSprite2DState
        {
            Name = sDto.Name,
            DisplayMember = sDto.DisplayMember,
            SpritePropertiesOffset = sDto.SpritePropertiesOffset,
            Ink = sDto.Ink,
            Blend = sDto.Blend,
            LocH = sDto.LocH,
            LocV = sDto.LocV,
            LocZ = sDto.LocZ,
            Rotation = sDto.Rotation,
            Skew = sDto.Skew,
            RegPoint = new APoint(sDto.RegPoint.X, sDto.RegPoint.Y),
            ForeColor = FromDto(sDto.ForeColor),
            BackColor = FromDto(sDto.BackColor),
            Editable = sDto.Editable,
            Width = sDto.Width,
            Height = sDto.Height,
            FlipH = sDto.FlipH,
            FlipV = sDto.FlipV,
        };

        if (TryResolveMember(memberMap, sDto.Member, out var mem) && mem != null)
        {
            state.Member = mem;
            sprite.SetMember(mem);
        }

        sprite.InitialState = state;
        sprite.LoadState(state);

        AddAnimator(sDto, sprite, movie, eventMediator);
        foreach (var behaviorDto in sDto.Behaviors)
        {
            behaviorDto.Apply(sprite, memberMap);
        }
        return sprite;
    }

    private static void BuildTempoSpriteFromDto(BlingoTempoSpriteDTO dto, BlingoMovie movie)
    {
        movie.Tempos.Add(dto.BeginFrame, sprite => TempoSpriteDtoConverter.Apply(dto, sprite));
    }

    private static void BuildColorPaletteSpriteFromDto(BlingoColorPaletteSpriteDTO dto, BlingoMovie movie, Dictionary<(int CastNum, int MemberNum), BlingoMember> memberMap)
    {
        var sprite = movie.ColorPalettes.Add(dto.BeginFrame);

        if (TryResolveMember<BlingoColorPaletteMember>(memberMap, dto.Member, out var paletteMember) && paletteMember != null)
        {
            sprite.SetMember(paletteMember);
        }

        ColorPaletteSpriteDtoConverter.Apply(dto, sprite);
        sprite.InitialState = sprite.GetState();
    }

    private static void BuildTransitionSpriteFromDto(BlingoTransitionSpriteDTO dto, BlingoMovie movie, Dictionary<(int CastNum, int MemberNum), BlingoMember> memberMap)
    {
        var sprite = movie.Transitions.Add(dto.BeginFrame);

        if (TryResolveMember<BlingoTransitionMember>(memberMap, dto.Member, out var transitionMember) && transitionMember != null)
        {
            sprite.SetMember(transitionMember);
        }

        TransitionSpriteDtoConverter.Apply(dto, sprite);
        sprite.InitialState = sprite.GetState();
    }

    private static void BuildSoundSpriteFromDto(BlingoSpriteSoundDTO dto, BlingoMovie movie, Dictionary<(int CastNum, int MemberNum), BlingoMember> memberMap)
    {
        if (!TryResolveMember<BlingoMemberSound>(memberMap, dto.Member, out var soundMember) || soundMember == null)
        {
            return;
        }

        var sprite = movie.Audio.Add(dto.Channel, dto.BeginFrame, soundMember);
        SoundSpriteDtoConverter.Apply(dto, sprite);
        sprite.InitialState = sprite.GetState();
    }

    private static BlingoScriptType MapScriptType(BlingoMemberScriptDTO.BlScriptType scriptType)
    {
        return scriptType switch
        {
            BlingoMemberScriptDTO.BlScriptType.ParentScript => BlingoScriptType.Parent,
            BlingoMemberScriptDTO.BlScriptType.MovieScript => BlingoScriptType.Movie,
            _ => BlingoScriptType.Behavior
        };
    }

    private static bool TryResolveMember(Dictionary<(int CastNum, int MemberNum), BlingoMember> memberMap, BlingoMemberRefDTO? reference, out BlingoMember? member)
    {
        member = null;
        if (reference == null || reference.CastLibNum <= 0 || reference.MemberNum <= 0)
        {
            return false;
        }

        if (memberMap.TryGetValue((reference.CastLibNum, reference.MemberNum), out var found))
        {
            member = found;
            return true;
        }

        return false;
    }

    private static bool TryResolveMember<TMember>(Dictionary<(int CastNum, int MemberNum), BlingoMember> memberMap, BlingoMemberRefDTO? reference, out TMember? member)
        where TMember : BlingoMember
    {
        member = null;
        if (!TryResolveMember(memberMap, reference, out var baseMember))
        {
            return false;
        }

        if (baseMember is TMember typed)
        {
            member = typed;
            return true;
        }

        return false;
    }
    private static BlingoFilmLoopMemberSprite BuildSprite2DVirtualFromDto(BlingoFilmLoopMemberSpriteDTO sDto)
    {

        var sprite = new BlingoFilmLoopMemberSprite
        {
            LocH = sDto.LocH,
            LocV = sDto.LocV,
            LocZ = sDto.LocZ,
            Rotation = sDto.Rotation,
            Skew = sDto.Skew,
            RegPoint = new APoint(sDto.RegPoint.X, sDto.RegPoint.Y),
            Ink = sDto.Ink,
            ForeColor = FromDto(sDto.ForeColor),
            BackColor = FromDto(sDto.BackColor),
            Blend = sDto.Blend,
            Width = sDto.Width,
            Height = sDto.Height,
            BeginFrame = sDto.BeginFrame,
            EndFrame = sDto.EndFrame,
            DisplayMember = sDto.DisplayMember,
            Channel = sDto.Channel,
            CastNum = sDto.Member.CastLibNum,
            FlipH = sDto.FlipH,
            FlipV = sDto.FlipV,
            Hilite = sDto.Hilite,
            MemberNumberInCast = sDto.Member.MemberNum,
            SpriteNum = sDto.SpriteNum,
            Name = sDto.Name,
        };
        var anim = sDto.Animator;
        ApplyOptions(sprite.AnimatorProperties.Position.Options, anim.PositionOptions);
        ApplyOptions(sprite.AnimatorProperties.Rotation.Options, anim.RotationOptions);
        ApplyOptions(sprite.AnimatorProperties.Skew.Options, anim.SkewOptions);
        ApplyOptions(sprite.AnimatorProperties.ForegroundColor.Options, anim.ForegroundColorOptions);
        ApplyOptions(sprite.AnimatorProperties.BackgroundColor.Options, anim.BackgroundColorOptions);
        ApplyOptions(sprite.AnimatorProperties.Blend.Options, anim.BlendOptions);

        foreach (var k in anim.Position)
            sprite.AnimatorProperties.Position.AddKeyFrame(k.Frame, new APoint(k.Value.X, k.Value.Y), (BlingoEaseType)k.Ease);
        foreach (var k in anim.Rotation)
            sprite.AnimatorProperties.Rotation.AddKeyFrame(k.Frame, k.Value, (BlingoEaseType)k.Ease);
        foreach (var k in anim.Skew)
            sprite.AnimatorProperties.Skew.AddKeyFrame(k.Frame, k.Value, (BlingoEaseType)k.Ease);
        foreach (var k in anim.ForegroundColor)
            sprite.AnimatorProperties.ForegroundColor.AddKeyFrame(k.Frame, FromDto(k.Value), (BlingoEaseType)k.Ease);
        foreach (var k in anim.BackgroundColor)
            sprite.AnimatorProperties.BackgroundColor.AddKeyFrame(k.Frame, FromDto(k.Value), (BlingoEaseType)k.Ease);
        foreach (var k in anim.Blend)
            sprite.AnimatorProperties.Blend.AddKeyFrame(k.Frame, k.Value, (BlingoEaseType)k.Ease);

        return sprite;
    }
    private static void AddAnimator(Blingo2DSpriteDTO sDto, BlingoSprite2D sprite, IBlingoSpritesPlayer spritesPlayer, IBlingoEventMediator eventMediator)
    {
        if (sDto.Animator == null)
            return;
        var animatorProps = CreateAnimatorProperties(sDto.Animator, sprite);
        sprite.AddAnimator(animatorProps, eventMediator);
    }

    public static BlingoSpriteAnimatorProperties CreateAnimatorProperties(BlingoSpriteAnimatorDTO sDto, IBlingoSprite2DLight sprite)
    {
        var animator = new BlingoSpriteAnimatorProperties();


        ApplyOptions(animator.Position.Options, sDto.PositionOptions);
        ApplyOptions(animator.Rotation.Options, sDto.RotationOptions);
        ApplyOptions(animator.Skew.Options, sDto.SkewOptions);
        ApplyOptions(animator.ForegroundColor.Options, sDto.ForegroundColorOptions);
        ApplyOptions(animator.BackgroundColor.Options, sDto.BackgroundColorOptions);
        ApplyOptions(animator.Blend.Options, sDto.BlendOptions);

        foreach (var k in sDto.Position)
            animator.Position.AddKeyFrame(k.Frame, new APoint(k.Value.X, k.Value.Y), (BlingoEaseType)k.Ease);
        foreach (var k in sDto.Rotation)
            animator.Rotation.AddKeyFrame(k.Frame, k.Value, (BlingoEaseType)k.Ease);
        foreach (var k in sDto.Skew)
            animator.Skew.AddKeyFrame(k.Frame, k.Value, (BlingoEaseType)k.Ease);
        foreach (var k in sDto.ForegroundColor)
            animator.ForegroundColor.AddKeyFrame(k.Frame, FromDto(k.Value), (BlingoEaseType)k.Ease);
        foreach (var k in sDto.BackgroundColor)
            animator.BackgroundColor.AddKeyFrame(k.Frame, FromDto(k.Value), (BlingoEaseType)k.Ease);
        foreach (var k in sDto.Blend)
            animator.Blend.AddKeyFrame(k.Frame, k.Value, (BlingoEaseType)k.Ease);

        return animator;
    }

    private static void ApplyOptions(BlingoTweenOptions target, BlingoTweenOptionsDTO dto)
    {
        target.Enabled = dto.Enabled;
        target.Curvature = dto.Curvature;
        target.ContinuousAtEndpoints = dto.ContinuousAtEndpoints;
        target.SpeedChange = (BlingoSpeedChangeType)dto.SpeedChange;
        target.EaseIn = dto.EaseIn;
        target.EaseOut = dto.EaseOut;
    }

    private static AColor FromDto(BlingoColorDTO dto)
        => new AColor(dto.Code, dto.R, dto.G, dto.B, dto.Name);

    private static string EnsureResourceDirectory(string resourceDir)
        => string.IsNullOrEmpty(resourceDir) ? Directory.GetCurrentDirectory() : resourceDir;
}

