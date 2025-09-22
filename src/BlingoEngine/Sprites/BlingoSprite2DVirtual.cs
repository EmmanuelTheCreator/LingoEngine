using AbstUI.Primitives;
using BlingoEngine.Animations;
using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.Events;
using BlingoEngine.FilmLoops;
using BlingoEngine.Members;
using BlingoEngine.Primitives;
using System.Diagnostics;

namespace BlingoEngine.Sprites
{
    [DebuggerDisplay("Sprite2DVirtual:{SpriteNum}) {Name}:Member={Member?.Name}:{BeginFrame}->{EndFrame}:Pos={LocH}x{LocV}:Size={Width}x{Height}")]
    public class BlingoSprite2DVirtual : BlingoSprite, IBlingoSprite2DLight, IBlingoSpriteWithMember
    {
        public const int SpriteNumOffset = 6;
        private readonly IBlingoSpritesPlayer _spritesPlayer;
        private int _ink;
        private float _width;
        private float _height;
        private BlingoMember? _Member;
        private Action<BlingoSprite2DVirtual>? _onRemoveMe;
        private int _constraint;
        private IAbstUITextureUserSubscription? _textureSubscription;


        #region Properties
        public override int SpriteNumWithChannel => SpriteNum + SpriteNumOffset;



        public override string Name { get; set; } = "";
        public int MemberNum
        {
            get => Member?.NumberInCast ?? 0;
            set
            {
                if (Member != null)
                    //Member = Member.GetMemberInCastByOffset(value);
                    Member = Member.Cast.Member[value];
            }
        }
        /// <summary>Channel default cast member.</summary>
        public int DisplayMember { get; set; }



        public BlingoInkType InkType { get => (BlingoInkType)_ink; set => Ink = (int)value; }
        public int Ink
        {
            get => _ink;
            set
            {
                _ink = value;
            }
        }

        public bool Hilite { get; set; }
        public bool Linked { get; private set; }
        public bool Loaded { get; private set; }
        public float Blend { get; set; } = 100f;
        public float LocH { get; set; }
        public float LocV { get; set; }
        public int LocZ { get; set; }
        public APoint Loc
        {
            get => (LocH, LocV); set
            {
                LocH = value.X;
                LocV = value.Y;
            }
        }

        public float Rotation { get; set; }
        public float Skew { get; set; }
        public bool FlipH { get; set; }
        public bool FlipV { get; set; }
        public int Constraint { get => _constraint; set => _constraint = value; }

        public APoint RegPoint { get; set; }
        public AColor ForeColor { get; set; } = AColors.Black;
        public AColor BackColor { get; set; } = AColors.White;



        public IBlingoMember? Member { get => _Member; set => SetMember(value); }

        public ARect Rect
        {
            get
            {
                var offset = GetRegPointOffset();
                float left = LocH - offset.X - Width / 2f;
                float top = LocV - offset.Y - Height / 2f;
                return new ARect(left, top, left + Width, top + Height);
            }
        }


        public float Width
        {
            get => _width;
            set => _width = value;
        }
        public float Height { get => _height; set => _height = value; }
        public IAbstTexture2D? Texture { get; private set; }

        public IBlingoMember? GetMember() => Member;


        #endregion


        private APoint GetRegPointOffset()
        {
            if (_Member is { } member)
            {
                var baseOffset = member.CenterOffsetFromRegPoint();
                if (member.Width != 0 && member.Height != 0)
                {
                    float scaleX = Width / member.Width;
                    float scaleY = Height / member.Height;
                    return new APoint(baseOffset.X * scaleX, baseOffset.Y * scaleY);
                }
                return baseOffset;
            }
            return new APoint();
        }


        public BlingoSprite2DVirtual(IBlingoEventMediator eventMediator, IBlingoSpritesPlayer spritesPlayer)
            : base(eventMediator)
        {
            _spritesPlayer = spritesPlayer;
        }

        public BlingoSprite2DVirtual(IBlingoEventMediator eventMediator, IBlingoSpritesPlayer spritesPlayer, BlingoSprite2D sp)
            : base(eventMediator)
        {
            _spritesPlayer = spritesPlayer;
            Name = sp.Name;
            LocH = sp.LocH;
            LocV = sp.LocV;
            LocZ = sp.LocZ;
            Rotation = sp.Rotation;
            Skew = sp.Skew;
            FlipH = sp.FlipH;
            FlipV = sp.FlipV;
            RegPoint = sp.RegPoint;
            ForeColor = sp.ForeColor;
            BackColor = sp.BackColor;
            Width = sp.Width;
            Height = sp.Height;
            InkType = sp.InkType;
            DisplayMember = sp.DisplayMember;
            SpriteNum = sp.SpriteNum;
            Hilite = sp.Hilite;
            SetMember(sp.Member);
        }

