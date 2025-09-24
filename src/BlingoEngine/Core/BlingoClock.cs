namespace BlingoEngine.Core
{
    /// <summary>
    /// Lingo Clock Listener interface.
    /// </summary>
    public interface IBlingoClockListener
    {
        void OnTick();
        void OnIdle(float deltaTime);
    }
    /// <summary>
    /// Lingo Clock interface.
    /// </summary>
    public interface IBlingoClock
    {
        int FrameRate { get; set; }
        int TickCount { get; }
        int EngineTickCount { get; }

        /// <summary>
        /// Resets the timertick count to 0;
        /// </summary>
        void Reset();
        void Subscribe(IBlingoClockListener listener);
        void Unsubscribe(IBlingoClockListener listener);
    }
    public class BlingoClock : IBlingoClock
    {
        public int FrameRate { get; set; } = 30;
        public int TickCount { get; private set; } = 0;
        public int EngineTickCount { get; private set; } = 0;

        private float _accumulatedTime = 0f;
        private readonly List<IBlingoClockListener> _listeners = new();

        public void Tick(float deltaTime)
        {
            TickCount++;
            _accumulatedTime += deltaTime;
            float frameTime = 1f / FrameRate;

            int framesDispatched = 0;
            while (_accumulatedTime >= frameTime)
            {
                foreach (var l in _listeners) l.OnTick();
                EngineTickCount++;
                framesDispatched++;
                _accumulatedTime -= frameTime;
            }

            if (framesDispatched == 0)
            {
                if (deltaTime > 0f)
                {
                    foreach (var listener in _listeners)
                        listener.OnIdle(deltaTime);
                }
            }
            else if (_accumulatedTime > 0f)
            {
                var idleDelta = _accumulatedTime;
                foreach (var listener in _listeners)
                    listener.OnIdle(idleDelta);
            }
        }

        public void Subscribe(IBlingoClockListener listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void Unsubscribe(IBlingoClockListener listener)
        {
            _listeners.Remove(listener);
        }
        public void Reset()
        {
            TickCount = 0;
            EngineTickCount = 0;
        }
    }


}

