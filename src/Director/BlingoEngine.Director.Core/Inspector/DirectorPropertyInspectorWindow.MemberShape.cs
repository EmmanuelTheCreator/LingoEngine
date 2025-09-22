using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.Windowing.Commands;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Members;
using BlingoEngine.Shapes;

namespace BlingoEngine.Director.Core.Inspector;

public partial class DirectorPropertyInspectorWindow
{
    private void AddShapeTab(BlingoMemberShape member)
    {
        var wrap = AddTab(PropetyTabNames.Shape);
        var row = _factory.CreatePanel("ShapeRow");
        row.Margin = new AMargin(5, 5, 0, 0);
        var shapeAdapter = new ShapeMemberCommandAdapter(this, member);
        var composer = row.Compose(_factory.ComponentFactory)
               .NextRow()
               .Columns(8)
               .AddEnumInput<ShapeMemberCommandAdapter, BlingoShapeType>("ShapeType", "Shape:", shapeAdapter, s => s.ShapeType, inputSpan: 6, labelSpan: 2)
               .AddCheckBox("ShapeClosed", "Filled:", shapeAdapter, s => s.Filled, inputSpan: 1, true)

               .NextRow()
               .AddNumericInputInt("ShapeWidth", "W:", shapeAdapter, s => s.Width)
               .AddNumericInputInt("ShapeHeight", "H:", shapeAdapter, s => s.Height, inputSpan: 5);
        if (member.ShapeType == BlingoShapeType.Rectangle || member.ShapeType == BlingoShapeType.Oval)
        {
            composer
                .NextRow()
                .AddButton("EditShape", "Edit", () =>
                {
                    _commandManager?.Handle(new OpenWindowCommand(DirectorMenuCodes.ShapeEditWindow));
                }, 6)
                .NextRow();
        }

        composer.Finalize();
        wrap.AddItem(row);
    }

    private sealed class ShapeMemberCommandAdapter : MemberCommandAdapterBase<BlingoMemberShape>
    {
        public ShapeMemberCommandAdapter(DirectorPropertyInspectorWindow window, BlingoMemberShape member)
            : base(window, member)
        {
        }

        public int ShapeType
        {
            get => Member.ShapeTypeInt;
            set => DispatchIfChanged(nameof(BlingoMemberShape.ShapeTypeInt), Member.ShapeTypeInt, value);
        }

        public bool Filled
        {
            get => Member.Filled;
            set => DispatchIfChanged(nameof(BlingoMemberShape.Filled), Member.Filled, value);
        }

        public int Width
        {
            get => Member.Width;
            set => DispatchIfChanged(nameof(BlingoMember.Width), Member.Width, value);
        }

        public int Height
        {
            get => Member.Height;
            set => DispatchIfChanged(nameof(BlingoMember.Height), Member.Height, value);
        }
    }
}
