using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine
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
        /// Horizontal location of the sprite on the stage. Read/write.
        /// </summary>
        float LocH { get; set; }

        /// <summary>
        /// Vertical location of the sprite on the stage. Read/write.
        /// </summary>
        float LocV { get; set; }

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
        float Height{ get; }

        /// <summary>
        /// Gets the cast member associated with this sprite. Read-only.
        /// </summary>
        LingoMember? Member { get; }

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

        /// <summary>
        /// Access to the score timeline and other score-related operations. Read-only.
        /// </summary>
        ILingoScore Score { get; }

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
        bool Visible { get; set; }
        LingoPoint Loc { get; set; }

        /// <summary>
        /// Changes the cast member displayed by this sprite using the cast member number.
        /// </summary>
        /// <param name="memberNumber">The index of the cast member.</param>
        void SetMember(int memberNumber);

        /// <summary>
        /// Changes the cast member displayed by this sprite using the cast member name.
        /// </summary>
        /// <param name="memberName">The name of the cast member.</param>
        void SetMember(string memberName);

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
    }


    public class LingoSprite : LingoScriptBase, ILingoSprite, ILingoSpriteEventHandler, ILingoMouseEventHandler
    {
        private List<IHasMouseDownEvent> _mouseDownBehaviors = new List<IHasMouseDownEvent>();
        private List<IHasMouseUpEvent> _mouseUpBehaviors = new List<IHasMouseUpEvent>();
        private List<IHasMouseMoveEvent> _mouseMoveBehaviors = new List<IHasMouseMoveEvent>();
        private List<IHasMouseEnterEvent> _mouseEnterBehaviors = new List<IHasMouseEnterEvent>();
        private List<IHasMouseExitEvent> _mouseExitBehaviors = new List<IHasMouseExitEvent>();
        private List<IHasBeginSpriteEvent> _beginSpriteBehaviors = new List<IHasBeginSpriteEvent>();
        private List<IHasEndSpriteEvent> _endSpriteBehaviors = new List<IHasEndSpriteEvent>();
        private List<IHasStepFrameEvent> _stepFrameBehaviors = new List<IHasStepFrameEvent>();
        private List<IHasPrepareFrameEvent> _prepareFrameBehaviors = new List<IHasPrepareFrameEvent>();
        private List<IHasEnterFrameEvent> _enterFrameBehaviors = new List<IHasEnterFrameEvent>();
        private List<IHasExitFrameEvent> _exitFrameBehaviors = new List<IHasExitFrameEvent>();
        private List<IHasFocusEvent> _focusBehaviors = new List<IHasFocusEvent>();
        private List<IHasBlurEvent> _blurBehaviors = new List<IHasBlurEvent>();

        private ILingoFrameworkSprite _frameworkSprite;
        private bool isMouseInside = false;
        private bool isDragging = false;
        private bool isDraggable = false;  // A flag to control dragging behavior
        private List<LingoSpriteBehavior> _behaviors = new List<LingoSpriteBehavior>();
        public T FrameworkObj<T>() where T : ILingoFrameworkSprite => (T)_frameworkSprite;
        public string Name { get => _frameworkSprite.Name; set => _frameworkSprite.Name = value; }

        public int SpriteNum { get; private set; }
        public int Ink { get; private set; }
        public bool Visible { get => _frameworkSprite.Visible; set => _frameworkSprite.Visible = value; }
        public bool Hilite { get; set; }
        public bool Linked { get; private set; }
        public bool Loaded { get; private set; }
        public bool MediaReady { get; private set; }
        public float LocH { get => _frameworkSprite.X; set => _frameworkSprite.X = value; }
        public float LocV { get => _frameworkSprite.Y; set => _frameworkSprite.Y = value; }
        public float Blend { get => _frameworkSprite.Blend; set => _frameworkSprite.Blend = value; }

        public LingoPoint RegPoint { get; set; }
        public LingoColor ForeColor { get; set; }
        public List<string> ScriptInstanceList { get; private set; } = new();

        private LingoMember? _Member;
        public LingoMember? Member { get => _Member; set => SetMember(value); }
        public LingoCast? Cast { get; private set; }
        public ILingoScore Score { get; }

        public int BeginFrame { get; set; }
        public int EndFrame { get; set; }

        public bool Editable { get; set; }
        public bool IsDraggable
        {
            get => isDraggable;
            set => isDraggable = value;
        }

        public LingoColor Color { get; set; }
        public LingoColor BgColor { get; set; }
        public new LingoRect Rect => new LingoRect(LocH, LocV, LocH + Width, LocV + Height);

        public int Size => Media.Length;

        public byte[] Media { get; set; } = new byte[] { };
        public byte[] Thumbnail { get; set; }
        public string ModifiedBy { get; set; }

        public LingoPoint Loc
        {
            get => new LingoPoint(LocH, LocV);
            set
            {
                LocH = value.X;
                LocV = value.Y;
            }
        }
        public LingoPoint Position
        {
            get => new LingoPoint(_frameworkSprite.X, _frameworkSprite.Y);
            set
            {
                _frameworkSprite.X = value.X;
                _frameworkSprite.Y = value.Y;
            }
        }


        public float Width =>  Member?.Width ?? Rect.Width;
        public float Height => Member?.Height ?? Rect.Height;
        /// <summary>
        /// Whether this sprite is currently active (i.e., the playhead is within its frame span).
        /// </summary>
        public bool IsActive { get; internal set; }
        // Not used in c#
        // public int ScriptText { get; set; }

        public LingoSprite(ILingoEnvironment environment)
            :base(environment)
        {
            Score = environment.Movie.Score;
            
        }
        public void Init(ILingoFrameworkSprite frameworkSprite)
        {
            _frameworkSprite = frameworkSprite;
        }
        internal void Init(int number, string name)
        {
            SpriteNum = number;
            Name = name;
        }
        
        public LingoSprite AddBehavior<T>() where T : LingoSpriteBehavior
        {
            var behavior = _env.Factory.CreateBehavior<T>();
            _behaviors.Add(behavior);
            if (behavior is IHasMouseDownEvent mouseDownEvent) _mouseDownBehaviors.Add(mouseDownEvent);
            if (behavior is IHasMouseUpEvent mouseUpEvent) _mouseUpBehaviors.Add(mouseUpEvent);
            if (behavior is IHasMouseMoveEvent mouseMoveEvent) _mouseMoveBehaviors.Add(mouseMoveEvent);
            if (behavior is IHasMouseEnterEvent mouseEnterEvent) _mouseEnterBehaviors.Add(mouseEnterEvent);
            if (behavior is IHasMouseExitEvent mouseExitEvent) _mouseExitBehaviors.Add(mouseExitEvent);
            if (behavior is IHasBeginSpriteEvent beginSpriteEvent) _beginSpriteBehaviors.Add(beginSpriteEvent);
            if (behavior is IHasEndSpriteEvent endSpriteEvent) _endSpriteBehaviors.Add(endSpriteEvent);
            if (behavior is IHasStepFrameEvent stepFrameEvent) _stepFrameBehaviors.Add(stepFrameEvent);
            if (behavior is IHasPrepareFrameEvent prepareFrameEvent) _prepareFrameBehaviors.Add(prepareFrameEvent);
            if (behavior is IHasEnterFrameEvent enterFrameEvent) _enterFrameBehaviors.Add(enterFrameEvent);
            if (behavior is IHasExitFrameEvent exitFrameEvent) _exitFrameBehaviors.Add(exitFrameEvent);
            if (behavior is IHasFocusEvent focusEvent) _focusBehaviors.Add(focusEvent);
            if (behavior is IHasBlurEvent blurEvent) _blurBehaviors.Add(blurEvent);
            return this;
        }

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

        internal virtual void DoBeginSprite() { BeginSprite(); _beginSpriteBehaviors.ForEach(b => b.BeginSprite()); }
        protected virtual void BeginSprite() { }
        internal virtual void DoEndSprite() { EndSprite(); _endSpriteBehaviors.ForEach(b => b.EndSprite()); }
        protected virtual void EndSprite() { }
        internal virtual void DoEnterFrame() { EnterFrame(); _enterFrameBehaviors.ForEach(b => b.EnterFrame()); }
        protected virtual void EnterFrame() { }
        internal virtual void DoExitFrame() { ExitFrame(); _exitFrameBehaviors.ForEach(b => b.ExitFrame()); }
        protected virtual void ExitFrame() { }
        internal virtual void DoStepFrame() { StepFrame(); _stepFrameBehaviors.ForEach(b => b.StepFrame()); }
        protected virtual void StepFrame() {  }
        internal virtual void DoPrepareFrame() { PrepareFrame(); _prepareFrameBehaviors.ForEach(b => b.PrepareFrame()); }
        protected virtual void PrepareFrame() { }

        


        internal void SetFrameworkSprite(ILingoFrameworkSprite fw) => _frameworkSprite = fw;
        private void UpdateFrameworkPosition()
        {
            _frameworkSprite?.SetPositionX(LocH);
            _frameworkSprite?.SetPositionY(LocV);
        }

        public bool PointInSprite(LingoPoint point)
        {
            var bounds = new LingoRect(LocH, LocV, LocH + Width, LocV + Height);
            return bounds.Contains(point);
        }

        public void SetMember(string memberName)
        {
            var member = _env.CastLib.GetMember(memberName);
            _Member = member ?? throw new Exception(Name + ":Member not found with name " + memberName);
            _frameworkSprite.MemberChanged();
        }

        
        public void SetMember(int memberNumber)
        {
            if (Cast == null) throw new Exception(Name + ":Cast not set for sprite: " + memberNumber);
            LingoMember member = Cast.GetMember(memberNumber);
            _Member = member ?? throw new Exception(Name + ":Member not found with number: " + memberNumber);
            
        }
        public void SetMember(LingoMember member)
        {
            _Member = member;
            _frameworkSprite.MemberChanged();
        }
        public void SetPosition(float x, float y)
        {
            LocH = x;
            LocV = y;
            UpdateFrameworkPosition();
        }
        public void SetPositionX(float x)
        {
            LocH = x;
            _frameworkSprite?.SetPositionX(x);
        }
        public void SetPositionY(float y)
        {
            LocV = y;
            _frameworkSprite?.SetPositionY(y);
        }
        public void SendToBack()
        {
            throw new NotImplementedException();
        }

        public void BringToFront()
        {
            throw new NotImplementedException();
        }

        public void MoveBackward()
        {
            throw new NotImplementedException();
        }

        public void MoveForward()
        {
            throw new NotImplementedException();
        }
        public LingoSprite Duplicate()
        {
            throw new NotImplementedException();
            // Create shallow copy, link same member, etc.
            //return new LingoSprite(_env, Score, Name + "_copy", SpriteNum)
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


        #region Mouse

        void ILingoMouseEventHandler.DoMouseMove(LingoMouse mouse)
        {
            // Only respond if the sprite is active and the mouse is within the bounding box
            if (IsActive && IsMouseInsideBoundingBox(mouse))
            {
                _mouseMoveBehaviors.ForEach(b => b.MouseMove(mouse));
                if (!isMouseInside)
                {
                    MouseEnter(mouse); // Mouse has entered the sprite
                    _mouseEnterBehaviors.ForEach(b => b.MouseEnter(mouse));
                    isMouseInside = true;
                }
            }
            else
            {
                if (isMouseInside)
                {
                    MouseExit(mouse); // Mouse has exited the sprite
                    _mouseExitBehaviors.ForEach(b => b.MouseExit(mouse));
                    isMouseInside = false;
                }
            }
            if (IsActive && isDraggable && isDragging)
                DoMouseDrag(mouse);
        }
        public virtual void MouseMove(LingoMouse mouse)
        {

        }
        /// <summary>
        /// Triggered when the mouse enters the sprite's bounds
        /// </summary>
        public virtual void MouseEnter(LingoMouse mouse)
        {
        }
        /// <summary>
        /// Triggered when the mouse exits the sprite's bounds
        /// </summary>
        public virtual void MouseExit(LingoMouse mouse)
        {
        }
        void ILingoMouseEventHandler.DoMouseDown(LingoMouse mouse)
        {
            if (isDraggable && IsMouseInsideBoundingBox(mouse))
                isDragging = true;
            MouseDown(mouse);
            _mouseDownBehaviors.ForEach(b => b.MouseDown(mouse));
        }

        protected virtual void MouseDown(LingoMouse mouse) { }
        void ILingoMouseEventHandler.DoMouseUp(LingoMouse mouse)
        {
            if (isDragging && isDragging)
                isDragging = false;
            MouseUp(mouse);
            _mouseUpBehaviors.ForEach(b => b.MouseUp(mouse));
        }
        protected virtual void MouseUp(LingoMouse mouse) { }
        private void DoMouseDrag(LingoMouse mouse)
        {
            LocH = mouse.MouseH;
            LocV = mouse.MouseV;
            MouseDrag(mouse);
        }
        protected virtual void MouseDrag(LingoMouse mouse) { }


        #endregion



        void ILingoSpriteEventHandler.DoFocus() => Focus();
        protected virtual void Focus() { } 
        void ILingoSpriteEventHandler.DoBlur() => Blur();
        protected virtual void Blur() { }



        /// <summary>
        /// Check if the mouse position is inside the bounding box of the sprite
        /// </summary>
        public bool IsMouseInsideBoundingBox(LingoMouse mouse) => mouse.MouseH >= LocH && mouse.MouseH <= LocH + Width && mouse.MouseV >= LocV && mouse.MouseV <= LocV + Height;

       
    }
}
