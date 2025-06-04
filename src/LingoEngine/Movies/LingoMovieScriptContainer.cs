using LingoEngine.Events;

namespace LingoEngine.Movies
{
    internal class LingoMovieScriptContainer
    {
        private readonly ILingoMovieEnvironment _movieEnvironment;

        private readonly List<LingoMovieScript> _movieScripts = new();
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
            var behavior = _movieEnvironment.Factory.CreateMovieScript<T>();
            _movieScripts.Add(behavior);

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
        }
        public void Remove(LingoMovieScript behavior)
        {
            _movieScripts.Remove(behavior);

            if (behavior is IHasMouseDownEvent mouseDownEvent) _mouseDownBehaviors.Remove(mouseDownEvent);
            if (behavior is IHasMouseUpEvent mouseUpEvent) _mouseUpBehaviors.Remove(mouseUpEvent);
            if (behavior is IHasMouseMoveEvent mouseMoveEvent) _mouseMoveBehaviors.Remove(mouseMoveEvent);
            if (behavior is IHasMouseEnterEvent mouseEnterEvent) _mouseEnterBehaviors.Remove(mouseEnterEvent);
            if (behavior is IHasMouseExitEvent mouseExitEvent) _mouseExitBehaviors.Remove(mouseExitEvent);
            if (behavior is IHasBeginSpriteEvent beginSpriteEvent) _beginSpriteBehaviors.Remove(beginSpriteEvent);
            if (behavior is IHasEndSpriteEvent endSpriteEvent) _endSpriteBehaviors.Remove(endSpriteEvent);
            if (behavior is IHasStepFrameEvent stepFrameEvent) _stepFrameBehaviors.Remove(stepFrameEvent);
            if (behavior is IHasPrepareFrameEvent prepareFrameEvent) _prepareFrameBehaviors.Remove(prepareFrameEvent);
            if (behavior is IHasEnterFrameEvent enterFrameEvent) _enterFrameBehaviors.Remove(enterFrameEvent);
            if (behavior is IHasExitFrameEvent exitFrameEvent) _exitFrameBehaviors.Remove(exitFrameEvent);
            if (behavior is IHasFocusEvent focusEvent) _focusBehaviors.Remove(focusEvent);
            if (behavior is IHasBlurEvent blurEvent) _blurBehaviors.Remove(blurEvent);
        }
    }
}
