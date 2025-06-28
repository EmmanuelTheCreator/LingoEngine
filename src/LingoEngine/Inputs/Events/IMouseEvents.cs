
namespace LingoEngine.Inputs.Events
{
    public interface IHasMouseWithinEvent
    {
        void MouseWithin(ILingoMouse mouse);
    }
    public interface IHasMouseLeaveEvent
    {
        void MouseLeave(ILingoMouse mouse);
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


    public interface IHasMouseEnterEvent
    {
        void MouseEnter(ILingoMouse mouse);
    }

    public interface IHasMouseExitEvent
    {
        void MouseExit(ILingoMouse mouse);
    }
}
