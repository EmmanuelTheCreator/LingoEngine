using LingoEngine.Inputs;

namespace LingoEngine.Events
{
    //public interface IEventSubscription
    //{
    //    void Release();
    //}
    //public class LingoEventMediator
    //{
    //    private List<Subscription> _subscriptions = new();

    //    public IEventSubscription Subscribe(Action action)
    //    {
    //        var subscription = new Subscription(() => action(), e => _subscriptions.Remove(e));
    //        _subscriptions.Add(subscription);
    //        return subscription;
    //    }
    //    public void Invoke()
    //    {
    //        _subscriptions.ForEach(s => s.Invoke());
    //    }
    //    private class Subscription : IEventSubscription
    //    {
    //        private readonly Action _doIt;
    //        private readonly Func<object, bool> _release;

    //        public Subscription(Action doIt, Func<object, bool> release)
    //        {
    //            _doIt = doIt;
    //            _release = release;
    //        }
    //        public void Invoke() => _doIt();

    //        public void Release()
    //        {
    //            _release(this);
    //        }
    //    }
    //}
    internal interface ILingoMovieScriptListener : ILingoMouseEventHandler
    {
        // Keyboard events
        void DoKeyDown(LingoKey key);
        void DoKeyUp(LingoKey key);

        // Mouse events
        new void DoMouseDown(LingoMouse mouse);
        new void DoMouseUp(LingoMouse mouse);
        new void DoMouseMove(LingoMouse mouse);

        // Movie Script Frame events
        void DoEnterFrame();
        void DoExitFrame();
    }
    internal interface ILingoMovieScriptSubscription
    {
        void Release();
    }
    internal class LingoMovieScriptMediator
    {
        private List<Subscription> _subscriptions = new();

        internal ILingoMovieScriptSubscription Subscribe(ILingoMovieScriptListener objectWithGlobalEvents)
        {
            var subscription = new Subscription(objectWithGlobalEvents, e => _subscriptions.Remove(e));
            _subscriptions.Add(subscription);
            return subscription;
        }
        // Dispatch KeyDown event to all subscribers
        public void KeyDown(LingoKey key) => _subscriptions.ForEach(s => s.KeyDown(key));

        // Dispatch KeyUp event to all subscribers
        public void KeyUp(LingoKey key) => _subscriptions.ForEach(s => s.KeyUp(key));

        // Dispatch MouseDown event to all subscribers
        public void MouseDown(LingoMouse mouse) => _subscriptions.ForEach(s => s.MouseDown(mouse));

        // Dispatch MouseUp event to all subscribers
        public void MouseUp(LingoMouse mouse) => _subscriptions.ForEach(s => s.MouseUp(mouse));

        // Dispatch MouseMove event to all subscribers
        public void MouseMove(LingoMouse mouse) => _subscriptions.ForEach(s => s.MouseMove(mouse));

        // Dispatch EnterFrame event to all subscribers
        public void EnterFrame() => _subscriptions.ForEach(s => s.EnterFrame());

        // Dispatch ExitFrame event to all subscribers
        public void ExitFrame() => _subscriptions.ForEach(s => s.ExitFrame());
        private class Subscription : ILingoMovieScriptSubscription
        {
            private readonly ILingoMovieScriptListener _target;
            private readonly Action<Subscription> _release;

            public Subscription(ILingoMovieScriptListener _target, Action<Subscription> release)
            {
                this._target = _target;
                _release = release;
            }
            public void KeyDown(LingoKey key) => _target.DoKeyDown(key);
            public void KeyUp(LingoKey key) => _target.DoKeyUp(key);
            public void MouseDown(LingoMouse mouse) => _target.DoMouseDown(mouse);
            public void MouseUp(LingoMouse mouse) => _target.DoMouseUp(mouse);
            public void MouseMove(LingoMouse mouse) => _target.DoMouseMove(mouse);
            public void EnterFrame() => _target.DoEnterFrame();
            public void ExitFrame() => _target.DoExitFrame();

            public void Release()
            {
                _release(this);
            }
        }
    }
}
