using AbstUI.Components.Graphics;
using AbstUI.Primitives;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Members;
using BlingoEngine.Primitives;

namespace BlingoEngine.Director.Core.Casts;

/// <summary>
/// Draws a single cast member on its own canvas. Rendering of the member
/// preview is delegated to <see cref="DirectorMemberThumbnail"/> so the logic
/// can be shared across frameworks.
/// </summary>
public class DirCastItem : IDirCastItem
{
    public const int Width = 58;
    public const int Height = 54;
    private const int LabelHeight = 12;

    private IBlingoMember? _member;
    private readonly int _slotNumber;
    private bool _selected;
    private bool _hovered;
    private readonly AbstGfxCanvas _canvas;
    private DirectorMemberThumbnail _thumb;

    public IBlingoMember? Member => _member;
    public AbstGfxCanvas Canvas => _canvas;
    public int SlotNumber => _slotNumber;

    public DirCastItem(IBlingoFrameworkFactory factory, IBlingoMember? member, int slotNumber, IDirectorIconManager? iconManager)
    {
        _member = member;
        _slotNumber = slotNumber;
        _canvas = factory.CreateGfxCanvas($"CastMemberCanvas_{slotNumber}", Width, Height);

        _thumb = new DirectorMemberThumbnail(Width - 2, Height - LabelHeight - 1, factory, iconManager, _canvas, 1, 1);
        Draw();
    }

    internal void MakeEmpty() => SetMember(null);
    public void SetMember(IBlingoMember? member)
    {
        _member = member;
        Draw();
    }

    public void SetSelected(bool selected)
    {
        _selected = selected;
        Draw();
    }

    public void SetHovered(bool hovered)
    {
        _hovered = hovered;
        Draw();
    }

    private void Draw()
    {
        _canvas.Clear(AColors.Transparent);
        // selection highlight
        if (_selected)
        {
            _canvas.DrawRect(ARect.New(0, 0, Width, Height), DirectorColors.BlueSelectColor, true);
        }
        else if (_hovered)
        {
            _canvas.DrawLine(new APoint(0, 0), new APoint(Width, 0), AColors.Black); // top
            _canvas.DrawLine(new APoint(0, 0), new APoint(0, Height), AColors.Black); // left
            _canvas.DrawLine(new APoint(0, Height), new APoint(Width, Height), AColors.Black); // bottom
            _canvas.DrawLine(new APoint(Width, 0), new APoint(Width, Height), AColors.Black); // right
        }
        else
        {
            _canvas.DrawLine(new APoint(0, 0), new APoint(Width, 0), DirectorColors.LineDarker); // top
            _canvas.DrawLine(new APoint(0, 0), new APoint(0, Height), DirectorColors.LineDarker); // left
            _canvas.DrawLine(new APoint(0, Height), new APoint(Width, Height), DirectorColors.LineLight); // bottom
            _canvas.DrawLine(new APoint(Width, 0), new APoint(Width, Height), DirectorColors.LineLight); // right
        }

        // draw member preview or empty slot
        if (_member != null)
        {
            _thumb.SetMember(_member);
        }
        else
        {
            _thumb.SetEmpty();
        }

        // label text
        string label;
        if (_member != null)
            label = $"{_member.NumberInCast}. {_member.Name}";
        else
            label = _slotNumber.ToString();
        _canvas.DrawText(new APoint(2, Height - LabelHeight ), label, null, _selected ? AColors.White : AColors.Black, 8, Width - 4);



    }

   
}


