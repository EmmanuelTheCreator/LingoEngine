using BlingoEngine.Events;
using BlingoEngine.Inputs;
using BlingoEngine.Primitives;
using BlingoEngine.Medias;
using BlingoEngine.Animations;
using BlingoEngine.Members;
using BlingoEngine.Casts;
using BlingoEngine.Movies;
using BlingoEngine.FilmLoops;
using BlingoEngine.Sprites.Events;
using BlingoEngine.Bitmaps;
using BlingoEngine.FrameworkCommunication;
using System.Diagnostics;
using BlingoEngine.Inputs.Events;
using AbstUI.Primitives;
using Microsoft.Extensions.DependencyInjection;
using AbstUI.Components;

namespace BlingoEngine.Sprites
{
    [DebuggerDisplay("LSprite2D:{SpriteNum}) {Name}:Member={Member?.Name}:{BeginFrame}->{EndFrame}:Pos={LocH}x{LocV}:Size={Width}x{Height}")]
    public class BlingoSprite2D : BlingoSprite, IBlingoMouseEventHandler, IBlingoSprite, IBlingoSpriteWithMember, IBlingoSprite2DLight, IAbstLayoutNode
    {
        public const int SpriteNumOffset = 6;
        private readonly List<BlingoSpriteBehavior> _behaviors = new();
        private readonly BlingoMovie _movie;
        private readonly IBlingoFrameworkFactory _frameworkFactory;
        private readonly IBlingoSpritesPlayer _spritesHolder;
        private readonly IBlingoMovieEnvironment _environment;

        public IReadOnlyList<BlingoSpriteBehavior> Behaviors => _behaviors;


        private bool _isMouseInside = false;
        private bool _isDragging = false;
        private bool _isDraggable = false;  // A flag to control dragging behavior
        private BlingoMember? _member;
        private Action<BlingoSprite2D>? _onRemoveMe;
        private bool _isFocus = false;
        private IAbstUITextureUserSubscription? _textureSubscription;
        private bool _flipH;
        private bool _flipV;
        private int _cursor;
        private int? _previousCursor;
        private int _constraint;
        private bool _directToStage;
        private float _blend = 100f;
        private ARect? _memberSourceRect;
        private bool _memberSourceRectChanged;


        #region Properties
        public override int SpriteNumWithChannel => SpriteNum + SpriteNumOffset;
        internal BlingoSpriteChannel? SpriteChannel { get; set; }

        // /// <inheritdoc/>
        //public T Framework<T>() where T : class, IBlingoFrameworkSprite => (T)_frameworkSprite;
        public float X { get => LocH; set => LocH = value; }
        public float Y { get => LocV; set => LocV = value; }
        public AMargin Margin { get; set; } = AMargin.Zero;
        public int ZIndex { get => LocZ; set => LocZ = value; }
        IAbstFrameworkNode IAbstNode.FrameworkObj { get => _frameworkSprite; set => throw new NotImplementedException(); } // not allowed to set.


        public override string Name { get => _frameworkSprite.Name; set => _frameworkSprite.Name = value; }
        public int MemberNum
        {
            get => Member?.NumberInCast ?? 0;
            set
            {
                if(MemberNum == value) return;
                if (Member != null)
                    //Member = Member.GetMemberInCastByOffset(value);
                    Member = Member.Cast.Member[value];
                OnPropertyChanged();
            }
        }
        /// <summary>Channel default cast member.</summary>
        public int DisplayMember { get; set; }
        /// <summary>Offset to the property list for behaviors.</summary>
        public int SpritePropertiesOffset { get; set; }

        private int _ink;
        private AColor _foreColor = AColors.Black;
        private AColor _backColor = AColors.White;

