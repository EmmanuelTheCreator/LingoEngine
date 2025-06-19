using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.LGodot.TestData;
using LingoEngine.Director.LGodot.Gfx;
using System.Diagnostics;

namespace LingoEngine.Director.LGodot.Gfx
{
    internal partial class DirGodotBinaryViewerWindow : BaseGodotWindow, IHasMenuItemSelectedEvent
    {
        private readonly IDirectorEventMediator _mediator;
        private readonly LineEdit _pathEdit = new LineEdit();
        //private VBoxContainer _hexRows = new VBoxContainer();
        private VBoxContainer _descTable = new VBoxContainer();
        private readonly ScrollContainer _scroll = new ScrollContainer();
        private readonly Dictionary<int, string> _knownOffsets = new();
        private readonly Dictionary<int, Color> _colors = new();
        private readonly HashSet<int> _styleBlocks = new();
        private XmedFileHints? _currentHints;
        private SubViewport _viewport;
        private TextureRect _textureRect;

        public DirGodotBinaryViewerWindow(IDirectorEventMediator mediator) : base("Binary Viewer")
        {
            _mediator = mediator;
            _mediator.SubscribeToMenu(DirectorMenuCodes.BinaryViewerWindow, () => Visible = !Visible);
            Size = new Vector2(1400, 600);
            CustomMinimumSize = Size;

            //_viewport = new SubViewport();
            //_viewport.SetDisable3D(true);
            //_viewport.TransparentBg = false;
            //_viewport.SetUpdateMode(SubViewport.UpdateMode.Always);
            //_viewport.SetSize(new Vector2I(1400, 4000));

            _viewport = new SubViewport();
            _viewport.SetDisable3D(true);
            _viewport.TransparentBg = false;
            _viewport.SetUpdateMode(SubViewport.UpdateMode.Always);
            AddChild(_viewport);

            _textureRect = new TextureRect
            {
                Texture = _viewport.GetTexture(),
                CustomMinimumSize = new Vector2(1200, 4000) // placeholder
            };
            _scroll.AddChild(_textureRect);


            var root = new VBoxContainer();
            root.Position = new Vector2(0, TitleBarHeight);
            root.Size = new Vector2(Size.X, Size.Y - TitleBarHeight);
            AddChild(root);

            var topBar = new HBoxContainer();
            topBar.AddChild(new Label { Text = "parse binary XMED" });
            topBar.AddChild(_pathEdit);
            var parseBtn = new Button { Text = "Parse" };
            parseBtn.Pressed += () => LoadFromPath(_pathEdit.Text);
            topBar.AddChild(parseBtn);

            var btn1 = new Button { Text = "Hallo" };
            btn1.Pressed += () => LoadBytes(XmedTestData.HalloDefault, XmedTestHints.HalloDefault);
            topBar.AddChild(btn1);
            var btn2 = new Button { Text = "MultiMulti" };
            btn2.Pressed += () => LoadBytes(XmedTestData.MultiStyleMultiLine, XmedTestHints.MultiStyleMultiLine);
            topBar.AddChild(btn2);
            var btn3 = new Button { Text = "MultiSingle" };
            btn3.Pressed += () => LoadBytes(XmedTestData.MultiStyleSingleLine, XmedTestHints.MultiStyleSingleLine);
            topBar.AddChild(btn3);

            root.AddChild(topBar);

            var body = new HBoxContainer();
            
            body.AddChild(_scroll);
            body.AddChild(_descTable);
            root.AddChild(body);
            _scroll.SizeFlagsVertical = Control.SizeFlags.Expand;
            _scroll.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            _scroll.CustomMinimumSize = new Vector2(1200, 400);
            //_hexRows.SizeFlagsVertical = Control.SizeFlags.Expand;
            //_hexRows.CustomMinimumSize = new Vector2(800, 400);
            //_hexRows.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = Colors.Green });

            _textureRect.QueueRedraw();
            _scroll.QueueRedraw();
        }
        public override void _Ready()
        {
            QueueRedraw(); // force _Draw() to run
        }
        private void LoadFromPath(string path)
        {
            if (File.Exists(path))
            {
                LoadBytes(File.ReadAllBytes(path));
            }
        }

