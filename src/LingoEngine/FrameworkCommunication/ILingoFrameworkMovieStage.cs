
namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkMovieStage
    {
        void DrawSprite(LingoSprite sprite);
        void RemoveSprite(LingoSprite sprite);
        void UpdateStage(List<LingoSprite> activeSprites);
    }
}
