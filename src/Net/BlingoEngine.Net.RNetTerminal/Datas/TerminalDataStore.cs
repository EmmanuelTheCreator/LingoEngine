using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetTerminal;
using BlingoEngine.Net.RNetTerminal.TestData;
using BlingoEngine.Net.RNetTerminal.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BlingoEngine.Net.RNetTerminal.Datas;

public sealed class TerminalDataStore
{
    private static readonly Lazy<TerminalDataStore> _instance = new(() => new TerminalDataStore());
    public static TerminalDataStore Instance => _instance.Value;

    private readonly List<Blingo2DSpriteDTO> _sprites = new();
    private readonly List<BlingoTempoSpriteDTO> _tempoSprites = new();
    private readonly List<BlingoColorPaletteSpriteDTO> _paletteSprites = new();
    private readonly List<BlingoTransitionSpriteDTO> _transitionSprites = new();
    private readonly List<BlingoSpriteSoundDTO> _soundSprites = new();
    private readonly Dictionary<string, List<BlingoMemberDTO>> _casts = new();
    private readonly Dictionary<int, int> _pendingSpriteOriginalBegins = new();
    private int _currentFrame;
    private SpriteRef? _selectedSprite;
    public int SpriteChannelCount = 100;
    private TerminalDataStore()
    {
    }

    public MovieStateDto MovieState { get; private set; } = new MovieStateDto(0,0,false);

    public int StageWidth { get; private set; } = 640;

    public int StageHeight { get; private set; } = 480;

    public int FrameCount { get; private set; } = 600;

    public bool ApplyLocalChanges { get; set; } = true;

    public event Action? SpritesChanged;
    public event Action<Blingo2DSpriteDTO>? SpriteChanged;
    public event Action? CastsChanged;
    public event Action<BlingoMemberDTO>? MemberChanged;
    public event Action<int>? FrameChanged;
    public event Action<SpriteRef?>? SelectedSpriteChanged;
    public event Action<SpriteRef, int, int>? SpriteMoveRequested;
    public event Action<MovieStateDto>? MovieStateChanged;

    public IReadOnlyList<Blingo2DSpriteDTO> GetSprites() => _sprites;

    public IReadOnlyDictionary<string, List<BlingoMemberDTO>> GetCasts() => _casts;

    public IReadOnlyList<BlingoTempoSpriteDTO> GetTempoSprites() => _tempoSprites;

    public IReadOnlyList<BlingoColorPaletteSpriteDTO> GetColorPaletteSprites() => _paletteSprites;

    public IReadOnlyList<BlingoTransitionSpriteDTO> GetTransitionSprites() => _transitionSprites;

    public IReadOnlyList<BlingoSpriteSoundDTO> GetSoundSprites() => _soundSprites;

    public Blingo2DSpriteDTO? FindSprite(SpriteRef sprite)
        => _sprites.FirstOrDefault(s => s.SpriteNum == sprite.SpriteNum && s.BeginFrame == sprite.BeginFrame);

    public RNetSpriteTypeDto GetSpriteType(SpriteRef sprite)
        => FindSprite(sprite) is { } sprite2D
            ? sprite2D.ToRNet()
            : RNetSpriteTypeDto.Unknown;

    public BlingoMemberDTO? FindMember(int castLibNum, int numberInCast)
        => _casts.Values.SelectMany(c => c)
            .FirstOrDefault(m => m.CastLibNum == castLibNum && m.NumberInCast == numberInCast);

    public BlingoMemberDTO? FindMember(int castLibNum, string name)
        => _casts.Values.SelectMany(c => c)
            .FirstOrDefault(m => m.CastLibNum == castLibNum && m.Name == name);

    public int GetFrame() => _currentFrame;

    public void SetFrame(int frame, bool updateMovieState = true)
    {
        if (_currentFrame == frame)
        {
            return;
        }
        _currentFrame = frame;
        if (updateMovieState && MovieState.Frame != frame)
        {
            MovieState = MovieState with { Frame = frame };
            MovieStateChanged?.Invoke(MovieState);
        }
        FrameChanged?.Invoke(frame);
    }

    public SpriteRef? GetSelectedSprite() => _selectedSprite;

    public void SelectSprite(SpriteRef? sprite)
    {
        if (_selectedSprite == sprite)
        {
            return;
        }
        _selectedSprite = sprite;
        SelectedSpriteChanged?.Invoke(sprite);
    }

    public void UpdateSprite(Blingo2DSpriteDTO sprite)
    {
        var idx = _sprites.FindIndex(s => s.SpriteNum == sprite.SpriteNum && s.BeginFrame == sprite.BeginFrame);
        if (idx >= 0)
        {
            _sprites[idx] = sprite;
            SpriteChanged?.Invoke(sprite);
        }
        else
        {
            _sprites.Add(sprite);
            SpritesChanged?.Invoke();
        }
    }

