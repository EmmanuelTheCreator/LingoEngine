using System.Linq.Expressions;
using AbstUI.Primitives;
using AbstUI.Tools;
using AbstUI.Components;
using AbstUI.Texts;
using AbstUI.Components.Graphics;
using AbstUI.Components.Inputs;
using AbstUI.Components.Buttons;
using AbstUI.Components.Texts;
using AbstUI.Styles;


namespace AbstUI.Components.Containers
{
    public static class GfxPanelExtensions
    {
        // todo : remove this!
        public static AColor? DefaultTextColor = AbstDefaultColors.TextColorLabels;
        public static GfxPanelBuilder Compose(this AbstPanel panel, IAbstComponentFactory factory)
        {
            var builder = new GfxPanelBuilder(panel, factory);
            return builder;
        }

        public static AbstGfxCanvas SetGfxCanvasAt(this AbstPanel container, string name, float x, float y, int width, int height)
        {
            var canvas = container.Factory.CreateGfxCanvas(name, width, height);
            container.AddItem(canvas, x, y);
            return canvas;
        }

        public static AbstLabel SetLabelAt(this AbstPanel container, string name, float x, float y, string? text = null, int fontSize = 11, int? labelWidth = null, AbstTextAlignment blingoTextAlignment = AbstTextAlignment.Left)
        {
            AbstLabel lbl = container.Factory.CreateLabel(name,text ??"");
            if (DefaultTextColor != null)
                lbl.FontColor = DefaultTextColor.Value;
            lbl.FontSize = fontSize;
            if(labelWidth.HasValue)
                lbl.Width = labelWidth.Value;
            lbl.TextAlignment = blingoTextAlignment;
            container.AddItem(lbl, x, y);
            return lbl;
        }
        public static AbstInputText SetInputTextAt<T>(this AbstPanel container, T element, string name, float x, float y, int width, Expression<Func<T,string?>> property, int maxLength = 0)
        {
            Action<T, string?> setter = property.CompileSetter();
            var control = container.Factory.CreateInputText(name, maxLength);
            control.OnCommit += () => setter(element, control.Text);
            control.Text = property.CompileGetter()(element)?.ToString() ?? string.Empty;
            control.Width = width;
            container.AddItem(control, x, y);
            return control;
        }
        public static AbstInputNumber<float> SetInputNumberAt<T>(this AbstPanel container, T element, string name, float x, float y, int width, Expression<Func<T,float>> property, float? min = null, float? max = null)
        {
            Action<T, float> setter = property.CompileSetter();
            var control = container.Factory.CreateInputNumberFloat(name, min, max);
            control.OnCommit += () => setter(element, control.Value);
            control.Value = property.CompileGetter()(element);
            control.Width = width;
            container.AddItem(control, x, y);
            return control;
        }
        public static AbstInputNumber<int> SetInputNumberAt<T>(this AbstPanel container, T element, string name, float x, float y, int width, Expression<Func<T,int>> property, int? min = null, int? max = null)
        {
            Action<T, int> setter = property.CompileSetter();
            var control = container.Factory.CreateInputNumberInt(name, min, max);
            control.OnCommit += () => setter(element, control.Value);
            control.Value = property.CompileGetter()(element);
            control.Width = width;
            container.AddItem(control, x, y);
            return control;
        }
        public static AbstInputCheckbox SetCheckboxAt<T>(this AbstPanel container, T element, string name, float x, float y, Expression<Func<T,bool>> property)
        {
            Action<T, bool> setter = property.CompileSetter();
            AbstInputCheckbox control = container.Factory.CreateInputCheckbox(name,x => setter(element, x));
            control.Checked = property.CompileGetter()(element);
            container.AddItem(control, x, y);
            return control;
        } 
        public static AbstInputCombobox SetComboBoxAt(this AbstPanel container, IEnumerable<KeyValuePair<string, string>> items, string name, float x, float y, int width = 100, string? selectedKey = null, Action<string?>? onChange = null)
        {
            var list = container.Factory.CreateInputCombobox(name, onChange);
            foreach (var item in items)
                list.AddItem(item.Key, item.Value);
            if (selectedKey != null)
                list.SelectedKey = selectedKey;
            list.Width = width;
            container.AddItem(list, x, y);
            return list;
        }
        public static AbstItemList SetInputListAt(this AbstPanel container, IEnumerable<KeyValuePair<string, string>> items, string name, float x, float y, int width = 100, string? selectedKey = null, Action<string?>? onChange = null)
        {
            var list = container.Factory.CreateItemList(name, onChange);
            foreach (var item in items)
                list.AddItem(item.Key, item.Value);
            if (selectedKey != null)
                list.SelectedKey = selectedKey;
            list.Width = width;
            container.AddItem(list, x, y);
            return list;
        }

