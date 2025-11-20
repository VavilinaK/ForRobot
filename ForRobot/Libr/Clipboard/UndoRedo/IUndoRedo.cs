using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForRobot.Libr.Clipboard.UndoRedo
{
    public interface IUndoRedo
    {
        bool CanUndo { get; }
        bool CanRedo { get; }

        void Undo();
        void Redo();

        void AddUndoCommand(IUndoableCommand command);
        void ClearUndoRedoHistory();

        event EventHandler UndoRedoStateChanged;
    }
}
