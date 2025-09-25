using Blingo.PacMan.Core.Datas;

namespace Blingo.PacMan.Core.Game;

public static class BlPacManDirectionExtensions
{
    public static bool IsHorizontal(this BlPacManDirection direction)
    {
        return direction is BlPacManDirection.Left or BlPacManDirection.Right;
    }

    public static bool IsVertical(this BlPacManDirection direction)
    {
        return direction is BlPacManDirection.Up or BlPacManDirection.Down;
    }

    public static BlPacManDirection GetOpposite(this BlPacManDirection direction)
    {
        return direction switch
        {
            BlPacManDirection.Left => BlPacManDirection.Right,
            BlPacManDirection.Right => BlPacManDirection.Left,
            BlPacManDirection.Up => BlPacManDirection.Down,
            BlPacManDirection.Down => BlPacManDirection.Up,
            _ => BlPacManDirection.None,
        };
    }
}
