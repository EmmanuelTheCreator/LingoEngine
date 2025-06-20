using Godot;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.LGodot.TestData;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Director.Core.Gfx;

namespace LingoEngine.Director.LGodot.Gfx
{
    internal partial class DirGodotBinaryViewerWindow : BaseGodotWindow, IDirFrameworkBinaryViewerWindow
    {
        private readonly IDirectorEventMediator _mediator;
        private readonly LineEdit _pathEdit = new LineEdit();
        //private VBoxContainer _hexRows = new VBoxContainer();
        private readonly VBoxContainer _descTable = new VBoxContainer();
        private readonly ScrollContainer _scroll = new ScrollContainer();
        private readonly ScrollContainer _descScroll = new ScrollContainer();
        private readonly Dictionary<int, string> _knownOffsets = new();
        private readonly Dictionary<int, int> _blockIndexByOffset = new();
        private readonly Dictionary<int, Color> _blockColors = new();
        private readonly HashSet<int> _styleOffsets = new();
        private readonly List<XmedBlock> _blocks = new();
        private XmedFileHints? _currentHints;
        private SubViewport _viewport;
        private TextureRect _textureRect;

        public DirGodotBinaryViewerWindow(IDirectorEventMediator mediator, DirectorBinaryViewerWindow directorBinaryViewerWindow) : base("Binary Viewer")
        {
            _mediator = mediator;
            directorBinaryViewerWindow.Init(this);
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
            _descScroll.AddChild(_descTable);
            body.AddChild(_descScroll);
            root.AddChild(body);
            _scroll.SizeFlagsVertical = Control.SizeFlags.Expand;
            _scroll.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            _scroll.CustomMinimumSize = new Vector2(1200, 400);
            _descScroll.SizeFlagsVertical = Control.SizeFlags.Expand;
            _descScroll.CustomMinimumSize = new Vector2(200, 400);
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
            int start = hints?.StartOffset ?? XmedInterpreter.FindXmedStart(data);
            if (start > 0 && start < data.Length)
                data = data[start..];

            _currentHints = hints;
            _knownOffsets.Clear();
            _blockIndexByOffset.Clear();
            _blockColors.Clear();
            _styleOffsets.Clear();
            _blocks.Clear();
            ClearChildren(_descTable);

            var interp = XmedInterpreter.Interpret(data, hints?.Blocks);
            var rawBlocks = new List<XmedBlock>(interp.Blocks);
            rawBlocks.Sort((a, b) => a.Start.CompareTo(b.Start));
            _blocks.AddRange(rawBlocks);

            int colorIndex = 0;
            for (int idx = 0; idx < _blocks.Count; idx++)
            {
                var block = _blocks[idx];
                var hue = (colorIndex * 0.1f) % 1f;
                var color = Color.FromHsv(hue, 0.3f, 1f);
                _blockColors[idx] = color;
                for (int i = 0; i < block.Length; i++)
                {
                    int off = block.Start + i;
                    _blockIndexByOffset[off] = idx;
                    _knownOffsets[off] = block.Description;
                    if (block.IsStyle)
                        _styleOffsets.Add(off);
                }
                colorIndex++;
            }

            // Cleanup previous draw content from viewport
            foreach (var child in _viewport.GetChildren())
            {
                if (child is Node node)
                {
                    _viewport.RemoveChild(node);
                    node.QueueFree();
                }
            }

            // Setup new drawing control
            var drawControl = new HexDrawControl(data, _blocks, _blockColors, _blockIndexByOffset, _styleOffsets);

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
                    if (_blockIndexByOffset.TryGetValue(i + j, out var idx))
                    {
                        var c = _blockColors.TryGetValue(idx, out var col) ? col : Colors.LightGray;
                        var style = new StyleBoxFlat { BgColor = c };
                        if (_styleOffsets.Contains(i + j))
                        {
                            style.BorderWidthLeft = 1;
                            style.BorderWidthRight = 1;
                            style.BorderWidthTop = 1;
                            style.BorderWidthBottom = 1;
                            style.BorderColor = Colors.Black;
                        }
                        lbl.AddThemeStyleboxOverride("panel", style);
                        var block = _blocks[idx];
                        if (i + j == block.Start)
                            tag = block.Description.Length > 6 ? block.Description.Substring(0,6) : block.Description;
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
            foreach (var block in _blocks)
            {
                var h = new HBoxContainer();
                var range = block.Length == 1
                    ? $"0x{block.Start:X4}"
                    : $"0x{block.Start:X4}-0x{block.Start + block.Length - 1:X4}";
                var offsetLabel = new Label { Text = range };
                var descLabel = new Label { Text = block.Description };
                var detailLabel = new Label { Text = block.Detail };

                var c = _blockColors.TryGetValue(idx, out var col) ? col : Color.FromHsv((idx * 0.1f) % 1f, 0.3f, 1f);
                var style = new StyleBoxFlat { BgColor = c };
                if (block.IsStyle)
                {
                    style.BorderWidthLeft = 1;
                    style.BorderWidthRight = 1;
                    style.BorderWidthTop = 1;
                    style.BorderWidthBottom = 1;
                    style.BorderColor = Colors.Black;
                }
                offsetLabel.AddThemeStyleboxOverride("panel", style);
                descLabel.AddThemeStyleboxOverride("panel", style);
                detailLabel.AddThemeStyleboxOverride("panel", style);
                h.AddChild(offsetLabel);
                h.AddChild(descLabel);
                if (block.Detail != block.Description)
                    h.AddChild(detailLabel);
                _descTable.AddChild(h);
                idx++;
            }
        }

        private void ClearChildren(Container container)
        {
            foreach (var child in container.GetChildren().ToArray())
            {
                if (child is Node node)
                    node.QueueFree();
            }
        }

        public bool IsOpen => Visible;
        public void OpenWindow() => Visible = true;
        public void CloseWindow() => Visible = false;
        public void MoveWindow(int x, int y) => Position = new Vector2(x, y);
    }
}
