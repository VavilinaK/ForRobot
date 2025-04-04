using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

namespace ForRobot
{
    /// <summary>Базовый класс с реализацией <see cref="INotifyPropertyChanged"/>.</summary>
    public abstract class BaseClass : INotifyPropertyChanged
    {
        #region Private variables

        //private readonly Stack<IUndoableCommand> _undoStack = new Stack<IUndoableCommand>();
        //private readonly Stack<IUndoableCommand> _redoStack = new Stack<IUndoableCommand>();

        private readonly Stack<IUndoableCommand> _undoStack = App.Current.UndoStack;
        private readonly Stack<IUndoableCommand> _redoStack = App.Current.RedoStack;

        private bool CanUndo() => _undoStack.Count > 0;
        private bool CanRedo() => _redoStack.Count > 0;

        #endregion Private variables

        #region Public variables

        [JsonIgnore]
        public ICommand UndoCommand { get; }
        [JsonIgnore]
        public ICommand RedoCommand { get; }

        #endregion Public variables

        #region Constructor

        public BaseClass()
        {
            this.UndoCommand = new RelayCommand(_ => Undo(), _ => CanUndo());
            this.RedoCommand = new RelayCommand(_ => Redo(), _ => CanRedo());
        }

        #endregion

        #region Private functions

        private void Undo()
        {
            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            CommandManager.InvalidateRequerySuggested();
        }

        private void Redo()
        {
            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion Private functions

        /// <inheritdoc cref="INotifyPropertyChanged"/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Защищённый метод для создания события <see cref="PropertyChanged"/>.</summary>
        /// <param name="propertyName">Имя изменившегося свойства. 
        /// Если значение не задано, то используется имя метода в котором был вызов.</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>Защищённый метод для создания события <see cref="PropertyChanged"/> множества свойств.</summary>
        /// <param name="propertyName">Массим имён изменённых свойств. 
        /// Если значение не задано, то используется имя метода в котором был вызов.</param>
        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            foreach (var prop in propertyNames)
            {
                this.RaisePropertyChanged(prop);
            }
        }

        /// <summary>Защищённый метод для присвоения значения полю и
        /// создания события <see cref="PropertyChanged"/>.</summary>
        /// <typeparam name="T">Тип поля и присваиваемого значения.</typeparam>
        /// <param name="propertyFiled">Ссылка на поле.</param>
        /// <param name="newValue">Присваиваемое значение.</param>
        /// <param name="trackUndo">
        /// <param name="propertyName">Имя изменившегося свойства. 
        /// Если значение не задано, то используется имя метода в котором был вызов.</param>
        /// <remarks>Метод предназначен для использования в сеттере свойства.<br/>
        /// Для проверки на изменение используется метод <see cref="object.Equals(object, object)"/>.
        /// Если присваиваемое значение не эквивалентно значению поля, то оно присваивается полю.<br/>
        /// После присвоения создаётся событие <see cref="PropertyChanged"/> вызовом
        /// метода <see cref="RaisePropertyChanged(string)"/>
        /// с передачей ему параметра <paramref name="propertyName"/>.<br/>
        /// После создания события вызывается метод <see cref="OnPropertyChanged(string, object, object)"/>.</remarks>
        protected void Set<T>(ref T propertyFiled, T newValue, bool trackUndo = true, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(propertyFiled, newValue))
                return;

            T oldValue = propertyFiled;
            propertyFiled = newValue;
            RaisePropertyChanged(propertyName);

            if (trackUndo) TrackUndo(oldValue, newValue, propertyName);

            OnPropertyChanged(propertyName, oldValue, newValue);
        }

        /// <summary>Защищённый виртуальный метод вызывается после присвоения значения
        /// свойству и после создания события <see cref="PropertyChanged"/>.</summary>
        /// <param name="propertyName">Имя изменившегося свойства.</param>
        /// <param name="oldValue">Старое значение свойства.</param>
        /// <param name="newValue">Новое значение свойства.</param>
        /// <remarks>Переопределяется в производных классах для реализации
        /// реакции на изменение значения свойства.<br/>
        /// Рекомендуется в переопределённом методе первым оператором вызывать базовый метод.<br/>
        /// Если в переопределённом методе не будет вызова базового, то возможно нежелательное изменение логики базового класса.</remarks>
        protected virtual void OnPropertyChanged(string propertyName, object oldValue, object newValue) { }

        protected void TrackUndo<T>(T oldValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            var command = new PropertyChangeCommand<T>(this, propertyName, oldValue, newValue);
            _undoStack.Push(command);
            _redoStack.Clear();
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
