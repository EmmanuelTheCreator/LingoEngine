namespace LingoEngine
{
    public interface ILingoMouseEventHandler
    {
        void DoMouseDown(LingoMouse mouse);
        void DoMouseUp(LingoMouse mouse);
        void DoMouseMove(LingoMouse mouse);
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

        public void DoMouseDown(LingoMouse mouse) => _target.DoMouseDown(mouse);
        public void DoMouseUp(LingoMouse mouse) => _target.DoMouseUp(mouse);
        public void DoMouseMove(LingoMouse mouse) => _target.DoMouseMove(mouse);

        public void Release()
        {
            _release(this);
        }
    }
}
