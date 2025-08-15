using System;

namespace ForRobot.Libr
{
    /// <summary>
    /// Событие изменения
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueChangedEventArgs<T> : EventArgs
    {
        public T OldValue { get; }
        public T NewValue { get; }

        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
