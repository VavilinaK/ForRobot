using System;
using System.Windows.Input;

namespace ForRobot.Libr.UndoRedo
{
    public interface IUndoableCommand : ICommand
    {
        string Description { get; }

        void Execute();
        void Unexecute();
    }
}
