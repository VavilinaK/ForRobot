using System;
using ForRobot.Libr.Clipboard.UndoRedo;

namespace ForRobot.Libr.Clipboard
{
    public class UndoRedoManager : IUndoRedo
    {
        #region Private variables

        private readonly CacheClipboardProvider _clipboardProvider;
        private readonly string _fileKey;
        private readonly UndoRedoStacks _stacks;

        #endregion Privae variables

        #region Public variables
        
        public bool CanUndo => _stacks.UndoStack.Count > 0;
        public bool CanRedo => _stacks.RedoStack.Count > 0;

        public event EventHandler UndoRedoStateChanged;
        
        #endregion Public variables

        public UndoRedoManager(CacheClipboardProvider clipboardProvider, string fileKey)
        {
            _clipboardProvider = clipboardProvider;
            _fileKey = fileKey;
            _stacks = _clipboardProvider.GetOrAddStacks(_fileKey);
        }

        #region Public functions
        
        public void Undo()
        {
            if (!this.CanUndo) return;

            var command = _stacks.UndoStack.Pop();
            command.Unexecute();
            _stacks.RedoStack.Push(command);
            OnUndoRedoStateChanged();
        }

        public void Redo()
        {
            if (!this.CanRedo) return;

            var command = _stacks.RedoStack.Pop();
            command.Execute();
            _stacks.UndoStack.Push(command);
            OnUndoRedoStateChanged();
        }

        public void AddUndoCommand(IUndoableCommand command)
        {
            _stacks.UndoStack.Push(command);
            _stacks.RedoStack.Clear();
            OnUndoRedoStateChanged();
        }

        public void ClearUndoRedoHistory()
        {
            _stacks.UndoStack.Clear();
            _stacks.RedoStack.Clear();
            OnUndoRedoStateChanged();
        }

        protected virtual void OnUndoRedoStateChanged() => UndoRedoStateChanged?.Invoke(this, EventArgs.Empty);

        #endregion Public functions
    }
}
