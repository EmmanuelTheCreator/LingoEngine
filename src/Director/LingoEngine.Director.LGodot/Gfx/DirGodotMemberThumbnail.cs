using Godot;
using LingoEngine.Members;
using LingoEngine.Pictures;
using LingoEngine.Texts;
using LingoEngine.LGodot.Pictures;
using System.Text;
using LingoEngine.Sounds;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class DirGodotMemberThumbnail : Control
{
    private readonly TextureRect _sprite;
    private readonly PanelContainer _iconPanel;
    private readonly TextureRect _iconTexture;
    private readonly Label _textLabel = new();
    private SubViewport _textViewport;
    private readonly IDirGodotIconManager _iconManager;

    public float ThumbWidth { get; }
    public float ThumbHeight { get; }

    private const float LabelHeight = 15;

    public DirGodotMemberThumbnail(float width, float height, IDirGodotIconManager iconManager)
    {
        ThumbWidth = width;
        ThumbHeight = height;
        _iconManager = iconManager;
        CustomMinimumSize = new Vector2(width, height);
        MouseFilter = MouseFilterEnum.Ignore;

        // Thumbnail image area
        var spriteContainer = new Control
        {
            SizeFlagsHorizontal = SizeFlags.Expand,
            SizeFlagsVertical = SizeFlags.Expand,
            MouseFilter = MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(ThumbWidth, ThumbHeight - LabelHeight)
        };

        _sprite = new TextureRect
        {
            StretchMode = TextureRect.StretchModeEnum.Scale,
            MouseFilter = MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = SizeFlags.Expand,
            SizeFlagsVertical = SizeFlags.Expand
        };
        spriteContainer.AddChild(_sprite);
        AddChild(spriteContainer);

        // Type icon overlay
        _iconPanel = new PanelContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(16, 16)
        };
        var typeStyle = new StyleBoxFlat
        {
            BgColor = Colors.White,
            BorderColor = Colors.Black,
            BorderWidthBottom = 1,
            BorderWidthTop = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1
        };
        _iconPanel.AddThemeStyleboxOverride("panel", typeStyle);
        _iconTexture = new TextureRect
        {
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _iconPanel.AddChild(_iconTexture);
        AddChild(_iconPanel);

        _iconPanel.AnchorLeft = 1;
        _iconPanel.AnchorRight = 1;
        _iconPanel.AnchorTop = 1;
        _iconPanel.AnchorBottom = 1;
        _iconPanel.OffsetRight = -2;
        _iconPanel.OffsetBottom = -2;
        _iconPanel.OffsetLeft = -_iconPanel.CustomMinimumSize.X - 2;
        _iconPanel.OffsetTop = -_iconPanel.CustomMinimumSize.Y - 2;

        //_textViewport = new SubViewport
        //{
        //    TransparentBg = true,
        //    Disable3D = true,
        //    Size = new Vector2I((int)(ThumbWidth - 2), (int)(ThumbHeight - LabelHeight - 2)),
        //    RenderTargetUpdateMode = SubViewport.UpdateMode.Always
        //};
        //AddChild(_textViewport); // ← this was missing

    }

    public void SetMember(ILingoMember member)
    {
        var icon = LingoMemberTypeIcons.GetIcon(member);
        _iconTexture.Texture = icon.HasValue ? _iconManager.Get(icon.Value) : null;

        

        switch (member)
        {
            case LingoMemberPicture pic:
                {
                    var godotPicture = pic.Framework<LingoGodotMemberPicture>();
                    godotPicture.Preload();

                    var original = godotPicture.Texture;
                    if (original != null)
                    {
                        var originalImage = original.GetImage();
                        originalImage.Convert(Image.Format.Rgba8);

                        var targetSize = new Vector2I((int)(ThumbWidth - 2), (int)(ThumbHeight - 2));
                        originalImage.Resize(targetSize.X, targetSize.Y, Image.Interpolation.Lanczos);

                        _sprite.Texture = ImageTexture.CreateFromImage(originalImage);
                    }
                    break;
                }

            case ILingoMemberTextBase textMember:
                {
                    SetupTextPreview(GetPreviewText(textMember));
                    break;
                }
            case LingoMemberSound soundMember:
                {
                    SetupTextPreview(soundMember.Name);
                    break;
                }

            default:
                _sprite.Texture = null;
                break;
        }
    }
    private void SetupTextPreview(string previewText)
    {
        _textViewport = new SubViewport
        {
            TransparentBg = true,
            Disable3D = true,
            Size = new Vector2I((int)(ThumbWidth - 2), (int)(ThumbHeight - LabelHeight - 2)),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always
        };

        _textLabel.Text = previewText;
        _textLabel.Size = new Vector2(ThumbWidth - 2, ThumbHeight - LabelHeight - 2);
        _textLabel.LabelSettings = new LabelSettings
        {
            FontSize = 10,
            LineSpacing = 11,
            FontColor = Colors.Black
        };
        _textLabel.AutowrapMode = TextServer.AutowrapMode.Word;

        _textViewport.AddChild(_textLabel);
        AddChild(_textViewport);
        _sprite.Texture = _textViewport.GetTexture();
        
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
