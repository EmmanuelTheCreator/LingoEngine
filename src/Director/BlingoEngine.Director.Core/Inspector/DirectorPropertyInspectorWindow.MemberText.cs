using System;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.Windowing.Commands;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Members;
using BlingoEngine.Texts;

namespace BlingoEngine.Director.Core.Inspector;

public partial class DirectorPropertyInspectorWindow
{
    private void AddTextTab(IBlingoMemberTextBase textMember)
    {
        var wrap = AddTab(PropetyTabNames.Text);
        var row = _factory.CreatePanel("TextRow");
        row.BackgroundColor = DirectorColors.BG_WhiteMenus;
        row.Margin = new AMargin(5, 5, 0, 0);
        var textAdapter = new TextMemberCommandAdapter(this, textMember);
        row.Compose(_factory.ComponentFactory)
           .NextRow()
           .Columns(8)
           .AddNumericInputFloat("TextWidth", "W:", textAdapter, s => s.Width)
           .AddNumericInputFloat("TextHeight", "H:", textAdapter, s => s.Height, inputSpan: 5)

           .NextRow()
           .Columns(2)
           .AddButton("EditText", "Edit", () =>
           {
               _commandManager?.Handle(new OpenWindowCommand(DirectorMenuCodes.TextEditWindow));
           })
           .Finalize();
        wrap.AddItem(row);
    }

    private sealed class TextMemberCommandAdapter : MemberCommandAdapterBase<IBlingoMemberTextBase>
    {
        public TextMemberCommandAdapter(DirectorPropertyInspectorWindow window, IBlingoMemberTextBase member)
            : base(window, member)
        {
        }

        public float Width
        {
            get => Member.Width;
            set
            {
                int newValue = (int)MathF.Round(value);
                if (Member.Width == newValue)
                    return;
                Dispatch(new APropertyValue(nameof(BlingoMember.Width), newValue));
            }
        }

        public float Height
        {
            get => Member.Height;
            set
            {
                int newValue = (int)MathF.Round(value);
                if (Member.Height == newValue)
                    return;
                Dispatch(new APropertyValue(nameof(BlingoMember.Height), newValue));
            }
        }
    }
}
