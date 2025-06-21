using System;
namespace LingoEngine.Director.Core.Stages
{
    public interface IHistoryManager
    {
        void Push(Action undoAction);
        void Undo();
    }
}
