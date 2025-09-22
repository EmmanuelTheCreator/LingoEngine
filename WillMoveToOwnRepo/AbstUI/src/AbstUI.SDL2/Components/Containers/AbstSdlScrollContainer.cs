using System;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Events;
using AbstUI.FrameworkCommunication;

namespace AbstUI.SDL2.Components.Containers
{
    internal class AbstSdlScrollContainer : AbstSdlScrollViewer, IAbstFrameworkScrollContainer, IFrameworkFor<AbstScrollContainer>, IDisposable, IHandleSdlEvent
    {
        public AbstSdlScrollContainer(AbstSdlComponentFactory factory) : base(factory)
        {
        }

        public object FrameworkNode => this;



        private readonly List<IAbstFrameworkLayoutNode> _children = new();

        public void AddItem(IAbstFrameworkLayoutNode child)
        {
            if (_children.Contains(child))
                return;
            
            _children.Add(child);
            if (child.FrameworkNode is AbstSdlComponent comp)
                comp.ComponentContext.SetParents(ComponentContext);
            ComponentContext.QueueRedraw(this);
        }

        public void RemoveItem(IAbstFrameworkLayoutNode child)
        {
            if (_children.Remove(child))
            {
                if (child.FrameworkNode is AbstSdlComponent comp)
                    comp.ComponentContext.SetParents(null);
                ComponentContext.QueueRedraw(this);
            }
        }

        public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _children.ToArray();

        
        protected override void RenderContent(AbstSDLRenderContext context)
        {
            float maxX = 0, maxY = 0;
            foreach (var child in _children)
            {
                if (child.FrameworkNode is not AbstSdlComponent comp)
                    continue;

                var ctx = comp.ComponentContext;
                var oldOffX = ctx.OffsetX;
                var oldOffY = ctx.OffsetY;

                // Render child relative to this container's origin and scroll position
                ctx.OffsetX += - ScrollHorizontal;
                ctx.OffsetY +=  - ScrollVertical;
                ctx.RenderToTexture(context);

                ctx.OffsetX = oldOffX;
                ctx.OffsetY = oldOffY;

                int childWidth = comp.ComponentContext.TargetWidth != 0
                    ? comp.ComponentContext.TargetWidth
                    : (int)Math.Ceiling(comp.Width);
                int childHeight = comp.ComponentContext.TargetHeight != 0
                    ? comp.ComponentContext.TargetHeight
                    : (int)Math.Ceiling(comp.Height);

                maxX = MathF.Max(maxX, comp.X + childWidth);
                maxY = MathF.Max(maxY, comp.Y + childHeight);
            }

            ContentWidth = maxX;
            ContentHeight = maxY;
        }

        public override bool CanHandleEvent(AbstSDLEvent e)
        {
            return true;
        }

        protected override void HandleContentEvent(AbstSDLEvent e)
        {
            // Forward mouse events to children accounting for current scroll offset
            //Console.WriteLine(e.Event.type);
            ContainerHelpers.HandleChildEvents(_children, e, -ScrollHorizontal + X, -ScrollVertical + Y);
        }



        public override void Dispose()
        {
            foreach (var child in _children)
                if (child.FrameworkNode is AbstSdlComponent comp)
                    comp.ComponentContext.SetParents(null);
            _children.Clear();
            base.Dispose();
        }
    }
}

