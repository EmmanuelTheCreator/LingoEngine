namespace LingoEngine.Movies
{
    /// <summary>
    /// Sprite Channel
    /// Represents an individual sprite channel in the Score.
    /// A Sprite object covers a sprite span, which is a range of frames in a given sprite channel. A Sprite 
    /// Channel object represents an entire sprite channel, regardless of the number of sprites it contains.
    /// Sprite channels are controlled by the Score by default. Use the Sprite Channel object to switch
    /// control of a sprite channel over to script during a Score recording session.
    /// A sprite channel can be referenced either by number or by name.
    /// • When referring to a sprite channel by number, you access the channel directly.This method is
    /// faster than referring to a sprite channel by name.However, because Director does not
    /// automatically update references to sprite channel numbers in script, a numbered reference to a
    /// sprite channel that has moved position in the Score will be broken.
    /// • When referring to a sprite channel by name, Director searches all channels, starting from the
    /// lowest numbered channel, and retrieves the sprite channel’s data when it finds the named sprite
    /// channel.This method is slower than referring to a sprite channel by number, especially when
    /// referring to large movies that contain many cast libraries, cast members, and sprites. However,
    /// a named reference to a sprite channel allows the reference to remain intact even if the sprite
    /// channel moves position in the Score.
    /// </summary>
    public interface ILingoSpriteChannel
    {
        /// <summary>
        ///  identifies the name of a sprite channel. Read/write during a Score recording session only.
        ///  Set the name of a sprite channel during a Score recording session—between calls to the Movie object’s beginRecording() and endRecording() methods.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Returns the number of a sprite channel. 
        /// </summary>
        int Number { get; }

        /// <summary>
        /// specifies whether a sprite channel is under script control (TRUE) or under Score control(FALSE)
        /// </summary>
        bool Scripted { get; }

        /// <summary>
        /// Gets the sprite currently assigned to the sprite channel.
        /// </summary>
        ILingoSprite? Sprite { get; }

        /// <summary>
        /// Creates a scripted sprite that can be controlled by script.
        /// </summary>
        void MakeScriptedSprite(LingoMember? member = null, LingoPoint? loc = null);

        /// <summary>
        /// to switch control of the sprite channel back to the Score.
        /// </summary>
        void RemoveScriptedSprite();
    }

    public class LingoSpriteChannel : ILingoSpriteChannel
    {
        /// <inheritdoc/>
        public string Name { get; set; } = "";
        /// <inheritdoc/>
        public int Number { get; private set; }
        /// <inheritdoc/>
        public bool Scripted {get; private set; }
        /// <inheritdoc/>
        public ILingoSprite? Sprite { get; private set; }

        public LingoSpriteChannel(int number)
        {
            Number = number;
        }


        /// <inheritdoc/>
        public void MakeScriptedSprite(LingoMember? member = null, LingoPoint? loc = null)
        {
            Scripted = true;
        }
        /// <inheritdoc/>
        public void RemoveScriptedSprite()
        {
            Scripted = false;
        }
    }
}
