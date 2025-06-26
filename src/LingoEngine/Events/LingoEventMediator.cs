using LingoEngine.Inputs;
using LingoEngine.Movies;

namespace LingoEngine.Events
{

    public class LingoEventMediator : ILingoEventMediator, ILingoMouseEventHandler, ILingoKeyEventHandler, ILingoSpriteEventHandler
    {
        private readonly List<IHasPrepareMovieEvent> _prepareMovies = new();
        private readonly List<IHasStartMovieEvent> _startMovies = new();
        private readonly List<IHasStopMovieEvent> _stopMovies = new();
        private readonly List<IHasMouseDownEvent> _mouseDowns = new();
        private readonly List<IHasMouseUpEvent> _mouseUps = new();
        private readonly List<IHasMouseMoveEvent> _mouseMoves = new();
        private readonly List<IHasMouseEnterEvent> _mouseEnters = new();
        private readonly List<IHasMouseExitEvent> _mouseExits = new();
        //private readonly List<IHasBeginSpriteEvent> _beginSprites = new();
        //private readonly List<IHasEndSpriteEvent> _endSprites = new();
        private readonly List<IHasStepFrameEvent> _stepFrames = new();
        private readonly List<IHasPrepareFrameEvent> _prepareFrames = new();
        private readonly List<IHasEnterFrameEvent> _enterFrames = new();
        private readonly List<IHasExitFrameEvent> _exitFrames = new();
        private readonly List<IHasFocusEvent> _focuss = new();
        private readonly List<IHasBlurEvent> _blurs = new();
        private readonly List<IHasKeyUpEvent> _keyUps = new();
        private readonly List<IHasKeyDownEvent> _keyDowns = new();


