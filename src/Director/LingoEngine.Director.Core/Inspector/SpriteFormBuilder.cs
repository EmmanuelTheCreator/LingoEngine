using System;
using System.Linq.Expressions;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Tools;

namespace LingoEngine.Director.Core.Inspector
{
    /// <summary>
    /// Helper to fluently build sprite property forms with automatic column layout.
    /// </summary>
    internal class SpriteFormBuilder
    {
        private readonly ILingoFrameworkFactory _factory;
        private readonly LingoGfxPanel _panel;
        private int _columns = 1;
        private int _currentColumn;
        private int _currentRow;
        private readonly float _rowHeight = 22f;
        private readonly float _labelWidth = 50f;
        private readonly float _inputWidth = 60f;
        private readonly float _gap = 5f;
        private readonly float _margin = 5f;

        private float ColumnWidth => _labelWidth + _inputWidth + _gap * 2;

        public float CurrentHeight => (_currentRow + 1) * _rowHeight;

        public SpriteFormBuilder(LingoGfxPanel panel, ILingoFrameworkFactory factory)
        {
            _panel = panel;
            _factory = factory;
        }

        public SpriteFormBuilder Columns(int cols)
        {
            _columns = Math.Max(1, cols);
            _currentColumn = 0;
            return this;
        }

        private float ColumnX(int col) => _margin + col * ColumnWidth;

        private (float labelX, float inputX, float y) Layout(bool showLabel, int inputSpan)
        {
            float baseX = ColumnX(_currentColumn);
            float y = _currentRow * _rowHeight;
            float labelX = baseX;
            float inputX = baseX + (showLabel ? _labelWidth + _gap : _gap);
            return (labelX, inputX, y);
        }

        private float ComputeInputWidth(int span)
        {
            return _inputWidth * span + _gap * 2 * (span - 1);
        }

        private void Advance(int span)
        {
            _currentColumn += span;
            while (_currentColumn >= _columns)
            {
                _currentColumn -= _columns;
                _currentRow++;
            }
        }

        public SpriteFormBuilder AddTextInput<T>(string name, string label, T target, Expression<Func<T, string?>> property, int inputSpan = 1)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();
            var (xLabel, xInput, y) = Layout(true, inputSpan);
            _panel.SetLabelAt(_factory, name + "Label", xLabel, y, label);
            var input = _panel.SetInputTextAt(_factory, target, name + "Input", xInput, y, (int)ComputeInputWidth(inputSpan), property);
            input.Text = getter(target) ?? string.Empty;
            Advance(1 + inputSpan);
            return this;
        }

        public SpriteFormBuilder AddNumericInput<T>(string name, string label, T target, Expression<Func<T, float>> property, int inputSpan = 1, bool showLabel = true)
        {
            var (xLabel, xInput, y) = Layout(showLabel, inputSpan);
            if (showLabel)
                _panel.SetLabelAt(_factory, name + "Label", xLabel, y, label);
            _panel.SetInputNumberAt(_factory, target, name + "Input", xInput, y, (int)ComputeInputWidth(inputSpan), property);
            Advance((showLabel ? 1 : 0) + inputSpan);
            return this;
        }

        public SpriteFormBuilder AddColorInput<T>(string name, string label, T target, Expression<Func<T, LingoColor>> property, int inputSpan = 1)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();
            var (xLabel, xInput, y) = Layout(true, inputSpan);
            _panel.SetLabelAt(_factory, name + "Label", xLabel, y, label);
            var picker = _factory.CreateColorPicker(name + "Picker", color => setter(target, color));
            picker.Color = getter(target);
            picker.Width = ComputeInputWidth(inputSpan);
            _panel.AddItem(picker, xInput, y);
            Advance(1 + inputSpan);
            return this;
        }
    }
}
