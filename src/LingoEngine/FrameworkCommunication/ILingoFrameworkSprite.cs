using System.Numerics;

namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkSprite
    {
        bool Visibility { get; set; }
        float Blend { get; set; }
        float X { get; set; }
        float Y { get; set; }
        string Name { get; set; }

        Vector2 GetGlobalMousePosition();
        void MemberChanged();
        void SetPositionX(float x);
        void SetPositionY(float y);
    }
}
