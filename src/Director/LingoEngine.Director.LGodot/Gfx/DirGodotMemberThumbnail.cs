using Godot;
using LingoEngine.Members;
using LingoEngine.Pictures;
using LingoEngine.Texts;
using LingoEngine.LGodot.Pictures;
using System.Text;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class DirGodotMemberThumbnail : Control
{
    private readonly Sprite2D _sprite = new();
    private readonly SubViewport _textViewport = new();
    private readonly Label _typeLabel;

    public float ThumbWidth { get; }
    public float ThumbHeight { get; }

    public DirGodotMemberThumbnail(float width, float height)
    {
        ThumbWidth = width;
        ThumbHeight = height;
        CustomMinimumSize = new Vector2(width, height);

        var style = new StyleBoxFlat
        {
            BgColor = Colors.White,
            BorderColor = Colors.DarkGray
        };
        style.BorderWidthBottom = 1;
        style.BorderWidthTop = 1;
        style.BorderWidthLeft = 1;
        style.BorderWidthRight = 1;
        AddThemeStyleboxOverride("panel", style);

        _textViewport.SetDisable3D(true);
        _textViewport.TransparentBg = true;
        _textViewport.SetUpdateMode(SubViewport.UpdateMode.Always);
        AddChild(_textViewport);

        AddChild(_sprite);
        _sprite.Position = new Vector2(width / 2f, height / 2f);

        _typeLabel = new Label
        {
            LabelSettings = new LabelSettings { FontSize = 8 },
            MouseFilter = MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CustomMinimumSize = new Vector2(10, 10)
        };
        var typeStyle = new StyleBoxFlat
        {
            BgColor = Colors.White,
            BorderColor = Colors.Black
        };
        typeStyle.BorderWidthBottom = 1;
        typeStyle.BorderWidthTop = 1;
        typeStyle.BorderWidthLeft = 1;
        typeStyle.BorderWidthRight = 1;
        _typeLabel.AddThemeStyleboxOverride("normal", typeStyle);
        _typeLabel.AddThemeColorOverride("font_color", Colors.Black);
        AddChild(_typeLabel);

        _typeLabel.AnchorLeft = 1;
        _typeLabel.AnchorRight = 1;
        _typeLabel.AnchorTop = 1;
        _typeLabel.AnchorBottom = 1;
        _typeLabel.OffsetRight = -2;
        _typeLabel.OffsetBottom = -2;
        _typeLabel.OffsetLeft = -_typeLabel.CustomMinimumSize.X - 2;
        _typeLabel.OffsetTop = -_typeLabel.CustomMinimumSize.Y - 2;
    }

    public void SetMember(ILingoMember member)
    {
        _typeLabel.Text = LingoMemberTypeIcons.GetIcon(member);
        foreach (var child in _textViewport.GetChildren())
            _textViewport.RemoveChild(child);

        switch (member)
        {
            case LingoMemberPicture pic:
                var godotPicture = pic.Framework<LingoGodotMemberPicture>();
                godotPicture.Preload();
                if (godotPicture.Texture != null)
                {
                    _sprite.Texture = godotPicture.Texture;
                    ResizeSprite(ThumbWidth - 2, ThumbHeight - 2);
                }
                break;
            case ILingoMemberTextBase textMember:
                var godotText = textMember.FrameworkObj;
                godotText.Preload();
                var label = new Label
                {
                    Text = GetPreviewText(textMember),
                    LabelSettings = new LabelSettings { FontSize = 10, LineSpacing = 11, FontColor = Colors.Black },
                    AutowrapMode = TextServer.AutowrapMode.Word,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                _textViewport.SetSize(new Vector2I((int)(ThumbWidth - 2), (int)(ThumbHeight - 2)));
                label.Size = new Vector2(ThumbWidth - 2, ThumbHeight - 2);
                _textViewport.AddChild(label);
                _sprite.Texture = _textViewport.GetTexture();
                ResizeSprite(ThumbWidth - 2, ThumbHeight - 2);
                break;
            default:
                _sprite.Texture = null;
                break;
        }
    }

    public void ResizeSprite(float targetWidth, float targetHeight)
    {
        if (_sprite.Texture == null) return;
        float scaleFactorW = targetWidth / _sprite.Texture.GetWidth();
        float scaleFactorH = targetHeight / _sprite.Texture.GetHeight();
        _sprite.Scale = new Vector2(scaleFactorW, scaleFactorH);
    }

    private static string GetPreviewText(ILingoMemberTextBase text)
    {
        var lines = text.Text.Replace("\r", "").Split('\n');
        var sb = new StringBuilder();
        int count = Math.Min(4, lines.Length);
        for (int i = 0; i < count; i++)
        {
            var line = lines[i];
            if (line.Length > 14)
                line = line.Substring(0, 14);
            sb.Append(line);
            if (i < count - 1)
                sb.Append('\n');
        }
        return sb.ToString();
    }
}
