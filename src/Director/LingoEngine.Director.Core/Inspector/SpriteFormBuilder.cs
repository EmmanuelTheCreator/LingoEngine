using System;
using System.ComponentModel;
using System.Linq.Expressions;
using LingoEngine.Director.Core.UI;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Tools;

namespace LingoEngine.Director.Core.Inspector
{
    /// <summary>
    /// Helper to fluently build sprite property forms with automatic column layout.
    /// </summary>
    public class GfxPanelBuilder
    {
        private readonly ILingoFrameworkFactory _factory;
        private readonly LingoGfxPanel _panel;
        private int _columns = 1;
        private int _currentColumn;
        private int _currentRow;
        private readonly float _rowHeight = 24f;
        private readonly float _labelSmallWidth = 24f;
        private readonly float _labelLargeWidth = 36f;
        private readonly float _inputWidth = 24f;
        private readonly float _gap = 1f;
        private readonly float _margin = 4f;
        private readonly float _totalWidth = 254f; // or panel.Width if dynamic
        private int _labelYOffset = 2; // offset for label Y position, can be adjusted if needed

        //private float ColumnWidth => GetLabelWidth();
        private float ColumnWidth => (_totalWidth - (_margin * 2)) / _columns;


        private float GetLabelWidth() => (_columns >= 4 
                                            ? _labelSmallWidth : _labelLargeWidth)
                                + _inputWidth + _gap * 2;

        public float CurrentHeight => (_currentRow + 1) * _rowHeight;

        public GfxPanelBuilder(LingoGfxPanel panel, ILingoFrameworkFactory factory)
        {
            _panel = panel;
            _factory = factory;
        }

        public GfxPanelBuilder Columns(int cols)
        {
            _columns = Math.Max(1, cols);
            _currentColumn = 0;
            return this;
        }

        private float ColumnX(int col) => _margin + col * ColumnWidth;
        //private float ComputeInputWidth(int span) => _inputWidth * span + _gap * 2 * (span - 1);
        private float ComputeInputWidth(int span, bool hasLabel, bool stretch)
        {
            if (stretch)
                return MathF.Floor(ColumnWidth * span - _gap);
            else
                return MathF.Min(ColumnWidth - _gap, 26f); // or a fixed width for tight numeric fields
        }
        private float ComputeInputWidth(int span) => ComputeInputWidth(span, true, true); // default stretch=true



        private (float labelX, float inputX, float y) Layout(int labelSpan, int inputSpan)
        {
            float y = _currentRow * _rowHeight;
            float baseX = ColumnX(_currentColumn);
            float labelX = baseX;
            float inputX = baseX + ColumnWidth * labelSpan;
            return (labelX, inputX, y);
        }




        private void Advance(int span)
        {
            _currentColumn += span;
            while (_currentColumn >= _columns)
            {
                _currentColumn = 0;
                _currentRow++;
            }
        }


        public GfxPanelBuilder AddLabel(string name, string text, int labelSpan = 1)
        {
            var (xLabel, xInput, y) = Layout(labelSpan, 0);
            _panel.SetLabelAt(_factory, name, xLabel, y, text);
            Advance(labelSpan);
            return this;
        }
         public GfxPanelBuilder AddTextInput<T>(string name, string label, T target, Expression<Func<T, string?>> property, int inputSpan = 1, int labelSpan = 1)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();
            var (xLabel, xInput, y) = Layout(labelSpan, inputSpan);
            _panel.SetLabelAt(_factory, name + "Label", xLabel, y+ _labelYOffset, label);
            var input = _panel.SetInputTextAt(_factory, target, name + "Input", xInput, y, (int)ComputeInputWidth(inputSpan, true, stretch: true), property);
            input.Text = getter(target) ?? string.Empty;
            Advance(labelSpan + inputSpan);
            return this;
        }

        public GfxPanelBuilder AddNumericInput<T>(string name, string label, T target, Expression<Func<T, float>> property,int inputSpan = 1, bool showLabel = true, bool stretch = false, int labelSpan = 1)
        {
            var (xLabel, xInput, y) = Layout(showLabel?labelSpan:0, inputSpan);
            if (showLabel)
                _panel.SetLabelAt(_factory, name + "Label", xLabel, y+ _labelYOffset, label);
            _panel.SetInputNumberAt(_factory, target, name + "Input", xInput, y,
                (int)ComputeInputWidth(inputSpan, showLabel, stretch), property);
            Advance((showLabel ? labelSpan : 0) + inputSpan);
            return this;
        } 
        public GfxPanelBuilder AddCheckBox<T>(string name, string label, T target, Expression<Func<T, bool>> property,int inputSpan = 1, bool showLabel = true, int labelSpan = 1)
        {
            var (xLabel, xInput, y) = Layout(showLabel?labelSpan:0, inputSpan);
            if (showLabel)
                _panel.SetLabelAt(_factory, name + "Label", xLabel, y+ _labelYOffset, label);
            _panel.SetCheckboxAt(_factory, target, name + "Label", xInput, y, property);
            Advance((showLabel ? labelSpan : 0) + inputSpan);
            return this;
        }

        public GfxPanelBuilder AddColorInput<T>(string name, string label, T target,Expression<Func<T, LingoColor>> property,int inputSpan = 1, int labelSpan = 1)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();
            var (xLabel, xInput, y) = Layout(labelSpan, inputSpan);
            _panel.SetLabelAt(_factory, name + "Label", xLabel, y+ _labelYOffset, label);
            var picker = _factory.CreateColorPicker(name + "Picker", color => setter(target, color));
            picker.Color = getter(target);
            //picker.Width = ComputeInputWidth(inputSpan);
            _panel.AddItem(picker, xInput, y);
            Advance(labelSpan + inputSpan);
            return this;
        }

        public GfxPanelBuilder AddEnumInput<T, TEnum>(string name, string label, T target,Expression<Func<T, int>> property, int inputSpan = 1, bool showLabel = true,int labelSpan = 1) 
            where TEnum : Enum
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();
            var (xLabel, xInput, y) = Layout(showLabel ? labelSpan : 0, inputSpan);
            if (showLabel)
                _panel.SetLabelAt(_factory, name + "Label", xLabel, y + _labelYOffset, label);
            var combo = _factory.CreateInputCombobox(name + "Combo", val =>
            {
                if (int.TryParse(val, out int v))
                    setter(target, v);
            });
            foreach (var value in Enum.GetValues(typeof(TEnum)))
            {
                int key = Convert.ToInt32(value);
                combo.AddItem(key.ToString(), value!.ToString());
            }
            combo.SelectedKey = getter(target).ToString();
            combo.Width = (int)ComputeInputWidth(inputSpan, showLabel, stretch: true);
            _panel.AddItem(combo, xInput, y);
            Advance((showLabel ? labelSpan : 0) + inputSpan);
            return this;
        }
        public GfxPanelBuilder Finalize()
        {
            _panel.Height = CurrentHeight- _rowHeight;
            return this;
        }
    }
}
