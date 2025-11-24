using System;
using System.Collections.Generic;

namespace ForRobot.Libr.Clipboard.UndoRedo
{
    public class PropertyChangeCommand<T> : IUndoableCommand
    {
        private readonly object _target;
        private readonly string _propertyName;
        private readonly T _oldValue;
        private readonly T _newValue;

        public string Description { get; }

        public PropertyChangeCommand(object target, string propertyName, T oldValue, T newValue, string description = "")
        {
            _target = target;
            _propertyName = propertyName;
            _oldValue = oldValue;
            _newValue = newValue;
            Description = description;
        }

        public event EventHandler CanExecuteChanged;
        //{
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}            

        public bool CanExecute(object parameter) => true;

        public void Execute() => this.SetValue(_oldValue);
        public void Execute(object parameter) => this.Execute();

        public void Unexecute() => this.SetValue(_newValue);

        private void SetValue(T value)
        {
            var property = _target.GetType().GetProperty(_propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(_target, value);
            }
        }
    }
}
