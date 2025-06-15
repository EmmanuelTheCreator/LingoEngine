using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Director.ScummVM
{
    using System.Collections.Generic;

    /// <summary>
    /// Global configuration manager for runtime debug and engine options.
    /// </summary>
    public static class ConfMan
    {
        private static readonly Dictionary<string, object> _options = new();

        /// <summary>
        /// Gets a boolean config option by name.
        /// </summary>
        public static bool GetBool(string key)
        {
            if (_options.TryGetValue(key, out var value) && value is bool b)
                return b;
            return false;
        }

        /// <summary>
        /// Sets a boolean config option by name.
        /// </summary>
        public static void SetBool(string key, bool value)
        {
            _options[key] = value;
        }

        /// <summary>
        /// Clears all config options.
        /// </summary>
        public static void Clear() => _options.Clear();
    }

}
