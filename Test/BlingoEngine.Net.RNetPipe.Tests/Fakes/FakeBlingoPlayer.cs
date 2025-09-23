using System;
using System.Threading;
using System.Threading.Tasks;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sounds;
using BlingoEngine.Stages;

namespace BlingoEngine.Net.RNetPipe.Tests.Fakes;

internal sealed class FakeBlingoPlayer : IBlingoPlayer
{
    private IBlingoMovie? _activeMovie;

    public IBlingoCast ActiveCastLib => throw new NotSupportedException();

    public IBlingoMovie? ActiveMovie => _activeMovie;

    public event Action<IBlingoMovie?>? ActiveMovieChanged;

    public void SetActiveMovie(IBlingoMovie? movie)
    {
        _activeMovie = movie;
        ActiveMovieChanged?.Invoke(movie);
    }

    public IBlingoSound Sound => throw new NotSupportedException();

    public bool MediaRequiresAsyncPreload { get; set; }

    public int CurrentSpriteNum => 0;

    public bool NetPreset => false;

    public bool ActiveWindow => false;

    public bool SafePlayer { get; set; }

    public string OrganizationName { get; set; } = string.Empty;

    public string ApplicationName { get; set; } = string.Empty;

    public string ApplicationPath { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int LastClick => 0;

    public int LastEvent => 0;

    public int LastKey => 0;

    public Version ProductVersion { get; set; } = new(1, 0);

    public IBlingoCastLibsContainer CastLibs => throw new NotSupportedException();

    public IBlingoMember? GetMember(BlingoMemberRef memberRef) => throw new NotSupportedException();

    public T? GetMember<T>(BlingoMemberRef memberRef) where T : class, IBlingoMember => throw new NotSupportedException();

    public IBlingoCast? GetCast(BlingoCastRef castRef) => throw new NotSupportedException();

    public Func<string> AlertHook { get; set; } = () => string.Empty;

    public IBlingoStage Stage => throw new NotSupportedException();

    public void Alert(string message)
    {
    }

    public void AppMinimize()
    {
    }

    public void Halt()
    {
    }

    public void Cursor(int cursorNum)
    {
    }

    public void Open(string applicationName)
    {
    }

    public void Quit()
    {
    }

    public bool WindowPresent() => false;

    public IBlingoCast CastLib(int number) => throw new NotSupportedException();

    public IBlingoCast CastLib(string name) => throw new NotSupportedException();

    public IBlingoPlayer LoadCastLibFromCsv(string castlibName, string pathAndFilenameToCsv, bool isInternal = false) => this;

    public Task<IBlingoPlayer> LoadAsync<TBlingoCastLibBuilder>() where TBlingoCastLibBuilder : class, IBlingoCastLibBuilder, new()
        => Task.FromResult<IBlingoPlayer>(this);

    public Task<IBlingoPlayer> LoadCastLibFromCsvAsync(string castlibName, string pathAndFilenameToCsv, bool isInternal = false)
        => Task.FromResult<IBlingoPlayer>(this);

    public IBlingoPlayer AddCastLib(string name, bool isInternal = false, Action<IBlingoCast>? configure = null) => this;

    public IBlingoMovie NewMovie(string movieName, bool andActivate = true) => throw new NotSupportedException();

    public IBlingoMovie? GetMovie(BlingoMovieRef movieRef) => null;

    public Task<IBlingoMovie> LoadMovieAsync(IBlingoMovieBuilder builder) => throw new NotSupportedException();

    public void RunDelayed(Action action, int milliseconds, CancellationTokenSource? cts = null) => action();

    public Task RunOnUIThreadAsync(Action action, CancellationToken ct = default)
    {
        action();
        return Task.CompletedTask;
    }

    public Task<T> RunOnUIThreadAsync<T>(Func<T> func, CancellationToken ct = default)
        => Task.FromResult(func());

    public Task<T> RunOnUIThreadAsync<T>(Func<Task<T>> func, CancellationToken ct = default)
        => func();

    public void RunOnUIThread(Action action) => action();
}
