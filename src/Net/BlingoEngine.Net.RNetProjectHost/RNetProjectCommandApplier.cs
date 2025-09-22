using System;
using System.Threading.Tasks;
using AbstUI.Commands;
using AbstUI.Primitives;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Casts.Commands;
using BlingoEngine.Members.Commands;
using BlingoEngine.Sprites.Commands;
using BlingoEngine.Members;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;
using BlingoEngine.Sprites;
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
            default:
                _logger.LogDebug("Ignoring unsupported RNet command {CommandType}.", command.GetType().Name);
                break;
        }
    }

    private void ApplySpriteProperty(SetSpritePropCmd command)
    {
        if (_player.ActiveMovie is not BlingoEngine.Movies.BlingoMovie movie)
        {
            _logger.LogWarning("Received sprite command but no active movie is loaded.");
            return;
        }

        var spriteType = ConvertSpriteType(command.SpriteType);
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

        if (!TryCreateSpritePropertyChange(sprite, command.Prop, command.Value, out var change))
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
        var memberType = ConvertMemberType(command.MemberType);
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

    private static BlingoMemberType ConvertMemberType(RNetMemberTypeDto memberType)
        => Enum.TryParse<BlingoMemberType>(memberType.ToString(), out var resolved)
            ? resolved
            : BlingoMemberType.Unknown;

    private static BlingoSpriteType ConvertSpriteType(RNetSpriteTypeDto spriteType)
        => Enum.TryParse<BlingoSpriteType>(spriteType.ToString(), out var resolved)
            ? resolved
            : BlingoSpriteType.Unknown;

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

        if (!TryCreateCastPropertyChange(cast, command.Prop, command.Value, out var change))
        {
            _logger.LogWarning(
                "Failed to convert cast property {Property} with value '{Value}'.",
                command.Prop,
                command.Value);
            return;
        }

        _commandManager.Handle(new BlingoUpdateCastPropertiesCommand(BlingoCastRef.FromCast(cast), new[] { change }));
    }

    private static bool TryCreateSpritePropertyChange(BlingoSprite sprite, string property, string value, out APropertyValue change)
    {
        change = default;

        return RNetPropertyValueConverter.TryCreatePropertyValue(sprite, property, value, out change);
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

    private static bool TryCreateCastPropertyChange(BlingoCast cast, string property, string value, out APropertyValue change)
    {
        change = default;

        return RNetPropertyValueConverter.TryCreatePropertyValue(cast, property, value, out change);
    }
}
