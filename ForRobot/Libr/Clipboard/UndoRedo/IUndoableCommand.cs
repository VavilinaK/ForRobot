using System;
using System.Windows.Input;

namespace ForRobot.Libr.Clipboard.UndoRedo
{
    public interface IUndoableCommand : ICommand
    {
        string Description { get; }

        void Execute();
        void Unexecute();
    }
}
