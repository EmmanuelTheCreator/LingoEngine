using System;
using System.Collections.Generic;

namespace LingoEngine.Director.Core.Tools
{
    public class HistoryManager : IHistoryManager
    {
        private readonly Stack<(Action undo, Action redo)> _undos = new();
        private readonly Stack<(Action undo, Action redo)> _redos = new();

        public bool CanUndo => _undos.Count > 0;
        public bool CanRedo => _redos.Count > 0;

        public void Push(Action undoAction, Action redoAction)
        {
            _undos.Push((undoAction, redoAction));
            _redos.Clear();
        }

        public void Undo()
        {
            if (_undos.Count > 0)
            {
                var pair = _undos.Pop();
                pair.undo();
                _redos.Push(pair);
            }
        }

        public void Redo()
        {
            if (_redos.Count > 0)
            {
                var pair = _redos.Pop();
                pair.redo();
                _undos.Push(pair);
            }
        }
    }
}