        private void LoadBytes(byte[] data, XmedFileHints? hints = null)
        {
            _currentHints = hints;
            _knownOffsets.Clear();
            _colors.Clear();
            _styleBlocks.Clear();
            ClearChildren(_descTable);

            var interp = XmedInterpreter.Interpret(data, hints?.Offsets, hints?.StyleBlocks);
            foreach (var kv in interp.Offsets)
                _knownOffsets[kv.Key] = kv.Value;
            foreach (var b in interp.StyleBlocks)
                _styleBlocks.Add(b);

            // Cleanup previous draw content from viewport
            foreach (var child in _viewport.GetChildren())
                _viewport.RemoveChild((Node)child);

            // Setup new drawing control
            var drawControl = new HexDrawControl(data, _knownOffsets, _colors, _styleBlocks);

            // Determine render size (based on byte count)
            var rowHeight = 24;
            var totalRows = (int)Math.Ceiling(data.Length / 32.0);
            var contentSize = new Vector2(1400, totalRows * rowHeight);

            drawControl.CustomMinimumSize = contentSize;
            drawControl.Size = contentSize;
            drawControl.CallDeferred("queue_redraw");

            _viewport.SetSize(new Vector2I((int)contentSize.X, (int)contentSize.Y));
            _viewport.AddChild(drawControl);

            _textureRect.SetTexture(_viewport.GetTexture());
            _textureRect.SetStretchMode(TextureRect.StretchModeEnum.Scale); // fallback: use "Stretch" or "Keep"
            _textureRect.CustomMinimumSize = contentSize;

            RenderDescriptions();
        }




        private void RenderHex(byte[] data)
        {
            int colorIndex = 0;
            for (int i = 0; i < data.Length; i += 32)
            {
                var row = new HBoxContainer();
                var hexPart = new HBoxContainer();
                var asciiPart = new Label();
                string ascii = string.Empty;
                for (int j = 0; j < 32 && i + j < data.Length; j++)
                {
                    if (j == 8)
                        hexPart.AddChild(new Control { CustomMinimumSize = new Vector2(10,1) });
                    byte b = data[i + j];
                    var v = new VBoxContainer();
                    var lbl = new Label { Text = $"{b:X2}" };
                    lbl.LabelSettings = new LabelSettings { FontSize = 14 };
                    string tag = "";
                    if (_knownOffsets.TryGetValue(i + j, out var desc))
                    {
                        if (!_colors.ContainsKey(i + j))
                        {
                            var hue = (colorIndex * 0.1f) % 1f;
                            _colors[i + j] = Color.FromHsv(hue, 0.3f, 1f);
                            colorIndex++;
                        }
                        var c = _colors[i + j];
                        var style = new StyleBoxFlat { BgColor = c };
                        if (_styleBlocks.Contains(i + j))
                        {
                            style.BorderWidthLeft = 1;
                            style.BorderWidthRight = 1;
                            style.BorderWidthTop = 1;
                            style.BorderWidthBottom = 1;
                            style.BorderColor = Colors.Black;
                        }
                        lbl.AddThemeStyleboxOverride("panel", style);
                        tag = desc.Length > 6 ? desc.Substring(0,6) : desc;
                    }
                    v.AddChild(lbl);
                    var tagLbl = new Label { Text = tag };
                    tagLbl.LabelSettings = new LabelSettings { FontSize = 8 };
                    v.AddChild(tagLbl);
                    hexPart.AddChild(v);
                    ascii += b >= 32 && b <= 126 ? ((char)b).ToString() : ".";
                }
                asciiPart.Text = ascii;
                asciiPart.LabelSettings = new LabelSettings { FontSize = 14 };
                row.AddChild(hexPart);
                row.AddChild(new Control { CustomMinimumSize = new Vector2(20,1) });
                row.AddChild(asciiPart);
                //_hexRows.AddChild(row);
                //Console.WriteLine($"RenderHex: added 1 rows");
            }
            //Console.WriteLine($"_hexRows.ChildCount = {_hexRows.GetChildCount()}");

        }

        private void RenderDescriptions()
        {
            int idx = 0;
            foreach (var kv in _knownOffsets)
            {
                var h = new HBoxContainer();
                var offsetLabel = new Label { Text = $"0x{kv.Key:X4}" };
                var descLabel = new Label { Text = kv.Value };
                if (!_colors.TryGetValue(kv.Key, out var c))
                {
                    c = Color.FromHsv((idx * 0.1f) % 1f, 0.3f, 1f);
                    _colors[kv.Key] = c;
                }
                var style = new StyleBoxFlat { BgColor = c };
                if (_styleBlocks.Contains(kv.Key))
                {
                    style.BorderWidthLeft = 1;
                    style.BorderWidthRight = 1;
                    style.BorderWidthTop = 1;
                    style.BorderWidthBottom = 1;
                    style.BorderColor = Colors.Black;
                }
                offsetLabel.AddThemeStyleboxOverride("panel", style);
                descLabel.AddThemeStyleboxOverride("panel", style);
                h.AddChild(offsetLabel);
                h.AddChild(descLabel);
                _descTable.AddChild(h);
                idx++;
            }
        }

        public void MenuItemSelected(string code) {}

        private void ClearChildren(Container container)
        {
            foreach (var child in container.GetChildren().ToArray())
            {
                if (child is Node node)
                    node.QueueFree();
            }
        }
    }
}
