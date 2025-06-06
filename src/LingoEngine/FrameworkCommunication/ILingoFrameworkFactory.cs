using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
using LingoEngine.Primitives;
using LingoEngine.Sounds;
using LingoEngine.Texts;

namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkFactory
    {
        LingoStage CreateStage(LingoClock lingoClock);
        LingoMovie AddMovie(LingoStage stage, LingoMovie lingoMovie);


        #region Members
        T CreateMember<T>(ILingoCast cast, string name = "") where T : LingoMember;
        LingoMemberPicture CreateMemberPicture(ILingoCast cast, string name = "", string? fileName = null, 
            LingoPoint regPoint = default);
        LingoMemberSound CreateMemberSound(ILingoCast cast, string name = "", string? fileName = null, LingoPoint regPoint = default);
        LingoMemberText CreateMemberText(ILingoCast cast, string name = "", string? fileName = null,
            LingoPoint regPoint = default);
        LingoMember CreateEmpty(ILingoCast cast, string name = "", string? fileName = null,
            LingoPoint regPoint = default);
        #endregion

        LingoSound CreateSound(ILingoCastLibsContainer castLibsContainer);
        LingoSoundChannel CreateSoundChannel(int number);

        T CreateSprite<T>(ILingoMovie movie, Action<LingoSprite> onRemoveMe) where T : LingoSprite;
        T CreateBehavior<T>() where T : LingoSpriteBehavior;
        T CreateMovieScript<T>() where T : LingoMovieScript;
        
    }
}