        public BlingoInkType InkType { get => (BlingoInkType)_ink; set => Ink = (int)value; }
        public int Ink
        {
            get => _ink;
            set
            {
                if(_ink == value) return;
                _ink = value;
                if (_frameworkSprite != null)
                    _frameworkSprite.Ink = value;
                OnPropertyChanged();
            }
        }
        public bool Visibility
        {
            get => _frameworkSprite.Visibility; 
            set
            {
                if (SpriteChannel != null)
                {
                    if (SpriteChannel.Visibility == value) return;
                    SpriteChannel.Visibility = value;
                }
                _frameworkSprite.Visibility = value;
                OnPropertyChanged();
            }
        }
        public bool Hilite { get; set; }
        public bool Linked { get; private set; }
        public bool Loaded { get; private set; }
        public bool MediaReady { get; private set; }
        public float Blend
        {
            get => _blend;
            set
            {
                if(_blend == value) return;
                _blend = value;
                ApplyBlend();
                OnPropertyChanged();
            }
        }
        public float LocH
        {
            get => _frameworkSprite.X;
            set
            {
                if(_frameworkSprite.X == value) return;
                _frameworkSprite.X = value;
                OnPropertyChanged();
            }
        }
        public float LocV
        {
            get => _frameworkSprite.Y;
            set
            {
                if(_frameworkSprite.Y == value) return;
                _frameworkSprite.Y = value;
                OnPropertyChanged();
            }
        }
        public int LocZ
        {
            get => _frameworkSprite.ZIndex;
            set
            {
                if (_frameworkSprite.ZIndex == value) return;
                _frameworkSprite.ZIndex = value;
                OnPropertyChanged();
            }
        }
        public APoint Loc { get => (_frameworkSprite.X, _frameworkSprite.Y); set => _frameworkSprite.SetPosition(value); }

        /// <summary>Rotation of the sprite in degrees.</summary>
        public float Rotation
        {
            get => _frameworkSprite.Rotation;
            set
            {
                if(_frameworkSprite.Rotation == value) return;
                _frameworkSprite.Rotation = value;
                OnPropertyChanged();
            }
        }
        public float Skew
        {
            get => _frameworkSprite.Skew;
            set
            {
                if(_frameworkSprite.Skew == value) return;
                _frameworkSprite.Skew = value;
                OnPropertyChanged();
            }
        }
        public bool FlipH { get => _flipH; 
            set {
                if(_flipH == value) return;
                _flipH = value;
                _frameworkSprite.FlipH = value;
                OnPropertyChanged();
            } }
        public bool FlipV { get => _flipV; 
            set 
            { 
                if(_flipV == value) return;
                _flipV = value; 
                _frameworkSprite.FlipV = value; 
                OnPropertyChanged();
            } }
        public float Top { get => Rect.Top; set { var o = GetRegPointOffset(); LocV = value + o.Y + Height / 2f; } }
        public float Bottom { get => Rect.Bottom; set => Top = value - Height; }
        public float Left { get => Rect.Left; set { var o = GetRegPointOffset(); LocH = value + o.X + Width / 2f; } }
        public float Right { get => Rect.Right; set => Left = value - Width; }
        public int Cursor
        {
            get => _cursor;
            set
            {
                if(_cursor == value) return;
                _cursor = value;
                OnPropertyChanged();
            }
        }
        public int Constraint { get => _constraint; set => _constraint = value; }
        public bool DirectToStage
        {
            get => _directToStage;
            set
            {
                if (_directToStage == value) return;
                _directToStage = value;
                // Rendering order is handled by the framework implementation,
                // so keep the engine Z index untouched.
                _frameworkSprite.DirectToStage = value;
                ApplyBlend();
                OnPropertyChanged();
            }
        }

        public APoint RegPoint { get; set; }
        public AColor ForeColor
        {
            get => _foreColor;
            set
            {
                if(_foreColor == value) return;
                _foreColor = value;
                OnPropertyChanged();
            }
        }
        public AColor BackColor
        {
            get => _backColor;
            set
            {
                if(_backColor == value) return;
                _backColor = value;
                OnPropertyChanged();
            }
        }
        public List<string> ScriptInstanceList { get; private set; } = new();



        public IBlingoMember? Member { get => _member; set => SetMember(value); }
        public BlingoCast? Cast { get; private set; }


        public bool Editable { get; set; }

        public bool IsDraggable
        {
            get => _isDraggable;
            set => _isDraggable = value;
        }

        public AColor Color { get; set; }

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

        public ARect? MemberSourceRect
        {
            get => _memberSourceRect;
            set
            {
                if (_memberSourceRect == value) return;
                _memberSourceRect = value;
                _memberSourceRectChanged = true;
                if (_frameworkSprite != null && _member != null)
                    MemberHasChanged(forceSizeUpdate: true);
                OnPropertyChanged();
            }
        }

        public int Size => Media.Length;

