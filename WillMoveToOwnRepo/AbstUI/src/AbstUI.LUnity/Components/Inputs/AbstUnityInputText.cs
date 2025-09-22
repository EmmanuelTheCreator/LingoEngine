using System;
using AbstUI.Components.Inputs;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using AbstUI.LUnity.Primitives;
using AbstUI.Primitives;
using UnityEngine;
using UnityEngine.UI;
using AbstUI.Styles;

namespace AbstUI.LUnity.Components.Inputs;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkInputText"/>.
/// </summary>
internal class AbstUnityInputText : AbstUnityComponent, IAbstFrameworkInputText, IFrameworkFor<AbstInputText>, IHasTextBackgroundBorderColor
{
    private readonly InputField _inputField;
    private readonly Text _textComponent;
    private readonly Image _image;
    private string _text = string.Empty;
    private AColor _textColor = new(0, 0, 0);
    private AColor _backgroundColor = AbstDefaultColors.Input_Bg;
    private AColor _borderColor = AbstDefaultColors.InputBorderColor;
    private bool _suppressValueChanged;
    private bool _textDirty;

    public AbstUnityInputText() : base(CreateGameObject(out var input, out var text, out var image))
    {
        _inputField = input;
        _textComponent = text;
        _image = image;
        _inputField.onValueChanged.AddListener(OnValueChanged);
        _inputField.onEndEdit.AddListener(OnEndEdit);
        _image.color = _backgroundColor.ToUnityColor();
    }

    private static GameObject CreateGameObject(out InputField input, out Text text, out Image image)
    {
        var go = new GameObject("InputField");
        image = go.AddComponent<Image>();
        input = go.AddComponent<InputField>();
        var textGo = new GameObject("Text");
        textGo.transform.parent = go.transform;
        text = textGo.AddComponent<Text>();
        input.textComponent = text;
        return go;
    }

    private void OnValueChanged(string value)
    {
        if (_suppressValueChanged)
            return;

        if (_text == value)
            return;

        _text = value;
        _textComponent.text = value;
        _textDirty = true;
        var (line, column) = GetLineColumn(_inputField.caretPosition);
        OnCaretChanged?.Invoke(line, column);
        ValueChanged?.Invoke();
    }

    private void OnEndEdit(string value)
    {
        if (_suppressValueChanged)
            return;

        _text = value;
        _textComponent.text = value;
        CommitPendingChanges();
    }

    public bool Enabled
    {
        get => _inputField.interactable;
        set => _inputField.interactable = value;
    }

    public string Text
    {
        get => _text;
        set
        {
            var newValue = value ?? string.Empty;
            if (_text == newValue) return;
            SetTextInternal(newValue, markDirty: false);
        }
    }

    public int MaxLength
    {
        get => _inputField.characterLimit;
        set => _inputField.characterLimit = value;
    }

    public string? Font { get; set; }

    public int FontSize { get; set; }

    public AColor TextColor
    {
        get => _textColor;
        set
        {
            _textColor = value;
            _textComponent.color = value.ToUnityColor();
        }
    }

    public AColor BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            _image.color = value.ToUnityColor();
        }
    }

    public AColor BorderColor
    {
        get => _borderColor;
        set => _borderColor = value;
    }

    public bool IsMultiLine
    {
        get => _inputField.lineType != InputField.LineType.SingleLine;
        set => _inputField.lineType = value ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
    }

    public bool HasSelection => _inputField.selectionAnchorPosition != _inputField.selectionFocusPosition;

    public void DeleteSelection()
    {
        if (!HasSelection) return;
        int start = Math.Min(_inputField.selectionAnchorPosition, _inputField.selectionFocusPosition);
        int end = Math.Max(_inputField.selectionAnchorPosition, _inputField.selectionFocusPosition);
        SetTextInternal(_text.Remove(start, end - start), markDirty: true);
        var (line, column) = GetLineColumn(start);
        SetCaretPosition(line, column);
    }

    public void SetCaretPosition(int line, int column)
    {
        int pos = GetOffset(line, column);
        _inputField.caretPosition = pos;
        _inputField.selectionAnchorPosition = pos;
        _inputField.selectionFocusPosition = pos;
        OnCaretChanged?.Invoke(line, column);
    }

    public (int line, int column) GetCaretPosition()
    {
        return GetLineColumn(_inputField.caretPosition);
    }

    public void SetSelection(int startLine, int startColumn, int endLine, int endColumn)
    {
        int s = GetOffset(startLine, startColumn);
        int e = GetOffset(endLine, endColumn);
        _inputField.selectionAnchorPosition = s;
        _inputField.selectionFocusPosition = e;
        _inputField.caretPosition = e;
        OnCaretChanged?.Invoke(endLine, endColumn);
    }

    public void InsertText(string text)
    {
        if (HasSelection)
            DeleteSelection();
        int pos = _inputField.caretPosition;
        SetTextInternal(_text.Insert(pos, text), markDirty: true);
        var (line, column) = GetLineColumn(pos + text.Length);
        SetCaretPosition(line, column);
    }

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

    public event Action? ValueChanged;
    public event Action? OnCommit;
    public event Action<int, int>? OnCaretChanged;

    private void SetTextInternal(string value, bool markDirty)
    {
        _text = value;
        _suppressValueChanged = true;
        try
        {
            _inputField.text = value;
            _textComponent.text = value;
        }
        finally
        {
            _suppressValueChanged = false;
        }

        if (markDirty)
        {
            _textDirty = true;
            ValueChanged?.Invoke();
        }
        else
        {
            _textDirty = false;
        }
    }

    private void CommitPendingChanges()
    {
        if (!_textDirty)
            return;

        _textDirty = false;
        ValueChanged?.Invoke();
        OnCommit?.Invoke();
    }
}
