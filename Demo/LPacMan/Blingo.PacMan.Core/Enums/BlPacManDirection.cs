namespace Blingo.PacMan.Core.Datas;

public enum BlPacManDirection
{
    None = 0,
    Left,
    Right,
    Up,
    Down,
}
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
