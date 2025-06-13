using System.Xml.Linq;

namespace LingoEngine
{
    public interface ILingoTimeoutObject
    {
        void Forget();
    }
    /*
    Object-oriented programming with Lingo 63
This statement uses the following elements:
• variableName is the variable you are placing the timeout object into.
• timeout indicates which type of Lingo object you are creating.
• timeoutName is the name you give to the timeout object. This name appears in the
timeOutList. It is the #name property of the object.
• new creates a new object.
• intMilliseconds indicates the frequency with which the timeout object should call the
handler you specify. This is the #period property of the object. For example, a value of 2000
calls the specified handler every 2 seconds.
• #handlerName is the name of the handler you want the object to call. This is the
#timeOutHandler property of the object. You represent it as a symbol by preceding the name
with the # sign. For example, a handler called on accelerate would be specified as
#accelerate.
• targetObject indicates which child object’s handler should be called. This is the #target
property of the object. It allows specificity when many child objects contain the same handlers.
If you omit this parameter, Director looks for the specified handler in the movie script.
The following statement creates a timeout object named timer1 that calls an on accelerate
handler in the child object car1 every 2 seconds:
-- Lingo syntax
myTimer = timeOut("timer1").new(2000, #accelerate, car1)
To determine when the next timeout message will be sent from a particular timeout object, check
its #time property. The value returned is the point in time, in milliseconds, when the next
timeout message will be sent. For example, the following statement determines the time when
the next timeout message will be sent from the timeout object timer1 and displays it in the
Message window:
-- Lingo syntax
put(timeout("timer1").time)
Using timeOutList
When you begin creating timeout objects, you can use timeOutList to check the number of
timeout objects that are active at a particular moment.
The following statement sets the variable x to the number of objects in timeOutList by using the
count property.
-- Lingo syntax
x = _movie.timeoutList.count
You can also refer to an individual timeout object by its number in the list.
The following statement deletes the second timeout object in timeOutList by using the
forget() method.
-- Lingo syntax
timeout(2).forget()
    */

    public class LingoTimeOutList
    {
        private List<ILingoTimeoutObject> _elements = new();
        public int Count { get; private set; }

        public ILingoTimeoutObject New(string name, int period, Action tickAction)
        {
            var obj = new LingoTimeoutObject(name, period, tickAction, (e) => _elements.Remove(e));
            _elements.Add(obj);
            return obj;
        }
        private class LingoTimeoutObject : ILingoTimeoutObject
        {
            private readonly Action _tickAction;
            private readonly Action<LingoTimeoutObject> _forget;

            public string Name { get; }
            /// <summary>
            /// in milliseconds
            /// </summary>
            public int Period { get; }


            public LingoTimeoutObject(string name, int period, Action tickAction, Action<LingoTimeoutObject> forget)
            {
                Name = name;
                Period = period;
                _tickAction = tickAction;
                _forget = forget;
                throw new NotImplementedException("Todo : Tick execution after a chosen timeout");
            }
            public void Tick()
                => _tickAction();

            public void Forget() => _forget(this);
        }
    }
}



