namespace LingoEngine.Core
{
    public interface ILingoClockListener
    {
        void OnTick();
    }
    public interface ILingoClock
    {
        int FrameRate { get; set; }
        int TickCount { get; }
        int EngineTickCount { get; }

        /// <summary>
        /// Resets the timertick count to 0;
        /// </summary>
        void Reset();
        void Subscribe(ILingoClockListener listener);
        void Unsubscribe(ILingoClockListener listener);
    }
    public class LingoClock : ILingoClock
    {
        public int FrameRate { get; set; } = 30;
        public int TickCount { get; private set; } = 0;
        public int EngineTickCount { get; private set; } = 0;

        private float _accumulatedTime = 0f;
        private readonly List<ILingoClockListener> _listeners = new();

        public void Tick(float deltaTime)
        {
            TickCount++;
            _accumulatedTime += deltaTime;
            float frameTime = 1f / FrameRate;

            while (_accumulatedTime >= frameTime)
            {
                foreach (var l in _listeners) l.OnTick();
                EngineTickCount++;

                _accumulatedTime -= frameTime;
            }
        }

        public void Subscribe(ILingoClockListener listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void Unsubscribe(ILingoClockListener listener)
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
