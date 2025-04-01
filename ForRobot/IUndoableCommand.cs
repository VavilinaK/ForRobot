using System;

namespace ForRobot
{
    public interface IUndoableCommand
    {
        /// <summary>
        /// Отменить действие
        /// </summary>
        void Execute();

        /// <summary>
        /// Вернуть действие
        /// </summary>
        void Undo();
    }
}
