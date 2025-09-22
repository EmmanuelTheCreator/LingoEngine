using AbstUI.Components;
using AbstUI.Components.Buttons;
using AbstUI.Components.Containers;
using AbstUI.Components.Graphics;
using AbstUI.Components.Inputs;
using AbstUI.Components.Menus;
using AbstUI.Components.Texts;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Buttons;
using AbstUI.SDL2.Components.Containers;
using AbstUI.SDL2.Components.Graphics;
using AbstUI.SDL2.Components.Inputs;
using AbstUI.SDL2.Components.Menus;
using AbstUI.SDL2.Components.Texts;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Styles;
using AbstUI.SDL2.Windowing;
using AbstUI.Styles;
using AbstUI.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.SDL2.Components
{
    /// <summary>
    /// Factory responsible for creating SDL specific GFX components.
    /// </summary>
    public class AbstSdlComponentFactory : AbstComponentFactoryBase, IAbstComponentFactory
    {
        private readonly ISdlRootComponentContext _rootContext;
        private readonly Lazy<IAbstSdlWindowManager> _windowManager;
        public readonly SdlFontManager FontManagerTyped;

        public ISdlRootComponentContext RootContext => _rootContext;
        public SdlFocusManager FocusManager => _rootContext.FocusManager;
        internal IAbstSdlWindowManager WindowManager => _windowManager.Value;

        public AbstSdlComponentFactory(ISdlRootComponentContext rootContext, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _rootContext = rootContext;
            _windowManager = new Lazy<IAbstSdlWindowManager>(() => (IAbstSdlWindowManager)serviceProvider.GetRequiredService<IAbstFrameworkWindowManager>());
            FontManagerTyped = (SdlFontManager)FontManager;
            AbstDefaultStyles.RegisterInputStyles(StyleManager);
        }

        public AbstSDLComponentContext CreateContext(IAbstSDLComponent component, AbstSDLComponentContext? parent = null)
            => new(_rootContext.ComponentContainer, component, parent) { Renderer = _rootContext.Renderer };

        public AbstSDLRenderContext CreateRenderContext(AbstSDLRenderContext? parent, System.Numerics.Vector2 origin)
            => new(_rootContext.Renderer, origin, FontManagerTyped, parent);

        public IAbstImagePainter CreateImagePainter(int width = 0, int height = 0)
            => new SDLImagePainter(FontManager, width, height, RootContext.Renderer);
        public IAbstImagePainter CreateImagePainterToTexture(int width = 0, int height = 0)
            => new SDLImagePainter(FontManager, width, height, RootContext.Renderer);

        public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height)
        {
            var canvas = new AbstGfxCanvas();
            var impl = new AbstSdlGfxCanvas(this, FontManagerTyped, width, height);
            canvas.Init(impl);
            InitComponent(canvas);
            canvas.Width = width;
            canvas.Height = height;
            canvas.Name = name;
            return canvas;
        }

        public AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name)
        {
            var panel = new AbstWrapPanel(this);
            var impl = new AbstSdlWrapPanel(this, orientation);
            panel.Init(impl);
            InitComponent(panel);
            panel.Name = name;
            panel.Orientation = orientation;
            return panel;
        }

        public AbstPanel CreatePanel(string name)
        {
            var panel = new AbstPanel(this);
            var impl = new AbstSdlPanel(this);
            panel.Init(impl);
            InitComponent(panel);
            panel.Name = name;
            return panel;
        }

        public AbstZoomBox CreateZoomBox(string name)
        {
            var box = new AbstZoomBox();
            var impl = new AbstSdlZoomBox(this);
            box.Init(impl);
            InitComponent(box);
            box.Name = name;
            return box;
        }

        public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y)
        {
            if (content is IAbstLayoutNode)
                throw new InvalidOperationException($"Content {content.Name} already supports layout — wrapping is unnecessary.");
            var panel = new AbstLayoutWrapper(content);
            var impl = new AbstSdlLayoutWrapper(this, panel);
            InitComponent(panel);
            if (x != null) panel.X = x.Value;
            if (y != null) panel.Y = y.Value;
            return panel;
        }

        public AbstTabContainer CreateTabContainer(string name)
        {
            var tab = new AbstTabContainer();
            var impl = new AbstSdlTabContainer(this);
            tab.Init(impl);
            InitComponent(tab);
            tab.Name = name;
            return tab;
        }

        public AbstTabItem CreateTabItem(string name, string title)
        {
            var tab = new AbstTabItem();
            var impl = new AbstSdlTabItem(this, tab);
            tab.Title = title;
            tab.Name = name;
            InitComponent(tab);
            return tab;
        }

        public AbstScrollContainer CreateScrollContainer(string name)
        {
            var scroll = new AbstScrollContainer();
            var impl = new AbstSdlScrollContainer(this);
            scroll.Init(impl);
            InitComponent(scroll);
            scroll.Name = name;
            return scroll;
        }

        public AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null)
        {
            var slider = new AbstInputSlider<float>();
            var impl = new AbstSdlInputSlider<float>(this);
            slider.Init(impl);
            InitComponent(slider);
            slider.Name = name;
            if (min.HasValue) slider.MinValue = min.Value;
            if (max.HasValue) slider.MaxValue = max.Value;
            if (step.HasValue) slider.Step = step.Value;
            if (onChange != null)
                slider.ValueChanged += () => onChange(slider.Value);
            return slider;
        }

        public AbstInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null)
        {
            var slider = new AbstInputSlider<int>();
            var impl = new AbstSdlInputSlider<int>(this);
            slider.Init(impl);
            InitComponent(slider);
            slider.Name = name;
            if (min.HasValue) slider.MinValue = min.Value;
            if (max.HasValue) slider.MaxValue = max.Value;
            if (step.HasValue) slider.Step = step.Value;
            if (onChange != null)
                slider.ValueChanged += () => onChange(slider.Value);
            return slider;
        }

        public AbstInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null, bool multiLine = false)
        {
            var input = new AbstInputText();
            var impl = new AbstSdlInputText(this, multiLine);
            input.Init(impl);
            InitComponent(input);
            input.Name = name;
            input.MaxLength = maxLength;
            if (onChange != null)
                input.ValueChanged += () => onChange(input.Text);
            return input;
        }

        public AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        {
            var minNullableNum = min.HasValue ? new NullableNum<float>(min.Value) : new NullableNum<float>();
            var maxNullableNum = max.HasValue ? new NullableNum<float>(max.Value) : new NullableNum<float>();
            return CreateInputNumber(name, minNullableNum, maxNullableNum, onChange);
        }

        public AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null)
        {
            var minNullableNum = min.HasValue ? new NullableNum<int>(min.Value) : new NullableNum<int>();
            var maxNullableNum = max.HasValue ? new NullableNum<int>(max.Value) : new NullableNum<int>();
            return CreateInputNumber(name, minNullableNum, maxNullableNum, onChange);
        }

        public AbstInputNumber<TValue> CreateInputNumber<TValue>(string name, NullableNum<TValue> min, NullableNum<TValue> max, Action<TValue>? onChange = null)
            where TValue : System.Numerics.INumber<TValue>
        {
            var input = new AbstInputNumber<TValue>();
            var impl = new AbstSdlInputNumber<TValue>(this);
            input.Init(impl);
            InitComponent(input);
            input.Name = name;
            if (min.HasValue) input.Min = min.Value;
            if (max.HasValue) input.Max = max.Value;
            return input;
        }

        public AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null)
        {
            var spin = new AbstInputSpinBox();
            var impl = new AbstSdlSpinBox(this);
            spin.Init(impl);
            InitComponent(spin);
            spin.Name = name;
            if (min.HasValue) spin.Min = min.Value;
            if (max.HasValue) spin.Max = max.Value;
            if (onChange != null)
                spin.ValueChanged += () => onChange(spin.Value);
            return spin;
        }

        public AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null)
        {
            var input = new AbstInputCheckbox();
            var impl = new AbstSdlInputCheckbox(this);
            input.Init(impl);
            InitComponent(input);
            input.Name = name;
            input.ValueChanged += () => onChange?.Invoke(input.Checked);
            return input;
        }

        public AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null)
        {
            var input = new AbstInputCombobox();
            var impl = new AbstSdlInputCombobox(this);
            input.Init(impl);
            InitComponent(input);
            input.Name = name;
            input.ValueChanged += () => onChange?.Invoke(input.SelectedKey);
            return input;
        }

        public AbstItemList CreateItemList(string name, Action<string?>? onChange = null)
        {
            var list = new AbstItemList();
            var impl = new AbstSdlInputItemList(this);
            list.Init(impl);
            InitComponent(list);
            list.Name = name;
            if (onChange != null)
                list.ValueChanged += () => onChange(list.SelectedKey);
            return list;
        }

        public AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null)
        {
            var picker = new AbstColorPicker();
            var impl = new AbstSdlColorPicker(this);
            picker.Init(impl);
            InitComponent(picker);
            picker.Name = name;
            if (onChange != null)
                picker.ValueChanged += () => onChange(picker.Color);
            return picker;
        }

        public AbstLabel CreateLabel(string name, string text = "")
        {
            var label = new AbstLabel();
            var impl = new AbstSdlLabel(this);
            label.Init(impl);
            InitComponent(label);
            label.Name = name;
            label.Text = text;
            return label;
        }

        public AbstButton CreateButton(string name, string text = "")
        {
            var button = new AbstButton();
            var impl = new AbstSdlButton(this);
            button.Init(impl);
            InitComponent(button);
            button.Name = name;
            button.Text = text;
            return button;
        }

        public AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null)
        {
            var button = new AbstStateButton();
            var impl = new AbstSdlStateButton(this);
            button.Init(impl);
            InitComponent(button);
            if (onChange != null)
                button.ValueChanged += () => onChange(button.IsOn);

            button.Name = name;
            button.Text = text;
            if (texture != null)
                button.TextureOn = texture;
            return button;
        }

        public AbstMenu CreateMenu(string name)
        {
            var menu = new AbstMenu();
            var impl = new AbstSdlMenu(this, name);
            menu.Init(impl);
            InitComponent(menu);
            return menu;
        }


        //public AbstWindow CreateWindow(string name, string title = "")
        //{

        //    var win = new AbstWindow();
        //    var impl = new AbstSdlWindow(win, this);
        //    win.Name = name;
        //    win.Title = title;
        //    InitComponent(win);
        //    return win;
        //}


        public AbstMenuItem CreateMenuItem(string name, string? shortcut = null)
        {
            var item = new AbstMenuItem();
            var impl = new AbstSdlMenuItem(this, name, shortcut);
            item.Init(impl);
            return item;
        }

        public AbstMenu CreateContextMenu(object window)
        {
            var menu = CreateMenu("ContextMenu");
            return menu;
        }

        public AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name)
        {
            var sep = new AbstHorizontalLineSeparator();
            var impl = new AbstSDLHorizontalLineSeparator(this ,sep);
            InitComponent(sep);
            sep.Name = name;
            return sep;
        }

        public AbstVerticalLineSeparator CreateVerticalLineSeparator(string name)
        {
            var sep = new AbstVerticalLineSeparator();
            var impl = new AbstSDLVerticalLineSeparator(this, sep);
            InitComponent(sep);
            sep.Name = name;
            return sep;
        }


    }
}
