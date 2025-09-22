using AbstUI.Components.Buttons;
using AbstUI.Components.Inputs;
using AbstUI.Components.Texts;
using AbstUI.Primitives;
using AbstUI.Tools;
using System.Linq.Expressions;

namespace AbstUI.Components.Containers
{
    public class GfxWrapPanelBuilderForToolBar 
    {
        private int _numberOfVLines = 0;
        private int _height;
        private readonly GfxWrapPanelBuilder _builder;

        public GfxWrapPanelBuilderForToolBar(AbstWrapPanel panel, GfxWrapPanelBuilder? parent, int height = 20) 
        {
            _height = height;
            _builder = new GfxWrapPanelBuilder(panel, parent);
        }

        public GfxWrapPanelBuilderForToolBar AddVLine()
        {
            _numberOfVLines++;
            var name = "vline" + _numberOfVLines;
            _builder.AddVLine(name, _height-4, 2);
            return this;
        }
        public GfxWrapPanelBuilderForToolBar AddButton(string name, string text, Action click, Action<AbstButton>? configure = null)
        {
            _builder.AddButton(name, text, click, configure);
            return this;
        }
        public GfxWrapPanelBuilderForToolBar AddStateButton<T>(string name, T target, IAbstTexture2D texture, Expression<Func<T, bool>> property, Action<AbstStateButton>? configure = null)
        {
            _builder.AddStateButton(name, target, texture,property, "", configure);
            return this;
        } 
        public GfxWrapPanelBuilderForToolBar AddStateButton<T>(string name, T target,string text, Expression<Func<T, bool>> property, Action<AbstStateButton>? configure = null)
        {
            _builder.AddStateButton(name, target, null,property, text, configure);
            return this;
        } 
        
       // TODO : add other controls.
    }
    public class GfxWrapPanelBuilder
    {
        private AbstWrapPanel _panel;
        private AbstWrapPanel _rootPanel;
        private IAbstComponentFactory _factory;
        private readonly GfxWrapPanelBuilder? _parent;

        public GfxWrapPanelBuilder(AbstWrapPanel panel, GfxWrapPanelBuilder? parent)
        {
            _rootPanel = panel;
            _panel = panel;
            _factory = panel.Factory;
            _parent = parent;
        }
        public AbstWrapPanel Finalize()
        {
            return _parent != null? _parent.Finalize() : _rootPanel;
        }

        public GfxWrapPanelBuilder NewLine(string name)
        {
            if(_parent != null)
                return _parent.NewLine(name);
            _panel = _factory.CreateWrapPanel(_rootPanel.Orientation == AOrientation.Vertical? AOrientation.Horizontal : AOrientation.Vertical, name);
            _panel.Width = _rootPanel.Width;
            _rootPanel.AddItem(_panel);
            return _panel.Compose(this);
        }
        public GfxWrapPanelBuilder Configure(Action<AbstWrapPanel> configure)
        {
            configure(_panel);
            return this;
        }


        public GfxWrapPanelBuilder AddLabel(string name, string text, int fontSize = 11, int? width = null, Action<AbstLabel>? configure = null)
        {
            AbstLabel label = _factory.CreateLabel(name, text);
            label.FontSize = fontSize;
            if (GfxPanelExtensions.DefaultTextColor.HasValue)
                label.FontColor = GfxPanelExtensions.DefaultTextColor.Value;
            if (width.HasValue)
                label.Width = width.Value;
            _panel.AddItem(label);
            if (configure != null)
                configure(label);
            return this;
        }

        public GfxWrapPanelBuilder AddTextInput<T>(string name, T target, Expression<Func<T, string?>> property, int width = 100, Action<AbstInputText>? configure = null)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();

            var input = _factory.CreateInputText(name);
            input.OnCommit += () => setter(target, input.Text);
            input.Text = getter(target) ?? string.Empty;
            input.Width = width;
            _panel.AddItem(input);
            if (configure != null)
                configure(input);
            return this;
        }

        public GfxWrapPanelBuilder AddNumericInputFloat<T>(string name, T target, Expression<Func<T, float>> property, int width = 40, float? min = null, float? max = null)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();

            var input = _factory.CreateInputNumberFloat(name, min, max);
            input.OnCommit += () => setter(target, input.Value);
            input.Value = getter(target);
            input.Width = width;
            _panel.AddItem(input);
            return this;
        }
        public GfxWrapPanelBuilder AddNumericInputInt<T>(string name, T target, Expression<Func<T, int>> property, int width = 40, int? min = null, int? max = null)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();

            var input = _factory.CreateInputNumberInt(name, min, max);
            input.OnCommit += () => setter(target, input.Value);
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

        public GfxWrapPanelBuilder AddColorPicker<T>(string name, T target, Expression<Func<T, AColor>> property, int width = 20)
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
        public GfxWrapPanelBuilder AddCombobox(string name, IEnumerable<KeyValuePair<string,string>> items, int width = 100, string? selectedKey = null, Action<string?>? onChange = null)
        {
            var list = _factory.CreateInputCombobox(name, onChange);
            foreach (var item in items)
                list.AddItem(item.Key, item.Value);
            if (selectedKey != null)
                list.SelectedKey = selectedKey;
            list.Width = width;
            _panel.AddItem(list);
            return this;
        }

        public GfxWrapPanelBuilder AddStateButton<T>(string name, T target, IAbstTexture2D? texture, Expression<Func<T, bool>> property, string text = "", Action<AbstStateButton>? configure = null)
        {
            var setter = property.CompileSetter();
            var getter = property.CompileGetter();
            AbstStateButton button = _factory.CreateStateButton(name, texture, text, value => setter(target, value));
            button.IsOn = getter(target);
            _panel.AddItem(button);
            configure?.Invoke(button);
            return this;
        } 
        public GfxWrapPanelBuilder AddButton(string name, string text, Action click, Action<AbstButton>? configure = null)
        {
            var button = _factory.CreateButton(name, text);
            _panel.AddItem(button);
            button.Pressed += () => click();
            configure?.Invoke(button);
            return this;
        }

        public GfxWrapPanelBuilder AddSliderFloat(string name, AOrientation orientation, Action<float>? onChange = null, float? min = null, float? max = null, float? step = null, Action<AbstInputSlider<float>>? configure = null)
        {
            var slider = _factory.CreateInputSliderFloat(orientation, name, min, max, step, onChange);
            _panel.AddItem(slider);
            configure?.Invoke(slider);
            return this;
        }

        public GfxWrapPanelBuilder AddSliderInt(string name, AOrientation orientation, Action<int>? onChange = null, int? min = null, int? max = null, int? step = null, Action<AbstInputSlider<int>>? configure = null)
        {
            var slider = _factory.CreateInputSliderInt(orientation, name, min, max, step, onChange);
            _panel.AddItem(slider);
            configure?.Invoke(slider);
            return this;
        }
        public GfxWrapPanelBuilder AddVLine(string name, float height = 0, float paddingTop = 0)
        {
            _panel.AddVLine(name, height, paddingTop);
            return this;
        }
        public GfxWrapPanelBuilder AddHLine(string name, float width = 0, float paddingLeft = 0)
        {
            _panel.AddHLine(name, width, paddingLeft);
            return this;
        } public GfxWrapPanelBuilder AddItem(IAbstNode element)
        {
            _panel.AddItem(element);
            return this;
        }



    }
}
