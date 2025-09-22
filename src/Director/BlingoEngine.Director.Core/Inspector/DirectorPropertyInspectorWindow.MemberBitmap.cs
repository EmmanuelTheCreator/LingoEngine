using System;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.Members;

namespace BlingoEngine.Director.Core.Inspector;

public partial class DirectorPropertyInspectorWindow
{
    private void AddBitmapTab(BlingoMemberBitmap member)
    {
        var wrapContainer = AddTab(PropetyTabNames.Bitmap);
        var container = _factory.CreatePanel("MemberDetailPanel");
        var bitmapAdapter = new BitmapMemberCommandAdapter(this, member);
        wrapContainer
            .AddItem(container);

        container.Compose(_factory.ComponentFactory)
               .Columns(4)
               .AddLabel("BitmapSize", "Dimensions: ", 2)
               .AddLabel("BitmapSizeV", member.Width + " x " + member.Height, 2)
               .AddCheckBox("BitmapHighLight", "Hightlight: ", bitmapAdapter, x => x.Hilite, 2, true, 2)
               .Columns(8)
               .AddNumericInputFloat("BitmapRegPointX", "RegPoint X:", bitmapAdapter, s => s.RegPointX, inputSpan: 1, labelSpan: 3)
               .AddNumericInputFloat("BitmapRegPointY", "Y:", bitmapAdapter, s => s.RegPointY, inputSpan: 4, labelSpan: 1)
               .Finalize();
    }

    private sealed class BitmapMemberCommandAdapter : MemberCommandAdapterBase<BlingoMemberBitmap>
    {
        public BitmapMemberCommandAdapter(DirectorPropertyInspectorWindow window, BlingoMemberBitmap member)
            : base(window, member)
        {
        }

        public bool Hilite
        {
            get => Member.Hilite;
            set => DispatchIfChanged(nameof(BlingoMember.Hilite), Member.Hilite, value);
        }

        public float RegPointX
        {
            get => Member.RegPoint.X;
            set
            {
                if (Math.Abs(Member.RegPoint.X - value) <= float.Epsilon)
                    return;
                var newPoint = new APoint(value, Member.RegPoint.Y);
                Dispatch(new APropertyValue(nameof(BlingoMember.RegPoint), newPoint));
            }
        }

        public float RegPointY
        {
            get => Member.RegPoint.Y;
            set
            {
                if (Math.Abs(Member.RegPoint.Y - value) <= float.Epsilon)
                    return;
                var newPoint = new APoint(Member.RegPoint.X, value);
                Dispatch(new APropertyValue(nameof(BlingoMember.RegPoint), newPoint));
            }
        }
    }
}