    public void UpdateMember(BlingoMemberDTO member)
    {
        var list = _casts.Values.FirstOrDefault(c =>
            c.Any(m => m.CastLibNum == member.CastLibNum && m.NumberInCast == member.NumberInCast));
        if (list != null)
        {
            var idx = list.FindIndex(m => m.CastLibNum == member.CastLibNum && m.NumberInCast == member.NumberInCast);
            if (idx >= 0)
            {
                list[idx] = member;
            }
            else
            {
                list.Add(member);
            }
        }
        MemberChanged?.Invoke(member);
    }

    public void MoveSprite(SpriteRef spriteRef, int delta)
    {
        if (delta == 0)
        {
            return;
        }

        var sprite = FindSprite(spriteRef);
        if (sprite == null)
        {
            return;
        }

        var originalBegin = sprite.BeginFrame;
        var length = Math.Max(0, sprite.EndFrame - sprite.BeginFrame);
        var maxStart = Math.Max(1, FrameCount - length);
        var newBegin = Math.Clamp(sprite.BeginFrame + delta, 1, maxStart);
        var newEnd = newBegin + length;
        if (newEnd > FrameCount)
        {
            newEnd = FrameCount;
            newBegin = Math.Max(1, newEnd - length);
        }

        if (newBegin == sprite.BeginFrame && newEnd == sprite.EndFrame)
        {
            return;
        }

        if (!ApplyLocalChanges)
        {
            _pendingSpriteOriginalBegins[spriteRef.SpriteNum] = originalBegin;
            SpriteMoveRequested?.Invoke(spriteRef, newBegin, newEnd);
            return;
        }

        sprite.BeginFrame = newBegin;
        sprite.EndFrame = newEnd;
        SpriteChanged?.Invoke(sprite);

        if (_selectedSprite.HasValue && _selectedSprite.Value.SpriteNum == sprite.SpriteNum)
        {
            SelectSprite(new SpriteRef(sprite.SpriteNum, sprite.BeginFrame));
        }
    }

