using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Settings;
using Blingo.PacMan.Core.Sprites.ParentScripts;

namespace Blingo.PacMan.Core.Game
{
    internal class PMGhostLogic
    {
        internal static PMTile? GetChaseTargetTile(PMGhostManager ghostManager, MrGhost ghostName, PMCharacter character, PMTile pacmanTile, PMDirection direction, PMTile scatterTarget)
        {
            return ghostName switch
            {
                MrGhost.Pinky => ResolvePinkyTarget(pacmanTile, direction) ?? pacmanTile,
                MrGhost.Inky => ResolveInkyTarget(ghostManager, pacmanTile, direction),
                MrGhost.Sue => ResolveSueTarget(pacmanTile, character, scatterTarget),
                _ => pacmanTile,
            };
        }


        private static PMTile? ResolvePinkyTarget(PMTile? pacmanTile, PMDirection direction) => StepForward(pacmanTile, direction, 4);
      
        private static PMTile? ResolveInkyTarget(PMGhostManager ghostManager, PMTile pacmanTile, PMDirection direction)
        {
            var blinkyTile = ghostManager.FindGhost(MrGhost.Blinky)?.CurrentTile;
            // Two tiles in front of pacman
            var pacmanTileNext = StepForward(pacmanTile, direction, 2) ?? pacmanTile;
            if (pacmanTileNext is null || blinkyTile is null)
                return null;

            var offsetColumn = pacmanTileNext.Column - blinkyTile.Column;
            var offsetRow = pacmanTileNext.Row - blinkyTile.Row;

            return pacmanTileNext.Map.GetTile(pacmanTileNext.Column + offsetColumn, pacmanTileNext.Row + offsetRow);
        }

        private static PMTile? ResolveSueTarget(PMTile pacmanTile, PMCharacter character, PMTile scatterTarget)
        {
            var current = character.GetTile();

            if (pacmanTile is null || current is null)
                return pacmanTile;

            var distance = PMTileMath.GetDistance(pacmanTile, current);
            return distance >= BlPacManTheme.Actor.SpriteSize ? pacmanTile : scatterTarget;
        }


        private static PMTile? StepForward(PMTile? pacmanTile, PMDirection direction, int steps)
        {
            var current = pacmanTile;
            for (var i = 0; i < steps && current is not null; i++)
                current = current.Get(direction);

            return current;
        }

    }
}
