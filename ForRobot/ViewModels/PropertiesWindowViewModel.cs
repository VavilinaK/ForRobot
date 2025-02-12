using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Configuration;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace ForRobot.ViewModels
{
    public class PropertiesWindowViewModel : BaseClass
    {
        #region Private variables

        //private System.Windows.Controls.TreeViewItem _selectedItem;

        #region Commands

        private RelayCommand _selectClosedTabCommand;

        private RelayCommand _saveCommand;

        private RelayCommand _cancelCommand;
        
        private RelayCommand _editPathForUpdateCommand;

        private RelayCommand _editPinCodeCommand;

        private RelayCommand _checkBoxAvailableFolderCommand;

        #endregion

        #endregion

        #region Public variables

        public ForRobot.Libr.Settings.Settings Settings { get; set; } = ForRobot.Libr.Settings.Settings.GetSettings();

        //public System.Windows.Controls.TreeViewItem SelectedItem { get => this._selectedItem; set => Set(ref this._selectedItem, value); }

        #region Commands

        /// <summary>
        /// Выбор закрытой вкладки
        /// </summary>
        public RelayCommand SelectClosedTabCommand
        {
            get
            {
                return _selectClosedTabCommand ??
                    (_selectClosedTabCommand = new RelayCommand(obj =>
                    {
                        var control = (System.Windows.Controls.TreeView)obj;
                        if (!this.EqualsPinCode())
                        {
                            control.Items.Cast<System.Windows.Controls.TreeViewItem>().Where(w => w.IsSelected).First().IsSelected = false;

                            var item = control.Items.Cast<System.Windows.Controls.TreeViewItem>().Where(w => (string)w.Tag == "General").First();
                            item.IsSelected = true;
                            //RaisePropertyChanged(nameof(item.IsSelected));


                            //this.SelectedItem = null;
                            //this.SelectedItem = control.Items.Cast<System.Windows.Controls.TreeViewItem>().Where(item => (string)item.Tag == "General").First();
                        }

                        //var control = (System.Windows.Controls.TreeViewItem)obj;
                        //if (control.IsSelected && !this.EqualsPinCode())
                        //{
                        //    control.IsSelected = false;
                        //    //((System.Windows.Controls.TreeView)control.Parent).SelectedItem = ((System.Windows.Controls.TreeView)control.Parent).ItemsSource.Cast<System.Windows.Controls.TreeViewItem>().Where(item => (string)item.Tag == "General").First();
                        //}


                        //if (((System.Windows.Controls.Primitives.ToggleButton)obj).IsChecked == true)
                        //{
                        //    string pass = "";
                        //    using (ForRobot.Views.Windows.InputWindow _inputWindow = new ForRobot.Views.Windows.InputWindow("Введите пин-код") { Title = "Управление процессом на роботе" })
                        //    {
                        //        if (_inputWindow.ShowDialog() == true)
                        //        {
                        //            StringBuilder Sb = new StringBuilder();
                        //            using (var hash = SHA256.Create())
                        //            {
                        //                Encoding enc = Encoding.UTF8;
                        //                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(_inputWindow.Answer));
                        //                foreach (byte b in result)
                        //                    Sb.Append(b.ToString("x2"));
                        //            }
                        //            pass = Sb.ToString();
                        //        }
                        //    }
                        //    if (!Equals(pass, Properties.Settings.Default.PinCode))
                        //        ((System.Windows.Controls.Primitives.ToggleButton)obj).IsChecked = false;
                        //}
                    }));
            }
        }

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

        /// <summary>
        /// Выбор отображающихся папок
        /// </summary>
        public RelayCommand CheckBoxAvailableFolderCommand
        {
            get
            {
                return _checkBoxAvailableFolderCommand ??
                    (_checkBoxAvailableFolderCommand = new RelayCommand(obj =>
                    {
                        var tuple = (System.Collections.Generic.KeyValuePair<string, bool>)obj;
                        this.Settings.AvailableFolders.Remove(this.Settings.AvailableFolders.Where(x => x.Key == tuple.Key).First().Key);
                        this.Settings.AvailableFolders.Add(tuple.Key, !tuple.Value);
                    }));
            }
        }

        /// <summary>
        /// Выбор отображающихся вкладок
        /// </summary>
        public RelayCommand CheckBoxAvailableTabCommand
        {
            get
            {
                return _checkBoxAvailableFolderCommand ??
                    (_checkBoxAvailableFolderCommand = new RelayCommand(obj =>
                    {
                        var tuple = (System.Collections.Generic.KeyValuePair<string, bool>)obj;
                        this.Settings.AvailableTab.Remove(this.Settings.AvailableTab.Where(x => x.Key == tuple.Key).First().Key);
                        this.Settings.AvailableTab.Add(tuple.Key, !tuple.Value);
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
