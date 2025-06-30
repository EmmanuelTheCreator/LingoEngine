using Godot;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Pictures;
using LingoEngine.Director.Core.Pictures.Commands;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Director.LGodot.Icons;
using LingoEngine.LGodot.Bitmaps;
using LingoEngine.LGodot.Primitives;
using System;

namespace LingoEngine.Director.LGodot.Pictures
{
    public partial class PaintToolbar : Panel
    {
        private readonly IDirectorIconManager _iconManager;
        private readonly ILingoCommandManager _commandManager;
        private DirectorToolButton? _selectedButton = null;
        private HFlowContainer _container;

        public event Action<PainterToolType>? ToolSelected;
        public PainterToolType SelectedTool { get; private set; } = PainterToolType.Pencil;
        public Color SelectedColor { get; private set; } = Colors.Black;
        public Color BGColor { get; set; } = DirectorColors.BG_WhiteMenus.ToGodotColor();

        public PaintToolbar(IDirectorIconManager iconManager, ILingoCommandManager commandManager)
        {
            _iconManager = iconManager;
            _commandManager = commandManager;

            SizeFlagsVertical = SizeFlags.ExpandFill;
            CustomMinimumSize = new Vector2(52, 200);

            _container = new HFlowContainer();
            _container.CustomMinimumSize = new Vector2(48, 100);
            _container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _container.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            _container.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
            _container.SizeFlagsVertical = SizeFlags.ShrinkBegin;
            _container.AddThemeConstantOverride("margin_left", 2);
            _container.AddThemeConstantOverride("margin_top", 2);
            AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = BGColor }); 
            AddChild(_container);
            //Separation = 2;
        }

        public override void _Ready()
        {
            AddToolButton(DirectorIcon.Pencil);
            AddToolButton(DirectorIcon.PaintBrush);
            AddToolButton(DirectorIcon.Eraser);
            AddToolButton(DirectorIcon.PaintLasso);
            AddToolButton(DirectorIcon.RectangleSelect);
            AddToolButton(DirectorIcon.PaintBucket);
            AddColorPickerForegound();
            AddColorPickerBackgound();

            ToolSelected?.Invoke(SelectedTool);
        }

        private void AddToolButton(DirectorIcon icon)
        {
            var btn = new DirectorToolButton();
            btn.Icon = ((LingoGodotImageTexture) _iconManager.Get(icon)).Texture;

            btn.Pressed = () =>
            {
                SelectButton(btn);
                var tool = icon switch
                {
                    DirectorIcon.Pencil => PainterToolType.Pencil,
                    DirectorIcon.PaintBrush => PainterToolType.PaintBrush,
                    DirectorIcon.Eraser => PainterToolType.Eraser,
                    DirectorIcon.PaintLasso => PainterToolType.SelectLasso,
                    DirectorIcon.RectangleSelect => PainterToolType.SelectRectangle,
                    DirectorIcon.ColorPicker => PainterToolType.ColorPicker,
                    DirectorIcon.PaintBucket => PainterToolType.Fill,
                    _ => throw new ArgumentOutOfRangeException(nameof(icon), icon.ToString())
                };

                SelectedTool = tool;
                _commandManager.Handle(new PainterToolSelectCommand(tool));
                ToolSelected?.Invoke(tool);
            };

            _container.AddChild(btn);
        }
        public void SelectTool(PainterToolType tool)
        {
            SelectedTool = tool;
            ToolSelected?.Invoke(tool);
        }
        private void SelectButton(DirectorToolButton btn)
        {
            if (_selectedButton != null)
                _selectedButton.IsSelected = false;

            btn.IsSelected = true;
            _selectedButton = btn;
        }

        private void AddColorPickerForegound() =>
            AddColorPicker(color => new PainterChangeForegroundColorCommand(color.ToLingoColor()));

        private void AddColorPickerBackgound() =>
            AddColorPicker(color => new PainterChangeBackgroundColorCommand(color.ToLingoColor()));

        private void AddColorPicker(Func<Color, ILingoCommand> toCommand)
        {
            var colorPicker = new ColorPickerButton
            {
                CustomMinimumSize = new Vector2(20, 20),
                SizeFlagsHorizontal = 0,
                SizeFlagsVertical = 0,
                Color = SelectedColor,
            };

            colorPicker.ColorChanged += color =>
            {
                SelectedColor = color;
                _commandManager.Handle(toCommand(color));
            };

            _container.AddChild(colorPicker);
        }


    }
}
