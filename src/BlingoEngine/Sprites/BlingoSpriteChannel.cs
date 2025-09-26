using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AbstUI;
using AbstUI.Primitives;
using BlingoEngine.Casts;
using BlingoEngine.Members;
using BlingoEngine.Medias;
using BlingoEngine.Movies;
using BlingoEngine.Primitives;

namespace BlingoEngine.Sprites
{
    /// <summary>
    /// Sprite Channel.
    /// Represents an individual sprite channel in the Score.
    /// A <see cref="BlingoSprite2D"/> object covers a sprite span, which is a range of frames in a given sprite channel. A <see cref="BlingoSprite2D"/>
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
    public interface IBlingoSpriteChannel : IBlingoSprite
    {


        /// <summary>
        /// Returns the number of a sprite channel. 
        /// </summary>
        int Number { get; }

        /// <summary>
        /// specifies whether a sprite channel is under script control (TRUE) or under Score control(FALSE)
        /// </summary>
        bool Scripted { get; }

        /// <summary>
        /// Gets the <see cref="BlingoSprite2D"/> currently assigned to the sprite channel.
        /// </summary>
        IBlingoSprite? Sprite { get; }

        /// <summary>
        /// Has a loaded sprite in the timeline on this channel
        /// </summary>
        bool HasSprite();

        /// <summary>
        /// Creates a scripted sprite that can be controlled by script.
        /// </summary>
        /// <param name="member">Optional <see cref="BlingoMember"/> to assign to the new sprite.</param>
        /// <param name="loc">Optional <see cref="APoint"/> location.</param>
        void MakeScriptedSprite(BlingoMember? member = null, APoint? loc = null);

        /// <summary>
        /// to switch control of the sprite channel back to the Score.
        /// </summary>
        void RemoveScriptedSprite();
    }

    public class BlingoSpriteChannel : IBlingoSpriteChannel, IHasPropertyChanged
    {
        private bool _puppet;
        private readonly BlingoMovie _movie;
        private bool _visibility = true;
        private IBlingoSprite? _sprite;
        public event Action<BlingoSpriteChannel, bool>? VisibilityChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <inheritdoc/>
        /// <inheritdoc/>
        public int Number { get; private set; }
        /// <inheritdoc/>
        public bool Scripted { get; private set; }
        public bool Visibility
        {
            get => _visibility;
            set
            {
                if (_visibility == value) return; // avoid inifinty loop
                _visibility = value;
                if (_sprite != null) _sprite.Visibility = value;
                VisibilityChanged?.Invoke(this, value);
                OnPropertyChanged();
            }
        }
        bool IBlingoSpriteBase.IsPuppetCached { get; set; }

        #region Properties proxy

        // Let it crach when there is no sprite set.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public int BeginFrame { get => _sprite.BeginFrame; set => _sprite.BeginFrame = value; }

        public AColor BackColor { get => _sprite.BackColor; set => _sprite.BackColor = value; }
        public float Blend { get => _sprite.Blend; set => _sprite.Blend = value; }
        public BlingoCast? Cast => _sprite.Cast;
        public AColor Color { get => _sprite.Color; set => _sprite.Color = value; }
        public bool Editable { get => _sprite.Editable; set => _sprite.Editable = value; }
        public bool Lock { get => _sprite.Lock; set => _sprite.Lock = value; }
        public int EndFrame { get => _sprite.EndFrame; set => _sprite.EndFrame = value; }
        public AColor ForeColor { get => _sprite.ForeColor; set => _sprite.ForeColor = value; }
        public bool Hilite { get => _sprite.Hilite; set => _sprite.Hilite = value; }
        public int Ink { get => _sprite.Ink; set => _sprite.Ink = value; }
        public BlingoInkType InkType { get => _sprite.InkType; set => _sprite.InkType = value; }
        public bool Linked => _sprite.Linked;
        public bool Loaded => _sprite.Loaded;
        public byte[] Media { get => _sprite.Media; set => _sprite.Media = value; }
        public bool MediaReady => _sprite.MediaReady;
        public void Play() => _sprite.Play();
        public void Stop() => _sprite.Stop();
        public void Pause() => _sprite.Pause();
        public void Seek(int milliseconds) => _sprite.Seek(milliseconds);
        public int Duration => _sprite.Duration;
        public int CurrentTime { get => _sprite.CurrentTime; set => _sprite.CurrentTime = value; }
        public BlingoMediaStatus MediaStatus => _sprite.MediaStatus;
        public float Width { get => _sprite.Width; set => _sprite.Width = value; }
        public float Height { get => _sprite.Height; set => _sprite.Height = value; }
        public IBlingoMember? Member
        {
            get => _sprite.Member;
            set => _sprite.Member = value;
        }
        public string ModifiedBy { get => _sprite.ModifiedBy; set => _sprite.ModifiedBy = value; }
        public string Name { get => _sprite.Name; set => _sprite.Name = value; }
        public ARect Rect => _sprite.Rect;
        public APoint RegPoint { get => _sprite.RegPoint; set => _sprite.RegPoint = value; }
        public APoint Loc { get => _sprite.Loc; set => _sprite.Loc = value; }
        public float LocH { get => _sprite.LocH; set => _sprite.LocH = value; }
        public float LocV { get => _sprite.LocV; set => _sprite.LocV = value; }
        public int LocZ { get => _sprite.LocZ; set => _sprite.LocZ = value; }
        public float Skew { get => _sprite.Skew; set => _sprite.Skew = value; }
        public float Rotation { get => _sprite.Rotation; set => _sprite.Rotation = value; }
        public bool FlipH { get => _sprite.FlipH; set => _sprite.FlipH = value; }
        public bool FlipV { get => _sprite.FlipV; set => _sprite.FlipV = value; }
        public float Top { get => _sprite.Top; set => _sprite.Top = value; }
        public float Bottom { get => _sprite.Bottom; set => _sprite.Bottom = value; }
        public float Left { get => _sprite.Left; set => _sprite.Left = value; }
        public float Right { get => _sprite.Right; set => _sprite.Right = value; }
        public int Cursor { get => _sprite.Cursor; set => _sprite.Cursor = value; }
        public int Constraint { get => _sprite.Constraint; set => _sprite.Constraint = value; }
        public bool DirectToStage { get => _sprite.DirectToStage; set => _sprite.DirectToStage = value; }
        public List<string> ScriptInstanceList => _sprite.ScriptInstanceList;
        public int Size => _sprite.Size;
        public int SpriteNum => _sprite.SpriteNum;
        public byte[] Thumbnail { get => _sprite.Thumbnail; set => _sprite.Thumbnail = value; }

