using LingoEngine.Core;
using LingoEngine.Events;

namespace LingoEngine.Movies
{
    public class LingoMovieScript : LingoScriptBase, ILingoMovieScriptListener, ILingoMouseEventHandler, IDisposable
    {
        public LingoMovieScript(ILingoMovieEnvironment env) : base(env)
        {
            ((LingoMovie)_env.Movie).SubscribeMovieScript(this);
        }
        public void Dispose()
        {
            ((LingoMovie)_env.Movie).UnsubscribeMovieScript(this);
        }

        void ILingoMovieScriptListener.DoEnterFrame() => EnterFrame();
        protected virtual void EnterFrame() { }

        void ILingoMovieScriptListener.DoExitFrame() => ExitFrame();
        protected virtual void ExitFrame() { }

        void ILingoMovieScriptListener.DoKeyDown(LingoKey key) => KeyDown(key);
        protected virtual void KeyDown(LingoKey key) { }

        void ILingoMovieScriptListener.DoKeyUp(LingoKey key) => KeyUp(key);
        protected virtual void KeyUp(LingoKey key) { }

        void ILingoMovieScriptListener.DoMouseDown(LingoMouse mouse) => MouseDown(mouse);
        protected virtual void MouseDown(LingoMouse key) { }

        void ILingoMovieScriptListener.DoMouseMove(LingoMouse mouse) => MouseMove(mouse);
        protected virtual void MouseMove(LingoMouse key) { }

        void ILingoMovieScriptListener.DoMouseUp(LingoMouse mouse) => MouseUp(mouse);
        protected virtual void MouseUp(LingoMouse key) { }

        void ILingoMouseEventHandler.DoMouseDown(LingoMouse mouse) => MouseDown(mouse);

        void ILingoMouseEventHandler.DoMouseUp(LingoMouse mouse) => MouseUp(mouse);

        void ILingoMouseEventHandler.DoMouseMove(LingoMouse mouse) => MouseMove(mouse);
    }
}
