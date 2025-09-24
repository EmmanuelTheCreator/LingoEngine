using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.SDLL;
using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.SDL2.Core;
using BlingoEngine.SDL2.Movies;
using BlingoEngine.Stages;


namespace BlingoEngine.SDL2.Stages;


public class SdlStage : AbstSdlComponent, IBlingoFrameworkStage, IDisposable
{
    private bool _isTransitioning;
    private readonly BlingoClock _clock;
    private readonly BlingoSdlFactory _factory;
    private readonly BlingoSdlRootContext _rootContext;
    private BlingoStage _stage = null!;
    private readonly HashSet<SdlMovie> _movies = new();
    private SdlMovie? _activeMovie;
    private SdlTexture2D? _transitionFrame;
    private ARect _transitionRect;
    /// <summary>
    /// screenshot of the previous frame
    /// </summary>
    private SdlTexture2D? _startFrame;
    private nint _spritesTexture;

    public float Scale { get; set; }
    public AMargin Margin { get; set; } = AMargin.Zero;

    public object FrameworkNode => this;

    public SdlStage(BlingoSdlRootContext rootContext, BlingoClock clock, BlingoSdlFactory factory)
        :base((AbstSdlComponentFactory)factory.ComponentFactory)
    {
        _rootContext = rootContext;
        _clock = clock;
        _factory = factory;

    }

    internal BlingoSdlRootContext RootContext => _rootContext;

    public BlingoStage BlingoStage => _stage;

    internal void Init(BlingoStage stage)
    {
        _stage = stage;
    }

    internal void ShowMovie(SdlMovie movie)
    {
        _movies.Add(movie);
    }
    internal void HideMovie(SdlMovie movie)
    {
        _movies.Remove(movie);
    }

    public void SetActiveMovie(BlingoMovie? blingoMovie)
    {
        _activeMovie?.Hide();
        if (blingoMovie == null) { _activeMovie = null; return; }
        var movie = blingoMovie.Framework<SdlMovie>();
        if (_activeMovie == movie) return;
        _activeMovie = movie;

        if (_spritesTexture != nint.Zero)
            SDL.SDL_DestroyTexture(_spritesTexture);
        _spritesTexture = SDL.SDL_CreateTexture(_factory.RootContext.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, (int)_stage.Width, (int)_stage.Height);
        ((AbstSdlComponent)movie.FrameworkNode).ComponentContext.SetParents(ComponentContext);
        movie.Show();
    }

    public override void Dispose()
    {
        _movies.Clear();
        if (_spritesTexture != nint.Zero)
            SDL.SDL_DestroyTexture(_spritesTexture);
        base.Dispose();
    }

    public void ApplyPropertyChanges()
    {

    }
    public void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured)
    {
        onCaptured(GetScreenshot());
    }
    public IAbstTexture2D GetScreenshot()
    {
        var texture = new SdlTexture2D(_spritesTexture, (int)_stage.Width, (int)_stage.Height, "StageShot_" + _activeMovie!.CurrentFrame, _factory.RootContext.Renderer);
        var clone = (SdlTexture2D)texture.Clone();
#if DEBUG
       // clone.DebugWriteToDiskInc(_factory.RootContext.Renderer);
#endif
        return clone;
    }


    public void ShowTransition(IAbstTexture2D startTexture)
    {
        _startFrame?.Dispose();
        _startFrame = (SdlTexture2D)startTexture.Clone();
        _isTransitioning = true;
    }


    public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect)
    {
        _transitionFrame = (SdlTexture2D)texture;
        _transitionRect = targetRect;
#if DEBUG
      //  _transitionFrame.DebugWriteToDisk(_factory.RootContext.Renderer);
#endif
        Render();
    }


    public void HideTransition()
    {
        _isTransitioning = false;
        _transitionFrame?.Dispose();
        _transitionFrame = null;
        _startFrame?.Dispose();
        _startFrame = null;

#if DEBUG
        SdlTexture2D.ResetDebuggerInc();
#endif
    }

    private nint _lastTexture = nint.Zero;
    public nint LastTexture => _lastTexture;


    public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
    {
        if (!Visibility) return nint.Zero;
        var texture = Render();
        return texture;
    }
    internal nint Render()
    {
        if (_activeMovie == null) return nint.Zero;
        var context = _factory.CreateRenderContext(null, System.Numerics.Vector2.Zero);

        RenderSprites(context);

        nint frameTex = RenderToTexture(context.Renderer, (int)_stage.Width, (int)_stage.Height, () =>
        {
            if (_isTransitioning)
                RenderTransitionFrame(context);
            else
            {
                var full = new SDL.SDL_Rect { x = 0, y = 0, w = (int)_stage.Width, h =  (int)_stage.Height };
                SDL.SDL_RenderCopy(context.Renderer, _spritesTexture, IntPtr.Zero, ref full);
            }
        });
        if (_lastTexture != nint.Zero)
            SDL.SDL_DestroyTexture(_lastTexture);
        _lastTexture = frameTex;
        return frameTex;
    }

    private void RenderSprites(AbstUI.SDL2.Core.AbstSDLRenderContext context)
    {
        nint prev = SDL.SDL_GetRenderTarget(context.Renderer);
        SDL.SDL_SetRenderTarget(context.Renderer, _spritesTexture);
        SDL.SDL_SetRenderDrawColor(context.Renderer, _stage.BackgroundColor.R, _stage.BackgroundColor.G, _stage.BackgroundColor.B, _stage.BackgroundColor.A);
        SDL.SDL_RenderClear(context.Renderer);
        if (_activeMovie != null)
        {
            var childMovieContext = ((AbstSdlComponent)_activeMovie.FrameworkNode).ComponentContext;
            childMovieContext.RenderToTexture(context);
        }
        //_activeMovie!.RenderSprites(context);
        //_factory.ComponentContainer.Render(context);
        SDL.SDL_SetRenderTarget(context.Renderer, prev);
    }

    private void RenderTransitionFrame(AbstUI.SDL2.Core.AbstSDLRenderContext context)
    {
        if (_startFrame != null)
        {
            var full = new SDL.SDL_Rect { x = 0, y = 0, w = (int)_stage.Width, h = (int)_stage.Height };
            SDL.SDL_RenderCopy(context.Renderer, _startFrame.Handle, IntPtr.Zero, ref full);
        }
        if (_transitionFrame != null)
        {
            var src = new SDL.SDL_Rect
            {
                x = (int)_transitionRect.Left,
                y = (int)_transitionRect.Top,
                w = (int)_transitionRect.Width,
                h = (int)_transitionRect.Height
            };
            var dst = src;
            SDL.SDL_RenderCopy(context.Renderer, _transitionFrame.Handle, ref src, ref dst);
        }
    }



    /// Renders with 'draw' into a target texture and returns it.
    static nint RenderToTexture(nint renderer, int width, int height, Action draw)
    {
        nint tex = SDL.SDL_CreateTexture(renderer,
            SDL.SDL_PIXELFORMAT_RGBA8888,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
            width, height);
        if (tex == nint.Zero) throw new Exception(SDL.SDL_GetError());
        SDL.SDL_SetTextureBlendMode(tex, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

        nint prev = SDL.SDL_GetRenderTarget(renderer);
        SDL.SDL_SetRenderTarget(renderer, tex);

        draw(); // your scene rendering

        SDL.SDL_SetRenderTarget(renderer, prev);
        return tex;
    }

  
   
}

