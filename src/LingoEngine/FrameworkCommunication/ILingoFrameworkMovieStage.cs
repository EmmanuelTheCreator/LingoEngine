
using LingoEngine.Movies;

namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkStage
    {
        void DrawSprite(LingoSprite sprite);
        void RemoveSprite(LingoSprite sprite);
        void UpdateStage();
    }
}
