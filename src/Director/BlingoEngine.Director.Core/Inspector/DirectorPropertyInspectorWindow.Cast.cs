using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using BlingoEngine.Casts;
using BlingoEngine.Director.Core.Casts.Commands;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Casts.Commands;

namespace BlingoEngine.Director.Core.Inspector;

public partial class DirectorPropertyInspectorWindow
{
    private void AddCastTab(IBlingoCast cast)
    {
        if (cast is not BlingoCast typedCast)
            return;

        var wrap = AddTab(PropetyTabNames.Cast);
        var container = _factory.CreatePanel("CastRow");
        container.BackgroundColor = DirectorColors.BG_WhiteMenus;
        container.Margin = new AMargin(5, 5, 0, 0);
        wrap.AddItem(container);

        var adapter = new CastCommandAdapter(this, typedCast);

        container.Compose(_factory.ComponentFactory)
            .Columns(8)
            .AddNumericInputInt("CastNumber", "Number:", adapter, m => m.Number, inputSpan: 1, labelSpan: 2, configure: input => input.Enabled = false)
            .AddTextInput("CastName", "Name:", adapter, m => m.Name, inputSpan: 3, labelSpan: 2)
            .Finalize();
    }

    private void DispatchCastCommand(BlingoCast cast, IReadOnlyList<APropertyValue> changes)
    {
        if (changes == null || changes.Count == 0)
            return;

        _commandManager.Handle(new BlingoUpdateCastPropertiesCommand(BlingoCastRef.FromCast(cast), changes));
    }

    private sealed class CastCommandAdapter : PropertyCommandAdapterBase<BlingoCast>
    {
        public CastCommandAdapter(DirectorPropertyInspectorWindow window, BlingoCast cast)
            : base(window, cast)
        {
        }

        private BlingoCast Cast => Target;

        public int Number
        {
            get => Cast.Number;
            set { }
        }

        public string Name
        {
            get => Cast.Name;
            set
            {
                var sanitized = value ?? string.Empty;
                DispatchIfChanged(nameof(BlingoCast.Name), Cast.Name ?? string.Empty, sanitized);
            }
        }

        protected override void DispatchChanges(BlingoCast target, IReadOnlyList<APropertyValue> changes)
            => Window.DispatchCastCommand(target, changes);
    }
}
