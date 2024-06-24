using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace ForRobot
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object param);
    }

    public class AsyncRelayCommand : IAsyncCommand
    {
        #region Private variables

        private bool _isExecuting;
        private readonly Func<object, Task> _execute;
        private readonly Func<object, bool> _canExecute;
        private readonly Action<Exception> _exceptionCallback;
        private readonly Dispatcher _dispatcher;

        #endregion

        #region Public variables
        
        #region Event

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        #endregion

        #endregion

        #region Constructors
        
        public AsyncRelayCommand(Func<object, Task> execute, Action<Exception> exceptionCallback) : this(execute, null, exceptionCallback) { }

        public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute, Action<Exception> exceptionCallback)
        {
            _execute = execute;
            _canExecute = canExecute;
            _exceptionCallback = exceptionCallback;
            _dispatcher = App.Current.Dispatcher;
        }

        #endregion

        #region Public functions

        private void InvalidateRequerySuggested()
        {
            if (_dispatcher.CheckAccess())
                CommandManager.InvalidateRequerySuggested();
            else
                _dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        public bool CanExecute(object param) => !_isExecuting && (_canExecute == null || _canExecute(param));

        public async Task ExecuteAsync(object param)
        {
            if (CanExecute(param))
            {
                try
                {
                    _isExecuting = true;
                    InvalidateRequerySuggested();
                    await _execute(param);
                }
                catch (Exception ex)
                {
                    await _dispatcher.BeginInvoke(_exceptionCallback, ex);
                }
                finally
                {
                    _isExecuting = false;
                    InvalidateRequerySuggested();
                }
            }
        }

        public void Execute(object param) => _ = ExecuteAsync(param);

        #endregion
    }
}
