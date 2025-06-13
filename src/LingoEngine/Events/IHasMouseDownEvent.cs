using LingoEngine.Inputs;
using LingoEngine.Movies;

namespace LingoEngine.Events
{
   

    public interface IHasMouseWithinEvent
    {
        void MouseWithin(ILingoMouse mouse);
    }
    public interface IHasMouseLeaveEvent
    {
        void MouseLeave(ILingoMouse mouse);
    }
    public interface IHasPrepareMovieEvent
    {
        void PrepareMovie();
    }
    public interface IHasStartMovieEvent
    {
        void StartMovie();
    }
    public interface IHasStopMovieEvent
    {
        void StopMovie();
    }
    public interface IHasKeyDownEvent
    {
        void KeyDown(ILingoKey mouse);
    }
    public interface IHasKeyUpEvent
    {
        void KeyUp(ILingoKey mouse);
    }
     public interface IHasMouseDownEvent
    {
        void MouseDown(ILingoMouse mouse);
    }

    public interface IHasMouseUpEvent
    {
        void MouseUp(ILingoMouse mouse);
    }

    public interface IHasMouseMoveEvent
    {
        void MouseMove(ILingoMouse mouse);
    }

    public interface IHasBeginSpriteEvent
    {
        void BeginSprite();
    }

    public interface IHasEndSpriteEvent
    {
        void EndSprite();
    }


    public interface IHasMouseEnterEvent
    {
        void MouseEnter(ILingoMouse mouse);
    }

    public interface IHasMouseExitEvent
    {
        void MouseExit(ILingoMouse mouse);
    }

   

    public interface IHasStepFrameEvent
    {
        void StepFrame();
    }

    public interface IHasPrepareFrameEvent
    {
        void PrepareFrame();
    }

    public interface IHasEnterFrameEvent
    {
        void EnterFrame();
    }

    public interface IHasExitFrameEvent
    {
        void ExitFrame();
    }

    public interface IHasFocusEvent
    {
        void Focus();
    }

    public interface IHasBlurEvent
    {
        void Blur();
    }

    public interface IHasSpriteSelectedEvent
    {
        void SpriteSelected(LingoEngine.Movies.LingoSprite sprite);
    }

}
