using System;

namespace ForRobot.Libr.UndoRedo
{
    public interface IUndoableCommand
    {
        string Description { get; }

        void Execute();
        void Unexecute();
    }
}