        public BlingoSprite2DVirtual(IBlingoEventMediator eventMediator, IBlingoSpritesPlayer spritesPlayer, BlingoFilmLoopMemberSprite sp, IBlingoCastLibsContainer castLibs) : this(eventMediator, spritesPlayer)
        {
            sp.LinkMember(castLibs);
            SetMember(sp.Member);
            Name = sp.Name;
            LocH = sp.LocH;
            LocV = sp.LocV;
            LocZ = sp.LocZ;
            Rotation = sp.Rotation;
            Skew = sp.Skew;
            FlipH = sp.FlipH;
            FlipV = sp.FlipV;
            RegPoint = sp.RegPoint;
            ForeColor = sp.ForeColor;
            BackColor = sp.BackColor;
            Width = sp.Width;
            Height = sp.Height;
            InkType = sp.InkType;
            DisplayMember = sp.DisplayMember;
            SpriteNum = sp.SpriteNum;
            Hilite = sp.Hilite;
        }
        public BlingoSpriteAnimatorProperties GetAnimatorProperties() => GetAnimator().Properties;


        #region Animator / Keyframes
        public IReadOnlyCollection<BlingoKeyFrameSetting>? GetKeyframes()
        {
            var animator = GetActorsOfType<BlingoSpriteAnimator>().FirstOrDefault();
            if (animator == null)
                return null;
            return animator.GetKeyframes();
        }

        public BlingoKeyFrameSetting? GetKeyFrameForFrame(int frame)
        {
            var animator = GetActorsOfType<BlingoSpriteAnimator>().FirstOrDefault();
            return animator?.GetKeyFrameForFrame(frame);
        }

        public void MoveKeyFrame(int from, int to)
        {
            var animator = GetActorsOfType<BlingoSpriteAnimator>().FirstOrDefault();
            if (animator == null)
                return;
            animator.MoveKeyFrame(from, to);
        }
        public bool DeleteKeyFrame(int frame)
        {
            var animator = GetActorsOfType<BlingoSpriteAnimator>().FirstOrDefault();
            if (animator == null)
                return false;
            return animator.DeleteKeyFrame(frame);
        }
        /// <summary>
        /// Adds animation keyframes for this sprite. When invoked for the first time
        /// it lazily creates a <see cref="BlingoSpriteAnimator"/> actor and stores it
        /// in the internal sprite actors list.
        /// </summary>
        /// <param name="keyframes">Tuple list containing frame number, X, Y, rotation and skew values.</param>
        public void AddKeyframes(params BlingoKeyFrameSetting[] keyframes)
        {
            if (keyframes == null || keyframes.Length == 0)
                return;
            GetAnimator().AddKeyFrames(keyframes);
        }



        public void UpdateKeyframe(BlingoKeyFrameSetting setting)
        {
            var animator = GetAnimator();
            animator.UpdateKeyFrame(setting);
            animator.RecalculateCache();
        }

        public void SetSpriteTweenOptions(bool positionEnabled, bool sizeEnabled, bool rotationEnabled, bool skewEnabled,
            bool foregroundColorEnabled, bool backgroundColorEnabled, bool blendEnabled,
            float curvature, bool continuousAtEnds, bool speedSmooth, float easeIn, float easeOut)
        {
            var animator = GetAnimator();
            animator.SetTweenOptions(positionEnabled, sizeEnabled, rotationEnabled, skewEnabled,
                foregroundColorEnabled, backgroundColorEnabled, blendEnabled,
                curvature, continuousAtEnds, speedSmooth, easeIn, easeOut);
        }
        public BlingoSpriteAnimator GetAnimator(BlingoSpriteAnimatorProperties? animatorProperties = null)
        {
            var animator = GetActorsOfType<BlingoSpriteAnimator>().FirstOrDefault();
            if (animator == null)
            {
                animator = new BlingoSpriteAnimator(this, _spritesPlayer, _eventMediator, animatorProperties, true);
                AddActor(animator);
            }

            return animator;
        }

        #endregion


        public ARect GetBoundingBox()
        {
            var animator = GetActorsOfType<BlingoSpriteAnimator>().FirstOrDefault();
            if (animator != null)
                return animator.GetBoundingBox();

            return Rect;
        }
        public ARect GetBoundingBoxForFrame(int frame)
        {
            var animator = GetActorsOfType<BlingoSpriteAnimator>().FirstOrDefault();
            if (animator != null)
                return animator.GetBoundingBoxForFrame(frame);

            return Rect;
        }


        public bool PointInSprite(APoint point)
        {
            return Rect.Contains(point);
        }


