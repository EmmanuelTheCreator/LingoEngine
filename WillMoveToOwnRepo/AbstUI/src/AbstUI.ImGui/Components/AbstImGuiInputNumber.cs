using System;
using System.Numerics;
using ImGuiNET;
using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Styles;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiInputNumber : AbstImGuiComponent, IAbstFrameworkInputNumber<float>, IHasTextBackgroundBorderColor, IDisposable
    {
        public AbstImGuiInputNumber(AbstImGuiComponentFactory factory) : base(factory)
        {
            _pendingValue = _value;
        }
        public bool Enabled { get; set; } = true;
        private float _value;
        private float _pendingValue;
        private bool _valueDirty;
        public float Value
        {
            get => _value;
            set
            {
                var clamped = Math.Clamp(value, Min, Max);
                if (Math.Abs(_value - clamped) < float.Epsilon)
                    return;
                _value = clamped;
                _pendingValue = _value;
                _valueDirty = false;
            }
        }
        public float Min { get; set; }
        public float Max { get; set; }
        public ANumberType NumberType { get; set; } = ANumberType.Float;
        public AMargin Margin { get; set; } = AMargin.Zero;
        public event Action? ValueChanged;
        public event Action? OnCommit;
        public object FrameworkNode => this;

        public int FontSize { get; set; } = 12;
        public AColor TextColor { get; set; } = AColors.Black;
        public AColor BackgroundColor { get; set; } = AbstDefaultColors.Input_Bg;
        public AColor BorderColor { get; set; } = AbstDefaultColors.InputBorderColor;
        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            if (!Visibility) return nint.Zero;

            global::ImGuiNET.ImGui.SetCursorScreenPos(context.Origin + new Vector2(X, Y));
            global::ImGuiNET.ImGui.PushID(Name);
            global::ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Text, TextColor.ToImGuiColor());
            global::ImGuiNET.ImGui.PushStyleColor(ImGuiCol.FrameBg, BackgroundColor.ToImGuiColor());
            global::ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Border, BorderColor.ToImGuiColor());
            if (!Enabled)
                global::ImGuiNET.ImGui.BeginDisabled();

            if (NumberType == ANumberType.Integer)
            {
                int val = _valueDirty ? (int)_pendingValue : (int)_value;
                if (global::ImGuiNET.ImGui.InputInt("##num", ref val, 1, 100, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    val = Math.Clamp(val, (int)Min, (int)Max);
                    _pendingValue = val;
                    _valueDirty = true;
                    ValueChanged?.Invoke();
                }
            }
            else
            {
                float val = _valueDirty ? _pendingValue : _value;
                if (global::ImGuiNET.ImGui.InputFloat("##num", ref val, 0f, 0f, "%.3f", ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    val = Math.Clamp(val, Min, Max);
                    _pendingValue = val;
                    _valueDirty = true;
                    ValueChanged?.Invoke();
                }
            }

            if (global::ImGuiNET.ImGui.IsItemDeactivatedAfterEdit())
            {
                CommitPendingValue();
            }

            if (global::ImGuiNET.ImGui.IsItemActive() && _valueDirty)
            {
                if (global::ImGuiNET.ImGui.IsKeyPressed(ImGuiKey.Enter) || global::ImGuiNET.ImGui.IsKeyPressed(ImGuiKey.KeypadEnter))
                {
                    CommitPendingValue();
                }
            }

            if (!Enabled)
                global::ImGuiNET.ImGui.EndDisabled();
            global::ImGuiNET.ImGui.PopStyleColor(3);
            global::ImGuiNET.ImGui.PopID();
            return AbstImGuiRenderResult.RequireRender();
        }

        public override void Dispose() => base.Dispose();

        private void CommitPendingValue()
        {
            if (!_valueDirty)
                return;

            float newValue = _pendingValue;

            if (NumberType == ANumberType.Integer)
            {
                newValue = Math.Clamp(newValue, Min, Max);
                newValue = (float)Math.Round(newValue);
            }
            else
            {
                newValue = Math.Clamp(newValue, Min, Max);
            }

            _valueDirty = false;
            if (Math.Abs(_value - newValue) < float.Epsilon)
            {
                _pendingValue = _value;
                OnCommit?.Invoke();
                return;
            }

            _value = newValue;
            _pendingValue = _value;
            ValueChanged?.Invoke();
            OnCommit?.Invoke();
        }
    }
}
