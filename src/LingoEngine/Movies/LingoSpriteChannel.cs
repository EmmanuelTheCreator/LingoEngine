using LingoEngine.Core;
using LingoEngine.Primitives;

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
    public interface ILingoSpriteChannel: ILingoSprite
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
        private bool _visibility = true;
        private ILingoSprite? _sprite;
        /// <inheritdoc/>
        /// <inheritdoc/>
        public int Number { get; private set; }
        /// <inheritdoc/>
        public bool Scripted {get; private set; }
        public bool Visibility { get => _visibility; set
            {
                if (_visibility == value) return; // avoid inifinty loop   
                _visibility = value;
                if (_sprite != null) _sprite.Visibility = value;
            }
        }
        /// <inheritdoc/>
        public ILingoSprite? Sprite => _sprite;

        public LingoSpriteChannel(int number)
        {
            Number = number;
        }
        internal void SetSprite(LingoSprite sprite)
        {
            _sprite = sprite;
            sprite.SpriteChannel = this;
            _sprite.Visibility = _visibility;
        }
        internal void RemoveSprite()
        {
            _sprite = null;
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


        // Let it crach when there is no sprite set.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public int BeginFrame { get => _sprite.BeginFrame; set => _sprite.BeginFrame = value; }

        public LingoColor BackColor { get => _sprite.BackColor; set => _sprite.BackColor = value; }
        public float Blend { get => _sprite.Blend; set => _sprite.Blend = value; }
        public LingoCast? Cast => _sprite.Cast;
        public LingoColor Color { get => _sprite.Color; set => _sprite.Color = value; }
        public bool Editable { get => _sprite.Editable; set => _sprite.Editable = value; }
        public int EndFrame { get => _sprite.EndFrame; set => _sprite.EndFrame = value; }
        public LingoColor ForeColor { get => _sprite.ForeColor; set => _sprite.ForeColor = value; }
        public bool Hilite { get => _sprite.Hilite; set => _sprite.Hilite = value; }
        public int Ink => _sprite.Ink;
        public bool Linked => _sprite.Linked;
        public bool Loaded => _sprite.Loaded;
        public byte[] Media { get => _sprite.Media; set => _sprite.Media = value; }
        public bool MediaReady => _sprite.MediaReady;
        public float Width => _sprite.Width;
        public float Height => _sprite.Height;
        public ILingoMember? Member { 
            get => _sprite.Member; 
            set => _sprite.Member = value; }
        public string ModifiedBy { get => _sprite.ModifiedBy; set => _sprite.ModifiedBy = value; }
        public string Name { get => _sprite.Name; set => _sprite.Name = value; }
        public LingoRect Rect => _sprite.Rect;
        public LingoPoint RegPoint { get => _sprite.RegPoint; set => _sprite.RegPoint = value; }
        public LingoPoint Loc { get => _sprite.Loc; set => _sprite.Loc = value; }
        public float LocH { get => _sprite.LocH; set => _sprite.LocH = value; }
        public float LocV { get => _sprite.LocV; set => _sprite.LocV = value; }
        public List<string> ScriptInstanceList => _sprite.ScriptInstanceList;
        public int Size => _sprite.Size;
        public int SpriteNum => _sprite.SpriteNum;
        public byte[] Thumbnail { get => _sprite.Thumbnail; set => _sprite.Thumbnail = value; }
        
        public int MemberNum => _sprite.MemberNum;

        public void SetMember(int memberNumber, int? castLibNum = null) => _sprite.SetMember(memberNumber, castLibNum);
        public void SetMember(string memberName, int? castLibNum = null) => _sprite.SetMember(memberName, castLibNum);
        public void SetMember(ILingoMember? member) => _sprite.SetMember(member);
        public void SendToBack() => _sprite.SendToBack();
        public void BringToFront() => _sprite.BringToFront();
        public void MoveBackward() => _sprite.MoveBackward();
        public void MoveForward() => _sprite.MoveForward();
        public bool Intersects(ILingoSprite other) => _sprite.Intersects(other);
        public bool Within(ILingoSprite other) => _sprite.Within(other);
        public (LingoPoint topLeft, LingoPoint topRight, LingoPoint bottomRight, LingoPoint bottomLeft) Quad() => _sprite.Quad();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
