namespace Blingo.PacMan.Core.Enums;

public enum PMDirection
{
    None = 0,
    Left,
    Right,
    Up,
    Down,
}
public static class BlPacManDirectionExtensions
{
    public static bool IsHorizontal(this PMDirection direction)
    {
        return direction is PMDirection.Left or PMDirection.Right;
    }

    public static bool IsVertical(this PMDirection direction)
    {
        return direction is PMDirection.Up or PMDirection.Down;
    }

    public static PMDirection GetOpposite(this PMDirection direction)
    {
        return direction switch
        {
            PMDirection.Left => PMDirection.Right,
            PMDirection.Right => PMDirection.Left,
            PMDirection.Up => PMDirection.Down,
            PMDirection.Down => PMDirection.Up,
            _ => PMDirection.None,
        };
    }
}
