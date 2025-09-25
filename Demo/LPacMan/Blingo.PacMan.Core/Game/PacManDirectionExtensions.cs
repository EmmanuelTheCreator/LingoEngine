namespace Blingo.PacMan.Core.Game;

public static class PacManDirectionExtensions
{
    public static bool IsHorizontal(this PacManDirection direction)
    {
        return direction is PacManDirection.Left or PacManDirection.Right;
    }

    public static bool IsVertical(this PacManDirection direction)
    {
        return direction is PacManDirection.Up or PacManDirection.Down;
    }

    public static PacManDirection GetOpposite(this PacManDirection direction)
    {
        return direction switch
        {
            PacManDirection.Left => PacManDirection.Right,
            PacManDirection.Right => PacManDirection.Left,
            PacManDirection.Up => PacManDirection.Down,
            PacManDirection.Down => PacManDirection.Up,
            _ => PacManDirection.None,
        };
    }
}