        public int MemberNum => _sprite.MemberNum;


        #endregion


        /// <inheritdoc/>
        public IBlingoSprite? Sprite => _sprite;

        public BlingoSpriteChannel(int number, BlingoMovie movie)
        {
            Number = number;
            _movie = movie;
        }



        public bool Puppet
        {
            get => _puppet;
            set
            {
                if (_puppet == value) return;
                _puppet = value;
                Scripted = value; // if puppet is set, we are scripted
                if (value)
                {
                    if (_sprite == null)
                    {
                        var spriteCached = _movie.Sprite2DManager.PuppetSpriteCacheTryGet(Number);
                        if (spriteCached != null)
                        {
                            _sprite = spriteCached;
                            _sprite.Puppet = true;
                        }
                        else
                        {
                            _sprite = _movie.AddSprite(Number, 1, _movie.FrameCount, 0, 0, c => c.Puppet = true);
                            if (_sprite is BlingoSprite2D sprite2D)
                                SetSprite(sprite2D);
                        }
                    }
                }
                else
                {
                    if (_sprite != null)
                    {
                        _puppet = false;
                        _sprite.Puppet = false;
                        _sprite.Member = null;
                        if (_sprite is BlingoSprite2D sprite2D)
                        {
                            sprite2D.Reset();
                            _movie.Sprite2DManager.PuppetSpriteCacheAdd(Number, sprite2D);
                            sprite2D.SpriteChannel = null;
                        }
                        _sprite = null;
                        //var typed = ((BlingoSprite)_sprite);
                        //typed.DoEndSprite();
                        //typed.RemoveMe();
                        ////_movie.RemoveSprite(_sprite);
                        //_sprite = null;
                    }
                }
                OnPropertyChanged();
            }
        }

        internal void SetSprite(BlingoSprite2D sprite)
        {
            _sprite = sprite;
            sprite.SpriteChannel = this;
            Visibility = sprite.Visibility;
        }
        internal void RemoveSprite()
        {
            _sprite = null;
        }


        /// <inheritdoc/>
        public void MakeScriptedSprite(BlingoMember? member = null, APoint? loc = null)
        {
            Scripted = true;
        }
        /// <inheritdoc/>
        public void RemoveScriptedSprite()
        {
            Scripted = false;
        }






        public IBlingoSprite SetMember(int memberNumber, int? castLibNum = null) => _sprite.SetMember(memberNumber, castLibNum);
        public IBlingoSprite SetMember(string memberName, int? castLibNum = null) => _sprite.SetMember(memberName, castLibNum);
        public IBlingoSprite SetMember(IBlingoMember? member) => _sprite.SetMember(member);
        public IBlingoSprite AddBehavior<T>(Action<T>? configure = null) where T : BlingoSpriteBehavior => _sprite.AddBehavior<T>(configure);
        public void SendToBack() => _sprite.SendToBack();
        public void BringToFront() => _sprite.BringToFront();
        public void MoveBackward() => _sprite.MoveBackward();
        public void MoveForward() => _sprite.MoveForward();
        public bool Intersects(IBlingoSprite other) => _sprite.Intersects(other);
        public bool Within(IBlingoSprite other) => _sprite.Within(other);
        public (APoint topLeft, APoint topRight, APoint bottomRight, APoint bottomLeft) Quad() => _sprite.Quad();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        public bool HasSprite() => _sprite != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}

