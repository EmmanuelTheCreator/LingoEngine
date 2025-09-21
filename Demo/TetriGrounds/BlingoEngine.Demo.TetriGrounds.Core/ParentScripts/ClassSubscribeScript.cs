// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;
using BlingoEngine.Movies;

namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 19_ClassSubscibe.ls
    /// <summary>
    /// Lightweight publish/subscribe helper mirroring the Lingo behaviour used for score callbacks.
    /// </summary>
    public class ClassSubscribeScript : BlingoParentScript
    {
        private readonly List<object> mySubscribers = new();
        private readonly List<Dictionary<string, Action<object?>>> mySubscribersData = new();

        public ClassSubscribeScript(IBlingoMovieEnvironment env) : base(env) { }

        /// <summary>
        /// Adds a subscriber and associates a callback that should be invoked when messages are dispatched.
        /// </summary>
        public int Subscribe(object obj, Action<object?> function)
        {
            if (mySubscribers.Contains(obj))
                return -1;
            mySubscribers.Add(obj);
            mySubscribersData.Add(new Dictionary<string, Action<object?>> { ["function"] = function });
            return mySubscribers.Count; // 1-based like Lingo
        }

        /// <summary>
        /// Returns the list of subscribers. Mainly used by debugging helpers.
        /// </summary>
        public IReadOnlyList<object> SubscribersGetAll() => mySubscribers;

        /// <summary>
        /// Retrieves a single subscriber by the original 1-based identifier.
        /// </summary>
        public object? SubscribersGetById(int val)
        {
            if (val < 1 || val > mySubscribers.Count) return null;
            return mySubscribers[val - 1];
        }

        /// <summary>
        /// Invokes all registered callbacks with the provided data object.
        /// </summary>
        public void ExecuteAllSubscribed(object? data)
        {
            for (int i = 0; i < mySubscribers.Count; i++)
            {
                object obj = mySubscribers[i];
                Action<object?> function = mySubscribersData[i]["function"];
                function(data);
            }
        }

        /// <summary>
        /// Removes every subscriber and clears associated data.
        /// </summary>
        public void SubscribersDestroy()
        {
            mySubscribers.Clear();
            mySubscribersData.Clear();
        }
    }
}