    public void PropertyHasChanged(PropertyTarget target, string name, string value, BlingoMemberDTO? member = null, bool force = false)
    {
        if (!force && !ApplyLocalChanges)
        {
            return;
        }

        if (target == PropertyTarget.Sprite)
        {
            if (!_selectedSprite.HasValue)
            {
                return;
            }

            var sprite = FindSprite(_selectedSprite.Value);
            if (sprite == null)
            {
                return;
            }

            switch (name)
            {
                case "LocH" when float.TryParse(value, out var locH):
                    sprite.LocH = locH;
                    break;
                case "LocV" when float.TryParse(value, out var locV):
                    sprite.LocV = locV;
                    break;
                case "LocZ" when int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var locZ):
                    sprite.LocZ = locZ;
                    break;
                case "Width" when float.TryParse(value, out var width):
                    sprite.Width = width;
                    break;
                case "Height" when float.TryParse(value, out var height):
                    sprite.Height = height;
                    break;
                case "BeginFrame" when int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var begin):
                    sprite.BeginFrame = begin;
                    break;
                case "EndFrame" when int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var end):
                    sprite.EndFrame = end;
                    break;
            }

            UpdateSprite(sprite);
        }
        else if (target == PropertyTarget.Member && member != null)
        {
            switch (name)
            {
                case "Name":
                    member.Name = value;
                    break;
                case "Comments":
                    member.Comments = value;
                    break;
            }

            UpdateMember(member);
        }
    }

    public void UpdateMovieState(MovieStateDto state)
    {
        var previous = MovieState;
        if (previous == state)
        {
            return;
        }

        MovieState = state;

        if (previous.Frame != state.Frame)
        {
            SetFrame(state.Frame, updateMovieState: false);
        }

        MovieStateChanged?.Invoke(MovieState);
    }

    public void ApplyMovieProperty(RNetMoviePropertyDto property)
    {
        var state = MovieState;
        var updated = state;
        var changed = false;

        switch (property.Prop)
        {
            case "CurrentFrame" when int.TryParse(property.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var frame):
                updated = updated with { Frame = frame };
                changed = true;
                break;
            case "Tempo" when int.TryParse(property.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var tempo):
                updated = updated with { Tempo = tempo };
                changed = true;
                break;
            case "IsPlaying" when bool.TryParse(property.Value, out var playing):
                updated = updated with { IsPlaying = playing };
                changed = true;
                break;
        }

        if (changed)
        {
            UpdateMovieState(updated);
        }
    }

    public void ApplySpriteDelta(SpriteDeltaDto delta)
    {
        var spriteRef = new SpriteRef(delta.SpriteNum, delta.BeginFrame);
        var sprite = FindSprite(spriteRef);
        if (sprite == null && _pendingSpriteOriginalBegins.TryGetValue(delta.SpriteNum, out var originalBegin))
        {
            sprite = FindSprite(new SpriteRef(delta.SpriteNum, originalBegin));
        }
        if (sprite == null)
        {
            sprite = _sprites.FirstOrDefault(s => s.SpriteNum == delta.SpriteNum);
        }
        if (sprite == null)
        {
            _pendingSpriteOriginalBegins.Remove(delta.SpriteNum);
            return;
        }

        var previousBegin = sprite.BeginFrame;
        var wasSelected = _selectedSprite.HasValue &&
                          _selectedSprite.Value.SpriteNum == sprite.SpriteNum &&
                          _selectedSprite.Value.BeginFrame == previousBegin;
        var length = Math.Max(0, sprite.EndFrame - sprite.BeginFrame);
        var maxStart = Math.Max(1, FrameCount - length);
        var newBegin = Math.Clamp(delta.BeginFrame <= 0 ? 1 : delta.BeginFrame, 1, maxStart);
        var newEnd = length > 0 ? newBegin + length : newBegin;
        if (FrameCount > 0 && newEnd > FrameCount)
        {
            newEnd = FrameCount;
            newBegin = Math.Max(1, newEnd - length);
        }
        if (newBegin != sprite.BeginFrame)
        {
            sprite.BeginFrame = newBegin;
            sprite.EndFrame = newEnd;
        }
        sprite.Member ??= new BlingoMemberRefDTO();
        sprite.Member.CastLibNum = delta.CastLibNum;
        sprite.Member.MemberNum = delta.MemberNum;
        sprite.LocH = delta.LocH;
        sprite.LocV = delta.LocV;
        sprite.LocZ = delta.Z;
        sprite.Width = delta.Width;
        sprite.Height = delta.Height;
        sprite.Rotation = delta.Rotation;
        sprite.Skew = delta.Skew;
        sprite.Blend = delta.Blend;
        sprite.Ink = delta.Ink;
        SpriteChanged?.Invoke(sprite);
        if (wasSelected)
        {
            SelectSprite(new SpriteRef(sprite.SpriteNum, sprite.BeginFrame));
        }
        _pendingSpriteOriginalBegins.Remove(sprite.SpriteNum);
    }

    public void ApplyMemberProperty(RNetMemberPropertyDto property)
    {
        var member = FindMember(property.CastLibNum, property.MemberNum);
        if (member == null)
        {
            return;
        }

        PropertyHasChanged(PropertyTarget.Member, property.Prop, property.Value, member, true);
    }

    public void LoadTestData()
    {
        _pendingSpriteOriginalBegins.Clear();
        UpdateMovieState(TestMovieBuilder.BuildMovieState());
        _sprites.Clear();
        _sprites.AddRange(TestMovieBuilder.BuildSprites());
        _tempoSprites.Clear();
        _tempoSprites.AddRange(TestMovieBuilder.BuildTempoSprites());
        _paletteSprites.Clear();
        _paletteSprites.AddRange(TestMovieBuilder.BuildPaletteSprites());
        _transitionSprites.Clear();
        _transitionSprites.AddRange(TestMovieBuilder.BuildTransitionSprites());
        _soundSprites.Clear();
        _soundSprites.AddRange(TestMovieBuilder.BuildSoundSprites());
        _casts.Clear();
        foreach (var kv in TestCastBuilder.BuildCastData())
        {
            _casts[kv.Key] = kv.Value;
        }
        StageWidth = 640;
        StageHeight = 480;
        FrameCount = 600;
        SpritesChanged?.Invoke();
        CastsChanged?.Invoke();
        SelectSprite(null);
    }

    public void Load(BlingoMovieDTO movie)
    {
        _pendingSpriteOriginalBegins.Clear();
        _sprites.Clear();
        _sprites.AddRange(movie.Sprite2Ds);
        _tempoSprites.Clear();
        _tempoSprites.AddRange(movie.TempoSprites);
        _paletteSprites.Clear();
        _paletteSprites.AddRange(movie.ColorPaletteSprites);
        _transitionSprites.Clear();
        _transitionSprites.AddRange(movie.TransitionSprites);
        _soundSprites.Clear();
        _soundSprites.AddRange(movie.SoundSprites);
        _casts.Clear();
        foreach (var cast in movie.Casts)
        {
            _casts[cast.Name] = cast.Members;
        }
        FrameCount = movie.FrameCount;
        SpriteChannelCount = movie.MaxSpriteChannelCount;
        UpdateMovieState(new MovieStateDto(0, movie.Tempo, false));
        SpritesChanged?.Invoke();
        CastsChanged?.Invoke();
        SelectSprite(null);
    }
    public void LoadFromProject(BlingoProjectDTO project)
    {
        if (project.Movies.Count > 0)
        {
            Load(project.Movies[0]);
        }
        if (project.Stage != null)
        {
            StageWidth = (int)project.Stage.Width;
            StageHeight = (int)project.Stage.Height;
        }
    }

}


