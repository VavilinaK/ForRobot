using System;

namespace ForRobot
{
    public class PropertyChangeCommand<T> : IUndoableCommand
    {
        private readonly BaseClass _target;
        private readonly string _propertyName;
        private readonly T _oldValue;
        private readonly T _newValue;

        public PropertyChangeCommand(BaseClass target, string propertyName, T oldValue, T newValue)
        {
            _target = target;
            _propertyName = propertyName;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public void Execute() => SetValue(_newValue);
        public void Undo() => SetValue(_oldValue);

        private void SetValue(T value)
        {
            var property = _target.GetType().GetProperty(_propertyName);
            property?.SetValue(_target, value);
        }
    }
}
