using LingoEngine.Core;
using LingoEngine.Events;
using System;

namespace LingoEngine.Movies
{
    internal class LingoMovieScriptContainer
    {
        private readonly ILingoMovieEnvironment _movieEnvironment;

        private readonly Dictionary<Type,LingoMovieScript> _movieScripts = new();
        private readonly List<IHasMouseDownEvent> _mouseDownBehaviors = new List<IHasMouseDownEvent>();
        private readonly List<IHasMouseUpEvent> _mouseUpBehaviors = new List<IHasMouseUpEvent>();
        private readonly List<IHasMouseMoveEvent> _mouseMoveBehaviors = new List<IHasMouseMoveEvent>();
        private readonly List<IHasMouseEnterEvent> _mouseEnterBehaviors = new List<IHasMouseEnterEvent>();
        private readonly List<IHasMouseExitEvent> _mouseExitBehaviors = new List<IHasMouseExitEvent>();
        private readonly List<IHasBeginSpriteEvent> _beginSpriteBehaviors = new List<IHasBeginSpriteEvent>();
        private readonly List<IHasEndSpriteEvent> _endSpriteBehaviors = new List<IHasEndSpriteEvent>();
        private readonly List<IHasStepFrameEvent> _stepFrameBehaviors = new List<IHasStepFrameEvent>();
        private readonly List<IHasPrepareFrameEvent> _prepareFrameBehaviors = new List<IHasPrepareFrameEvent>();
        private readonly List<IHasEnterFrameEvent> _enterFrameBehaviors = new List<IHasEnterFrameEvent>();
        private readonly List<IHasExitFrameEvent> _exitFrameBehaviors = new List<IHasExitFrameEvent>();
        private readonly List<IHasFocusEvent> _focusBehaviors = new List<IHasFocusEvent>();
        private readonly List<IHasBlurEvent> _blurBehaviors = new List<IHasBlurEvent>();

        public LingoMovieScriptContainer(ILingoMovieEnvironment movieEnvironment)
        {
            _movieEnvironment = movieEnvironment;
        }
        public void Add<T>() where T : LingoMovieScript
        {
            var ms = _movieEnvironment.Factory.CreateMovieScript<T>((LingoMovie)_movieEnvironment.Movie);
            _movieScripts.Add(typeof(T),ms);

            if (ms is IHasMouseDownEvent mouseDownEvent) _mouseDownBehaviors.Add(mouseDownEvent);
            if (ms is IHasMouseUpEvent mouseUpEvent) _mouseUpBehaviors.Add(mouseUpEvent);
            if (ms is IHasMouseMoveEvent mouseMoveEvent) _mouseMoveBehaviors.Add(mouseMoveEvent);
            if (ms is IHasMouseEnterEvent mouseEnterEvent) _mouseEnterBehaviors.Add(mouseEnterEvent);
            if (ms is IHasMouseExitEvent mouseExitEvent) _mouseExitBehaviors.Add(mouseExitEvent);
            if (ms is IHasBeginSpriteEvent beginSpriteEvent) _beginSpriteBehaviors.Add(beginSpriteEvent);
            if (ms is IHasEndSpriteEvent endSpriteEvent) _endSpriteBehaviors.Add(endSpriteEvent);
            if (ms is IHasStepFrameEvent stepFrameEvent) _stepFrameBehaviors.Add(stepFrameEvent);
            if (ms is IHasPrepareFrameEvent prepareFrameEvent) _prepareFrameBehaviors.Add(prepareFrameEvent);
            if (ms is IHasEnterFrameEvent enterFrameEvent) _enterFrameBehaviors.Add(enterFrameEvent);
            if (ms is IHasExitFrameEvent exitFrameEvent) _exitFrameBehaviors.Add(exitFrameEvent);
            if (ms is IHasFocusEvent focusEvent) _focusBehaviors.Add(focusEvent);
            if (ms is IHasBlurEvent blurEvent) _blurBehaviors.Add(blurEvent);
        }
        public void Remove(LingoMovieScript ms)
        {
            _movieScripts.Remove(ms.GetType());

            if (ms is IHasMouseDownEvent mouseDownEvent) _mouseDownBehaviors.Remove(mouseDownEvent);
            if (ms is IHasMouseUpEvent mouseUpEvent) _mouseUpBehaviors.Remove(mouseUpEvent);
            if (ms is IHasMouseMoveEvent mouseMoveEvent) _mouseMoveBehaviors.Remove(mouseMoveEvent);
            if (ms is IHasMouseEnterEvent mouseEnterEvent) _mouseEnterBehaviors.Remove(mouseEnterEvent);
            if (ms is IHasMouseExitEvent mouseExitEvent) _mouseExitBehaviors.Remove(mouseExitEvent);
            if (ms is IHasBeginSpriteEvent beginSpriteEvent) _beginSpriteBehaviors.Remove(beginSpriteEvent);
            if (ms is IHasEndSpriteEvent endSpriteEvent) _endSpriteBehaviors.Remove(endSpriteEvent);
            if (ms is IHasStepFrameEvent stepFrameEvent) _stepFrameBehaviors.Remove(stepFrameEvent);
            if (ms is IHasPrepareFrameEvent prepareFrameEvent) _prepareFrameBehaviors.Remove(prepareFrameEvent);
            if (ms is IHasEnterFrameEvent enterFrameEvent) _enterFrameBehaviors.Remove(enterFrameEvent);
            if (ms is IHasExitFrameEvent exitFrameEvent) _exitFrameBehaviors.Remove(exitFrameEvent);
            if (ms is IHasFocusEvent focusEvent) _focusBehaviors.Remove(focusEvent);
            if (ms is IHasBlurEvent blurEvent) _blurBehaviors.Remove(blurEvent);
        }
        internal void RaiseMouseDown(ILingoMouse mouse) => _mouseDownBehaviors.ForEach(x => x.MouseDown(mouse));
        internal void RaiseMouseUp(ILingoMouse mouse) => _mouseUpBehaviors.ForEach(x => x.MouseUp(mouse));
        internal void RaiseMouseMove(ILingoMouse mouse) => _mouseMoveBehaviors.ForEach(x => x.MouseMove(mouse));
        internal void RaiseMouseEnter(ILingoMouse mouse) => _mouseEnterBehaviors.ForEach(x => x.MouseEnter(mouse));
        internal void RaiseMouseExit(ILingoMouse mouse) => _mouseExitBehaviors.ForEach(x => x.MouseExit(mouse));
        internal void RaiseBeginSprite() => _beginSpriteBehaviors.ForEach(x => x.BeginSprite());
        internal void RaiseEndSprite() => _endSpriteBehaviors.ForEach(x => x.EndSprite());
        internal void RaiseStepFrame() => _stepFrameBehaviors.ForEach(x => x.StepFrame());
        internal void RaisePrepareFrame() => _prepareFrameBehaviors.ForEach(x => x.PrepareFrame());
        internal void RaiseEnterFrame() => _enterFrameBehaviors.ForEach(x => x.EnterFrame());
        internal void RaiseExitFrame() => _exitFrameBehaviors.ForEach(x => x.ExitFrame());
        internal void RaiseFocus() => _focusBehaviors.ForEach(x => x.Focus());
        internal void RaiseBlur() => _blurBehaviors.ForEach(x => x.Blur());

        internal T? Get<T>() where T : LingoMovieScript
        {
            if (_movieScripts.TryGetValue(typeof(T), out var ms)) return ms as T;
            return null;
        }
    }
}
