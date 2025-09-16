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
            //var property = _target.GetType().GetProperty(_propertyName);
            var property = GetPropertyInfo(_propertyName);
            property?.SetValue(_target, value);
        }

        private System.Reflection.PropertyInfo GetPropertyInfo(string propertyName)
        {
            System.Reflection.PropertyInfo property = null;
            Type type = this._target.GetType();
            foreach (var item in propertyName.Split('.'))
            {
                property = type.GetProperty(item);
                type = property.PropertyType;
            }
            return property;
        }
    }
}
