namespace LingoEngine.Core
{
    public interface ILingoSystem
    {
        /// <summary>
        /// Wait for a given time (in seconds)
        /// </summary>
        /// <param name="time"></param>
        void Wait(float time);
        /// <summary>
        /// Gets the current system time
        /// </summary>
        float CurrentTime { get; }
        /// <summary>
        /// Gets the platform (e.g., Windows, Linux)
        /// </summary>
        string Platform { get; }
        /// <summary>
        /// Gets current memory usage
        /// </summary>
        long MemoryUsage { get; }
        /// <summary>
        /// Set global variable
        /// </summary>
        void SetVariable(string name, object value);
        /// <summary>
        ///  Get global variable
        /// </summary>
        T? GetVariable<T>(string name);
        /// <summary>
        ///  Get global variable
        /// </summary>
        object? GetVariable(string name);  
        /// <summary>
        /// Post a system-level message
        /// </summary>
        /// <param name="message"></param>
        void PostMessage(string message);  
        /// <summary>
        /// Show a message to the user
        /// </summary>
        /// <param name="message"></param>
        void ShowMessage(string message);  
    }

    public class LingoSystem : ILingoSystem
    {
        private Dictionary<string, object> _globalVariables = new();

        public void Wait(float time)
        {
            throw new NotImplementedException("Wait functionality is not implemented yey, need a non blocking systems that stops the clock and not the thread.");
            //Thread.Sleep((int)(time * 1000));  // Sleep for 'time' seconds
        }

        public float CurrentTime => (float)(DateTime.Now - DateTime.MinValue).TotalSeconds;

        public string Platform => Environment.OSVersion.Platform.ToString();  // Get platform (e.g., Windows, Linux)

        public long MemoryUsage
        {
            get
            {
                // TODO: move this to framework implementation, to keep System.Diagnostics nmespace out of the library
                // You can get memory usage based on the system's available APIs (using System.Diagnostics or others)
                var process = System.Diagnostics.Process.GetCurrentProcess();
                return process.WorkingSet64;  // Returns memory usage in bytes
            }
        }

        public void SetVariable(string name, object value)
        {
            _globalVariables[name] = value;
        }

        public T? GetVariable<T>(string name) => GetVariable(name) is T value ? value : default;
        public object? GetVariable(string name)
        {
            return _globalVariables.ContainsKey(name) ? _globalVariables[name] : null;
        }

        public void PostMessage(string message)
        {
            // TODO: move to framework
            Console.WriteLine($"System message: {message}");  // Simple console message posting
        }

        public void ShowMessage(string message)
        {
            // TODO: move to framework
            Console.WriteLine(message);  // Show message on the console (or use a custom UI implementation)
        }
    }
}