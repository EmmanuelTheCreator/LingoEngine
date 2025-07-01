using LingoEngine.Primitives;

namespace LingoEngine.Sprites
{
    /// <summary>
    /// Defines sprite functionality required by the engine. Implemented by
    /// rendering back-ends to represent a sprite on screen.
    /// </summary>
    public interface ILingoFrameworkSprite
    {
        /// <summary>Controls the sprite's visibility.</summary>
        bool Visibility { get; set; }
        float Blend { get; set; }
        float X { get; set; }
        float Y { get; set; }
        float Width { get; }
        float Height { get; }
        string Name { get; set; }
        LingoPoint RegPoint { get; set; }
        float SetDesiredHeight { get; set; }
        float SetDesiredWidth { get; set; }
        int ZIndex { get; set; }

        /// <summary>Rotation of the sprite in degrees.</summary>
        float Rotation { get; set; }

        /// <summary>Horizontal skew angle of the sprite in degrees.</summary>
        float Skew { get; set; }

        // <summary>Vertical skew angle of the sprite in degrees.</summary>
        //float SkewY { get; set; }

        /// <summary>Notify that the underlying member changed.</summary>
        void MemberChanged();

        /// <summary>Remove the sprite from the stage.</summary>
        void RemoveMe();
        /// <summary>Show the sprite.</summary>
        void Show();
        /// <summary>Hide the sprite.</summary>
        void Hide();
        /// <summary>Set the sprite position.</summary>
        void SetPosition(LingoPoint point);

        /// <summary>
        /// Indicates whether the sprite is flipped horizontally.
        /// </summary>
        bool FlipH { get; set; }

        /// <summary>
        /// Indicates whether the sprite is flipped vertically.
        /// </summary>
        bool FlipV { get; set; }

        /// <summary>
        /// Draw this sprite directly to the stage, bypassing the score composition.
        /// </summary>
        bool DirectToStage { get; set; }

        /// <summary>
        /// Ink effect to apply when rendering the sprite.
        /// </summary>
        int Ink { get; set; }
    }
}
