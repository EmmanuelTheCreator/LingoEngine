using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.Net.RNetTerminal.Datas;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;

namespace BlingoEngine.Net.RNetTerminal.Views;

internal sealed class StageView : View
{
    private int _movieWidth;
    private int _movieHeight;
    private IReadOnlyList<Blingo2DSpriteDTO> _sprites = System.Array.Empty<Blingo2DSpriteDTO>();
    private int _frame;
    private SpriteRef? _selectedSprite;
    private static readonly Color[] _overlapColors =
    {
        Color.DarkGray,
        Color.Gray,
        Color.Magenta,
        Color.Magenta,
        Color.BrightCyan,
        Color.BrightYellow,
    };

    public StageView()
    {
        CanFocus = true;
        SetScheme(new Scheme
        {
            Normal = new Attribute(Color.White, Color.Black)
        });
        var store = TerminalDataStore.Instance;
        _frame = store.GetFrame();
        _selectedSprite = store.GetSelectedSprite();
        store.FrameChanged += f =>
        {
            _frame = f;
            SetNeedsDraw();
        };
        store.SelectedSpriteChanged += s =>
        {
            _selectedSprite = s;
            SetNeedsDraw();
        };
        store.SpritesChanged += ReloadData;
        store.CastsChanged += ReloadData;
        store.SpriteChanged += _ => SetNeedsDraw();
        store.MemberChanged += _ => SetNeedsDraw();
        ReloadData();
    }


    public void RequestRedraw() => SetNeedsDraw();

