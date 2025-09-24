using AbstUI.Primitives;
using AbstUI.Windowing;
using AbstUI.Components.Containers;

namespace AbstUI.Inputs
{
    /// <summary>
    /// On-screen keyboard navigated via joystick.
    /// Supports letters, digits, space and backspace.
    /// </summary>
    public interface IAbstJoystickKeyboard
    {
        AColor BackgroundColor { get; set; }
        AColor BorderColor { get; set; }
        int CellSize { get; set; }
        int CellSpacing { get; set; }
        string? FontName { get; set; }
        int FontSize { get; set; }
        int Margin { get; set; }
        int MaxLength { get; set; }
        IAbstFrameworkPanel RootFrameworkNode { get; }
        AColor? SelectedBackgroundColor { get; set; }
        AColor SelectedColor { get; set; }
        bool ShowTitleBar { get; set; }
        string Text { get; }
        AColor TextColor { get; set; }
        string Title { get; set; }

        event Action? Closed;
        event Action? EnterPressed;
        event Action<string>? KeySelected;
        event Action<string>? TextChanged;

        void Close();
        void Dispose();
        void EnableKey(bool enableNumbers, bool enableLetters, bool enableSpecialKeys);
        void EnableMouse(bool state);
        string ExecuteSelectedKey();
        string GetSelectedKey();
        void MoveDown();
        void MoveLeft();
        void MoveRight();
        void MoveUp();
        void Open(APoint? position = null);
        void RaiseKeyDown(AbstKeyEvent key);
        void RaiseKeyUp(AbstKeyEvent key);
        void SetWhiteTheme();
        void SetWindow(IAbstWindowDialogReference window);
        void UpdateStyle();
    }
}


