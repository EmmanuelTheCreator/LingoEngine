using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
using LingoEngine.Sounds;

namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkFactory
    {
        LingoStage CreateStage();
        LingoMemberPicture CreateMemberPicture(int number, string name = "");

        LingoMemberSound CreateMemberSound(int number, string name = "");
        LingoSound CreateSound();
        LingoSoundChannel CreateSoundChannel(int number);
        T CreateSprite<T>(ILingoMovie movie) where T : LingoSprite;
        LingoSpriteBehavior CreateBehavior<T>() where T : LingoSpriteBehavior;
    }
}
