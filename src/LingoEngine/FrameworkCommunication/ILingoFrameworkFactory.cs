using LingoEngine;

namespace ArkGodot.DirectorProxy
{
    public interface ILingoFrameworkFactory
    {
        T CreateSprite<T>(ILingoScore score) where T : LingoSprite;
    }
}
