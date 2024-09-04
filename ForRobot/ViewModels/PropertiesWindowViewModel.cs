using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Configuration;
using System.ComponentModel;
using System.Security.Cryptography;

namespace ForRobot.ViewModels
{
    public class PropertiesWindowViewModel : BaseClass
    {
        #region Private variables

        #region Commands

        private RelayCommand _saveCommand;

        private RelayCommand _cancelCommand;
        
        private RelayCommand _editPathForUpdateCommand;

        private RelayCommand _editPinCodeCommand;

        #endregion

        #endregion

        #region Public variables

        public ForRobot.Libr.Settings.Settings Settings { get; set; } = ForRobot.Libr.Settings.Settings.GetSettings();

        #region Commands

        /// <summary>
        /// Сохранение настроек
        /// </summary>
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                    (_saveCommand = new RelayCommand(obj =>
                    {
                        this.Settings.Save();
                        if(MessageBox.Show("Чтобы изменения вступили в силу, необходимо перезапустить приложение.\n\nПерезапустить интерфейс?", "Сохранение настроек", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes)
                        {
                            string sAppPath = Directory.GetCurrentDirectory();
                            System.Diagnostics.Process process = new System.Diagnostics.Process()
                            {
                                StartInfo = new System.Diagnostics.ProcessStartInfo()
                                {
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    WorkingDirectory = sAppPath,
                                    CreateNoWindow = true,
                                    FileName = "cmd.exe",
                                    Arguments = $"/K taskkill /im {Application.ResourceAssembly.GetName().Name}.exe /f& START \"\" \"{sAppPath + "\\" + Application.ResourceAssembly.GetName().Name + ".exe"}\"",
                                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                                }
                            };
                            new System.Threading.Thread(() => process.Start()).Start();
                        }
                        App.Current.PropertiesWindow.Close();
                    }));
            }
        }

        /// <summary>
        /// Закрытие окна
        /// </summary>
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ??
                    (_cancelCommand = new RelayCommand(obj =>
                    {
                        App.Current.PropertiesWindow.Close();
                    }));
            }
        }

        /// <summary>
        /// Изменение директивы каталога с новой версией
        /// </summary>
        public RelayCommand EditPathForUpdateCommand
        {
            get
            {
                return _editPathForUpdateCommand ??
                    (_editPathForUpdateCommand = new RelayCommand(obj =>
                    {
                        if (!this.EqualsPinCode())
                            return;

                        using (ForRobot.Views.Windows.InputWindow inputWindow = new ForRobot.Views.Windows.InputWindow("Введите путь к новой папке с обновлениями"))
                        {
                            if (inputWindow.ShowDialog() == true && Directory.Exists(inputWindow.Answer))
                            {
                                Properties.Settings.Default.UpdatePath = inputWindow.Answer;
                                Properties.Settings.Default.Save();
                            }
                        }
                    }));
            }
        }

        /// <summary>
        /// Изменение ПИН-кода
        /// </summary>
        public RelayCommand EditPinCodeCommand
        {
            get
            {
                return _editPinCodeCommand ??
                    (_editPinCodeCommand = new RelayCommand(obj =>
                    {
                        if (!this.EqualsPinCode("Введите старый пин-код"))
                            return;

                        using (ForRobot.Views.Windows.InputWindow inputWindow = new ForRobot.Views.Windows.InputWindow("Введите новый пин-код"))
                        {
                            if (inputWindow.ShowDialog() == true)
                            {
                                Properties.Settings.Default.PinCode = this.Sha256(inputWindow.Answer);
                                Properties.Settings.Default.Save();
                            }
                        }
                    }));
            }
        }

        #endregion

        #endregion

        #region Constructor

        public PropertiesWindowViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Хэширование строки
        /// </summary>
        /// <param name="str">Строка для хэширования</param>
        /// <returns></returns>
        private string Sha256(string str)
        {
            StringBuilder Sb = new StringBuilder();
            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(str));
                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }
        
        /// <summary>
        /// Ввод пин-кода пользователем
        /// </summary>
        /// <param name="sInputBoxText">Question, текст в InputBox</param>
        /// <returns>Верный ли введенный пользователем пин-код</returns>
        private bool EqualsPinCode(string sInputBoxText = "Введите пин-код")
        {
            ForRobot.Views.Windows.InputWindow inputWindow = new ForRobot.Views.Windows.InputWindow(sInputBoxText);
            inputWindow.ShowDialog();
            return this.Sha256(inputWindow.Answer) == Properties.Settings.Default.PinCode;
        }

        #endregion
    }
}
