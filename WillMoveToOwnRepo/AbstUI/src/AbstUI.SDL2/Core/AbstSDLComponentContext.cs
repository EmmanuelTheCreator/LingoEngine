using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.SDLL;
using System;
using System.ComponentModel;

namespace AbstUI.SDL2.Core;

public class AbstSDLComponentContext : IDisposable
{
    private readonly AbstSDLComponentContainer _container;
    private bool _requireRender = true;
    private bool _requireRenderFromChild = true;
    internal IAbstSDLComponent? Component { get; private set; }
    internal AbstSDLComponentContext? LogicalParent { get; private set; }
    internal AbstSDLComponentContext? VisualParent { get; private set; }
    private HashSet<IAbstSDLComponent> _modifiedChildren = new();
    public event Action<IAbstSDLComponent>? OnRequestRedraw;
    public nint Texture { get; private set; }
    public nint Renderer { get; set; }
    public int TargetWidth { get; set; }
    public int TargetHeight { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool Visible { get; set; } = true;
    public float Blend { get; set; } = 100;
    public int? InkType { get; set; }
    public float OffsetX { get; set; }
    public float OffsetY { get; set; }
    public bool FlipH { get; set; }
    public bool FlipV { get; set; }
    public SDL.SDL_Rect? SourceRect { get; set; }
    public bool AlwaysOnTop { get; set; }
    public int ZIndex { get; private set; }
    public SDL.SDL_BlendMode BlendMode { get; set; } = SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND;
    public bool RequireToRedraw => _requireRender || _requireRenderFromChild;

    internal AbstSDLComponentContext(
        AbstSDLComponentContainer container,
        IAbstSDLComponent? component = null,
        AbstSDLComponentContext? parent = null)
    {
        _container = container;
        LogicalParent = parent;
        VisualParent = parent;
        Component = component;
        _container.Register(this);
    }

    public void SetZIndex(int zIndex)
    {
        ZIndex = zIndex;
        _container.Activate(this);
        if (Component != null)
            VisualParent?.QueueRedrawFromChild(Component);
    }

    public void SetParents(AbstSDLComponentContext? logicalParent, AbstSDLComponentContext? visualParent = null)
    {
        LogicalParent = logicalParent;
        VisualParent = visualParent ?? logicalParent;
        if (_requireRender && Component != null && VisualParent != null)
            VisualParent.QueueRedrawFromChild(Component);
    }

    public void QueueRedraw(IAbstSDLComponent component)
    {
        if (_requireRender) return;
        _requireRender = true;
        VisualParent?.QueueRedrawFromChild(component);
        OnRequestRedraw?.Invoke(component);
    }
    public void QueueRedrawFromChild(IAbstSDLComponent component)
    {
        if (_modifiedChildren.Contains(component))
            return;
        _modifiedChildren.Add(component);
        _requireRenderFromChild = true;
        VisualParent?.QueueRedrawFromChild(Component ?? component);
        OnRequestRedraw?.Invoke(component);
    }

    public bool HasModifiedChildren() => _modifiedChildren.Any();
    public IEnumerable<IAbstSDLComponent> GetModifiedChildren() => _modifiedChildren;

    public void RenderToTexture(AbstSDLRenderContext renderContext)
    {
        if (!Visible)
            return;

        Renderer = renderContext.Renderer;

        if ((_requireRender || _requireRenderFromChild) && Component is { })
        {
            var renderResult = Component.Render(renderContext);
            Texture = renderResult.Texture;
            _requireRender = renderResult.DoRender;
            _modifiedChildren.Clear();
            _requireRenderFromChild = false;
        }

        if (Texture == nint.Zero)
            return;

        int drawX = (int)(X + OffsetX);
        int drawY = (int)(Y + OffsetY);
        SDL.SDL_Rect dst = new SDL.SDL_Rect
        {
            x = drawX,
            y = drawY,
            w = TargetWidth,
            h = TargetHeight
        };
        SDL.SDL_SetTextureAlphaMod(Texture, PercentToByte(Blend));
        SDL.SDL_SetTextureBlendMode(Texture, BlendMode);
        SDL.SDL_RendererFlip flip = SDL.SDL_RendererFlip.SDL_FLIP_NONE;
        if (FlipH)
            flip |= SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL;
        if (FlipV)
            flip |= SDL.SDL_RendererFlip.SDL_FLIP_VERTICAL;
        var name = "";
        if (Component is AbstSdlComponent comp) name = comp.Name;
        //Console.WriteLine($"SDL CTX BLIT dst=({drawX},{drawY},{TargetWidth},{TargetHeight}) {name}");

        if (SourceRect is SDL.SDL_Rect src)
        {
            SDL.SDL_RenderCopyEx(Renderer, Texture, ref src, ref dst, 0, nint.Zero, flip);
        }
        else
        {
            SDL.SDL_RenderCopyEx(Renderer, Texture, nint.Zero, ref dst, 0, nint.Zero, flip);
        }
    }

    public void Dispose()
    {
        Texture = nint.Zero;
        _container.Unregister(this);
    }

    static byte PercentToByte(float percent) => (byte)Math.Clamp(percent * 2.55f, 0, 255);
}
