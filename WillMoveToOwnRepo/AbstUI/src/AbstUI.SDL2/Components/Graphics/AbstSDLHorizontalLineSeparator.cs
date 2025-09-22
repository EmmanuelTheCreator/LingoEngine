using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;
using AbstUI.Primitives;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;

namespace AbstUI.SDL2.Components.Graphics
{
    public class AbstSDLVerticalLineSeparator : AbstSdlComponent, IFrameworkFor<AbstVerticalLineSeparator> , IAbstFrameworkVerticalLineSeparator
    {
        private AbstVerticalLineSeparator _sep;

        public AbstSDLVerticalLineSeparator(AbstSdlComponentFactory factory, AbstVerticalLineSeparator sep)
            :base(factory)
        {
            _sep = sep;
            _sep.Init(this);
        }

        public AMargin Margin { get;  set; }

        public object FrameworkNode => this;

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            return nint.Zero;
        }
        // Todo implementation
    }
    public class AbstSDLHorizontalLineSeparator : AbstSdlComponent, IFrameworkFor<AbstHorizontalLineSeparator> , IAbstFrameworkHorizontalLineSeparator
    {
        private AbstHorizontalLineSeparator _sep;
        public AMargin Margin { get; set; }

        public object FrameworkNode => this;
        public AbstSDLHorizontalLineSeparator(AbstSdlComponentFactory factory, AbstHorizontalLineSeparator sep)
            :base(factory)
        {
            _sep = sep;
            _sep.Init(this);
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            return nint.Zero;
        }
        // Todo implementation

    }
}
