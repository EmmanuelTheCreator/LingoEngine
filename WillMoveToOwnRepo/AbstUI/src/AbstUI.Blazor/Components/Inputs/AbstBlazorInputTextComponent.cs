using System;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Inputs;

public class AbstBlazorInputTextComponent : AbstBlazorComponentModelBase, IAbstFrameworkInputText, IFrameworkFor<AbstInputText>, IHasTextBackgroundBorderColor
{
    private string _text = string.Empty;
    private int _caretIndex;
    private int _selectionStartIndex = -1;
    private bool _textDirty;
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                _textDirty = false;
                RaiseChanged();
            }
        }
    }

    private int _maxLength;
    public int MaxLength
    {
        get => _maxLength;
        set { if (_maxLength != value) { _maxLength = value; RaiseChanged(); } }
    }

    private string? _font;
    public string? Font
    {
        get => _font;
        set { if (_font != value) { _font = value; RaiseChanged(); } }
    }

    private int _fontSize = 14;
    public int FontSize
    {
        get => _fontSize;
        set { if (_fontSize != value) { _fontSize = value; RaiseChanged(); } }
    }

    private AColor _textColor = AColors.Black;
    public AColor TextColor
    {
        get => _textColor;
        set { if (!_textColor.Equals(value)) { _textColor = value; RaiseChanged(); } }
    }

    private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
    public AColor BackgroundColor
    {
        get => _backgroundColor;
        set { if (!_backgroundColor.Equals(value)) { _backgroundColor = value; RaiseChanged(); } }
    }

    private AColor _borderColor = AbstDefaultColors.InputBorderColor;
    public AColor BorderColor
    {
        get => _borderColor;
        set { if (!_borderColor.Equals(value)) { _borderColor = value; RaiseChanged(); } }
    }

    private bool _isMultiLine;
    public bool IsMultiLine
    {
        get => _isMultiLine;
        set { if (_isMultiLine != value) { _isMultiLine = value; RaiseChanged(); } }
    }

    public bool HasSelection => _selectionStartIndex != -1 && _selectionStartIndex != _caretIndex;

    public void DeleteSelection()
    {
        if (!HasSelection) return;
        int start = Math.Min(_selectionStartIndex, _caretIndex);
        int end = Math.Max(_selectionStartIndex, _caretIndex);
        _text = _text.Remove(start, end - start);
        _caretIndex = start;
        _selectionStartIndex = -1;
        RaiseChanged();
        MarkDirty();
        var (line, column) = GetLineColumn(start);
        OnCaretChanged?.Invoke(line, column);
    }

    public void SetCaretPosition(int line, int column)
    {
        _caretIndex = GetOffset(line, column);
        _selectionStartIndex = -1;
        OnCaretChanged?.Invoke(line, column);
    }

    public (int line, int column) GetCaretPosition()
    {
        return GetLineColumn(_caretIndex);
    }

    public void SetSelection(int startLine, int startColumn, int endLine, int endColumn)
    {
        _selectionStartIndex = GetOffset(startLine, startColumn);
        _caretIndex = GetOffset(endLine, endColumn);
        if (_selectionStartIndex == _caretIndex)
            _selectionStartIndex = -1;
        OnCaretChanged?.Invoke(endLine, endColumn);
    }

    public void InsertText(string text)
    {
        if (HasSelection)
            DeleteSelection();
        _text = _text.Insert(_caretIndex, text);
        _caretIndex += text.Length;
        RaiseChanged();
        MarkDirty();
        var (line, column) = GetLineColumn(_caretIndex);
        OnCaretChanged?.Invoke(line, column);
    }

    private bool _enabled = true;
    public bool Enabled
    {
        get => _enabled;
        set { if (_enabled != value) { _enabled = value; RaiseChanged(); } }
    }

    public event Action? ValueChanged;
    public event Action? OnCommit;

    public void RaiseCommit()
    {
        if (!_textDirty)
            return;

        _textDirty = false;
        OnCommit?.Invoke();
    }

    internal void MarkDirty()
    {
        _textDirty = true;
        ValueChanged?.Invoke();
    }

    public event Action<int, int>? OnCaretChanged;

    private (int line, int column) GetLineColumn(int index)
    {
        index = Math.Clamp(index, 0, _text.Length);
        int line = 0;
        int column = 0;
        for (int i = 0; i < index; i++)
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
            if (_text[index] == '\n') currentLine++;
            index++;
        }
        return Math.Clamp(index + column, 0, _text.Length);
    }
}
