using AbstUI.Texts;
using AbstUI.Primitives;
using AbstUI.Windowing;
using AbstUI.Components.Graphics;
using AbstUI.Components.Containers;
using AbstUI.Components;

namespace AbstUI.Inputs
{
    
    public static class AbstJoystickKeyboardExtensions
    {
        public static IAbstJoystickKeyboard CreateJoystickKeyboard(this IAbstComponentFactory factory,Action<AbstJoystickKeyboard>? configure = null, AbstJoystickKeyboard.KeyboardLayoutType layoutType = AbstJoystickKeyboard.KeyboardLayoutType.Azerty, bool showEscapeKey = false, APoint? position = null)
        {
            IAbstWindowManager windowManager = factory.GetRequiredService<IAbstWindowManager>();
            var keyboard = new AbstJoystickKeyboard(factory, layoutType, showEscapeKey);
            configure?.Invoke(keyboard);
            var rootNode = keyboard.RootFrameworkNode;
            var window = windowManager.ShowCustomDialog(keyboard.Title, rootNode, position)!;
            window.Dialog!.Borderless = true;
            keyboard.SetWindow(window);
            return keyboard;
        }
    }

    /// <inheritdoc/>
    public class AbstJoystickKeyboard : IAbstKeyEventHandler<AbstKeyEvent>, IDisposable, IAbstJoystickKeyboard
    {
        private readonly IAbstComponentFactory _factory;
        private readonly string[][] _layout;
        private readonly int _cols;
        private readonly AbstGfxCanvas _canvas;
        private readonly AbstPanel _root;
        private IAbstWindowDialogReference _window = null!;
        private IAbstMouseSubscription _mouseDownSub = null!;
        private IAbstMouseSubscription _mouseMoveSub = null!;
        private IAbstMouse<AbstMouseEvent> _mouse = null!;
        private AbstKey _key = null!;
        private int _selectedRow;
        private int _selectedCol;
        private bool _enableMouse = true;
        private bool _enableKeyNumbers = true;
        private bool _enableKeyLetters = true;
        private bool _enableKeySpecial = true;

        public IAbstFrameworkPanel RootFrameworkNode => _root.Framework<IAbstFrameworkPanel>();

        public enum KeyboardLayoutType
        {
            Azerty,
            Qwerty
        }


        #region Properties



        /// <summary>Color used for the border of the selected key.</summary>
        public AColor SelectedColor { get; set; } = AColors.White;

        /// <summary>Optional fill color for the selected key.</summary>
        public AColor? SelectedBackgroundColor { get; set; } = AColors.DarkGray;

        /// <summary>Background color of the keyboard.</summary>
        public AColor BackgroundColor { get; set; } = AColors.Black;
        public AColor BorderColor { get; set; } = AColors.DarkGray;
        public AColor TextColor { get; set; } = AColors.White;

        /// <summary>Font name used for key labels.</summary>
        public string? FontName { get; set; }
        public int CellSpacing { get; set; } = 2;
        public int Margin { get; set; } = 5;
        public int FontSize { get; set; } = 12;
        public int CellSize { get; set; } = 32;
        public bool FirstLetterUpper { get; set; } = true;

        private string _title = "Keyboard";

