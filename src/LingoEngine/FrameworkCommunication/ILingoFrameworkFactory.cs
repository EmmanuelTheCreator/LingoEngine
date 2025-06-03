using LingoEngine;
using LingoEngine.Sounds;

namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkFactory
    {
        LingoMemberSound CreateMemberSound(int number, string name = "");
        LingoSound CreateSound();
        LingoSoundChannel CreateSoundChannel(int number);
        T CreateSprite<T>(ILingoScore score) where T : LingoSprite;
    }
}
