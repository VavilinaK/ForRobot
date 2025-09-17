using System;

namespace ForRobot
{
    public interface IUndoRedoManager
    {
        /// <summary>
        /// Отмена действия
        /// </summary>
        void Undo();
        /// <summary>
        /// Везврат действия
        /// </summary>
        void Redo();

        bool CanUndo { get; }
        bool CanRedo { get; }

        event EventHandler UndoRedoStateChanged;
    }

    public interface IUndoableCommand
    {
        /// <summary>
        /// Отменить действие
        /// </summary>
        void Undo();

        /// <summary>
        /// Вернуть действие
        /// </summary>
        void Redo();
    }
}
