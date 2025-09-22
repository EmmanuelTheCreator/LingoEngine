using System;
using Godot;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.LGodot.Primitives;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;

namespace AbstUI.LGodot.Components.Inputs
{
    /// <summary>
    /// Godot implementation of <see cref="IAbstFrameworkInputText"/> using composition.
    /// </summary>
    public class AbstGodotInputText : IAbstFrameworkInputText, IHasTextBackgroundBorderColor, IDisposable, IFrameworkFor<AbstInputText>
    {
        private readonly Action<string>? _onChange;
        private readonly IAbstFontManager _fontManager;
        private Control _control;
        private LineEdit? _lineEdit;
        private TextEdit? _textEdit;

        private string? _font;
        private AMargin _margin = AMargin.Zero;
        private event Action? _onValueChanged;
        private event Action? _onCommit;
        private bool _suppressTextChangeNotification;
        private bool _textDirty;
        private bool _submitOnNextTextChange;

        private float _wantedWidth = 10;
        private float _wantedHeight = 10;
        private AColor _textColor = AColors.Black;
        private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
        private AColor _borderColor = AbstDefaultColors.InputBorderColor;
        private bool _isMultiLine;
        public event Action<int, int>? OnCaretChanged;
        public object FrameworkNode => _control;

        #region Properties


        public float X
        {
            get => _control.Position.X;
            set => _control.Position = new Vector2(value, _control.Position.Y);
        }

        public float Y
        {
            get => _control.Position.Y;
            set => _control.Position = new Vector2(_control.Position.X, value);
        }

        public float Width
        {
            get => _control.Size.X;
            set
            {
                _wantedWidth = value;
                _control.CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
                _control.Size = new Vector2(value, _wantedHeight);
            }
        }

        public float Height
        {
            get => _control.Size.Y;
            set
            {
                _wantedHeight = value;
                _control.CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
                _control.Size = new Vector2(_wantedWidth, value);
            }
        }

        public bool Visibility
        {
            get => _control.Visible;
            set => _control.Visible = value;
        }
       
        public bool Enabled
        {
            get => _lineEdit?.Editable ?? _textEdit?.Editable ?? true;
            set
            {
                if (_lineEdit != null) _lineEdit.Editable = value;
                if (_textEdit != null) _textEdit.Editable = value;
            }
        }

