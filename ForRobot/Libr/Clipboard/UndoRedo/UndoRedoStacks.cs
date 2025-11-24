using System;
using System.Collections.Generic;

namespace ForRobot.Libr.Clipboard.UndoRedo
{
    public class UndoRedoStacks
    {
        public Stack<IUndoableCommand> UndoStack { get; } = new Stack<IUndoableCommand>();
        public Stack<IUndoableCommand> RedoStack { get; } = new Stack<IUndoableCommand>();
    }
}
