using BlingoEngine.Primitives;
using BlingoEngine.Members;
using BlingoEngine.Animations;
using BlingoEngine.Bitmaps;
using AbstUI.Primitives;

namespace BlingoEngine.Sprites
{
    /// <summary>
    /// Lingo Sprite2 DLight interface.
    /// </summary>
    public interface IBlingoSprite2DLight
    {
        AColor BackColor { get; set; }
        float Blend { get; set; }
        int Constraint { get; set; }
        int DisplayMember { get; set; }
        bool FlipH { get; set; }
        bool FlipV { get; set; }
        AColor ForeColor { get; set; }
        float Height { get; set; }
        bool Hilite { get; set; }
        int Ink { get; set; }
        BlingoInkType InkType { get; set; }
        bool Linked { get; }
        bool Loaded { get; }
        APoint Loc { get; set; }
        float LocH { get; set; }
        float LocV { get; set; }
        int LocZ { get; set; }
        IBlingoMember? Member { get; set; }
        int MemberNum { get; set; }
        string Name { get; set; }
        ARect Rect { get; }
        APoint RegPoint { get; set; }
        float Rotation { get; set; }
        float Skew { get; set; }
        int SpriteNumWithChannel { get; }
        float Width { get; set; }
        int SpriteNum { get; }

        void AddKeyframes(params BlingoKeyFrameSetting[] keyframes);
        string GetFullName();
        void SetSpriteTweenOptions(bool positionEnabled, bool sizeEnabled, bool rotationEnabled, bool skewEnabled, bool foregroundColorEnabled, bool backgroundColorEnabled, bool blendEnabled, float curvature, bool continuousAtEnds, bool speedSmooth, float easeIn, float easeOut);
        void UpdateKeyframe(BlingoKeyFrameSetting setting);
        void UpdateTexture(IAbstTexture2D texture);


        IReadOnlyCollection<BlingoKeyFrameSetting>? GetKeyframes();
        BlingoKeyFrameSetting? GetKeyFrameForFrame(int frame);
        void MoveKeyFrame(int from, int to);
        bool DeleteKeyFrame(int frame);

    }
}

