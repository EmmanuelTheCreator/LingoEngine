using LingoEngine.Primitives;

namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkSprite
    {
        bool Visibility { get; set; }
        float Blend { get; set; }
        float X { get; set; }
        float Y { get; set; }
        float Width { get; }
        float Height { get;  }
        string Name { get; set; }
        LingoPoint RegPoint { get; set; }
        float SetDesiredHeight { get; set; }
        float SetDesiredWidth { get; set; }
        int ZIndex { get; set; }

        void MemberChanged();

        void RemoveMe();
        void Show();
        void Hide();
        void SetPosition(LingoPoint point);
    }
}
