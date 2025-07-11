using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Members;
using LingoEngine.Sounds;
using LingoEngine.Texts;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Bitmaps;

namespace LingoEngine.Director.Core.Casts;

/// <summary>
/// Simple cross-platform thumbnail preview for cast members.
/// </summary>
public class DirectorMemberThumbnail : IDisposable
{
    private const int LabelHeight = 15;
    public int ThumbWidth { get; }
    public int ThumbHeight { get; }
    /// <summary>Canvas used for drawing the preview.</summary>
    public LingoGfxCanvas Canvas { get; }
    private readonly IDirectorIconManager? _iconManager;

    public DirectorMemberThumbnail(int width, int height, ILingoFrameworkFactory factory, IDirectorIconManager? iconManager = null)
    {
        ThumbWidth = width;
        ThumbHeight = height;
        Canvas = factory.CreateGfxCanvas("MemberThumbnailCanvas", width, height);
        _iconManager = iconManager;
    }

    /// <summary>
    /// Draws the specified member on the canvas.
    /// </summary>
    public void SetMember(ILingoMember member)
    {
        Canvas.Clear(DirectorColors.BG_WhiteMenus);
        Canvas.DrawRect(LingoRect.New(1, 1, ThumbWidth-2, ThumbHeight-2), LingoColorList.White, true);
        Canvas.DrawRect(LingoRect.New(1, 1, ThumbWidth-2, ThumbHeight-2), LingoColorList.Gray, false);
        switch (member)
        {
            case LingoMemberBitmap pic:
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
                var miniIconSize = 16;
                var data = _iconManager.Get(icon.Value);
                var x = ThumbWidth - miniIconSize-2;
                var y = ThumbHeight - miniIconSize-2;
                Canvas.DrawRect(LingoRect.New(x,y, miniIconSize, miniIconSize), LingoColorList.White,true);
                Canvas.DrawPicture(data, miniIconSize-2, miniIconSize-2, new LingoPoint(x+1, y+1));
            }
        }
    }

    private void DrawPicture(LingoMemberBitmap picture)
    {
        picture.Preload();
        var impl = picture.Framework<ILingoFrameworkMemberBitmap>();
        if (impl.Texture == null)
            return;
        var w = impl.Width;
        var h = impl.Height;
       
        Canvas.DrawPicture(impl.Texture, ThumbWidth-4, ThumbHeight-4, new LingoPoint(2, 2));
    }

    private void DrawText(string text)
    {
        const int fontSize = 10;
        const int lineHeight = fontSize + 2;

        int lineCount = text.Split('\n').Length;
        int textHeight = lineCount * lineHeight;
        int startY = (int)Math.Max((ThumbHeight - textHeight) / 2f, 0);

        int maxWidth = ThumbWidth - 4;
        Canvas.DrawText(new LingoPoint(2, startY), text, null, new LingoColor(0, 0, 0), fontSize, maxWidth);
    }

    private static string GetPreviewText(ILingoMemberTextBase text)
    {
        var lines = text.Text.Replace("\r", "").Split('\n');
        var sb = new System.Text.StringBuilder();
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

    public void Dispose()
    {
        Canvas.Dispose();
    }
}
