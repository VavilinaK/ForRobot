using System;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;

using LiveCharts;
using LiveCharts.Wpf;

using ForRobot.Libr;

namespace ForRobot.ViewModels
{
    public class ToolBarViewModel : BaseClass, IDisposable
    {
        #region Private variables

        private Model.Robot _robot;

        #region Commands

        //private RelayCommand _openConnectionCommand;

        private RelayCommand _sendCommand;

        private RelayCommand _deleteRobotCommand;

        private RelayCommand _runRobotCommand;

        private RelayCommand _pauseRobotCommand;

        private RelayCommand _stopRobotCommand;

        private RelayCommand _selectProgrammRobotCommand;

        #endregion

        #endregion

        #region Public variables

        public Model.Robot Robot
        {
            get => this._robot;
            set
            {
                this._robot = value;
                RaisePropertyChanged("Robot");
            }
        }

        public string PathToController
        {
            get => this.Robot.PathControllerField;
            set => this.Robot.PathControllerField = value;
        }

        #region Events

        /// <summary>
        /// Событие отправки программы в директорию робота
        /// </summary>
        public event Func<object, EventArgs, Task> Send;
        /// <summary>
        /// Событие изменения хоста и порта
        /// </summary>
        public event EventHandler HostAndPort;
        /// <summary>
        /// Событие логирования действия
        /// </summary>
        public event EventHandler<LogEventArgs> Log;
        /// <summary>
        /// Событие логирования ошибки
        /// </summary>
        public event EventHandler<LogErrorEventArgs> LogError;

        #endregion

        #endregion

        #region Construct

        public ToolBarViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            //if (string.IsNullOrWhiteSpace(host))
            //    throw new ArgumentNullException("host");


            //if (int.Equals(port, 0))
            //    throw new ArgumentNullException("port");
        }

        #endregion

        #region Private function

        private async Task _OnSend(Func<object, EventArgs, Task> func)
        {
            Func<object, EventArgs, Task> handler = func;

            if (handler == null)
                return;

            Delegate[] invocationList = handler.GetInvocationList();
            Task[] handlerTasks = new Task[invocationList.Length];

            for (int i = 0; i < invocationList.Length; i++)
            {
                handlerTasks[i] = ((Func<object, EventArgs, Task>)invocationList[i])(this, EventArgs.Empty);
            }

            await Task.WhenAll(handlerTasks);
        }

        #endregion

        #region Public functions

        public void OpenConnection(string host, int port, int timeout_milliseconds)
        {
            this.Robot = new Model.Robot(host, port);
            this.Robot.HostAndPort += this.HostAndPort;
            this.Robot.Log += this.Log;
            this.Robot.LogError += this.LogError;
            this.Robot.OpenConnection(timeout_milliseconds);
        }

        public async Task OnSend() => await this._OnSend(this.Send);

        #region Commands

        //public RelayCommand OpenConnectionCommand
        //{
        //    get
        //    {
        //        return _openConnectionCommand ??
        //            (_openConnectionCommand = new RelayCommand(obj =>
        //            {
        //                this.Robot.HostAndPort += this.HostAndPort;
        //                this.Robot.Log += this.Log;
        //                this.Robot.LogError += this.LogError;
        //                this.Robot.OpenConnection();
        //            }));
        //    }
        //}

        public RelayCommand SendCommand
        {
            get
            {
                return _sendCommand ??
                    (_sendCommand = new RelayCommand(obj =>
                    {
                        this.Send.Invoke(this, null);
                    }));
            }
        }

        public RelayCommand DeleteRobotCommand
        {
            get
            {
                return _deleteRobotCommand ??
                    (_deleteRobotCommand = new RelayCommand(obj =>
                    {
                        if (MessageBox.Show($"Удалить робота с соединением {this.Robot.Host}:{this.Robot.Port}?", "Удаление", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                        {
                            Properties.Settings.Default.SaveData.Remove($"{this.Robot.Host}:{this.Robot.Port}");
                            Properties.Settings.Default.Save();
                            this.Dispose();
                            ((Views.Pages.PageMain1)App.Current.MainWindowView.ViewModel.NowPage).ViewModel.Items.Remove((Themes.ToolBarTrayForRobot)obj);
                        }
                    }));
            }
        }

        public RelayCommand RunRobotCommand
        {
            get
            {
                return _runRobotCommand ??
                    (_runRobotCommand = new RelayCommand(obj =>
                    {
                        Task.Run(() => this.Robot.Run());
                    }));
            }
        }

        public RelayCommand PauseRobotCommand
        {
            get
            {
                return _pauseRobotCommand ??
                    (_pauseRobotCommand = new RelayCommand(obj =>
                    {
                        Task.Run(() => this.Robot.Pause());
                    }));
            }
        }

        public RelayCommand StopRobotCommand
        {
            get
            {
                return _stopRobotCommand ??
                    (_stopRobotCommand = new RelayCommand(obj =>
                    {
                        Task.Run(() => this.Robot.Stop());
                    }));
            }
        }

        public RelayCommand SelectRobotCommand
        {
            get
            {
                return _selectProgrammRobotCommand ??
                    (_selectProgrammRobotCommand = new RelayCommand(obj =>
                    {
                        //Task.Run(() => this.Robot.SelectProgramm());
                    }));
            }
        }

        #endregion

        #endregion

        #region Implementations of IDisposable

        ~ToolBarViewModel() => Dispose(false);

        public void Dispose() => this.Dispose(true);

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Robot.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
