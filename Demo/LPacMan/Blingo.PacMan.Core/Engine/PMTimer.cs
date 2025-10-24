namespace Blingo.PacMan.Core.Engine
{
    internal class PMTimer
    {
        private readonly long _waitTime;
        private long _start;
        private long _pauseTimeInSeconds;

        private long TimeStampInMilliSeconds => (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public PMTimer(long waitTimeInSeconds)
        {
            _waitTime = waitTimeInSeconds;
            _start = TimeStampInMilliSeconds;
        }
      
        public void Pause() => _pauseTimeInSeconds = TimeStampInMilliSeconds;
        public void Resume() => _start += TimeStampInMilliSeconds - _pauseTimeInSeconds;

        public long Elapsed => TimeStampInMilliSeconds - _start;
        public bool HasElapsed(long? timeInSeconds = null)
        {
            if (!timeInSeconds.HasValue)
                timeInSeconds = _waitTime;
            return Elapsed > timeInSeconds.Value * 1000;
        }
    }
}
