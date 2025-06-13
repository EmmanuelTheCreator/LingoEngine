using LingoEngine.Inputs;

namespace LingoEngine.Events
{
    public interface ILingoMouseEventHandler
    {
        void RaiseMouseDown(LingoMouse mouse);
        void RaiseMouseUp(LingoMouse mouse);
        void RaiseMouseMove(LingoMouse mouse);
    }
    public interface ILingoMouseEventSubscription
    {
        void Release();
    }

    public class LingoMouseEventSubscription : ILingoMouseEventSubscription
    {
        private readonly ILingoMouseEventHandler _target;
        private readonly Action<LingoMouseEventSubscription> _release;

        public LingoMouseEventSubscription(ILingoMouseEventHandler target, Action<LingoMouseEventSubscription> release)
        {
            _target = target;
            _release = release;
        }

        public void DoMouseDown(LingoMouse mouse) => _target.RaiseMouseDown(mouse);
        public void DoMouseUp(LingoMouse mouse) => _target.RaiseMouseUp(mouse);
        public void DoMouseMove(LingoMouse mouse) => _target.RaiseMouseMove(mouse);

        public void Release()
        {
            _release(this);
        }
    }
}
