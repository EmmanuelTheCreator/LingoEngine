using LingoEngine.Core;
using LingoEngine.Primitives;

namespace LingoEngine.Movies
{
    /// <summary>
    /// Represents a sprite in the score with visual, timing, and behavioral properties.
    /// Mirrors Lingo’s sprite object functionality.
    /// </summary>
    public interface ILingoSprite
    {
        /// <summary>
        /// The frame number at which the sprite appears. Read/write.
        /// </summary>
        int BeginFrame { get; set; }

        /// <summary>
        /// Background color for the sprite. Read/write.
        /// </summary>
        LingoColor BgColor { get; set; }

        /// <summary>
        /// Specifies the blend percentage (0–100) of the sprite’s visibility. Read/write.
        /// </summary>
        float Blend { get; set; }

        /// <summary>
        /// Reference to the cast associated with this sprite. Read-only.
        /// </summary>
        LingoCast? Cast { get; }

        /// <summary>
        /// Foreground color tint of the sprite. Read/write.
        /// </summary>
        LingoColor Color { get; set; }

        /// <summary>
        /// Whether the sprite is editable by the user (e.g., for text input). Read/write.
        /// </summary>
        bool Editable { get; set; }

        /// <summary>
        /// The frame number at which the sprite stops displaying. Read/write.
        /// </summary>
        int EndFrame { get; set; }

        /// <summary>
        /// Foreground color of the sprite, often used in text. Read/write.
        /// </summary>
        LingoColor ForeColor { get; set; }

        /// <summary>
        /// Whether the sprite is highlighted. Read/write.
        /// </summary>
        bool Hilite { get; set; }

        /// <summary>
        /// The ink effect applied to the sprite. Read-only.
        /// </summary>
        int Ink { get; }

        /// <summary>
        /// Returns TRUE if the sprite’s cast member is linked to an external file. Read-only.
        /// </summary>
        bool Linked { get; }

        /// <summary>
        /// Returns TRUE if the sprite's media is fully loaded. Read-only.
        /// </summary>
        bool Loaded { get; }

       

        /// <summary>
        /// Identifies the specified cast member as a media byte array. Read/write.
        /// Use for copying or swapping media content at runtime.
        /// </summary>
        byte[] Media { get; set; }

        /// <summary>
        /// Returns TRUE if the sprite’s media is initialized and ready. Read-only.
        /// </summary>
        bool MediaReady { get; }
        float Width { get; }
        float Height { get; }

        /// <summary>
        /// Gets the cast member associated with this sprite. 
        /// </summary>
        ILingoMember? Member { get; set;  }

        /// <summary>
        /// Returns or sets the user or system who last modified the sprite.
        /// </summary>
        string ModifiedBy { get; set; }

        /// <summary>
        /// Returns or sets the name of the sprite.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The rectangular boundary of the sprite (top-left to bottom-right). Read/write.
        /// </summary>
        LingoRect Rect { get; }

        /// <summary>
        /// Specifies the registration point of a cast member. Read/write.
        /// </summary>
        LingoPoint RegPoint { get; set; }
        LingoPoint Loc { get; set; }
        /// <summary>
        /// Horizontal location of the sprite on the stage. Read/write.
        /// </summary>
        float LocH { get; set; }

        /// <summary>
        /// Vertical location of the sprite on the stage. Read/write.
        /// </summary>
        float LocV { get; set; }

        /// <summary>
        /// List of script instance names or types attached to the sprite.
        /// </summary>
        List<string> ScriptInstanceList { get; }

        /// <summary>
        /// Returns the size of the media in memory, in bytes. Read-only.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// The unique index number of the sprite in the score. Read-only.
        /// </summary>
        int SpriteNum { get; }

        /// <summary>
        /// Returns or sets a small thumbnail representation of the sprite’s media.
        /// </summary>
        byte[] Thumbnail { get; set; }

        /// <summary>
        /// Controls whether the sprite is visible on the Stage. Read/write.
        /// </summary>
        bool Visibility { get; set; } 
        int MemberNum { get; }
        

        /// <summary>
        /// Changes the cast member displayed by this sprite using the cast member number.
        /// </summary>
        /// <param name="memberNumber">The index of the cast member.</param>
        void SetMember(int memberNumber, int? castLibNum = null);

        /// <summary>
        /// Changes the cast member displayed by this sprite using the cast member name.
        /// </summary>
        /// <param name="memberName">The name of the cast member.</param>
        void SetMember(string memberName, int? castLibNum = null);
        void SetMember(ILingoMember? member);

        /// <summary>
        /// Sends the sprite to the back of the display order (lowest layer).
        /// </summary>
        void SendToBack();

        /// <summary>
        /// Brings the sprite to the front of the display order (topmost layer).
        /// </summary>
        void BringToFront();

        /// <summary>
        /// Moves the sprite one layer backward in the display order.
        /// </summary>
        void MoveBackward();

        /// <summary>
        /// Moves the sprite one layer forward in the display order.
        /// </summary>
        void MoveForward();

        bool Intersects(ILingoSprite other);
        bool Within(ILingoSprite other);
        (LingoPoint topLeft, LingoPoint topRight, LingoPoint bottomRight, LingoPoint bottomLeft) Quad();

    }
}
