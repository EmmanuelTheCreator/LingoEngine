using System;
using System.Threading.Tasks;
using AbstUI.Commands;
using AbstUI.Primitives;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Casts.Commands;
using BlingoEngine.Members;
using BlingoEngine.Members.Commands;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Commands;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Commands;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.Net.RNetProjectHost;

/// <summary>
/// Listens for incoming RNet commands and replays them through the existing command manager
/// so property mutations share the same undoable logic used by the Director tooling.
/// </summary>
internal sealed class RNetProjectCommandApplier : IDisposable
{
    private readonly IRNetProjectServer _server;
    private readonly IAbstCommandManager _commandManager;
    private readonly IBlingoPlayer _player;
    private readonly ILogger<RNetProjectCommandApplier> _logger;

    public RNetProjectCommandApplier(
        IRNetProjectServer server,
        IAbstCommandManager commandManager,
        IBlingoPlayer player,
        ILogger<RNetProjectCommandApplier> logger)
    {
        _server = server;
        _commandManager = commandManager;
        _player = player;
        _logger = logger;

        _server.NetCommandReceived += OnNetCommandReceived;
    }

    public void Dispose()
        => _server.NetCommandReceived -= OnNetCommandReceived;

    private void OnNetCommandReceived(IRNetCommand command)
    {
        var commandType = command.GetType().Name;
        var task = HandleCommandAsync(command);
        if (!task.IsCompletedSuccessfully)
        {
            _ = task.ContinueWith(t =>
            {
                if (t.Exception is { } ex)
                {
                    _logger.LogError(ex, "Error applying RNet command {CommandType}.", commandType);
                }
            }, TaskScheduler.Default);
        }
    }

