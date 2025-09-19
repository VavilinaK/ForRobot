using System;

namespace ForRobot
{
    public interface IUndoableCommand
    {
        string Description { get; }

        void Execute();
        void Unexecute();
    }
}
