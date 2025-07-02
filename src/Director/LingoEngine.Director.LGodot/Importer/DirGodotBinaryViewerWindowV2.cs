using Godot;
using LingoEngine.Director.Core.Importer;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.Director.Core.UI;
using ProjectorRays.Common;
using System;
using System.Collections.Generic;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class DirGodotBinaryViewerWindowV2 : BaseGodotWindow, IDirFrameworkBinaryViewerWindowV2
{
    private readonly ScrollContainer _scroll = new();
    private readonly ScrollContainer _descScroll = new();
    private readonly VBoxContainer _descTable = new();
    private readonly List<HBoxContainer> _descRows = new();
    private readonly StyleBoxFlat _normalRowStyle = new();
    private readonly StyleBoxFlat _selectedRowStyle = new() { BgColor = Colors.SkyBlue };
    private int _selectedRow = -1;
    private SubViewport _viewport;
    private TextureRect _textureRect;

    private readonly Dictionary<int, Color> _blockColors = new();
    private readonly Dictionary<int, int> _blockIndexByOffset = new();
    private readonly List<RayStreamAnnotation> _blocks = new();

    public DirGodotBinaryViewerWindowV2(DirectorBinaryViewerWindowV2 viewerWindow, IDirGodotWindowManager windowManager)
        : base(DirectorMenuCodes.BinaryViewerWindowV2, "Binary Viewer V2", windowManager)
    {
        viewerWindow.Init(this);
        Size = new Vector2(1000, 600);
        CustomMinimumSize = Size;

        _viewport = new SubViewport();
        _viewport.SetDisable3D(true);
        _viewport.TransparentBg = false;
        _viewport.SetUpdateMode(SubViewport.UpdateMode.Once);
        AddChild(_viewport);

        _textureRect = new TextureRect { Texture = _viewport.GetTexture() };
        _scroll.AddChild(_textureRect);

        var root = new HBoxContainer();
        root.Position = new Vector2(0, TitleBarHeight);
        root.Size = new Vector2(Size.X, Size.Y - TitleBarHeight);
        AddChild(root);

        root.AddChild(_scroll);
        _descScroll.AddChild(_descTable);
        root.AddChild(_descScroll);
        _scroll.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        _descScroll.CustomMinimumSize = new Vector2(200, 400);
        _descScroll.SizeFlagsVertical = SizeFlags.ExpandFill;

        if (viewerWindow.Data != null && viewerWindow.InfoToShow != null)
            LoadBytes(viewerWindow.Data, viewerWindow.InfoToShow);
    }

    private void ForceRefreshViewPort()
    {
        _viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
        _viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
        _viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
    }

    private void LoadBytes(byte[] data, RayStreamAnnotatorDecorator annotator)
    {
        _blockColors.Clear();
        _blockIndexByOffset.Clear();
        _blocks.Clear();
        ClearChildren(_descTable);

        _blocks.AddRange(annotator.Annotations);
        long baseOffset = annotator.StreamOffsetBase;
        int colorIndex = 0;
        for (int idx = 0; idx < _blocks.Count; idx++)
        {
            var block = _blocks[idx];
            var hue = (colorIndex * 0.1f) % 1f;
            var color = Color.FromHsv(hue, 0.3f, 1f);
            _blockColors[idx] = color;
            int start = (int)(block.Address - baseOffset);
            for (int i = 0; i < block.Length; i++)
            {
                int off = start + i;
                if (off >= 0 && off < data.Length)
                    _blockIndexByOffset[off] = idx;
            }
            colorIndex++;
        }

        foreach (var child in _viewport.GetChildren())
            if (child is Node node)
            {
                _viewport.RemoveChild(node);
                node.QueueFree();
            }

        var drawControl = new HexDrawControlV2(data, annotator);
        drawControl.ByteClicked += OnByteClicked;
        var rowHeight = 24;
        var totalRows = (int)Math.Ceiling(data.Length / 32.0);
        var contentSize = new Vector2(1400, totalRows * rowHeight);

        drawControl.CustomMinimumSize = contentSize;
        drawControl.Size = contentSize;
        drawControl.CallDeferred("queue_redraw");

        _viewport.SetSize(new Vector2I((int)contentSize.X, (int)contentSize.Y));
        _viewport.AddChild(drawControl);

        _textureRect.Texture = _viewport.GetTexture();
        _textureRect.SetStretchMode(TextureRect.StretchModeEnum.Scale);
        _textureRect.CustomMinimumSize = contentSize;

        RenderDescriptions();
        ForceRefreshViewPort();
    }

    private void RenderDescriptions()
    {
        _descRows.Clear();
        int idx = 0;
        foreach (var block in _blocks)
        {
            var h = new HBoxContainer();
            h.AddThemeStyleboxOverride("panel", _normalRowStyle);
            var range = block.Length == 1
                ? $"0x{block.Address:X4}"
                : $"0x{block.Address:X4}-0x{block.Address + block.Length - 1:X4}";
            var offsetLabel = new Label { Text = range };
            var descLabel = new Label { Text = block.Description };

            var c = _blockColors.TryGetValue(idx, out var col) ? col : Color.FromHsv((idx * 0.1f) % 1f, 0.3f, 1f);
            var style = new StyleBoxFlat { BgColor = c };
            offsetLabel.AddThemeStyleboxOverride("panel", style);
            descLabel.AddThemeStyleboxOverride("panel", style);
            h.AddChild(offsetLabel);
            h.AddChild(descLabel);
            _descTable.AddChild(h);
            _descRows.Add(h);
            idx++;
        }
    }

    private static void ClearChildren(Container container)
    {
        foreach (var child in container.GetChildren().ToArray())
            if (child is Node node)
            {
                container.RemoveChild(node);
                node.QueueFree();
            }
    }

    private void OnByteClicked(int blockIndex)
    {
        HighlightRow(blockIndex);
    }

    private void HighlightRow(int index)
    {
        if (_selectedRow >= 0 && _selectedRow < _descRows.Count)
            _descRows[_selectedRow].AddThemeStyleboxOverride("panel", _normalRowStyle);

        _selectedRow = index;
        if (index >= 0 && index < _descRows.Count)
        {
            _descRows[index].AddThemeStyleboxOverride("panel", _selectedRowStyle);
            _descScroll.ScrollVertical = _descRows[index].Position.Y;
        }
    }

    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
        _scroll.Size = new Vector2(size.X - 200, size.Y - TitleBarHeight);
        _descScroll.Size = new Vector2(200, size.Y - TitleBarHeight);
    }
}