        public byte[] Media { get; set; } = new byte[] { };
        public byte[] Thumbnail { get; set; } = new byte[] { };
        public string ModifiedBy { get; set; } = "";


        /// <inheritdoc/>
        public int Duration => (_frameworkSprite as IBlingoFrameworkSpriteVideo)?.Duration ?? 0;

        /// <inheritdoc/>
        public int CurrentTime
        {
            get => (_frameworkSprite as IBlingoFrameworkSpriteVideo)?.CurrentTime ?? 0;
            set
            {
                if (_frameworkSprite is IBlingoFrameworkSpriteVideo video)
                {
                    if (video.CurrentTime == value) return;
                    video.CurrentTime = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public BlingoMediaStatus MediaStatus => (_frameworkSprite as IBlingoFrameworkSpriteVideo)?.MediaStatus ?? BlingoMediaStatus.Closed;

        public float Width
        {
            get => _frameworkSprite.Width;
            set => _frameworkSprite.DesiredWidth = value;
        }
        public float Height
        {
            get => _frameworkSprite.Height;
            set => _frameworkSprite.DesiredHeight = value;
        }

        public IBlingoMember? GetMember() => Member;

        bool IBlingoSpriteBase.IsPuppetCached { get; set; }
       
        #endregion




        // Not used in c#
        // public int ScriptText { get; set; }

#pragma warning disable CS8618
        public BlingoSprite2D(IBlingoMovieEnvironment environment, IBlingoSpritesPlayer spritesHolder) : base(environment.Events)
#pragma warning restore CS8618
        {
            _movie = (BlingoMovie)environment.Movie;
            _frameworkFactory = environment.Factory;
            _spritesHolder = spritesHolder;
            _environment = environment;
        }
        public void Init(IBlingoFrameworkSprite frameworkSprite)
        {
            _frameworkSprite = frameworkSprite;
            _frameworkSprite.Ink = _ink;
            ApplyBlend();
            if (_memberSourceRectChanged && _member != null)
                MemberHasChanged(forceSizeUpdate: true);
        }

        #region Behaviors

        public IBlingoSprite AddBehavior<T>(Action<T>? configure = null) where T : BlingoSpriteBehavior
        {
            SetBehavior<T>(configure);
            return this;
        }
        public T SetBehavior<T>(Action<T>? configure = null) where T : BlingoSpriteBehavior
        {
            var behavior = _movie.GetServiceProvider().GetRequiredService<T>();
            behavior.SetMe(this);
            _behaviors.Add(behavior);
            if (configure != null)
                configure(behavior);
            return behavior;
        }
        private void CallBehaviorForEvents<T>(Action<T> action)
        {
            foreach (var item in _behaviors.OfType<T>())
                action(item);
        }
        #endregion


        #region Animation / keyframes

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

        public void SetSpriteTweenOptions(bool positionEnabled, bool sizeEnabled, bool rotationEnabled, bool skewEnabled,
            bool foregroundColorEnabled, bool backgroundColorEnabled, bool blendEnabled,
            float curvature, bool continuousAtEnds, bool speedSmooth, float easeIn, float easeOut)
        {
            GetAnimator().SetTweenOptions(positionEnabled, sizeEnabled, rotationEnabled, skewEnabled,
                foregroundColorEnabled, backgroundColorEnabled, blendEnabled,
                curvature, continuousAtEnds, speedSmooth, easeIn, easeOut);
        }
        private BlingoSpriteAnimator GetAnimator()
        {
            var animator = GetActorsOfType<BlingoSpriteAnimator>().FirstOrDefault();
            if (animator == null)
            {
                animator = new BlingoSpriteAnimator(this, _movie, _eventMediator,null,false);
                AddActor(animator);
            }

            return animator;
        }
        public void AddAnimator(BlingoSpriteAnimatorProperties animatorProps, IBlingoEventMediator eventMediator)
        {
            var animator = GetActorsOfType<BlingoSpriteAnimator>().FirstOrDefault();
            if (animator != null) throw new Exception("Animator already set");
            animator = new BlingoSpriteAnimator(this, _movie, eventMediator, animatorProps,false);
            AddActor(animator);
        }
        #endregion
        /*
        When the movie first starts, events occur in the following order:
1 prepareMovie
2 prepareFrame Immediately after the prepareFrame event, Director plays sounds, draws
sprites, and performs any transitions or palette effects. This event occurs before the enterFrame
event. A prepareFrame handler is a good location for script that you want to run before the
frame draws.
3 beginSprite This event occurs when the playhead enters a sprite span.
4 startMovie This event occurs in the first frame that plays.
34 Chapter 2: Director Scripting Essentials
When the movie encounters a frame, events occur in the following order:
1 beginSprite This event occurs only if new sprites begin in the frame.
2 stepFrame
3 prepareFrame
4 enterFrame After enterFrame and before exitFrame, Director handles any time delays
required by the tempo setting, idle events, and keyboard and mouse events.
5 exitFrame
6 endSprite This event occurs only if the playhead exits a sprite in the frame.
When a movie stops, events occur in the following order:
1 endSprite T
        2 stopMovie

        */

        internal override void DoBeginSprite()
        {
            if (_textureSubscription == null && _member != null)
            {
                _member.UsedBy(this);
                MemberHasChanged();
            }
            // Subscribe all behaviors
            _behaviors.ForEach(b =>
            {
                _eventMediator.Subscribe(b, SpriteNum + 6, true); //  we ignore mouse because it has to be within the boundingbox
                if (_movie.IsPlaying)
                    if (b is IHasBeginSpriteEvent beginSpriteEvent) beginSpriteEvent.BeginSprite();

            });
            base.DoBeginSprite();
        }
        internal virtual BlingoMember? DoPreStepFrame()
        {
            if (_member != null && _member.HasChanged)
            {
#if DEBUG
                if (_member.NumberInCast == 44)
                {

                }
#endif
                _frameworkSprite.ApplyMemberChangesOnStepFrame();
                return _member;
            }
            return null;
        }
        internal override void DoEndSprite()
        {
            _behaviors.ForEach(b =>
            {
                // Unsubscribe all behaviors
                _eventMediator.Unsubscribe(b, true); //  we ignore mouse because it has to be within the boundingbox
                if (_movie.IsPlaying)
                    if (b is IHasEndSpriteEvent endSpriteEvent) endSpriteEvent.EndSprite();
            });
            FrameworkObj.Hide();
            _textureSubscription?.Release();
            _textureSubscription = null;
            // Release the old member link with this sprite
            _member?.ReleaseFromRefUser(this);
            base.DoEndSprite();
        }




        internal void SetFrameworkSprite(IBlingoFrameworkSprite fw) => _frameworkSprite = fw;

        public bool PointInSprite(APoint point)
        {
            return Rect.Contains(point);
        }
        private void ApplyBlend()
        {
            _frameworkSprite.Blend = _directToStage ? 1f : _blend;
        }

        private APoint GetRegPointOffset()
        {
            if (_member is null)
                return new APoint();

            var (baseOffset, sourceWidth, sourceHeight) = GetMemberSourceMetrics();
            if (sourceWidth != 0 && sourceHeight != 0)
            {
                float scaleX = Width / sourceWidth;
                float scaleY = Height / sourceHeight;
                return new APoint(baseOffset.X * scaleX, baseOffset.Y * scaleY);
            }
            return baseOffset;
        }


        private void ApplyMemberSourceRectSize(bool force)
        {
            if (_frameworkSprite == null || _member == null)
                return;

            float targetWidth;
            float targetHeight;

            if (_member is BlingoMemberBitmap && MemberSourceRect is { } rect)
            {
                targetWidth = rect.Width;
                targetHeight = rect.Height;
            }
            else
            {
                targetWidth = _member.Width;
                targetHeight = _member.Height;
            }

            if (targetWidth > 0 && (force || Width == 0))
                Width = targetWidth;

            if (targetHeight > 0 && (force || Height == 0))
                Height = targetHeight;
        }

        public (APoint offset, float sourceWidth, float sourceHeight) GetMemberSourceMetrics()
        {
            if (_member == null)
                return (new APoint(), 0f, 0f);

            if (_member is BlingoMemberBitmap && MemberSourceRect is { } rect)
            {
                var regPoint = _member.RegPoint;
                var baseOffset = new APoint(regPoint.X - rect.Width + (rect.Width/2), regPoint.Y - rect.Height + (rect.Height / 2));
                return (baseOffset, rect.Width, rect.Height);
            }

            var defaultOffset = _member.CenterOffsetFromRegPoint();
            return (defaultOffset, _member.Width, _member.Height);
        }


        #region Member / MemberChange

        public IBlingoSprite SetMember(string memberName, int? castLibNum = null)
        {
            var member = _environment.GetMember<BlingoMember>(memberName, castLibNum);
            var newMember = member ?? throw new Exception(Name + ":Member not found with name " + memberName);
            SetMember(newMember);
            return this;
        }


        public IBlingoSprite SetMember(int memberNumber, int? castLibNum = null)
        {
            var member = _environment.GetMember<BlingoMember>(memberNumber, castLibNum);

            var newMember = member ?? throw new Exception(Name + ":Member not found with number: " + memberNumber);
            SetMember(newMember);
            return this;
        }
        public IBlingoSprite SetMember(IBlingoMember? member)
        {
            if (_member == member) return this;
            var _lastMember = _member;
            if (_member != null && (_member.Type == BlingoMemberType.Script || _member.Type == BlingoMemberType.Sound || _member.Type == BlingoMemberType.Transition || _member.Type == BlingoMemberType.Unknown || _member.Type == BlingoMemberType.Palette || _member.Type == BlingoMemberType.Movie || _member.Type == BlingoMemberType.Font || _member.Type == BlingoMemberType.Cursor))
                return this;
            // Release the old member link with this sprite
            if (member != _member)
            {
                _member?.ReleaseFromRefUser(this);
            }
            _member = member as BlingoMember;
            if (_member != null)
            {
                _member.UsedBy(this);
                RegPoint = _member.RegPoint;
            }
            if (member is BlingoFilmLoopMember filmLoop)
            {
                filmLoop.PrepareFilmloop();// to get the size
                Width = filmLoop.Width;
                Height = filmLoop.Height;
            }

            MemberHasChanged();
            return this;
        }

        private void MemberHasChanged(bool forceSizeUpdate = false)
        {
            var forceUpdate = forceSizeUpdate || _memberSourceRectChanged;
            var isCropping = _member is BlingoMemberBitmap && MemberSourceRect is { };
            var existingPlayer = GetActorsOfType<BlingoFilmLoopPlayer>().FirstOrDefault();
            if (_member is BlingoFilmLoopMember)
            {
                if (existingPlayer == null)
                {
                    existingPlayer = new BlingoFilmLoopPlayer(this, _eventMediator, _environment.CastLibsContainer);
                    AddActor(existingPlayer);
                }
                if (IsActive)
                    existingPlayer.BeginSprite();
            }
            else if (existingPlayer != null)
            {
                if (IsActive)
                    existingPlayer.EndSprite();
                RemoveActor(existingPlayer);
            }

            if (_frameworkSprite != null)
            {
                var shouldForceSize = forceUpdate && isCropping;
                ApplyMemberSourceRectSize(shouldForceSize);
                _frameworkSprite.MemberChanged();
                _memberSourceRectChanged = false;
            }
            else
            {
                _memberSourceRectChanged = forceUpdate && isCropping;
            }

            OnPropertyChanged();
        }
        #endregion


        public BlingoFilmLoopPlayer? GetFilmLoopPlayer() => GetActorsOfType<BlingoFilmLoopPlayer>().FirstOrDefault();


        #region ZIndex/locZ
        public void SendToBack()
        {
            LocZ = 1;
        }

        public void BringToFront()
        {
            var maxZ = ((BlingoMovie)_environment.Movie).GetMaxLocZ();
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
        public BlingoSprite2D Duplicate()
        {
            throw new NotImplementedException();
            // Create shallow copy, link same member, etc.
            //return new BlingoSprite(_env, Score, Name + "_copy", SpriteNum)
            //{
            //    Member = Member,
            //    LocH = LocH,
            //    LocV = LocV,
            //    Blend = Blend,
            //    Visible = Visible,
            //    Color = Color,
            //    //Rect = Rect
            //    // etc.
            //};
        }


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


        #region Mouse

        public void RaiseMouseMove(BlingoMouseEvent mouse)
        {
            if (!_movie.IsPlaying) return;
            //if (IsActive && Member?.Name == "B_Play")
            //{
            //    var inside = IsMouseInsideBoundingBox(mouse.Mouse);
            //    Console.WriteLine($"Mouse:{mouse.Mouse.MouseH}x{mouse.Mouse.MouseV} \t{Member?.Name}\t{inside}\tpos={LocH}x{LocV}\tRect={Rect};Size: {Width}x{Height}");
            //}
            // Only respond if the sprite is active and the mouse is within the bounding box
            if (IsActive && IsMouseInsideBoundingBox(mouse.Mouse))
            {

                CallBehaviorForEvents<IHasMouseMoveEvent>(b => b.MouseMove(mouse));
                _eventMediator.RaiseMouseMove(mouse);

                if (!_isMouseInside)
                {
                    MouseEnter(mouse); // Mouse has entered the sprite
                    CallBehaviorForEvents<IHasMouseEnterEvent>(b => b.MouseEnter(mouse));
                    _eventMediator.RaiseMouseEnter(mouse);
                    _isMouseInside = true;
                }
            }
            else
            {
                if (_isMouseInside)
                {
                    MouseExit(mouse); // Mouse has exited the sprite
                    CallBehaviorForEvents<IHasMouseExitEvent>(b => b.MouseExit(mouse));
                    _eventMediator.RaiseMouseExit(mouse);
                    _isMouseInside = false;
                }
            }
            if (IsActive && _isDraggable && _isDragging)
                DoMouseDrag(mouse);
        }
        public virtual void MouseMove(BlingoMouseEvent mouse)
        {

        }
        /// <summary>
        /// Triggered when the mouse enters the sprite's bounds
        /// </summary>
        public virtual void MouseEnter(BlingoMouseEvent mouse)
        {
            if (_cursor != 0)
            {
                _previousCursor = mouse.Mouse.Cursor.Cursor;
                mouse.Mouse.Cursor.Cursor = _cursor;
            }
        }
        /// <summary>
        /// Triggered when the mouse exits the sprite's bounds
        /// </summary>
        public virtual void MouseExit(BlingoMouseEvent mouse)
        {
            if (_previousCursor.HasValue)
            {
                mouse.Mouse.Cursor.Cursor = _previousCursor.Value;
                _previousCursor = null;
            }
        }
        public void RaiseMouseDown(BlingoMouseEvent mouse)
        {
            if (_isDraggable && IsMouseInsideBoundingBox(mouse.Mouse))
                _isDragging = true;
            if (_isMouseInside)
            {
                MouseDown(mouse);
                CallBehaviorForEvents<IHasMouseDownEvent>(b => b.MouseDown(mouse));
                _eventMediator.RaiseMouseDown(mouse);
            }
        }

        protected virtual void MouseDown(BlingoMouseEvent mouse) { }
        public void RaiseMouseUp(BlingoMouseEvent mouse)
        {
            if (_isDragging && _isDragging)
                _isDragging = false;
            if (_isMouseInside)
            {
                MouseUp(mouse);
                CallBehaviorForEvents<IHasMouseUpEvent>(b => b.MouseUp(mouse));
                _eventMediator.RaiseMouseUp(mouse);
            }
        }
        protected virtual void MouseUp(BlingoMouseEvent mouse) { }
        public void RaiseMouseWheel(BlingoMouseEvent mouse)
        {
            if (_isMouseInside)
            {
                CallBehaviorForEvents<IHasMouseWheelEvent>(b => b.MouseWheel(mouse));
                _eventMediator.RaiseMouseWheel(mouse);
            }
        }

        private void DoMouseDrag(BlingoMouseEvent mouse)
        {
            LocH = mouse.MouseH;
            LocV = mouse.MouseV;
            //_environment.Player.Stage.AddKeyFrame(this);
            MouseDrag(mouse);
        }
        protected virtual void MouseDrag(BlingoMouseEvent mouse) { }


        #endregion



        #region Blur/Focus
        public virtual void Focus()
        {
            if (_isFocus) return;
            _isFocus = true;
            _eventMediator.RaiseFocus();
        }
        public virtual void Blur()
        {
            if (!_isFocus) return;
            _isFocus = false;
            _eventMediator.RaiseBlur();
        }
        #endregion



        /// <summary>
        /// Determines whether the mouse is inside the sprite, considering
        /// transparent pixels for <see cref="BlingoInkType.BackgroundTransparent"/>.
        /// </summary>
        public bool IsMouseInsideBoundingBox(BlingoMouse mouse)
        {
            var rect = Rect; // BlingoRect.New(LocH,LocV,Width,Height);
            if (!rect.Contains(mouse.MouseLoc))
                return false;

            if (InkType != BlingoInkType.BackgroundTransparent || Member == null)
                return true;

            //var rect = Rect;
            if (Member.Width == 0 || Member.Height == 0 || Width == 0 || Height == 0)
                return true;

            var relX = (mouse.MouseH - rect.Left) / Width;
            var relY = (mouse.MouseV - rect.Top) / Height;

            int pixelX = (int)(relX * Member.Width);
            int pixelY = (int)(relY * Member.Height);

            if (FlipH)
                pixelX = Member.Width - 1 - pixelX;
            if (FlipV)
                pixelY = Member.Height - 1 - pixelY;

            return !Member.IsPixelTransparent(pixelX, pixelY);
        }

        /// <summary>
        /// Check if the given point is inside the bounding box of the sprite
        /// </summary>
        public bool IsPointInsideBoundingBox(float x, float y)
            => Rect.Contains((x, y));

        internal void CallBehavior<T>(Action<T> actionOnSpriteBehaviour) where T : IBlingoSpriteBehavior
        {
            var behavior = _behaviors.OfType<T>().FirstOrDefault();
            if (behavior == null) return;
            actionOnSpriteBehaviour(behavior);
        }
        internal TResult? CallBehavior<T, TResult>(Func<T, TResult> actionOnSpriteBehaviour) where T : IBlingoSpriteBehavior
        {
            var behavior = _behaviors.OfType<T>().FirstOrDefault();
            if (behavior == null) return default;
            return actionOnSpriteBehaviour(behavior);
        }





        public override void OnRemoveMe()
        {
            _member?.ReleaseFromRefUser(this);
            _frameworkSprite.RemoveMe();
            if (_onRemoveMe != null)
                _onRemoveMe(this);
        }

        public void MemberHasBeenRemoved()
        {
            _member = null;
        }

        public void SetOnRemoveMe(Action<BlingoSprite2D> onRemoveMe) => _onRemoveMe = onRemoveMe;

        public override string GetFullName() => $"{SpriteNum}.{Name}.{Member?.Name}";

        internal void ChangeSpriteNumIKnowWhatImDoingOnlyInternal(int spriteNum)
        {
            SpriteNum = spriteNum;
        }
        #region Clone and Get/Load State

        public override Action<BlingoSprite> GetCloneAction()
        {
            var baseAction = base.GetCloneAction();
            Action<BlingoSprite> action = s => { };
            var member = Member;
            var x = LocH;
            var y = LocV;
            var z = LocZ;
            var width = Width;
            var height = Height;
            var skew = Skew;
            var ink = Ink;
            var blend = Blend;
            var hilite = Hilite;
            var rotation = Rotation;
            var flipH = FlipH;
            var flipV = FlipV;
            var cursor = Cursor;
            var foreColor = ForeColor;
            var backColor = BackColor;
            var editable = Editable;
            var isDraggable = IsDraggable;
            action = s =>
            {
                baseAction(s);
                var sprite2D = (BlingoSprite2D)s;
                sprite2D.SetMember(member);
                sprite2D.LocH = x;
                sprite2D.LocV = y;
                sprite2D.LocZ = z;
                sprite2D.Width = width;
                sprite2D.Height = height;
                sprite2D.Skew = skew;
                sprite2D.Ink = ink;
                sprite2D.Blend = blend;
                sprite2D.Hilite = hilite;
                sprite2D.Rotation = rotation;
                sprite2D.FlipH = flipH;
                sprite2D.FlipV = flipV;
                sprite2D.Cursor = cursor;
                sprite2D.ForeColor = foreColor;
                sprite2D.BackColor = backColor;
                sprite2D.Editable = editable;
                sprite2D.IsDraggable = isDraggable;
            };

            return action;
        }

        protected override BlingoSpriteState CreateState() => new BlingoSprite2DState();

        protected override void OnLoadState(BlingoSpriteState state)
        {
            if (state is not BlingoSprite2DState s || Puppet) return;
            MemberSourceRect = s.MemberSourceRect;
            if (s.Width > 0) Width = s.Width;
            if (s.Height > 0) Height = s.Height;
            //SetMember(s.Member); // Gives problems in SDL
            DisplayMember = s.DisplayMember;
            SpritePropertiesOffset = s.SpritePropertiesOffset;
            Ink = s.Ink;
            Hilite = s.Hilite;
            Blend = s.Blend;
            LocH = s.LocH;
            LocV = s.LocV;
            LocZ = s.LocZ;

            Rotation = s.Rotation;
            Skew = s.Skew;
            FlipH = s.FlipH;
            FlipV = s.FlipV;
            Cursor = s.Cursor;
            Constraint = s.Constraint;
            DirectToStage = s.DirectToStage;
            RegPoint = s.RegPoint;
            ForeColor = s.ForeColor;
            BackColor = s.BackColor;
            Editable = s.Editable;
            IsDraggable = s.IsDraggable;
        }

        protected override void OnGetState(BlingoSpriteState state)
        {
            if (state is not BlingoSprite2DState s) return;
            s.Member = (BlingoMember?)Member;
            s.DisplayMember = DisplayMember;
            s.SpritePropertiesOffset = SpritePropertiesOffset;
            s.Ink = Ink;
            s.Hilite = Hilite;
            s.Blend = Blend;
            s.LocH = LocH;
            s.LocV = LocV;
            s.LocZ = LocZ;
            s.Width = Width;
            s.Height = Height;
            s.Rotation = Rotation;
            s.Skew = Skew;
            s.FlipH = FlipH;
            s.FlipV = FlipV;
            s.Cursor = Cursor;
            s.Constraint = Constraint;
            s.DirectToStage = DirectToStage;
            s.RegPoint = RegPoint;
            s.ForeColor = ForeColor;
            s.BackColor = BackColor;
            s.Editable = Editable;
            s.IsDraggable = IsDraggable;
            s.MemberSourceRect = MemberSourceRect;
        }

        #endregion
        public void UpdateTexture(IAbstTexture2D texture)
        {
            _frameworkSprite.SetTexture(texture);
        }

        public void FWTextureHasChanged(IAbstTexture2D? texture2D, bool useSubscription = true)
        {
            if (useSubscription)

            {
                _textureSubscription?.Release();
                if (texture2D != null)
                    _textureSubscription = texture2D.AddUser(this);
                else
                    _textureSubscription = null;
            }
        }


        #region Media/Video
        /// <inheritdoc/>
        public void Play()
        {
            if (_frameworkSprite is IBlingoFrameworkSpriteVideo video)
            {
                if (MediaStatus == BlingoMediaStatus.Playing) return;
                video.Play();
                _eventMediator.RaiseStartVideo();
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (_frameworkSprite is IBlingoFrameworkSpriteVideo video)
            {
                if (MediaStatus == BlingoMediaStatus.Error || MediaStatus == BlingoMediaStatus.Closed) return;
                video.Stop();
                _eventMediator.RaiseStopVideo();
                _eventMediator.RaiseEndVideo();
            }
        }

        /// <inheritdoc/>
        public void Pause()
        {
            if (_frameworkSprite is IBlingoFrameworkSpriteVideo video)
            {
                video.Pause();
                _eventMediator.RaisePauseVideo();
            }
        }

        /// <inheritdoc/>
        public void Seek(int milliseconds)
        {
            if (_frameworkSprite is IBlingoFrameworkSpriteVideo video)
                video.Seek(milliseconds);
        }

        #endregion

        internal void Reset()
        {
            Stop();

            Width = 0;
            Height = 0;
            LocH = 0;
            LocV = 0;
            LocZ = SpriteNum;
            FlipH = false;
            FlipV = false;
            Rotation = 0;
            Skew = 0;
            Blend = 100;
            InkType = BlingoInkType.Copy;
            Editable = false;
            IsDraggable = false;
            ForeColor = AColors.Black;
            BackColor = AColors.White;
            Loaded = false;
            Linked = false;
            MediaReady = false;
            SetMember(null);

            // fields
            _behaviors.Clear();
            _isMouseInside = false;
            _isDragging = false;
            _isFocus = false;
            _previousCursor = null;

            // base class
            _lock = false;
        }

        public T Framework<T>() where T : IAbstFrameworkNode
        {
            throw new NotImplementedException();
        }
    }
}