    private void ReloadData()
    {
        var store = TerminalDataStore.Instance;
        _movieWidth = store.StageWidth;
        _movieHeight = store.StageHeight;
        _sprites = store.GetSprites();

        SetNeedsDraw();
    }
    protected override bool OnDrawingContent(DrawContext? context)
    {
        var bounds = Viewport;
        //return base.OnDrawingContent(context);
        
        var w = bounds.Width;
        var h = bounds.Height;
        //Driver.SetAttribute(ColorScheme.Normal);
        SetAttribute(new Attribute(Color.Green));
        

        for (var y = 0; y < h; y++)
        {
            Move(0, y);
            for (var x = 0; x < w; x++)
            {
                AddRune(' ');
            }
        }
        
        var chars = new char[w, h];
        var colors = new Color[w, h];
        var counts = new int[w, h];

        foreach (var sprite in _sprites
                     .Where(s => s.BeginFrame <= _frame && _frame <= s.EndFrame)
                     .OrderBy(s => s.LocZ))
        {
            if (sprite.Member == null) continue;
            var member = TerminalDataStore.Instance.FindMember(sprite.Member!.CastLibNum, sprite.Member.MemberNum);
            if (member == null)
            {
                continue;
            }
            var x = (int)(sprite.LocH / _movieWidth * w);
            var y = (int)(sprite.LocV / _movieHeight * h);
            var sw = (int)(sprite.Width / _movieWidth * w);
            var sh = (int)(sprite.Height / _movieHeight * h);
            if (sw <= 0 || sh <= 0 || sh > 2 || sw > 10)
            {
                sw = 1;
                sh = 1;
            }
            if (member is BlingoMemberTextDTO textMember)
            {
                var text = !string.IsNullOrWhiteSpace(textMember.MarkDownText)? RetrieveTextOnly(textMember.MarkDownText) : member.Name;
                var count = System.Math.Min(sw, text.Length);
                for (var i = 0; i < count; i++)
                {
                    var sx = x + i;
                    var sy = y;
                    if (sx < 0 || sx >= w || sy < 0 || sy >= h)
                    {
                        continue;
                    }
                    counts[sx, sy]++;
                    chars[sx, sy] = text[i];
                    var bg = _selectedSprite.HasValue && sprite.SpriteNum == _selectedSprite.Value.SpriteNum && sprite.BeginFrame == _selectedSprite.Value.BeginFrame
                        ? Color.Blue
                        : _overlapColors[System.Math.Min(counts[sx, sy] - 1, _overlapColors.Length - 1)];
                    colors[sx, sy] = bg;
                }
            }
            else
            {
                var ch = member.Name.Length > 0 ? member.Name[0] : ' ';
                for (var dy = 0; dy < sh; dy++)
                {
                    for (var dx = 0; dx < sw; dx++)
                    {
                        var sx = x + dx;
                        var sy = y + dy;
                        if (sx < 0 || sx >= w || sy < 0 || sy >= h)
                        {
                            continue;
                        }
                        counts[sx, sy]++;
                        chars[sx, sy] = ch;
                        var bg = _selectedSprite.HasValue && sprite.SpriteNum == _selectedSprite.Value.SpriteNum && sprite.BeginFrame == _selectedSprite.Value.BeginFrame
                            ? Color.Blue
                            : _overlapColors[System.Math.Min(counts[sx, sy] - 1, _overlapColors.Length - 1)];
                        colors[sx, sy] = bg;
                    }
                }
            }
        }

        for (var y = 0; y < h; y++)
        {
            Move(0, y);
            for (var x = 0; x < w; x++)
            {
                var col = colors[x, y];
                SetAttribute(new Attribute(Color.Black, col));
                AddRune(chars[x, y] == '\0' ? ' ' : chars[x, y]);
            }
        }
        return true;
    }
    protected override bool OnMouseEvent(MouseEventArgs me)
    {
        var bounds = Viewport;
        if (me.Flags.HasFlag(MouseFlags.Button1Clicked))
        {
            var w = bounds.Width;
            var h = bounds.Height;
            var mouse = me.Position;
            foreach (var sprite in _sprites
                         .Where(s => s.BeginFrame <= _frame && _frame <= s.EndFrame)
                         .OrderByDescending(s => s.LocZ))
            {
                var x = (int)(sprite.LocH / _movieWidth * w);
                var y = (int)(sprite.LocV / _movieHeight * h);
                var sw = (int)(sprite.Width / _movieWidth * w);
                var sh = (int)(sprite.Height / _movieHeight * h);
                if (sw <= 0 || sh <= 0 || sh > 2 || sw > 10)
                {
                    sw = 1;
                    sh = 1;
                }
                if (mouse.X >= x && mouse.X < x + sw && mouse.Y >= y && mouse.Y < y + sh)
                {
                    var sel = new SpriteRef(sprite.SpriteNum, sprite.BeginFrame);
                    TerminalDataStore.Instance.SelectSprite(sel);

                    break;
                }
            }
            return true;
        }
        return base.OnMouseEvent(me);
    }

    public static string RetrieveTextOnly(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return string.Empty;

        var text = markdown;

        // 1. Remove custom {{...}} tags
        text = Regex.Replace(text, @"\{\{.*?\}\}", string.Empty, RegexOptions.Singleline);

        // 2. Remove images ![alt](url)
        text = Regex.Replace(text, @"!\[.*?\]\(.*?\)", string.Empty);

        // 3. Replace links [text](url) → keep only "text"
        text = Regex.Replace(text, @"\[(.*?)\]\(.*?\)", "$1");

        // 4. Remove headings (#, ##, ###) → keep only text
        text = Regex.Replace(text, @"^\s{0,3}#{1,6}\s*", string.Empty, RegexOptions.Multiline);

        // 5. Remove bold **text** → keep only "text"
        text = Regex.Replace(text, @"\*\*(.*?)\*\*", "$1");

        // 6. Remove italic *text* → keep only "text"
        text = Regex.Replace(text, @"\*(.*?)\*", "$1");

        // 7. Remove underline __text__ → keep only "text"
        text = Regex.Replace(text, @"__(.*?)__", "$1");

        // 8. Collapse multiple spaces (but keep newlines)
        text = Regex.Replace(text, @"[^\S\r\n]+", " ");

        // 9. Collapse 3+ newlines into 2 (keep paragraph separation)
        text = Regex.Replace(text, @"(\r?\n){3,}", "\n\n");


        return text.Trim();
    }
}

