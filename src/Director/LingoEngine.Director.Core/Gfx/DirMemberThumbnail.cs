using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Members;
using LingoEngine.Pictures;
using LingoEngine.Sounds;
using LingoEngine.Texts;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Gfx;

/// <summary>
/// Simple cross-platform thumbnail preview for cast members.
/// </summary>
public class DirMemberThumbnail : IDisposable
{
    private const int LabelHeight = 15;
    public float ThumbWidth { get; }
    public float ThumbHeight { get; }
    /// <summary>Canvas used for drawing the preview.</summary>
    public LingoGfxCanvas Canvas { get; }
    private readonly IDirIconManager? _iconManager;

    public DirMemberThumbnail(float width, float height, ILingoFrameworkFactory factory, IDirIconManager? iconManager = null)
    {
        ThumbWidth = width;
        ThumbHeight = height;
        Canvas = factory.CreateGfxCanvas((int)width, (int)height);
        _iconManager = iconManager;
    }

    /// <summary>
    /// Draws the specified member on the canvas.
    /// </summary>
    public void SetMember(ILingoMember member)
    {
        Canvas.Clear(DirectorColors.BG_WhiteMenus);
        switch (member)
        {
            case LingoMemberPicture pic:
                DrawPicture(pic);
                break;
            case ILingoMemberTextBase text:
                DrawText(GetPreviewText(text));
                break;
            case LingoMemberSound sound:
                DrawText(sound.Name);
                break;
        }

        if (_iconManager != null)
        {
            var icon = LingoMemberTypeIcons.GetIconType(member);
            if (icon.HasValue)
            {
                var data = _iconManager.GetData(icon.Value);
                var x = ThumbWidth - data.Width - 2;
                var y = ThumbHeight - data.Height - 2;
                Canvas.DrawPicture(data.Data, data.Width, data.Height, new LingoPoint(x, y), data.Format);
            }
        }
    }

    private void DrawPicture(LingoMemberPicture picture)
    {
        picture.Preload();
        var impl = picture.Framework<ILingoFrameworkMemberPicture>();
        if (impl.ImageData == null)
            return;
        var w = impl.Width;
        var h = impl.Height;
        Canvas.DrawPicture(impl.ImageData, w, h, new LingoPoint(0, 0), LingoPixelFormat.Rgba8888);
    }

    private void DrawText(string text)
    {
        Canvas.DrawText(new LingoPoint(2, 2), text, null, new LingoColor(0, 0, 0), 10);
    }

    private static string GetPreviewText(ILingoMemberTextBase text)
    {
        var lines = text.Text.Replace("\r", "").Split('\n');
        var sb = new System.Text.StringBuilder();
        int count = System.Math.Min(4, lines.Length);
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

    public void Dispose()
    {
        Canvas.Dispose();
    }
}
