using BlingoEngine.Movies.Events;

namespace BlingoEngine.Core
{
    public class ActorList
    {
        private List<IHasStepFrameEvent> _actors = new();
        public void Add(IHasStepFrameEvent actor) => _actors.Add(actor);
        public void Append(IHasStepFrameEvent actor) => _actors.Add(actor);

        public void Clear()
        {
            _actors.Clear();
        }

        public int GetPos(IHasStepFrameEvent actor) => _actors.IndexOf(actor)+1;


        public void Remove(IHasStepFrameEvent actor) => _actors.Remove(actor);
        public void DeleteOne(IHasStepFrameEvent actor) => _actors.Remove(actor);
        public void Delete(IHasStepFrameEvent actor) => _actors.Remove(actor);

        internal void Invoke()
        {
            foreach (var actor in _actors.ToArray()) // make a copy of the array so that it can be modified during scripts.
                actor.StepFrame();
        }
    }
}

