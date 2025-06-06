using LingoEngine.Primitives;

namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkSprite
    {
        bool Visibility { get; set; }
        float Blend { get; set; }
        float X { get; set; }
        float Y { get; set; }
        string Name { get; set; }
        LingoPoint RegPoint { get; set; }

       
        void MemberChanged();

        void RemoveMe();
        void Show();
        void Hide();
        void SetPosition(LingoPoint point);
    }
}
