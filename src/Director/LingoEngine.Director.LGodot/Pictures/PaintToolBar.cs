using Godot;
using LingoEngine.Commands;
using LingoEngine.Core;
using LingoEngine.Director.Core.Commands;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.LGodot.Primitives;
using System;

namespace LingoEngine.Director.LGodot.Pictures
{
    public partial class PaintToolbar : Panel
    {
        private readonly IDirGodotIconManager _iconManager;
        private readonly ILingoCommandManager _commandManager;
        private PaintToolButton? _selectedButton = null;
        private HFlowContainer _container;
        public PainterToolType SelectedTool { get; private set; } = PainterToolType.Pencil;
        public Color SelectedColor { get; private set; } = Colors.Black;
        public Color BGColor { get; set; } = new Color(200, 200, 200);

        public PaintToolbar(IDirGodotIconManager iconManager, ILingoCommandManager commandManager)
        {
            _iconManager = iconManager;
            _commandManager = commandManager;

            SizeFlagsVertical = SizeFlags.ExpandFill;
            CustomMinimumSize = new Vector2(50, 200);

            _container = new HFlowContainer();
            _container.CustomMinimumSize = new Vector2(48, 100);
            _container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _container.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            _container.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
            _container.SizeFlagsVertical = SizeFlags.ShrinkBegin;
            _container.AddThemeConstantOverride("margin_left", 2);
            _container.AddThemeConstantOverride("margin_top", 2);
            _container.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = BGColor }); 
            AddChild(_container);
            //Separation = 2;
        }

        public override void _Ready()
        {
            AddToolButton(DirGodotEditorIcon.Pencil);
            AddToolButton(DirGodotEditorIcon.PaintBrush);
            AddToolButton(DirGodotEditorIcon.Eraser);
            AddToolButton(DirGodotEditorIcon.PaintBucket);
            AddColorPickerForegound();
            AddColorPickerBackgound();
        }

        private void AddToolButton(DirGodotEditorIcon icon)
        {
            var btn = new PaintToolButton();
            btn.Icon = _iconManager.Get(icon);

            btn.Pressed = () =>
            {
                SelectButton(btn);
                var tool = icon switch
                {
                    DirGodotEditorIcon.Pencil => PainterToolType.Pencil,
                    DirGodotEditorIcon.PaintBrush => PainterToolType.PaintBrush,
                    DirGodotEditorIcon.Eraser => PainterToolType.Eraser,
                    DirGodotEditorIcon.ColorPicker => PainterToolType.ColorPicker,
                    DirGodotEditorIcon.PaintBucket => PainterToolType.Fill,
                    _ => throw new ArgumentOutOfRangeException(nameof(icon), icon.ToString())
                };

                SelectedTool = tool;
                _commandManager.Handle(new PainterToolSelectCommand(tool));
            };

            _container.AddChild(btn);
        }
        private void SelectButton(PaintToolButton btn)
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
