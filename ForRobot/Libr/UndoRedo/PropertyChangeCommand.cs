using System;
using System.Collections.Generic;

namespace ForRobot.Libr.UndoRedo
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

        public void Unexecute() => SetValue(_oldValue);
        public void Execute() => SetValue(_newValue);

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