        public void SetMember(IBlingoMember? member)
        {
            if (_Member == member) return;
            if (_Member != null && (_Member.Type == BlingoMemberType.Script || _Member.Type == BlingoMemberType.Sound || _Member.Type == BlingoMemberType.Transition || _Member.Type == BlingoMemberType.Unknown || _Member.Type == BlingoMemberType.Palette || _Member.Type == BlingoMemberType.Movie || _Member.Type == BlingoMemberType.Font || _Member.Type == BlingoMemberType.Cursor))
                return;
            // Release the old member link with this sprite
            if (member != _Member)
            {
                _Member?.ReleaseFromRefUser(this);
            }
            _Member = member as BlingoMember;
            if (_Member != null)
            {
                _Member.UsedBy(this);
                RegPoint = _Member.RegPoint;
                _Member.Preload();
                if (Width == 0 || Height == 0)
                {
                    Width = _Member.Width;
                    Height = _Member.Height;
                }
            }
        }



        #region ZIndex/locZ
        public void SendToBack()
        {
            LocZ = 1;
        }

        public void BringToFront()
        {
            var maxZ = _spritesPlayer.GetMaxLocZ();
            LocZ = maxZ + 1;
        }

        public void MoveBackward()
        {
            LocZ--;
        }

        public void MoveForward()
        {
            LocZ++;
        }
        #endregion



        #region Math methods

        public bool Intersects(IBlingoSprite other)
        {
            return Rect.Intersects(other.Rect);
        }

        public bool Within(IBlingoSprite other)
        {
            var center = Rect.Center;
            return other.Rect.Contains(center);
        }

        public (APoint topLeft, APoint topRight, APoint bottomRight, APoint bottomLeft) Quad()
        {
            var rect = Rect;
            return (
                rect.TopLeft,
                new APoint(rect.Right, rect.Top),
                rect.BottomRight,
                new APoint(rect.Left, rect.Bottom)
            );
        }

        #endregion



        /// <summary>
        /// Check if the given point is inside the bounding box of the sprite
        /// </summary>
        public bool IsPointInsideBoundingBox(float x, float y)
            => Rect.Contains((x, y));




        public override void OnRemoveMe()
        {
            _textureSubscription?.Release();
            _textureSubscription = null;
            Texture = null;
            _Member?.ReleaseFromRefUser(this);
            if (_onRemoveMe != null)
                _onRemoveMe(this);
        }

        public void MemberHasBeenRemoved()
        {
            _textureSubscription?.Release();
            _textureSubscription = null;
            Texture = null;
            _Member = null;
        }

        public BlingoFilmLoopPlayer? GetFilmLoopPlayer() => GetActorsOfType<BlingoFilmLoopPlayer>().FirstOrDefault();

        public void SetOnRemoveMe(Action<BlingoSprite2DVirtual> onRemoveMe) => _onRemoveMe = onRemoveMe;

        public override string GetFullName() => $"{SpriteNum}.{Name}.{Member?.Name}";

        public void UpdateTexture(IAbstTexture2D texture)
        {
            _textureSubscription?.Release();
            Texture = texture;
            _textureSubscription = texture.AddUser(this);
        }

        protected override BlingoSpriteState CreateState() => new BlingoSprite2DVirtualState();

        protected override void OnLoadState(BlingoSpriteState state)
        {
            if (state is not BlingoSprite2DVirtualState s) return;
            SetMember(s.Member);
            DisplayMember = s.DisplayMember;
            Ink = s.Ink;
            Hilite = s.Hilite;
            Linked = s.Linked;
            Loaded = s.Loaded;
            Blend = s.Blend;
            LocH = s.LocH;
            LocV = s.LocV;
            LocZ = s.LocZ;
            Rotation = s.Rotation;
            Skew = s.Skew;
            FlipH = s.FlipH;
            FlipV = s.FlipV;
            Constraint = s.Constraint;
            RegPoint = s.RegPoint;
            ForeColor = s.ForeColor;
            BackColor = s.BackColor;
            Width = s.Width;
            Height = s.Height;
        }

        protected override void OnGetState(BlingoSpriteState state)
        {
            if (state is not BlingoSprite2DVirtualState s) return;
            s.Member = (BlingoMember?)Member;
            s.DisplayMember = DisplayMember;
            s.Ink = Ink;
            s.Hilite = Hilite;
            s.Linked = Linked;
            s.Loaded = Loaded;
            s.Blend = Blend;
            s.LocH = LocH;
            s.LocV = LocV;
            s.LocZ = LocZ;
            s.Rotation = Rotation;
            s.Skew = Skew;
            s.FlipH = FlipH;
            s.FlipV = FlipV;
            s.Constraint = Constraint;
            s.RegPoint = RegPoint;
            s.ForeColor = ForeColor;
            s.BackColor = BackColor;
            s.Width = Width;
            s.Height = Height;
        }
    }
}