    private Task HandleCommandAsync(IRNetCommand command)
        => _player.RunOnUIThreadAsync(() =>
        {
            try
            {
                ApplyCommand(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying RNet command {CommandType}.", command.GetType().Name);
            }
        });

    private void ApplyCommand(IRNetCommand command)
    {
        switch (command)
        {
            case SetSpritePropCmd spriteCmd:
                ApplySpriteProperty(spriteCmd);
                break;
            case SetMemberPropCmd memberCmd:
                ApplyMemberProperty(memberCmd);
                break;
            case SetCastPropCmd castCmd:
                ApplyCastProperty(castCmd);
                break;
            case GoToFrameCmd goToFrameCmd:
                ApplyGoToFrame(goToFrameCmd);
                break;
            case RewindCmd:
                ApplyRewind();
                break;
            case PauseCmd:
                ApplyPause();
                break;
            case ResumeCmd:
                ApplyResume();
                break;
            default:
                _logger.LogDebug("Ignoring unsupported RNet command {CommandType}.", command.GetType().Name);
                break;
        }
    }

    private void ApplyGoToFrame(GoToFrameCmd command)
    {
        if (_player.ActiveMovie is not IBlingoMovie movie)
        {
            _logger.LogWarning("Received go-to-frame command but no active movie is loaded.");
            return;
        }

        var target = Math.Clamp(command.Frame, 1, movie.FrameCount);
        if (movie.IsPlaying)
        {
            movie.GoTo(target);
        }
        else
        {
            movie.GoToAndStop(target);
        }
    }

    private void ApplyRewind()
    {
        if (_player.ActiveMovie is null)
        {
            _logger.LogWarning("Received rewind command but no active movie is loaded.");
            return;
        }

        _commandManager.Handle(new MovieRewindCommand());
    }

    private void ApplyPause()
    {
        if (_player.ActiveMovie is not IBlingoMovie movie)
        {
            _logger.LogWarning("Received pause command but no active movie is loaded.");
            return;
        }

        if (!movie.IsPlaying)
        {
            return;
        }

        _commandManager.Handle(new PlayMovieCommand());
    }

    private void ApplyResume()
    {
        if (_player.ActiveMovie is not IBlingoMovie movie)
        {
            _logger.LogWarning("Received resume command but no active movie is loaded.");
            return;
        }

        if (movie.IsPlaying)
        {
            return;
        }

        _commandManager.Handle(new PlayMovieCommand());
    }

    private void ApplySpriteProperty(SetSpritePropCmd command)
    {
        if (_player.ActiveMovie is not BlingoEngine.Movies.BlingoMovie movie)
        {
            _logger.LogWarning("Received sprite command but no active movie is loaded.");
            return;
        }

        var spriteType = command.SpriteType.ConvertTo<BlingoSpriteType>();
        var spriteRef = new BlingoSpriteRef(command.SpriteNum, command.BeginFrame, spriteType);
        var sprite = movie.GetSprite(spriteRef);
        if (sprite is null && spriteType != BlingoSpriteType.Unknown)
        {
            sprite = movie.GetSprite(new BlingoSpriteRef(command.SpriteNum, command.BeginFrame, BlingoSpriteType.Unknown));
        }
        if (sprite is null)
        {
            _logger.LogWarning(
                "Unable to resolve sprite {SpriteNum}/{BeginFrame} ({SpriteType}) for property {Property}.",
                command.SpriteNum,
                command.BeginFrame,
                command.SpriteType,
                command.Prop);
            return;
        }

        if (!RNetPropertyValueConverter.TryCreatePropertyValue(sprite, command.Prop, command.Value, out var change))
        {
            _logger.LogWarning(
                "Failed to convert sprite property {Property} with value '{Value}'.",
                command.Prop,
                command.Value);
            return;
        }

        var resolved = BlingoSpriteRef.FromSprite(sprite);
        _commandManager.Handle(new BlingoUpdateSpritePropertiesCommand(resolved, new[] { change }));
    }

    private void ApplyMemberProperty(SetMemberPropCmd command)
    {
        var memberType = command.MemberType.ConvertTo<BlingoMemberType>();
        var memberRef = new BlingoMemberRef(command.CastLibNum, command.MemberNum, memberType);
        var member = _player.GetMember(memberRef);
        if (member is null && memberType != BlingoMemberType.Unknown)
        {
            member = _player.GetMember(new BlingoMemberRef(command.CastLibNum, command.MemberNum, BlingoMemberType.Unknown));
        }

        if (member is null)
        {
            _logger.LogWarning(
                "Unable to resolve member {Cast}/{Member} ({MemberType}) for property {Property}.",
                command.CastLibNum,
                command.MemberNum,
                command.MemberType,
                command.Prop);
            return;
        }

        if (!TryCreateMemberPropertyChange(member, command.Prop, command.Value, out var change))
        {
            _logger.LogWarning(
                "Failed to convert member property {Property} with value '{Value}'.",
                command.Prop,
                command.Value);
            return;
        }

        var resolved = BlingoMemberRef.FromMember(member);
        _commandManager.Handle(new BlingoUpdateMemberPropertiesCommand(resolved, new[] { change }));
    }

    private void ApplyCastProperty(SetCastPropCmd command)
    {
        var castRef = new BlingoCastRef(command.CastLibNum);
        var cast = _player.GetCast(castRef) as BlingoCast;
        if (cast is null)
        {
            _logger.LogWarning(
                "Unable to resolve cast library {CastLib} for property {Property}.",
                command.CastLibNum,
                command.Prop);
            return;
        }

        if (!RNetPropertyValueConverter.TryCreatePropertyValue(cast, command.Prop, command.Value, out var change))
        {
            _logger.LogWarning(
                "Failed to convert cast property {Property} with value '{Value}'.",
                command.Prop,
                command.Value);
            return;
        }

        _commandManager.Handle(new BlingoUpdateCastPropertiesCommand(BlingoCastRef.FromCast(cast), new[] { change }));
    }

    private static bool TryCreateMemberPropertyChange(IBlingoMember member, string property, string value, out APropertyValue change)
    {
        change = default;

        switch (property)
        {
            case "RegPointX":
                if (!RNetPropertyValueConverter.TryConvertToType(typeof(float), value, out var newX) || newX is null)
                {
                    return false;
                }

                var pointX = member.RegPoint;
                change = new APropertyValue(nameof(BlingoMember.RegPoint), new APoint((float)newX, pointX.Y));
                return true;
            case "RegPointY":
                if (!RNetPropertyValueConverter.TryConvertToType(typeof(float), value, out var newY) || newY is null)
                {
                    return false;
                }

                var pointY = member.RegPoint;
                change = new APropertyValue(nameof(BlingoMember.RegPoint), new APoint(pointY.X, (float)newY));
                return true;
        }

        return RNetPropertyValueConverter.TryCreatePropertyValue(member, property, value, out change);
    }
}
