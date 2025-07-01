using System.Text.Json;
using System.Reflection;
using LingoEngine.Movies;
using LingoEngine.Core;
using LingoEngine.Texts;
using LingoEngine.Sounds;
using LingoEngine.IO.Data.DTO;
using LingoEngine.Animations;
using LingoEngine.Primitives;
using System.Linq;
using LingoEngine.Members;
using LingoEngine.Casts;
using LingoEngine.Sprites;
using LingoEngine.FilmLoops;
using LingoEngine.Bitmaps;

namespace LingoEngine.IO;

public class JsonStateRepository
{
    public void Save(string filePath, LingoMovie movie)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(dir))
            dir = Directory.GetCurrentDirectory();
        var dto = ToDto(movie, dir);
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(dto, options);
        File.WriteAllText(filePath, json);
    }

    public LingoMovie Load(string filePath, LingoPlayer player)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(dir))
            dir = Directory.GetCurrentDirectory();

        var json = File.ReadAllText(filePath);
        var dto = JsonSerializer.Deserialize<LingoMovieDTO>(json) ?? throw new Exception("Invalid movie file");

        return Load(dto, player, dir);
    }

    public LingoMovie Load(LingoMovieDTO dto, LingoPlayer player, string resourceDir)
    {
        if (string.IsNullOrEmpty(resourceDir))
            resourceDir = Directory.GetCurrentDirectory();

        return BuildMovieFromDto(dto, player, resourceDir);
    }

    private static LingoMovie BuildMovieFromDto(LingoMovieDTO dto, LingoPlayer player, string dir)
    {
        var movie = (LingoMovie)player.NewMovie(dto.Name);
        movie.Tempo = dto.Tempo;

        var castMap = new Dictionary<int, LingoCast>();
        var memberMap = new Dictionary<int, LingoMember>();

        foreach (var castDto in dto.Casts)
        {
            var cast = (LingoCast)movie.CastLib.AddCast(castDto.Name);
            cast.PreLoadMode = (PreLoadModeType)castDto.PreLoadMode;
            castMap[castDto.Number] = cast;

            foreach (var memDto in castDto.Members)
            {
                var reg = new LingoPoint(memDto.RegPoint.X, memDto.RegPoint.Y);
                string fileName = memDto.FileName;
                if (memDto is LingoMemberPictureDTO pic && !string.IsNullOrEmpty(pic.ImageFile))
                    fileName = Path.Combine(dir, pic.ImageFile);
                if (memDto is LingoMemberSoundDTO snd && !string.IsNullOrEmpty(snd.SoundFile))
                    fileName = Path.Combine(dir, snd.SoundFile);

                var member = (LingoMember)cast.Add((LingoMemberType)memDto.Type, memDto.NumberInCast, memDto.Name, fileName, reg);
                member.Width = memDto.Width;
                member.Height = memDto.Height;
                member.Size = memDto.Size;
                member.Comments = memDto.Comments;
                member.PurgePriority = memDto.PurgePriority;

                if (member is LingoMemberText txt && memDto is LingoMemberTextDTO txtDto)
                    txt.Text = txtDto.Text;
                if (member is LingoMemberField fld && memDto is LingoMemberFieldDTO fldDto)
                    fld.Text = fldDto.Text;
                if (member is LingoMemberSound sndMem && memDto is LingoMemberSoundDTO sndDto)
                {
                    sndMem.Loop = sndDto.Loop;
                    sndMem.IsLinked = sndDto.IsLinked;
                    sndMem.LinkedFilePath = sndDto.LinkedFilePath;
                }

                memberMap[memDto.Number] = member;
            }
        }

        foreach (var sDto in dto.Sprites)
        {
            var sprite = movie.AddSprite<LingoSprite>(sDto.SpriteNum, sDto.Name, s =>
            {
                s.Puppet = sDto.Puppet;
                s.Lock = sDto.Lock;
                s.Visibility = sDto.Visibility;
                s.LocH = sDto.LocH;
                s.LocV = sDto.LocV;
                s.LocZ = sDto.LocZ;
                s.Rotation = sDto.Rotation;
                s.Skew = sDto.Skew;
                s.RegPoint = new LingoPoint(sDto.RegPoint.X, sDto.RegPoint.Y);
                s.Ink = sDto.Ink;
                s.ForeColor = FromDto(sDto.ForeColor);
                s.BackColor = FromDto(sDto.BackColor);
                s.Blend = sDto.Blend;
                s.Editable = sDto.Editable;
                s.Width = sDto.Width;
                s.Height = sDto.Height;
                s.BeginFrame = sDto.BeginFrame;
                s.EndFrame = sDto.EndFrame;
                s.DisplayMember = sDto.DisplayMember;
                s.SpritePropertiesOffset = sDto.SpritePropertiesOffset;
            });

            if (memberMap.TryGetValue(sDto.MemberNum, out var mem))
                sprite.SetMember(mem);

            if (sDto.Animator != null)
            {
                var animator = new LingoSpriteAnimator(sprite, movie.GetEnvironment());
                var addActor = typeof(LingoSprite).GetMethod("AddActor", BindingFlags.NonPublic | BindingFlags.Instance);
                addActor?.Invoke(sprite, new object[] { animator });

                ApplyOptions(animator.Position.Options, sDto.Animator.PositionOptions);
                ApplyOptions(animator.Rotation.Options, sDto.Animator.RotationOptions);
                ApplyOptions(animator.Skew.Options, sDto.Animator.SkewOptions);
                ApplyOptions(animator.ForegroundColor.Options, sDto.Animator.ForegroundColorOptions);
                ApplyOptions(animator.BackgroundColor.Options, sDto.Animator.BackgroundColorOptions);
                ApplyOptions(animator.Blend.Options, sDto.Animator.BlendOptions);

                foreach (var k in sDto.Animator.Position)
                    animator.Position.AddKeyFrame(k.Frame, new LingoPoint(k.Value.X, k.Value.Y), (LingoEaseType)k.Ease);
                foreach (var k in sDto.Animator.Rotation)
                    animator.Rotation.AddKeyFrame(k.Frame, k.Value, (LingoEaseType)k.Ease);
                foreach (var k in sDto.Animator.Skew)
                    animator.Skew.AddKeyFrame(k.Frame, k.Value, (LingoEaseType)k.Ease);
                foreach (var k in sDto.Animator.ForegroundColor)
                    animator.ForegroundColor.AddKeyFrame(k.Frame, FromDto(k.Value), (LingoEaseType)k.Ease);
                foreach (var k in sDto.Animator.BackgroundColor)
                    animator.BackgroundColor.AddKeyFrame(k.Frame, FromDto(k.Value), (LingoEaseType)k.Ease);
                foreach (var k in sDto.Animator.Blend)
                    animator.Blend.AddKeyFrame(k.Frame, k.Value, (LingoEaseType)k.Ease);

                animator.GetType().GetMethod("RecalculateCache", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(animator, null);
            }

        }

        return movie;
    }

    private static LingoMovieDTO ToDto(LingoMovie movie, string dir)
    {
        return new LingoMovieDTO
        {
            Name = movie.Name,
            Number = movie.Number,
            Tempo = movie.Tempo,
            FrameCount = movie.FrameCount,
            Casts = movie.CastLib.GetAll().Select(c => ToDto((LingoCast)c, dir)).ToList(),
            Sprites = GetAllSprites(movie).Select(ToDto).ToList()
        };
    }

    private static LingoCastDTO ToDto(LingoCast cast, string dir)
    {
        return new LingoCastDTO
        {
            Name = cast.Name,
            FileName = cast.FileName,
            Number = cast.Number,
            PreLoadMode = (PreLoadModeTypeDTO)cast.PreLoadMode,
            Members = cast.GetAll().Select(m => ToDto(m, dir)).ToList()
        };
    }

    private static LingoMemberDTO ToDto(ILingoMember member, string dir)
    {
        var baseDto = new LingoMemberDTO
        {
            Name = member.Name,
            Number = member.Number,
            CastLibNum = member.CastLibNum,
            NumberInCast = member.NumberInCast,
            Type = (LingoMemberTypeDTO)member.Type,
            RegPoint = new LingoPointDTO { X = member.RegPoint.X, Y = member.RegPoint.Y },
            Width = member.Width,
            Height = member.Height,
            Size = member.Size,
            Comments = member.Comments,
            FileName = member.FileName,
            PurgePriority = member.PurgePriority
        };

        return member switch
        {
            LingoMemberField field => new LingoMemberFieldDTO
            {
                Name = baseDto.Name,
                Number = baseDto.Number,
                CastLibNum = baseDto.CastLibNum,
                NumberInCast = baseDto.NumberInCast,
                Type = baseDto.Type,
                RegPoint = baseDto.RegPoint,
                Width = baseDto.Width,
                Height = baseDto.Height,
                Size = baseDto.Size,
                Comments = baseDto.Comments,
                FileName = baseDto.FileName,
                PurgePriority = baseDto.PurgePriority,
                IsFocused = field.IsFocused,
                Text = field.Text
            },
            LingoMemberSound sound => new LingoMemberSoundDTO
            {
                Name = baseDto.Name,
                Number = baseDto.Number,
                CastLibNum = baseDto.CastLibNum,
                NumberInCast = baseDto.NumberInCast,
                Type = baseDto.Type,
                RegPoint = baseDto.RegPoint,
                Width = baseDto.Width,
                Height = baseDto.Height,
                Size = baseDto.Size,
                Comments = baseDto.Comments,
                FileName = baseDto.FileName,
                PurgePriority = baseDto.PurgePriority,
                Stereo = sound.Stereo,
                Length = sound.Length,
                Loop = sound.Loop,
                IsLinked = sound.IsLinked,
                LinkedFilePath = sound.LinkedFilePath,
                SoundFile = SaveSound(sound, dir)
            },
            LingoMemberText text => new LingoMemberTextDTO
            {
                Name = baseDto.Name,
                Number = baseDto.Number,
                CastLibNum = baseDto.CastLibNum,
                NumberInCast = baseDto.NumberInCast,
                Type = baseDto.Type,
                RegPoint = baseDto.RegPoint,
                Width = baseDto.Width,
                Height = baseDto.Height,
                Size = baseDto.Size,
                Comments = baseDto.Comments,
                FileName = baseDto.FileName,
                PurgePriority = baseDto.PurgePriority,
                Text = text.Text
            },
            LingoMemberBitmap picture => new LingoMemberPictureDTO
            {
                Name = baseDto.Name,
                Number = baseDto.Number,
                CastLibNum = baseDto.CastLibNum,
                NumberInCast = baseDto.NumberInCast,
                Type = baseDto.Type,
                RegPoint = baseDto.RegPoint,
                Width = baseDto.Width,
                Height = baseDto.Height,
                Size = baseDto.Size,
                Comments = baseDto.Comments,
                FileName = baseDto.FileName,
                PurgePriority = baseDto.PurgePriority,
                ImageFile = SavePicture(picture, dir)
            },
            _ => baseDto
        };
    }

    private static LingoSpriteDTO ToDto(LingoSprite sprite)
    {
        var dto = new LingoSpriteDTO
        {
            Name = sprite.Name,
            SpriteNum = sprite.SpriteNum,
            MemberNum = sprite.MemberNum,
            DisplayMember = sprite.DisplayMember,
            SpritePropertiesOffset = sprite.SpritePropertiesOffset,
            Puppet = sprite.Puppet,
            Lock = sprite.Lock,
            Visibility = sprite.Visibility,
            LocH = sprite.LocH,
            LocV = sprite.LocV,
            LocZ = sprite.LocZ,
            Rotation = sprite.Rotation,
            Skew = sprite.Skew,
            RegPoint = new LingoPointDTO { X = sprite.RegPoint.X, Y = sprite.RegPoint.Y },
            Ink = sprite.Ink,
            ForeColor = ToDto(sprite.ForeColor),
            BackColor = ToDto(sprite.BackColor),
            Blend = sprite.Blend,
            Editable = sprite.Editable,
            Width = sprite.Width,
            Height = sprite.Height,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame
        };

        foreach (var actor in GetSpriteActors(sprite))
        {
            switch (actor)
            {
                case LingoSpriteAnimator anim:
                    dto.Animator = ToDto(anim);
                    break;
                case LingoFilmLoopPlayer fl:
                    // Film loop player state is transient, no need to save
                    break;
            }
        }

        return dto;
    }

    private static IEnumerable<LingoSprite> GetAllSprites(LingoMovie movie)
    {
        var field = typeof(LingoMovie).GetField("_allTimeSprites", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field?.GetValue(movie) is IEnumerable<LingoSprite> sprites)
            return sprites;
        return Enumerable.Empty<LingoSprite>();
    }

    private static IEnumerable<object> GetSpriteActors(LingoSprite sprite)
    {
        var field = typeof(LingoSprite).GetField("_spriteActors", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field?.GetValue(sprite) is IEnumerable<object> actors)
            return actors;
        return Enumerable.Empty<object>();
    }

    private static LingoSpriteAnimatorDTO ToDto(LingoSpriteAnimator animator)
    {
        return new LingoSpriteAnimatorDTO
        {
            Position = animator.Position.KeyFrames.Select(k => new LingoPointKeyFrameDTO
            {
                Frame = k.Frame,
                Value = new LingoPointDTO { X = k.Value.X, Y = k.Value.Y },
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            PositionOptions = ToDto(animator.Position.Options),

            Rotation = animator.Rotation.KeyFrames.Select(k => new LingoFloatKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value,
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            RotationOptions = ToDto(animator.Rotation.Options),

            Skew = animator.Skew.KeyFrames.Select(k => new LingoFloatKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value,
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            SkewOptions = ToDto(animator.Skew.Options),

            ForegroundColor = animator.ForegroundColor.KeyFrames.Select(k => new LingoColorKeyFrameDTO
            {
                Frame = k.Frame,
                Value = ToDto(k.Value),
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            ForegroundColorOptions = ToDto(animator.ForegroundColor.Options),

            BackgroundColor = animator.BackgroundColor.KeyFrames.Select(k => new LingoColorKeyFrameDTO
            {
                Frame = k.Frame,
                Value = ToDto(k.Value),
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            BackgroundColorOptions = ToDto(animator.BackgroundColor.Options),

            Blend = animator.Blend.KeyFrames.Select(k => new LingoFloatKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value,
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            BlendOptions = ToDto(animator.Blend.Options)
        };
    }

    private static LingoTweenOptionsDTO ToDto(LingoTweenOptions options)
    {
        return new LingoTweenOptionsDTO
        {
            Enabled = options.Enabled,
            Curvature = options.Curvature,
            ContinuousAtEndpoints = options.ContinuousAtEndpoints,
            SpeedChange = (LingoSpeedChangeTypeDTO)options.SpeedChange,
            EaseIn = options.EaseIn,
            EaseOut = options.EaseOut
        };
    }

    private static LingoColorDTO ToDto(LingoColor color)
    {
        return new LingoColorDTO
        {
            Code = color.Code,
            Name = color.Name,
            R = color.R,
            G = color.G,
            B = color.B
        };
    }

    private static string SavePicture(LingoMemberBitmap picture, string dir)
    {
        if (picture.ImageData == null)
            return string.Empty;

        var ext = GetPictureExtension(picture);
        var name = $"{picture.NumberInCast}_{SanitizeFileName(picture.Name)}.{ext}";
        var path = Path.Combine(dir, name);
        File.WriteAllBytes(path, picture.ImageData);
        return name;
    }

    private static string SaveSound(LingoMemberSound sound, string dir)
    {
        var source = !string.IsNullOrEmpty(sound.FileName) && File.Exists(sound.FileName)
            ? sound.FileName
            : sound.LinkedFilePath;
        if (string.IsNullOrEmpty(source) || !File.Exists(source))
            return string.Empty;

        var ext = GetSoundExtension(source);
        var name = $"{sound.NumberInCast}_{SanitizeFileName(sound.Name)}{ext}";
        var dest = Path.Combine(dir, name);
        File.Copy(source, dest, true);
        return name;
    }

    private static string GetSoundExtension(string path)
    {
        var ext = Path.GetExtension(path);
        if (string.IsNullOrEmpty(ext))
            return ".wav";
        return ext.ToLowerInvariant();
    }

    private static string GetPictureExtension(LingoMemberBitmap picture)
    {
        var format = picture.Format.ToLowerInvariant();
        if (format.Contains("png") || format.Contains("gif") || format.Contains("tiff"))
            return "png";
        return "bmp";
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    private static void ApplyOptions(LingoTweenOptions target, LingoTweenOptionsDTO dto)
    {
        target.Enabled = dto.Enabled;
        target.Curvature = dto.Curvature;
        target.ContinuousAtEndpoints = dto.ContinuousAtEndpoints;
        target.SpeedChange = (LingoSpeedChangeType)dto.SpeedChange;
        target.EaseIn = dto.EaseIn;
        target.EaseOut = dto.EaseOut;
    }

    private static LingoColor FromDto(LingoColorDTO dto)
        => new LingoColor(dto.Code, dto.R, dto.G, dto.B, dto.Name);
}