        public void Subscribe(object ms)
        {
            if (ms is IHasPrepareMovieEvent prepareMovieEvent) _prepareMovies.Add(prepareMovieEvent);
            if (ms is IHasStartMovieEvent startMovieEvent) _startMovies.Add(startMovieEvent);
            if (ms is IHasStopMovieEvent stopMovieEvent) _stopMovies.Add(stopMovieEvent);
            if (ms is IHasMouseDownEvent mouseDownEvent) _mouseDowns.Add(mouseDownEvent);
            if (ms is IHasMouseUpEvent mouseUpEvent) _mouseUps.Add(mouseUpEvent);
            if (ms is IHasMouseMoveEvent mouseMoveEvent) _mouseMoves.Add(mouseMoveEvent);
            if (ms is IHasMouseEnterEvent mouseEnterEvent) _mouseEnters.Add(mouseEnterEvent);
            if (ms is IHasMouseExitEvent mouseExitEvent) _mouseExits.Add(mouseExitEvent);
            //if (ms is IHasBeginSpriteEvent beginSpriteEvent) _beginSprites.Add(beginSpriteEvent);
            //if (ms is IHasEndSpriteEvent endSpriteEvent) _endSprites.Add(endSpriteEvent);
            if (ms is IHasStepFrameEvent stepFrameEvent) _stepFrames.Add(stepFrameEvent);
            if (ms is IHasPrepareFrameEvent prepareFrameEvent) _prepareFrames.Add(prepareFrameEvent);
            if (ms is IHasEnterFrameEvent enterFrameEvent) _enterFrames.Add(enterFrameEvent);
            if (ms is IHasExitFrameEvent exitFrameEvent) _exitFrames.Add(exitFrameEvent);
            if (ms is IHasFocusEvent focusEvent) _focuss.Add(focusEvent);
            if (ms is IHasBlurEvent blurEvent) _blurs.Add(blurEvent);
            if (ms is IHasKeyUpEvent keyUpEvent) _keyUps.Add(keyUpEvent);
            if (ms is IHasKeyDownEvent keyDownEvent) _keyDowns.Add(keyDownEvent);
        }
        public void Unsubscribe(object ms)
        {
            if (ms is IHasPrepareMovieEvent prepareMovieEvent) _prepareMovies.Remove(prepareMovieEvent);
            if (ms is IHasStartMovieEvent startMovieEvent) _startMovies.Remove(startMovieEvent);
            if (ms is IHasStopMovieEvent stopMovieEvent) _stopMovies.Remove(stopMovieEvent);
            if (ms is IHasMouseDownEvent mouseDownEvent) _mouseDowns.Remove(mouseDownEvent);
            if (ms is IHasMouseUpEvent mouseUpEvent) _mouseUps.Remove(mouseUpEvent);
            if (ms is IHasMouseMoveEvent mouseMoveEvent) _mouseMoves.Remove(mouseMoveEvent);
            if (ms is IHasMouseEnterEvent mouseEnterEvent) _mouseEnters.Remove(mouseEnterEvent);
            if (ms is IHasMouseExitEvent mouseExitEvent) _mouseExits.Remove(mouseExitEvent);
            //if (ms is IHasBeginSpriteEvent beginSpriteEvent) _beginSprites.Remove(beginSpriteEvent);
            //if (ms is IHasEndSpriteEvent endSpriteEvent) _endSprites.Remove(endSpriteEvent);
            if (ms is IHasStepFrameEvent stepFrameEvent) _stepFrames.Remove(stepFrameEvent);
            if (ms is IHasPrepareFrameEvent prepareFrameEvent) _prepareFrames.Remove(prepareFrameEvent);
            if (ms is IHasEnterFrameEvent enterFrameEvent) _enterFrames.Remove(enterFrameEvent);
            if (ms is IHasExitFrameEvent exitFrameEvent) _exitFrames.Remove(exitFrameEvent);
            if (ms is IHasFocusEvent focusEvent) _focuss.Remove(focusEvent);
            if (ms is IHasBlurEvent blurEvent) _blurs.Remove(blurEvent);
            if (ms is IHasKeyUpEvent keyUpEvent) _keyUps.Remove(keyUpEvent);
            if (ms is IHasKeyDownEvent keyDownEvent) _keyDowns.Remove(keyDownEvent);
        }
        internal void RaisePrepareMovie() => _prepareMovies.ForEach(x => x.PrepareMovie());
        internal void RaiseStartMovie() => _startMovies.ForEach(x => x.StartMovie());
        internal void RaiseStopMovie() => _stopMovies.ForEach(x => x.StopMovie());
        public void RaiseMouseDown(LingoMouse mouse) => _mouseDowns.ForEach(x => x.MouseDown(mouse));
        public void RaiseMouseUp(LingoMouse mouse) => _mouseUps.ForEach(x => x.MouseUp(mouse));
        public void RaiseMouseMove(LingoMouse mouse) => _mouseMoves.ForEach(x => x.MouseMove(mouse));
        internal void RaiseMouseEnter(ILingoMouse mouse) => _mouseEnters.ForEach(x => x.MouseEnter(mouse));
        internal void RaiseMouseExit(ILingoMouse mouse) => _mouseExits.ForEach(x => x.MouseExit(mouse));
        //internal void RaiseBeginSprite() => _beginSprites.ForEach(x => x.BeginSprite());
        //internal void RaiseEndSprite() => _endSprites.ForEach(x => x.EndSprite());
        internal void RaiseStepFrame() => _stepFrames.ForEach(x => x.StepFrame());
        internal void RaisePrepareFrame() => _prepareFrames.ForEach(x => x.PrepareFrame());
        internal void RaiseEnterFrame() => _enterFrames.ForEach(x => x.EnterFrame());
        internal void RaiseExitFrame() => _exitFrames.ForEach(x => x.ExitFrame());
        public void RaiseFocus() => _focuss.ForEach(x => x.Focus());
        public void RaiseBlur() => _blurs.ForEach(x => x.Blur());
        public void RaiseKeyUp(LingoKey key) => _keyUps.ForEach(x => x.KeyUp(key));
        public void RaiseKeyDown(LingoKey key) => _keyDowns.ForEach(x => x.KeyDown(key));


    }
}
