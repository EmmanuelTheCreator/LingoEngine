using LingoEngine.FrameworkCommunication;
using LingoEngine.Pictures.LingoEngine;

namespace LingoEngine.Core
{
    public class LingoCursor : ILingoCursor
    {
        private ILingoFrameworkMouse _frameworkObj;
        private int _lastCursor;
        private int _cursor;
        private LingoMouseCursor _cursorType;
        private bool _isCursorVisible = true;
        private LingoMemberPicture? image;
        public int this[int index]
        {
            get => _cursor;
            set => Cursor = value;
        }

        public LingoCursor(ILingoFrameworkMouse frameworkObj)
        {
            _frameworkObj = frameworkObj;
        }
        public LingoMemberPicture? Image { get => image; set { 
                image = value;
                CursorType = LingoMouseCursor.Custom;
            } }
        public int Cursor
        {
            get => _cursor; set
            {
                _cursor = value;
                _lastCursor = value;
                _cursorType = (LingoMouseCursor)value;
                if (value == 100)
                    _frameworkObj.SetCursor(Image);
                else
                    _frameworkObj.SetCursor((LingoMouseCursor)value);
            }
        }
        public LingoMouseCursor CursorType { get => _cursorType; set =>  Cursor = (int)value; }

        public bool IsCursorVisible => _isCursorVisible;

        public void HideCursor()
        {
            _isCursorVisible = false;
            _frameworkObj.HideMouse(true);
        }

        public void ResetCursor() => Cursor = _lastCursor;

        public void ShowCursor()
        {
            _isCursorVisible = true;
            _frameworkObj.HideMouse(false);
        }
    }
}
