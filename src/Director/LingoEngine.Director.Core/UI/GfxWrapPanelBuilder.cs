using LingoEngine.Bitmaps;
using LingoEngine.Director.Core.Styles;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Tools;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LingoEngine.Director.Core.UI
{
    public class GfxWrapPanelBuilder
    {
        private LingoGfxWrapPanel _panel;
        private ILingoFrameworkFactory _factory;

        public GfxWrapPanelBuilder(LingoGfxWrapPanel panel, ILingoFrameworkFactory factory)
        {
            _panel = panel;
            _factory = factory;
        }
        public GfxWrapPanelBuilder AddLabel(string text, int fontSize = 11)
        {
            var label = _factory.CreateLabel("Label_" + Guid.NewGuid(), text);
            label.FontSize = fontSize;
            label.FontColor = DirectorColors.TextColorLabels;
            _panel.AddItem(label);
            return this;
        }

        public GfxWrapPanelBuilder AddTextInput<T>(string name, T target, Expression<Func<T, string?>> property, int width = 100)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();

            var input = _factory.CreateInputText(name, 0, value => setter(target, value));
            input.Text = getter(target) ?? string.Empty;
            input.Width = width;
            _panel.AddItem(input);
            return this;
        }

        public GfxWrapPanelBuilder AddNumericInput<T>(string name, T target, Expression<Func<T, float>> property, int width = 40, float min = 0, float max = 100)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();

            var input = _factory.CreateInputNumber(name, min, max, value => setter(target, value));
            input.Value = getter(target);
            input.Width = width;
            _panel.AddItem(input);
            return this;
        }

        public GfxWrapPanelBuilder AddCheckbox<T>(string name, T target, Expression<Func<T, bool>> property)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();

            var checkbox = _factory.CreateInputCheckbox(name, value => setter(target, value));
            checkbox.Checked = getter(target);
            _panel.AddItem(checkbox);
            return this;
        }

        public GfxWrapPanelBuilder AddColorPicker<T>(string name, T target, Expression<Func<T, LingoColor>> property, int width = 20)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();

            var colorPicker = _factory.CreateColorPicker(name, value => setter(target, value));
            colorPicker.Color = getter(target);
            colorPicker.Width = width;
            _panel.AddItem(colorPicker);
            return this;
        }

        public GfxWrapPanelBuilder AddItemList(string name, IEnumerable<KeyValuePair<string,string>> items, int width = 100, string? selectedKey = null, Action<string?>? onChange = null)
        {
            var list = _factory.CreateItemList(name, onChange);
            foreach (var item in items)
                list.AddItem(item.Key, item.Value);
            if (selectedKey != null)
                list.SelectedKey = selectedKey;
            list.Width = width;
            _panel.AddItem(list);
            return this;
        }

        public GfxWrapPanelBuilder AddStateButton<T>(string name, T target, ILingoImageTexture texture, Expression<Func<T, bool>> property, string text = "", Action<LingoGfxStateButton>? configure = null)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();
            LingoGfxStateButton button = _factory.CreateStateButton(name, texture, text, value => setter(target, value));
            button.IsOn = getter(target);
            _panel.AddItem(button);
            configure?.Invoke(button);
            return this;
        }
    }
}
