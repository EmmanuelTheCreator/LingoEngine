using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Primitives;
using LingoEngine.SDL2;
using LingoEngine.SDL2.SDLL;

namespace LingoEngine.SDL2.Movies;

public class SdlMovie : ILingoFrameworkMovie, IDisposable
{
    private readonly SdlStage _stage;
    private readonly Action<SdlMovie> _removeMethod;
    private readonly HashSet<SdlSprite> _drawnSprites = new();
    private readonly HashSet<SdlSprite> _allSprites = new();
    private LingoMovie _movie;

    public SdlMovie(SdlStage stage, LingoMovie movie, Action<SdlMovie> removeMethod)
    {
        _stage = stage;
        _movie = movie;
        _removeMethod = removeMethod;
        stage.ShowMovie(this);
    }

    internal void Show() => _stage.ShowMovie(this);
    internal void Hide() => _stage.HideMovie(this);

    public void UpdateStage()
    {
        var renderer = _stage.RootContext.Renderer;
        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
        SDL.SDL_RenderClear(renderer);
        foreach (var s in _drawnSprites)
        {
            s.Update();
            s.Render(renderer);
        }
        SDL.SDL_RenderPresent(renderer);
    }

    internal void CreateSprite<T>(T lingoSprite) where T : LingoSprite
    {
        var sprite = new SdlSprite(lingoSprite, _stage.RootContext.Renderer,
            s => _drawnSprites.Add(s),
            s => _drawnSprites.Remove(s),
            s => { _drawnSprites.Remove(s); _allSprites.Remove(s); });
        _allSprites.Add(sprite);
    }

    public void RemoveMe()
    {
        _removeMethod(this);
    }

    LingoPoint ILingoFrameworkMovie.GetGlobalMousePosition() => (0, 0);

    public void Dispose()
    {
        Hide();
        RemoveMe();
    }
}
