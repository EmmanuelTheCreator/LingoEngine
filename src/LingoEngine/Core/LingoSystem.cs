namespace LingoEngine
{
    public interface ILingoSystem
    {
        void Wait(float time);  // Wait for a given time (in seconds)
        float CurrentTime { get; }  // Gets the current system time
        string Platform { get; }  // Gets the platform (e.g., Windows, Linux)
        long MemoryUsage { get; }  // Gets current memory usage

        void SetVariable(string name, object value);  // Set global variable
        object GetVariable(string name);  // Get global variable

        void PostMessage(string message);  // Post a system-level message
        void ShowMessage(string message);  // Show a message to the user
    }

    public class LingoSystem : ILingoSystem
    {
        private Dictionary<string, object> _globalVariables = new();

        public void Wait(float time)
        {
            Thread.Sleep((int)(time * 1000));  // Sleep for 'time' seconds
        }

        public float CurrentTime => (float)(DateTime.Now - DateTime.MinValue).TotalSeconds;

        public string Platform => Environment.OSVersion.Platform.ToString();  // Get platform (e.g., Windows, Linux)

        public long MemoryUsage
        {
            get
            {
                // You can get memory usage based on the system's available APIs (using System.Diagnostics or others)
                var process = System.Diagnostics.Process.GetCurrentProcess();
                return process.WorkingSet64;  // Returns memory usage in bytes
            }
        }

        public void SetVariable(string name, object value)
        {
            _globalVariables[name] = value;
        }

        public object? GetVariable(string name)
        {
            return _globalVariables.ContainsKey(name) ? _globalVariables[name] : null;
        }

        public void PostMessage(string message)
        {
            Console.WriteLine($"System message: {message}");  // Simple console message posting
        }

        public void ShowMessage(string message)
        {
            Console.WriteLine(message);  // Show message on the console (or use a custom UI implementation)
        }
    }
}