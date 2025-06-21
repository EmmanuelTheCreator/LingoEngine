using System;
using System.Collections.Generic;

namespace LingoEngine.Director.Core.Stages
{
    public class HistoryManager : IHistoryManager
    {
        private readonly Stack<Action> _undos = new();

        public void Push(Action undoAction) => _undos.Push(undoAction);

        public void Undo()
        {
            if (_undos.Count > 0)
            {
                var action = _undos.Pop();
                action();
            }
        }
    }
}
