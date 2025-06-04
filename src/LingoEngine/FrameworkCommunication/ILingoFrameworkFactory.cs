using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
using LingoEngine.Sounds;
using LingoEngine.Texts;

namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkFactory
    {
        LingoStage CreateStage();


        #region Members
        T CreateMember<T>(ILingoCast cast, string name = "") where T : LingoMember;
        LingoMemberPicture CreateMemberPicture(ILingoCast cast, string name = "");
        LingoMemberSound CreateMemberSound(ILingoCast cast, string name = "");
        LingoMemberText CreateMemberText(ILingoCast cast, string name = ""); 
        #endregion

        LingoSound CreateSound(ILingoCastLibsContainer castLibsContainer);
        LingoSoundChannel CreateSoundChannel(int number);

        T CreateSprite<T>(ILingoMovie movie) where T : LingoSprite;
        T CreateBehavior<T>() where T : LingoSpriteBehavior;
        T CreateMovieScript<T>() where T : LingoMovieScript;
        
    }
}