        /// <summary>Window title.</summary>
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                ApplyWindowChrome();
            }
        }

        private bool _showTitleBar = false;

        /// <summary>Whether to show the window title bar and close button.</summary>
        public bool ShowTitleBar
        {
            get => _showTitleBar;
            set
            {
                _showTitleBar = value;
                ApplyWindowChrome();
            }
        }

        /// <summary>Current text entered through the keyboard.</summary>
        public string Text { get; private set; } = string.Empty;

        /// <summary>Maximum allowed length of <see cref="Text"/>.</summary>
        public int MaxLength { get; set; } = int.MaxValue;

        #endregion


        /// <summary>Raised when the keyboard window is closed.</summary>
        public event Action? Closed;
        public event Action? EnterPressed;
        public event Action<string>? TextChanged;

        /// <summary>Raised when a key is selected.</summary>
        public event Action<string>? KeySelected;

        /// <summary>Supported keyboard layouts.</summary>


        public AbstJoystickKeyboard(IAbstComponentFactory factory, KeyboardLayoutType layoutType = KeyboardLayoutType.Azerty, bool showEscapeKey = false)
        {
            _factory = factory;
            _layout = layoutType == KeyboardLayoutType.Azerty ? BuildAzertyLayout(showEscapeKey) : BuildQwertyLayout(showEscapeKey);
            _cols = _layout.Max(r => r.Length);
            var width = _cols * (CellSize + CellSpacing);
            var height = _layout.Length * (CellSize + CellSpacing);
            //_window = _factory.CreateWindow("BlingoJoystickKeyboard", _title);
            _root = _factory.CreatePanel("BlingoJoystickKeyboardPanel");
            _root.Width = width + Margin * 2;
            _root.Height = height + Margin * 2;
            _canvas = _factory.CreateGfxCanvas("BlingoJoystickKeyboardCanvas", width, height);
            _canvas.X = Margin;
            _canvas.Y = Margin;
            _root.AddItem(_canvas);
        }

        public void SetWhiteTheme()
        {
            SelectedColor = AColors.Black;
            SelectedBackgroundColor = AColors.LightGray;
            BackgroundColor = AColors.White;
            BorderColor = AColors.LightGray;
            TextColor = AColors.Black;
        }

        public void SetWindow(IAbstWindowDialogReference window)
        {
            _window = window;
            if (_window.Dialog == null) throw new Exception("Dialog is null in subscription for keyboard");
            _mouse = (IAbstMouse<AbstMouseEvent>)_window.Dialog.Mouse;
            _key = (AbstKey)_window.Dialog.Key;
            _window.Dialog.OnWindowStateChanged += OnWindowStateChanged;
            _mouseDownSub = _mouse.OnMouseDown(OnMouseDown);
            _mouseMoveSub = _mouse.OnMouseMove(OnMouseMove);
            _key.Subscribe(this);
            ApplyWindowChrome();
            DrawKeyboard();
            //_window.Width = width + Margin * 2;
            //_window.Height = height + Margin * 2;
            _window.Dialog.BackgroundColor = BackgroundColor;
        }

        public void Dispose()
        {

            _mouseDownSub.Release();
            _mouseMoveSub.Release();
            _key.Unsubscribe(this);
            if (_window.Dialog != null)
            {
                _window.Dialog.OnWindowStateChanged -= OnWindowStateChanged;
                _window.Dialog.Dispose();
            }
        }
        public void UpdateStyle()
        {
            var width = _cols * (CellSize + CellSpacing);
            var height = _layout.Length * (CellSize + CellSpacing);
            _canvas.Width = width;
            _canvas.Height = height;

            if (_window.Dialog != null)
            {
                _window.Dialog.BackgroundColor = BackgroundColor;
                _window.Dialog.SetSize(width+10, height);
            }
            DrawKeyboard();
        }
        private void OnWindowStateChanged(bool state)
        {
            if (!state)
                Closed?.Invoke();
        }

        /// <summary>Enables hardware keyboard input.</summary>
        public void EnableKey(bool enableNumbers, bool enableLetters, bool enableSpecialKeys)
        {
            _enableKeyNumbers = enableNumbers;
            _enableKeyLetters = enableLetters;
            _enableKeySpecial = enableSpecialKeys;
        }

        /// <summary>Enables hardware mouse input.</summary>
        public void EnableMouse(bool state) => _enableMouse = state;

        private static string[][] BuildQwertyLayout(bool showEsc)
        {
            string[][] layout =
            [
                ["1","2","3","4","5","6","7","8","9","0"],
                ["Q","W","E","R","T","Y","U","I","O","P","BACKSPACE"],
                ["A","S","D","F","G","H","J","K","L","ENTER"],
                ["Z","X","C","V","B","N","M","SPACE"]
            ];
            if (showEsc)
                layout[0] = layout[0].Concat(["ESC"]).ToArray();
            return layout;
        }

        private static string[][] BuildAzertyLayout(bool showEsc)
        {
            string[][] layout =
            [
                ["1","2","3","4","5","6","7","8","9","0"],
                ["A","Z","E","R","T","Y","U","I","O","P","BACKSPACE"],
                ["Q","S","D","F","G","H","J","K","L","M","ENTER"],
                ["W","X","C","V","B","N","SPACE"]
            ];
            if (showEsc)
                layout[0] = layout[0].Concat(new[] { "ESC" }).ToArray();
            return layout;
        }

        private void ApplyWindowChrome()
        {
            //if (ShowTitleBar)
            //    _window.Title = _title;
            //else
            //    _window.Title = string.Empty;
            //_window.Borderless = !ShowTitleBar;
        }

        private void DrawKeyboard()
        {
            _canvas.Clear(BackgroundColor);
            for (int r = 0; r < _layout.Length; r++)
            {
                for (int c = 0; c < _layout[r].Length; c++)
                {
                    var x = c * (CellSize + CellSpacing);
                    var y = r * (CellSize + CellSpacing);
                    var key = _layout[r][c];
                    var selected = r == _selectedRow && c == _selectedCol;
                    if (selected && SelectedBackgroundColor.HasValue)
                        _canvas.DrawRect(ARect.New(x, y, CellSize, CellSize), SelectedBackgroundColor.Value, true);
                    var border = selected ? SelectedColor : BorderColor;
                    _canvas.DrawRect(ARect.New(x, y, CellSize, CellSize), border, false, 1);
                    var label = key switch { "SPACE" => "Space", "BACKSPACE" => "<-", "ENTER" => "OK", "ESC" => "Esc", _ => key };
                    _canvas.DrawText(new APoint(x + 2, y + 10), label, FontName, TextColor, FontSize, CellSize - 4, AbstTextAlignment.Center);
                }
            }
        }

        /// <summary>Moves selection one cell up.</summary>
        public void MoveUp()
        {
            _selectedRow = (_selectedRow - 1 + _layout.Length) % _layout.Length;
            _selectedCol = Math.Min(_selectedCol, _layout[_selectedRow].Length - 1);
            DrawKeyboard();
        }

        /// <summary>Moves selection one cell down.</summary>
        public void MoveDown()
        {
            _selectedRow = (_selectedRow + 1) % _layout.Length;
            _selectedCol = Math.Min(_selectedCol, _layout[_selectedRow].Length - 1);
            DrawKeyboard();
        }

        /// <summary>Moves selection one cell left.</summary>
        public void MoveLeft()
        {
            var len = _layout[_selectedRow].Length;
            _selectedCol = (_selectedCol - 1 + len) % len;
            DrawKeyboard();
        }

        /// <summary>Moves selection one cell right.</summary>
        public void MoveRight()
        {
            var len = _layout[_selectedRow].Length;
            _selectedCol = (_selectedCol + 1) % len;
            DrawKeyboard();
        }

        /// <summary>Gets the string value of the currently selected key without executing it.</summary>
        public string GetSelectedKey()
        {
            var key = _layout[_selectedRow][_selectedCol];
            return key switch
            {
                "SPACE" => " ",
                "BACKSPACE" => "\b",
                "ENTER" => string.Empty,
                "ESC" => string.Empty,
                _ => key
            };
        }

        /// <summary>Executes the currently selected key and updates <see cref="Text"/>.</summary>
        public string ExecuteSelectedKey()
        {
            var key = _layout[_selectedRow][_selectedCol];
            var result = GetSelectedKey();
            if (key == "ENTER")
            {
                EnterPressed?.Invoke();
            }
            else if (key == "ESC")
            {
                Close();
            }
            else
            {
                ProcessInput(result);
            }
            return result;
        }

        private void ProcessInput(string value)
        {
            if (value == "\b")
            {
                if (Text.Length > 0)
                    Text = Text.Substring(0, Text.Length - 1);
            }
            else
            {
                if (Text.Length == 0 && value == " ") return; // no leading space
                if (Text.Length + value.Length <= MaxLength)
                {
                    value = value.ToLower();
                    if (FirstLetterUpper && ((Text.Length == 0 && value.Length == 1) || (Text.Length >2 && Text[Text.Length-1] == ' ')))
                        value = value.ToUpperInvariant();
                    Text += value;
                }
            
            }

            TextChanged?.Invoke(value);
            KeySelected?.Invoke(value);
            DrawKeyboard();
        }

        public void RaiseKeyUp(AbstKeyEvent key) { }
        public void RaiseKeyDown(AbstKeyEvent key)
        {
            if (_window.Dialog == null || !_window.Dialog.IsOpen) return;
            if (!(_enableKeyNumbers || _enableKeyLetters || _enableKeySpecial)) return;
            var name = key.Key.ToUpperInvariant();
            bool isDigit = name.Length == 1 && char.IsDigit(name[0]);
            bool isLetter = name.Length == 1 && char.IsLetter(name[0]);
            bool isSpecial = !isDigit && !isLetter;
            if ((isDigit && !_enableKeyNumbers) || (isLetter && !_enableKeyLetters) || (isSpecial && !_enableKeySpecial))
                return;
            switch (name)
            {
                case "UP":
                    MoveUp();
                    break;
                case "DOWN":
                    MoveDown();
                    break;
                case "LEFT":
                    MoveLeft();
                    break;
                case "RIGHT":
                    MoveRight();
                    break;
                case "ENTER":
                case "RETURN":
                    //EnterPressed?.Invoke();
                    ExecuteSelectedKey();
                    break;
                case "SPACE":
                    ProcessInput(" ");
                    break;
                case "BACKSPACE":
                    ProcessInput("\b");
                    break;
                case "ESC":
                case "ESCAPE":
                    Close();
                    break;
                default:
                    if (name.Length == 1 && char.IsLetterOrDigit(name[0]))
                        ProcessInput(name);
                    break;
            }
        }




        private void OnMouseMove(AbstMouseEvent e)
        {
            //Console.WriteLine($"Mouse moved: {e.MouseH}, {e.MouseV}");  
            if (_window.Dialog == null || !_window.Dialog.Visibility) return;
            if (!_enableMouse) return;
            var localX = e.MouseH - Margin;
            var localY = e.MouseV - Margin;
            var row = (int)(localY / (CellSize + CellSpacing));
            var col = (int)(localX / (CellSize + CellSpacing));
            if (row < 0 || row >= _layout.Length || col < 0 || col >= _layout[row].Length) return;
            _selectedRow = row;
            _selectedCol = col;
            DrawKeyboard();
        }

        private void OnMouseDown(AbstMouseEvent e)
        {
            if (_window.Dialog == null || !_window.Dialog.Visibility) return;
            if (!_enableMouse) return;
            OnMouseMove(e);
            ExecuteSelectedKey();
            e.ContinuePropagation = false;
        }

        /// <summary>Opens the keyboard popup window at the given position or centered if none.</summary>
        public void Open(APoint? position = null)
        {
            ApplyWindowChrome();
            if (_window.Dialog == null) return;
            if (position.HasValue)
            {
                _window.Dialog.X = position.Value.X;
                _window.Dialog.Y = position.Value.Y;
                _window.Dialog.Popup();
            }
            else
            {
                _window.Dialog.PopupCentered();
            }
        }

        /// <summary>Closes the keyboard popup window.</summary>
        public void Close()
        {
            if (_window.Dialog == null) return;
            _window.Dialog.Hide();
        }
    }
}