        public static (AbstButton Button, IAbstLayoutNode Layout) SetButtonAt(this AbstPanel container, string name, string text, float x, float y, Action onClick, int width = 80)
        {
            var control = container.Factory.CreateButton(name, text);
            control.Width = width;
            control.Pressed += onClick;
            IAbstLayoutNode layout = (IAbstLayoutNode)container.AddItem(control, x, y);
            return (control, layout);
        }
        public static AbstInputSlider<float> SetSliderAt<T>(this AbstPanel container, T element, string name, float x, float y, int width, AOrientation orientation, Expression<Func<T, float>> property, float? min = null, float? max = null, float? step = null)
        {
            Action<T, float> setter = property.CompileSetter();
            var slider = container.Factory.CreateInputSliderFloat(orientation, name, min, max, step, v => setter(element, v));
            slider.Value = property.CompileGetter()(element);
            slider.Width = width;
            container.AddItem(slider, x, y);
            return slider;
        }

        public static AbstInputSlider<int> SetSliderAt<T>(this AbstPanel container, T element, string name, float x, float y, int width, AOrientation orientation, Expression<Func<T, int>> property, int? min = null, int? max = null, int? step = null)
        {
            Action<T, int> setter = property.CompileSetter();
            var slider = container.Factory.CreateInputSliderInt(orientation, name, min, max, step, v => setter(element, v));
            slider.Value = property.CompileGetter()(element);
            slider.Width = width;
            container.AddItem(slider, x, y);
            return slider;
        }
        public static AbstStateButton SetStateButtonAt<T>(this AbstPanel container, T element, string name, float x, float y, Expression<Func<T,bool>> property, IAbstTexture2D? texture = null, string? label = null)
        {
            Action<T, bool> setter = property.CompileSetter();
            AbstStateButton control = container.Factory.CreateStateButton(name, texture, label ??"", onChange: val => setter(element, val));
            control.IsOn = property.CompileGetter()(element);
            container.AddItem(control, x, y);
            return control;
        }
        public static AbstGfxCanvas AddVLine(this AbstPanel container, string name, float x, float y, float height)
        {
            var paintPanel = container.Factory.CreateGfxCanvas(name, 2, (int)height);
            paintPanel.DrawLine(new APoint(0, 0), new APoint(0, height), AbstDefaultColors.LineLight, 1);
            paintPanel.DrawLine(new APoint(1, 0), new APoint(1, height), AbstDefaultColors.LineDark, 1);
            container.AddItem(paintPanel, x, y);
            return paintPanel;
        } 
        public static AbstGfxCanvas AddHLine(this AbstPanel container, string name, float x, float y, float width)
        {
            var paintPanel = container.Factory.CreateGfxCanvas(name, (int)width, 2);
            paintPanel.DrawLine(new APoint(0, 0), new APoint(width, 0), AbstDefaultColors.LineLight, 1);
            paintPanel.DrawLine(new APoint(0, 1), new APoint(width, 1), AbstDefaultColors.LineDark, 1);
            container.AddItem(paintPanel, x, y);
            return paintPanel;
        }
        public static void AddPopupButtons(this AbstPanel container, Action okAction, Action onClose)
        {
            container.AddVLine("VLineBtns", container.Width - 100, 10, container.Height - 10 - 10);

            container.SetButtonAt("OKBtn", "OK", container.Width - 90, 10, () => { okAction(); onClose(); },80);
            container.SetButtonAt("CancelBtn", "Cancel", container.Width - 90, 40, onClose,80);
        }

    }
}