        private string _text = string.Empty;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                _textDirty = false;
                _submitOnNextTextChange = false;
                _suppressTextChangeNotification = true;
                try
                {
                    if (_lineEdit != null) _lineEdit.Text = value;
                    if (_textEdit != null) _textEdit.Text = value;
                }
                finally
                {
                    _suppressTextChangeNotification = false;
                }
            }
        }
        public int ZIndex
        {
            get => _zIndex;
            set
            {
                _zIndex = value;
                var val = _zIndex;
                if (val > 4028) val = 4028;
                if (val < -4028) val = -4028;

                if (_lineEdit != null) _lineEdit.ZIndex = val;
                if (_textEdit != null) _textEdit.ZIndex = val;
            }
        }

        private int _maxLength;
        public int MaxLength
        {
            get => _maxLength;
            set
            {
                if (_lineEdit != null) _lineEdit.MaxLength = value;
                _maxLength = value;
            }
        }

        string IAbstFrameworkNode.Name
        {
            get => _control.Name;
            set => _control.Name = value;
        }

        public string? Font
        {
            get => _font;
            set
            {
                _font = value;
                if (string.IsNullOrEmpty(value))
                {
                    _control.RemoveThemeFontOverride("font");
                }
                else
                {
                    var font = _fontManager.Get<FontFile>(value);
                    if (font != null)
                        _control.AddThemeFontOverride("font", font);
                }
            }
        }

        private int _fontSize;
        private int _caretColumn;
        private int _caretLine;
        private int _zIndex;

        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;

                Font? baseFont = string.IsNullOrEmpty(_font)
                    ? _fontManager.GetDefaultFont<Font>()
                    : _fontManager.Get<Font>(_font);
                if (baseFont == null)
                    return;

                var variation = new FontVariation
                {
                    BaseFont = baseFont
                };

                var theme = new Theme();
                var cls = _control.GetClass();
                theme.SetFont("font", cls, variation);
                theme.SetFontSize("font_size", cls, _fontSize);
                _control.Theme = theme;
            }
        }



        public AColor TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                _control.AddThemeColorOverride("font_color", _textColor.ToGodotColor());
            }
        }

        public AColor BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                _control.AddThemeColorOverride("background_color", _backgroundColor.ToGodotColor());
            }
        }

        public AColor BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                _control.AddThemeColorOverride("border_color", _borderColor.ToGodotColor());
            }
        }

        public AMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                _control.AddThemeConstantOverride("margin_left", (int)_margin.Left);
                _control.AddThemeConstantOverride("margin_right", (int)_margin.Right);
                _control.AddThemeConstantOverride("margin_top", (int)_margin.Top);
                _control.AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
            }
        }



        public bool IsMultiLine
        {
            get => _isMultiLine; set
            {
                if (_isMultiLine == value)
                    return;
                _isMultiLine = value;
                InitControl(_isMultiLine);
            }
        }

        public bool HasSelection => _lineEdit?.HasSelection() ?? _textEdit?.HasSelection() ?? false;

        #endregion



        public AbstGodotInputText(AbstInputText input, IAbstFontManager fontManager, Action<string>? onChange, bool multiLine = false)
        {
            _onChange = onChange;
            _fontManager = fontManager;
            IsMultiLine = multiLine;
            _control = InitControl(multiLine);

            input.Init(this);
        }

        private Control InitControl(bool multiLine)
        {
            if (_lineEdit != null)
            {
                _lineEdit.TextChanged -= OnLineEditTextChanged;
                _lineEdit.TextSubmitted -= OnLineEditTextSubmitted;
                _lineEdit.FocusExited -= OnTextEdit_FocusExited;
            }
            if (_textEdit != null)
            {
                _textEdit.TextChanged -= OnTextEditTextChanged;
                _textEdit.CaretChanged -= _textEdit_CaretChanged;
                _textEdit.FocusExited -= OnTextEdit_FocusExited;
                _textEdit.GuiInput -= OnTextEditGuiInput;
            }
            if (_control != null)
                _control.Ready -= ControlReady;
            _lineEdit = null;
            _textEdit = null;
            if (multiLine)
            {
                _textEdit = new TextEdit();
                _control = _textEdit;
                _textEdit.DeselectOnFocusLossEnabled = false;
            }
            else
            {
                _lineEdit = new LineEdit();
                _lineEdit.DeselectOnFocusLossEnabled = false;
                _control = _lineEdit;
            }

            _control.CustomMinimumSize = new Vector2(2, 2);
            _control.SizeFlagsHorizontal = 0;
            _control.SizeFlagsVertical = 0;

            if (_lineEdit != null)
            {
                _lineEdit.TextChanged += OnLineEditTextChanged;
                _lineEdit.TextSubmitted += OnLineEditTextSubmitted;
                _lineEdit.FocusExited += OnTextEdit_FocusExited;
            }
            if (_textEdit != null)
            {
                _textEdit.TextChanged += OnTextEditTextChanged;
                _textEdit.CaretChanged += _textEdit_CaretChanged;
                _textEdit.FocusExited += OnTextEdit_FocusExited;
                _textEdit.GuiInput += OnTextEditGuiInput;
            }

            _control.Ready += ControlReady;
            return _control;
        }


        public void Dispose()
        {
            if (_lineEdit != null)
            {
                _lineEdit.TextChanged -= OnLineEditTextChanged;
                _lineEdit.TextSubmitted -= OnLineEditTextSubmitted;
                _lineEdit.FocusExited -= OnTextEdit_FocusExited;
            }
            if (_textEdit != null)
            {
                _textEdit.TextChanged -= OnTextEditTextChanged;
                _textEdit.CaretChanged -= _textEdit_CaretChanged;
                _textEdit.FocusExited -= OnTextEdit_FocusExited;
                _textEdit.GuiInput -= OnTextEditGuiInput;
            }

            _control.QueueFree();
        }
        private void OnTextEdit_FocusExited()
        {
            _ = GetCaretPosition();
            _ = HasSelection;
            CommitPendingChanges();
        }

        private void ControlReady()
        {
            _control.CustomMinimumSize = new Vector2(_wantedWidth, _wantedHeight);
            _control.Size = new Vector2(_wantedWidth, _wantedHeight);
        }
        private void OnLineEditTextChanged(string _)
        {
            if (_lineEdit == null)
                return;

            _text = _lineEdit.Text ?? string.Empty;

            if (_suppressTextChangeNotification)
                return;

            _textDirty = true;
            _onValueChanged?.Invoke();
            _onChange?.Invoke(Text);
        }

        private void OnLineEditTextSubmitted(string _)
        {
            CommitPendingChanges();
        }
        private void _textEdit_CaretChanged()
        {
            if (_textEdit == null) return;
            _caretColumn = _textEdit.GetCaretColumn();
            _caretLine = _textEdit.GetCaretLine();
            OnCaretChanged?.Invoke(_caretLine, _caretColumn);
        }
        private void OnTextEditTextChanged()
        {
            if (_textEdit == null)
                return;

            _text = _textEdit.Text ?? string.Empty;

            if (_suppressTextChangeNotification)
                return;

            _textDirty = true;

            _onValueChanged?.Invoke();
            _onChange?.Invoke(Text);

            if (_submitOnNextTextChange)
            {
                CommitPendingChanges();
            }
        }

        private void OnTextEditGuiInput(InputEvent @event)
        {
            if (@event is not InputEventKey keyEvent)
                return;

            if (!keyEvent.Pressed || keyEvent.Echo)
                return;

            if (keyEvent.Keycode != Key.Enter && keyEvent.Keycode != Key.KpEnter)
                return;

            _submitOnNextTextChange = true;
        }

        private void CommitPendingChanges()
        {
            if (!_textDirty)
            {
                _submitOnNextTextChange = false;
                return;
            }

            _textDirty = false;
            _submitOnNextTextChange = false;

            _onCommit?.Invoke();
        }

        private (int line, int column) GetLineColumn(int index)
        {
            int line = 0;
            int column = 0;
            for (int i = 0; i < index && i < _text.Length; i++)
            {
                if (_text[i] == '\n')
                {
                    line++;
                    column = 0;
                }
                else
                {
                    column++;
                }
            }
            return (line, column);
        }

        private int GetOffset(int line, int column)
        {
            int index = 0;
            int currentLine = 0;
            while (index < _text.Length && currentLine < line)
            {
                if (_text[index] == '\n')
                    currentLine++;
                index++;
            }
            return Math.Clamp(index + column, 0, _text.Length);
        }



        public void DeleteSelection()
        {
            if (!HasSelection) return;
            if (_lineEdit != null)
            {
                int start = _lineEdit.GetSelectionFromColumn();
                int end = _lineEdit.GetSelectionToColumn();
                _text = _text.Remove(start, end - start);
                _lineEdit.Text = _text;
                _lineEdit.CaretColumn = start;
                _lineEdit.Deselect();
                OnCaretChanged?.Invoke(0, start);
            }
            else if (_textEdit != null)
            {
                int startLine = _textEdit.GetSelectionFromLine();
                int startCol = _textEdit.GetSelectionFromColumn();
                int endLine = _textEdit.GetSelectionToLine();
                int endCol = _textEdit.GetSelectionToColumn();
                int startIndex = GetOffset(startLine, startCol);
                int endIndex = GetOffset(endLine, endCol);
                _text = _text.Remove(startIndex, endIndex - startIndex);
                _textEdit.Text = _text;
                _textEdit.SetCaretLine(startLine);
                _textEdit.SetCaretColumn(startCol);
                _textEdit.Deselect();
                OnCaretChanged?.Invoke(startLine, startCol);
            }
        }

        public void SetCaretPosition(int line, int column)
        {
            if (_lineEdit != null)
            {
                _lineEdit.CaretColumn = column;
                _lineEdit.Deselect();
                OnCaretChanged?.Invoke(0, column);
            }
            else if (_textEdit != null)
            {
                _textEdit.SetCaretLine(line);
                _textEdit.SetCaretColumn(column);
                _textEdit.Deselect();
                OnCaretChanged?.Invoke(line, column);
            }
        }

        public (int line, int column) GetCaretPosition()
        {
            if (_lineEdit != null)
                return (0, _lineEdit.CaretColumn);
            if (_textEdit != null)
                return (_textEdit.GetCaretLine(), _textEdit.GetCaretColumn());
            return (0, 0);
        }

        public void SetSelection(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (_lineEdit != null)
            {
                _lineEdit.Select(startColumn, endColumn);
                _lineEdit.CaretColumn = endColumn;
            }
            else if (_textEdit != null)
            {
                _textEdit.Call("select", startLine, startColumn, endLine, endColumn);
                _textEdit.SetCaretLine(endLine);
                _textEdit.SetCaretColumn(endColumn);
            }
        }

        public void InsertText(string text)
        {
            if (HasSelection)
                DeleteSelection();
            var (line, column) = GetCaretPosition();
            int index = GetOffset(line, column);
            _text = _text.Insert(index, text);
            if (_lineEdit != null)
            {
                _lineEdit.Text = _text;
                int newIndex = index + text.Length;
                _lineEdit.CaretColumn = GetLineColumn(newIndex).column;
                _lineEdit.Deselect();
                OnCaretChanged?.Invoke(0, _lineEdit.CaretColumn);
            }
            else if (_textEdit != null)
            {
                _textEdit.Text = _text;
                int newIndex = index + text.Length;
                var (nLine, nCol) = GetLineColumn(newIndex);
                _textEdit.SetCaretLine(nLine);
                _textEdit.SetCaretColumn(nCol);
                _textEdit.Deselect();
                OnCaretChanged?.Invoke(nLine, nCol);
            }
        }

        event Action? IAbstFrameworkNodeInput.ValueChanged
        {
            add => _onValueChanged += value;
            remove => _onValueChanged -= value;
        }

        event Action? IAbstFrameworkNodeInput.OnCommit
        {
            add => _onCommit += value;
            remove => _onCommit -= value;
        }


    }
}

