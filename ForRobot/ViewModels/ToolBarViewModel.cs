using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Security.Cryptography;

using LiveCharts;
using LiveCharts.Wpf;

using Microsoft.VisualBasic;

using ForRobot.Libr;

namespace ForRobot.ViewModels
{
    public class ToolBarViewModel : BaseClass, IDisposable
    {
        #region Private variables

        private Model.Robot _robot;

        private OpenFileDialog _openFileDialog { get; set; } = new OpenFileDialog()
        {
            RestoreDirectory = true,
            InitialDirectory = Directory.GetCurrentDirectory(),
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false
        };

        #region Commands

        private RelayCommand _openCodingCommand;

        //private RelayCommand _openConnectionCommand;

        private RelayCommand _pathOfProgramFolderCommand;

        private RelayCommand _sendCommand;

        private RelayCommand _deleteRobotCommand;

        private RelayCommand _runRobotCommand;

        private RelayCommand _pauseRobotCommand;

        private RelayCommand _stopRobotCommand;

        private RelayCommand _selectProgrammRobotCommand;

        #endregion

        #endregion

        #region Public variables

        public Model.Robot Robot { get => this._robot; set => Set(ref this._robot, value); }

        //public string PathToController
        //{
        //    get => this.Robot.PathControllerFolder;
        //    set => this.Robot.PathControllerFolder = value;
        //}

        #region Events

        /// <summary>
        /// Событие отправки программы в директорию робота
        /// </summary>
        public event Func<object, EventArgs, Task> Send;
        /// <summary>
        /// Событие изменения свойств робота
        /// </summary>
        public event EventHandler ChangeRobot;
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
            this.Robot.ChangeRobot += this.ChangeRobot;
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
        //                this.Robot.ChangeRobot += this.ChangeRobot;
        //                this.Robot.Log += this.Log;
        //                this.Robot.LogError += this.LogError;
        //                this.Robot.OpenConnection();
        //            }));
        //    }
        //}

        /// <summary>
        /// Выбор пути до каталога робота
        /// </summary>
        public RelayCommand PathOfProgramFolderCommand
        {
            get
            {
                return _pathOfProgramFolderCommand ??
                    (_pathOfProgramFolderCommand = new RelayCommand(obj =>
                    {
                        using (var fbd = new FolderBrowserDialog())
                        {
                            DialogResult result = fbd.ShowDialog();

                            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                                this.Robot.PathProgramm = fbd.SelectedPath;
                            //{
                            //    string[] files = Directory.GetFiles(fbd.SelectedPath);

                                //    //System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
                                //}
                        }
                    }));
            }
        }

        public RelayCommand OpenCodingCommand
        {
            get
            {
                return _openCodingCommand ??
                    (_openCodingCommand = new RelayCommand(obj =>
                    {
                        if (((ToggleButton)((RoutedEventArgs)obj).OriginalSource).IsChecked ?? true)
                        {
                            StringBuilder Sb = new StringBuilder();
                            using (var hash = SHA256.Create())
                            {
                                Encoding enc = Encoding.UTF8;
                                byte[] result = hash.ComputeHash(enc.GetBytes(Microsoft.VisualBasic.Interaction.InputBox("Введите пин-код", "Управление программой", "",
                                    (int)(App.Current.MainWindowView.Left + (App.Current.MainWindowView.Width / 2) - 200),
                                    (int)(App.Current.MainWindowView.Top + (App.Current.MainWindowView.Height / 2) - 100))));

                                foreach (byte b in result)
                                    Sb.Append(b.ToString("x2"));
                            }
                            if (!Equals(Sb.ToString(), Properties.Settings.Default.PinCode))
                                ((ToggleButton)((RoutedEventArgs)obj).OriginalSource).IsChecked = false;
                        }
                    }));
            }
        }

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
                        if (!this.Robot.IsConnection || System.Windows.MessageBox.Show($"Удалить робота с соединением {this.Robot.Host}:{this.Robot.Port}?", "Удаление", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                        {
                            Properties.Settings.Default.SaveConnection.Remove($"{this.Robot.Host}:{this.Robot.Port}");
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
