using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiInputText : AbstImGuiComponent, IAbstFrameworkInputText, IHasTextBackgroundBorderColor, IDisposable
    {
        private int _caretIndex;
        private int _selectionStartIndex = -1;
        private bool _textDirty;

        public AbstImGuiInputText(AbstImGuiComponentFactory factory, bool multiLine) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set
            {
                var newValue = value ?? string.Empty;
                if (_text == newValue)
                    return;
                _text = newValue;
                _textDirty = false;
            }
        }
        public int MaxLength { get; set; }
        public string? Font { get; set; }
        public int FontSize { get; set; } = 12;
        public AMargin Margin { get; set; } = AMargin.Zero;
        public object FrameworkNode => this;
        public AColor TextColor { get; set; } = AColors.Black;
        public AColor BackgroundColor { get; set; } = AbstDefaultColors.Input_Bg;
        public AColor BorderColor { get; set; } = AbstDefaultColors.InputBorderColor;

        public bool IsMultiLine { get; set; }

        public bool HasSelection => _selectionStartIndex != -1 && _selectionStartIndex != _caretIndex;

        public void DeleteSelection()
        {
            if (!HasSelection) return;
            int start = Math.Min(_selectionStartIndex, _caretIndex);
            int end = Math.Max(_selectionStartIndex, _caretIndex);
            _text = _text.Remove(start, end - start);
            _caretIndex = start;
            _selectionStartIndex = -1;
            _textDirty = true;
            var (line, column) = GetLineColumn(start);
            OnCaretChanged?.Invoke(line, column);
            ValueChanged?.Invoke();
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
            _textDirty = true;
            var (line, column) = GetLineColumn(_caretIndex);
            OnCaretChanged?.Invoke(line, column);
            ValueChanged?.Invoke();
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

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            if (!Visibility) return nint.Zero;

            // place relative to current context origin
            global::ImGuiNET.ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));

            if (Width > 0) global::ImGuiNET.ImGui.SetNextItemWidth(Width);

            global::ImGuiNET.ImGui.PushID(Name);
            global::ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Text, TextColor.ToImGuiColor());
            global::ImGuiNET.ImGui.PushStyleColor(ImGuiCol.FrameBg, BackgroundColor.ToImGuiColor());
            global::ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Border, BorderColor.ToImGuiColor());
            if (!Enabled) global::ImGuiNET.ImGui.BeginDisabled();

            uint cap = MaxLength > 0 ? (uint)MaxLength : 1024u;
            ImGuiInputTextFlags flags = ImGuiInputTextFlags.None;
            if (!IsMultiLine)
            {
                flags |= ImGuiInputTextFlags.EnterReturnsTrue;
            }

            bool textEdited = global::ImGuiNET.ImGui.InputText("##text", ref _text, cap, flags);
            if (textEdited)
            {
                _caretIndex = _text.Length;
                _selectionStartIndex = -1;
                _textDirty = true;
                var (line, column) = GetLineColumn(_caretIndex);
                OnCaretChanged?.Invoke(line, column);
                ValueChanged?.Invoke();
            }

            if (global::ImGuiNET.ImGui.IsItemDeactivatedAfterEdit())
            {
                CommitPendingChanges();
            }

            if (global::ImGuiNET.ImGui.IsItemActive() && _textDirty)
            {
                if (global::ImGuiNET.ImGui.IsKeyPressed(ImGuiKey.Enter) || global::ImGuiNET.ImGui.IsKeyPressed(ImGuiKey.KeypadEnter))
                {
                    CommitPendingChanges();
                }
            }

            if (!Enabled) global::ImGuiNET.ImGui.EndDisabled();
            global::ImGuiNET.ImGui.PopStyleColor(3);
            global::ImGuiNET.ImGui.PopID();
            return AbstImGuiRenderResult.RequireRender();
        }


        public override void Dispose() => base.Dispose();

        private void CommitPendingChanges()
        {
            if (!_textDirty)
                return;

            _textDirty = false;
            OnCommit?.Invoke();
        }
    }
}
