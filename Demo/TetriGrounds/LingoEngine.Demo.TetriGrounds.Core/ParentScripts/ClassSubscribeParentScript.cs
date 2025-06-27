using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.Movies;
using System.Reflection;

namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 19_ClassSubscibe.ls
    public class ClassSubscribeParentScript : LingoParentScript
    {
        private readonly List<object> mySubscribers = new();
        private readonly List<Dictionary<string, string>> mySubscribersData = new();

        public ClassSubscribeParentScript(ILingoMovieEnvironment env) : base(env) { }

        public int Subscribe(object obj, string function)
        {
            if (mySubscribers.Contains(obj))
                return -1;
            mySubscribers.Add(obj);
            mySubscribersData.Add(new Dictionary<string, string> { ["function"] = function });
            return mySubscribers.Count; // 1-based like Lingo
        }

        public IReadOnlyList<object> SubscribersGetAll() => mySubscribers;

        public object? SubscribersGetById(int val)
        {
            if (val < 1 || val > mySubscribers.Count) return null;
            return mySubscribers[val - 1];
        }

        public void ExecuteAllSubscribed(string data)
        {
                throw new NotImplementedException();
            //for (int i = 0; i < mySubscribers.Count; i++)
            //{
            //    object obj = mySubscribers[i];
            //    string function = mySubscribersData[i]["function"];
            //    if (obj is IHasLingoMessage msg)
            //    {
            //        msg.HandleMessage(function, data);
            //    }
            //    MethodInfo? mi = obj.GetType().GetMethod("ReturnFromSaveScore", BindingFlags.Public | BindingFlags.Instance);
            //    mi?.Invoke(obj, new object[] { data });
            //}
        }

        public void SubscribersDestroy()
        {
            mySubscribers.Clear();
            mySubscribersData.Clear();
        }
    }
}
