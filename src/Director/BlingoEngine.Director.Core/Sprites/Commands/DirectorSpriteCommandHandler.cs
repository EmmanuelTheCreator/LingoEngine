using AbstUI.Commands;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Sprites;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Commands;

namespace BlingoEngine.Director.Core.Sprites.Commands;

public sealed class DirectorSpriteCommandHandler : BlingoSpriteCommandHandler
{
    private readonly DirSpritesManager _spritesManager;

    public DirectorSpriteCommandHandler(DirSpritesManager spritesManager, BlingoPlayer player, IHistoryManager historyManager)
        : base(player, historyManager)
    {
        _spritesManager = spritesManager;
    }

    protected override void OnAfterMutation(BlingoSprite sprite, bool requiresStageRefresh)
    {
        base.OnAfterMutation(sprite, requiresStageRefresh);
        _spritesManager.ChannelChanged(sprite.SpriteNumWithChannel);
        _spritesManager.Mediator.RaiseSpriteSelected(sprite);
    }
}
