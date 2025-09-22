using AbstUI.Primitives;

namespace AbstUI.Components.Buttons;

public interface IHasButtonBorderStates
{
    void SetBorderStateColors(AColor normal, AColor hover, AColor pressed);
}

public static class ButtonBorderStateExtensions
{
    public static void SetUniformBorderColor(this IHasButtonBorderStates button, AColor color)
    {
        button.SetBorderStateColors(color, color, color);
    }
}
