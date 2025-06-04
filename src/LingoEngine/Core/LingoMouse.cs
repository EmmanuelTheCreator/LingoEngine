using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Primitives;

namespace LingoEngine.Core
{
    /// <summary>
    /// Provides access to a user’s mouse activity, including mouse movement and mouse clicks.
    /// </summary>
    public interface ILingoMouse
    {
        /// <summary>
        /// Mouse property; returns the last active sprite clicked by the user. Read-only.
        /// </summary>
        LingoSprite? ClickOn { get; }

        /// <summary>
        /// Returns TRUE if the user double-clicked the mouse; otherwise FALSE.
        /// Read-only. Set by the system when a double-click occurs.
        /// </summary>
        bool DoubleClick { get; }

        /// <summary>
        /// Returns the character where the mouse was clicked, typically used in text contexts.
        /// </summary>
        char MouseChar { get; }

        /// <summary>
        /// Returns TRUE while the mouse button is held down; otherwise FALSE.
        /// </summary>
        bool MouseDown { get; }

        /// <summary>
        /// Returns the horizontal position of the mouse pointer relative to the Stage (pixels).
        /// </summary>
        float MouseH { get; }

        /// <summary>
        /// Returns the line number of text the mouse is over, usually for field members.
        /// </summary>
        int MouseLine { get; }

        /// <summary>
        /// Returns the (H, V) point location of the mouse on the Stage.
        /// </summary>
        LingoPoint MouseLoc { get; }

        /// <summary>
        /// Returns or sets the cast member underneath the mouse pointer.
        /// </summary>
        LingoMember? MouseMember { get; set; }

        /// <summary>
        /// Returns TRUE on the frame when the mouse button is released.
        /// </summary>
        bool MouseUp { get; }

        /// <summary>
        /// Returns the vertical position of the mouse pointer relative to the Stage (pixels).
        /// </summary>
        float MouseV { get; }

        /// <summary>
        /// Returns the word that the mouse pointer is over, typically in a field.
        /// </summary>
        string MouseWord { get; }

        /// <summary>
        /// Returns TRUE while the right mouse button is held down (Windows only).
        /// </summary>
        bool RightMouseDown { get; }

        /// <summary>
        /// Returns TRUE on the frame when the right mouse button is released (Windows only).
        /// </summary>
        bool RightMouseUp { get; }

        /// <summary>
        /// Returns TRUE if the mouse is still down from a prior frame.
        /// </summary>
        bool StillDown { get; }
        bool LeftMouseDown { get; }
        bool MiddleMouseDown { get; }
    }


    public class LingoMouse : ILingoMouse
    {
        private bool _lastMouseDownState = false; // Previous mouse state (used to detect "StillDown")

        private HashSet<ILingoMouseEventHandler> _subscriptions = new();
        private readonly LingoStage _lingoStage;

        public LingoMember? MouseMember { get; set; }
        public LingoPoint MouseLoc => new LingoPoint(MouseH, MouseV);

        public float MouseH { get; set; }
        public float MouseV { get; set; }
        public bool MouseUp { get; set; }
        public bool MouseDown { get; set; }
        public bool RightMouseUp { get; set; }
        public bool RightMouseDown { get; set; }
        public bool StillDown => MouseDown && _lastMouseDownState;
        public bool DoubleClick { get; set; }
        public char MouseChar => ' ';
        public string MouseWord => "";
        public int MouseLine => 0;
        public LingoSprite? ClickOn => _lingoStage.GetSpriteUnderMouse();

        public bool LeftMouseDown { get; set; }
        public bool MiddleMouseDown { get; set; }


        public LingoMouse(LingoStage lingoMovieStage)
        {
            _lingoStage = lingoMovieStage;
        }

        /// <summary>
        /// Called from communiction framework mouse
        /// </summary>
        public void DoMouseUp() => DoOnAll(x => x.DoMouseUp(this));
        public void DoMouseDown() => DoOnAll(x => x.DoMouseDown(this));
        public void DoMouseMove() => DoOnAll(x => x.DoMouseMove(this));
        private void DoOnAll(Action<ILingoMouseEventHandler> action)
        {
            foreach (var subscription in _subscriptions)
                action(subscription);
        }

        /// <summary>
        /// Subscribe to mouse events
        /// </summary>
        public LingoMouse Subscribe(ILingoMouseEventHandler handler)
        {
            if (_subscriptions.Contains(handler)) return this;
            _subscriptions.Add(handler);
            return this;
        }
        public LingoMouse Unsubscribe(ILingoMouseEventHandler handler)
        {
            _subscriptions.Remove(handler);
            return this;
        }
        // Method to update the mouse state at the end of each frame
        internal void UpdateMouseState()
        {
            _lastMouseDownState = MouseDown;  // Save current mouse state for next frame
        }

        internal bool IsSubscribed(LingoSprite sprite) => _subscriptions.Contains(sprite);
    }
}
