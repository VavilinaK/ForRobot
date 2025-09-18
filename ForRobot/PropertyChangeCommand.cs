using System;
using System.Collections.Generic;

namespace ForRobot
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

        public void Undo() => SetValue(_oldValue);
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

    public class CompositeCommand : IUndoableCommand
    {
        private readonly List<IUndoableCommand> _commands = new List<IUndoableCommand>();
        private readonly string _description;

        public CompositeCommand(string description = "")
        {
            _description = description;
        }

        public string Description => _description;

        public void AddCommand(IUndoableCommand command)
        {
            _commands.Add(command);
        }

        public void Execute()
        {
            foreach (var command in _commands)
            {
                command.Execute();
            }
        }

        public void Undo()
        {
            for (int i = _commands.Count - 1; i >= 0; i--)
            {
                _commands[i].Undo();
            }
        }
    }
}
