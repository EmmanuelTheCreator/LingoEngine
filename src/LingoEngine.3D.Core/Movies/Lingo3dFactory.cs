using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Pictures;
using LingoEngine.Primitives;
using LingoEngine.Sounds;
using LingoEngine.Shapes;
using LingoEngine.Texts;
using LingoEngine.Gfx;

namespace LingoEngine._3D;

/// <summary>
/// Simple factory that delegates to an underlying <see cref="ILingoFrameworkFactory"/>
/// while exposing helpers for 3D specific sprites and movies.
/// </summary>
public class Lingo3dFactory : ILingoFrameworkFactory
{
    private readonly ILingoFrameworkFactory _baseFactory;

    public Lingo3dFactory(ILingoFrameworkFactory baseFactory)
    {
        _baseFactory = baseFactory;
    }

    public LingoStage CreateStage(LingoPlayer lingoPlayer) => _baseFactory.CreateStage(lingoPlayer);
    public LingoMovie AddMovie(LingoStage stage, LingoMovie lingoMovie) => _baseFactory.AddMovie(stage, lingoMovie);
    public T CreateMember<T>(ILingoCast cast, int numberInCast, string name = "") where T : LingoMember => _baseFactory.CreateMember<T>(cast, numberInCast, name);
    public LingoMemberPicture CreateMemberPicture(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default) => _baseFactory.CreateMemberPicture(cast, numberInCast, name, fileName, regPoint);
    public LingoMemberSound CreateMemberSound(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default) => _baseFactory.CreateMemberSound(cast, numberInCast, name, fileName, regPoint);
    public LingoMemberFilmLoop CreateMemberFilmLoop(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default) => _baseFactory.CreateMemberFilmLoop(cast, numberInCast, name, fileName, regPoint);
    public LingoMemberShape CreateMemberShape(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default) => _baseFactory.CreateMemberShape(cast, numberInCast, name, fileName, regPoint);
    public LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default) => _baseFactory.CreateMemberField(cast, numberInCast, name, fileName, regPoint);
    public LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default) => _baseFactory.CreateMemberText(cast, numberInCast, name, fileName, regPoint);
    public LingoMember CreateEmpty(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default) => _baseFactory.CreateEmpty(cast, numberInCast, name, fileName, regPoint);
    public LingoSound CreateSound(ILingoCastLibsContainer castLibsContainer) => _baseFactory.CreateSound(castLibsContainer);
    public LingoSoundChannel CreateSoundChannel(int number) => _baseFactory.CreateSoundChannel(number);
    public LingoMouse CreateMouse(LingoStage stage) => _baseFactory.CreateMouse(stage);
    public LingoKey CreateKey() => _baseFactory.CreateKey();
    public LingoGfxCanvas CreateGfxCanvas(int width, int height) => _baseFactory.CreateGfxCanvas(width, height);
    public LingoWrapPanel CreateWrapPanel(LingoOrientation orientation) => _baseFactory.CreateWrapPanel(orientation);
    public LingoPanel CreatePanel() => _baseFactory.CreatePanel();
    public LingoGfxTabContainer CreateTabContainer() => _baseFactory.CreateTabContainer();
    public LingoInputText CreateInputText(int maxLength = 0) => _baseFactory.CreateInputText(maxLength);
    public LingoInputNumber CreateInputNumber(float min = 0, float max = 100) => _baseFactory.CreateInputNumber(min, max);
    public LingoInputCheckbox CreateInputCheckbox() => _baseFactory.CreateInputCheckbox();
    public LingoInputCombobox CreateInputCombobox() => _baseFactory.CreateInputCombobox();
    public LingoLabel CreateLabel(string text = "") => _baseFactory.CreateLabel(text);
    public T CreateSprite<T>(ILingoMovie movie, Action<LingoSprite> onRemoveMe) where T : LingoSprite => _baseFactory.CreateSprite<T>(movie, onRemoveMe);
    public T CreateBehavior<T>(LingoMovie lingoMovie) where T : LingoSpriteBehavior => _baseFactory.CreateBehavior<T>(lingoMovie);
    public T CreateMovieScript<T>(LingoMovie lingoMovie) where T : LingoMovieScript => _baseFactory.CreateMovieScript<T>(lingoMovie);
}
