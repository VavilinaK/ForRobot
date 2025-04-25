using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Configuration;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using AvalonDock.Layout;

using ForRobot.Model.Detals;

namespace ForRobot.ViewModels
{
    public class PropertiesWindowViewModel : BaseClass
    {
        #region Private variables

        private ForRobot.Libr.Settings.Settings _settings = ForRobot.Libr.Settings.Settings.GetSettings();
        private System.Windows.Controls.TreeViewItem _selectedItem;
        private string _selectedDetalTypeName;
        private string _selectedDetalTypeScript;

        #region Commands

        //private RelayCommand _selectClosedTabCommand;
        //private RelayCommand _selectedMainTreeViewItem;        
        private RelayCommand _standartSettingsCommand;
        //private RelayCommand _saveCommand;
        //private RelayCommand _cancelCommand;        
        private RelayCommand _editPathForUpdateCommand;
        //private RelayCommand _editPinCodeCommand;
        private RelayCommand _checkBoxAvailableFolderCommand;

        #endregion

        #endregion

        #region Public variables

        public ForRobot.Libr.Settings.Settings Settings { get => this._settings; set => Set(ref this._settings, value); }

        public System.Windows.Controls.TreeViewItem SelectedItem { get => this._selectedItem; set => Set(ref this._selectedItem, value); }

        public string SelectedDetalTypeName
        {
            get => this._selectedDetalTypeName ?? (this._selectedDetalTypeName = ForRobot.Model.Detals.DetalTypes.Plita);
            set
            {
                Set(ref this._selectedDetalTypeName, value);
                this.RaisePropertyChanged(nameof(this.StandartNameFile));
            }
        }

        public string SelectedDetalTypeScript
        {
            get => this._selectedDetalTypeScript ?? (this._selectedDetalTypeScript = ForRobot.Model.Detals.DetalTypes.Plita);
            set
            {
                Set(ref this._selectedDetalTypeScript, value);
                this.RaisePropertyChanged(nameof(this.ScriptName));
            }
        }

        public string StandartNameFile
        {
            get
            {
                switch (this.SelectedDetalTypeName)
                {
                    case string a when a == DetalTypes.Plita:
                        return this.Settings.PlitaProgramName;

                    case string b when b == DetalTypes.Plita:
                        return this.Settings.PlitaStringerProgramName;

                    case string c when c == DetalTypes.Plita:
                        return this.Settings.PlitaTreugolnikProgramName;

                    default:
                        return string.Empty;
                }
            }
            set
            {
                switch (this.SelectedDetalTypeName)
                {
                    case string a when a == DetalTypes.Plita:
                        this.Settings.PlitaProgramName = value;
                        break;

                    case string b when b == DetalTypes.Plita:
                        this.Settings.PlitaStringerProgramName = value;
                        break;

                    case string c when c == DetalTypes.Plita:
                        this.Settings.PlitaTreugolnikProgramName = value;
                        break;
                }
            }
        }

        public string ScriptName
        {
            get
            {
                switch (this.SelectedDetalTypeScript)
                {
                    case string a when a == DetalTypes.Plita:
                        return this.Settings.PlitaScriptName;

                    case string b when b == DetalTypes.Plita:
                        return this.Settings.PlitaStringerScriptName;

                    case string c when c == DetalTypes.Plita:
                        return this.Settings.PlitaTreugolnikScriptName;

                    default:
                        return string.Empty;
                }
            }
            set
            {
                switch (this.SelectedDetalTypeScript)
                {
                    case string a when a == DetalTypes.Plita:
                        this.Settings.PlitaScriptName = value;
                        break;

                    case string b when b == DetalTypes.Plita:
                        this.Settings.PlitaStringerScriptName = value;
                        break;

                    case string c when c == DetalTypes.Plita:
                        this.Settings.PlitaTreugolnikScriptName = value;
                        break;
                }
            }
        }

        public List<LayoutAnchorable> Anchorables
        {
            get
            {
                if (App.Current.MainWindowView.MainFrame?.Content is Views.Pages.PageMain3)
                {
                    var dockingManager = (App.Current.MainWindowView.MainFrame?.Content as Views.Pages.PageMain3).DockingManeger;
                    return dockingManager.Layout.Descendents().OfType<LayoutAnchorable>().ToList();
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// Коллекция видов деталей
        /// </summary>
        public ObservableCollection<string> DetalTypesCollection
        {
            get
            {
                List<string> detalTypesList = new List<string>();
                foreach (var f in typeof(ForRobot.Model.Detals.DetalTypes).GetFields())
                {
                    detalTypesList.Add(f.GetValue(null).ToString());
                }
                return new ObservableCollection<string>(detalTypesList);
            }
        }

        #region Commands

        /// <summary>
        /// Выбор закрытого элемента управления
        /// </summary>
        public ICommand SelectClosedControlCommand { get; } = new RelayCommand(obj => SelectClosedControl(obj as System.Windows.Controls.Control));
        /// <summary>
        /// Возвращение к стандартным настройкам
        /// </summary>
        public ICommand StandartSettingsCommand { get => this._standartSettingsCommand ?? (this._standartSettingsCommand = new RelayCommand(_ => { this.Settings = new Libr.Settings.Settings(); })); }
        /// <summary>
        /// Сохранение настроек
        /// </summary>
        public ICommand SaveSettingsCommand { get; } = new RelayCommand(obj => SaveSettings(obj as Libr.Settings.Settings));
        /// <summary>
        /// Закрытие окна
        /// </summary>
        public ICommand CancelCommand { get; } = new RelayCommand(_ => { App.Current.PropertiesWindow.Close(); });
        /// <summary>
        /// Команда изменения директивы каталога с новой версией программы
        /// </summary>
        public ICommand EditPathForUpdateCommand { get => this._editPathForUpdateCommand ?? (this._editPathForUpdateCommand = new RelayCommand(_ => this.EditPathForUpdat())); }
        /// <summary>
        /// Изменение ПИН-кода
        /// </summary>
        public ICommand EditPinCodeCommand { get; } = new RelayCommand(_ => EditPinCode());
        /// <summary>
        /// Комманда изменения checkBox отображающихся папок
        /// </summary>
        public ICommand CheckBoxAvailableFolderCommand { get => this._checkBoxAvailableFolderCommand ?? 
                                                               (this._checkBoxAvailableFolderCommand = new RelayCommand(obj => CheckBoxAvailableFolder((System.Collections.Generic.KeyValuePair<string, bool>)obj))); }
        /// <summary>
        /// Удаление изменений интерфейса
        /// </summary>
        public ICommand DeleteLayoutAnchorableConfigCommand { get; } = new RelayCommand(_ => DeleteLayoutAnchorable());

        ///// <summary>
        ///// Выбор закрытой вкладки
        ///// </summary>
        //public RelayCommand SelectClosedTabCommand
        //{
        //    get
        //    {
        //        return _selectClosedTabCommand ??
        //            (_selectClosedTabCommand = new RelayCommand(obj =>
        //            {
        //                var control = (System.Windows.Controls.TreeView)obj;
        //                if (!this.EqualsPinCode())
        //                {
        //                    control.Items.Cast<System.Windows.Controls.TreeViewItem>().Where(w => w.IsSelected).First().IsSelected = false;

        //                    var item = control.Items.Cast<System.Windows.Controls.TreeViewItem>().Where(w => (string)w.Tag == "General").First();
        //                    item.IsSelected = true;
        //                    //RaisePropertyChanged(nameof(item.IsSelected));


        //                    //this.SelectedItem = null;
        //                    //this.SelectedItem = control.Items.Cast<System.Windows.Controls.TreeViewItem>().Where(item => (string)item.Tag == "General").First();
        //                }

        //                //var control = (System.Windows.Controls.TreeViewItem)obj;
        //                //if (control.IsSelected && !this.EqualsPinCode())
        //                //{
        //                //    control.IsSelected = false;
        //                //    //((System.Windows.Controls.TreeView)control.Parent).SelectedItem = ((System.Windows.Controls.TreeView)control.Parent).ItemsSource.Cast<System.Windows.Controls.TreeViewItem>().Where(item => (string)item.Tag == "General").First();
        //                //}


        //                //if (((System.Windows.Controls.Primitives.ToggleButton)obj).IsChecked == true)
        //                //{
        //                //    string pass = "";
        //                //    using (ForRobot.Views.Windows.InputWindow _inputWindow = new ForRobot.Views.Windows.InputWindow("Введите пин-код") { Title = "Управление процессом на роботе" })
        //                //    {
        //                //        if (_inputWindow.ShowDialog() == true)
        //                //        {
        //                //            StringBuilder Sb = new StringBuilder();
        //                //            using (var hash = SHA256.Create())
        //                //            {
        //                //                Encoding enc = Encoding.UTF8;
        //                //                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(_inputWindow.Answer));
        //                //                foreach (byte b in result)
        //                //                    Sb.Append(b.ToString("x2"));
        //                //            }
        //                //            pass = Sb.ToString();
        //                //        }
        //                //    }
        //                //    if (!Equals(pass, Properties.Settings.Default.PinCode))
        //                //        ((System.Windows.Controls.Primitives.ToggleButton)obj).IsChecked = false;
        //                //}
        //            }));
        //    }
        //}

        //public RelayCommand SelectedMainTreeViewItem
        //{
        //    get
        //    {
        //        return _selectedMainTreeViewItem ??
        //            (_selectedMainTreeViewItem = new RelayCommand(obj =>
        //            {
        //                var control = (System.Windows.Controls.TreeViewItem)obj;
        //                control.IsSelected = false;
        //                (control.Items[0] as System.Windows.Controls.TreeViewItem).IsSelected = true;
        //                this.SelectedItem = control.Items[0] as System.Windows.Controls.TreeViewItem;
        //            }));
        //    }
        //}

        ///// <summary>
        ///// Возвращение к стандартным настройкам
        ///// </summary>
        //public RelayCommand StandartSettingsCommand
        //{
        //    get
        //    {
        //        return _standartSettingsCommand ??
        //            (_standartSettingsCommand = new RelayCommand(obj =>
        //            {
        //                this.Settings = new Libr.Settings.Settings();
        //            }));
        //    }
        //}

        ///// <summary>
        ///// Сохранение настроек
        ///// </summary>
        //public RelayCommand SaveCommand
        //{
        //    get
        //    {
        //        return _saveCommand ??
        //            (_saveCommand = new RelayCommand(obj =>
        //            {
        //                this.Settings.Save();
        //                if(MessageBox.Show("Чтобы изменения вступили в силу, необходимо перезапустить приложение.\n\nПерезапустить интерфейс?", "Сохранение настроек", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes)
        //                {
        //                    string sAppPath = Directory.GetCurrentDirectory();
        //                    System.Diagnostics.Process process = new System.Diagnostics.Process()
        //                    {
        //                        StartInfo = new System.Diagnostics.ProcessStartInfo()
        //                        {
        //                            UseShellExecute = false,
        //                            RedirectStandardOutput = true,
        //                            RedirectStandardError = true,
        //                            WorkingDirectory = sAppPath,
        //                            CreateNoWindow = true,
        //                            FileName = "cmd.exe",
        //                            Arguments = $"/K taskkill /im {Application.ResourceAssembly.GetName().Name}.exe /f& START \"\" \"{sAppPath + "\\" + Application.ResourceAssembly.GetName().Name + ".exe"}\"",
        //                            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
        //                        }
        //                    };
        //                    new System.Threading.Thread(() => process.Start()).Start();
        //                }
        //                App.Current.PropertiesWindow.Close();
        //            }));
        //    }
        //}

        ///// <summary>
        ///// Закрытие окна
        ///// </summary>
        //public RelayCommand CancelCommand
        //{
        //    get
        //    {
        //        return _cancelCommand ??
        //            (_cancelCommand = new RelayCommand(obj =>
        //            {
        //                App.Current.PropertiesWindow.Close();
        //            }));
        //    }
        //}

        ///// <summary>
        ///// Изменение директивы каталога с новой версией
        ///// </summary>
        //public RelayCommand EditPathForUpdateCommand
        //{
        //    get
        //    {
        //        return _editPathForUpdateCommand ??
        //            (_editPathForUpdateCommand = new RelayCommand(obj =>
        //            {
        //                if (!this.EqualsPinCode())
        //                    return;

        //                using (ForRobot.Views.Windows.InputWindow inputWindow = new ForRobot.Views.Windows.InputWindow("Введите путь к новой папке с обновлениями"))
        //                {
        //                    if (inputWindow.ShowDialog() == true && Directory.Exists(inputWindow.Answer))
        //                    {
        //                        Properties.Settings.Default.UpdatePath = inputWindow.Answer;
        //                        Properties.Settings.Default.Save();
        //                    }
        //                }
        //            }));
        //    }
        //}

        ///// <summary>
        ///// Изменение ПИН-кода
        ///// </summary>
        //public RelayCommand EditPinCodeCommand
        //{
        //    get
        //    {
        //        return _editPinCodeCommand ??
        //            (_editPinCodeCommand = new RelayCommand(obj =>
        //            {
        //                if (!this.EqualsPinCode("Введите старый пин-код"))
        //                    return;

        //                using (ForRobot.Views.Windows.InputWindow inputWindow = new ForRobot.Views.Windows.InputWindow("Введите новый пин-код"))
        //                {
        //                    if (inputWindow.ShowDialog() == true)
        //                    {
        //                        Properties.Settings.Default.PinCode = this.Sha256(inputWindow.Answer);
        //                        Properties.Settings.Default.Save();
        //                    }
        //                }
        //            }));
        //    }
        //}

        ///// <summary>
        ///// Выбор отображающихся папок
        ///// </summary>
        //public RelayCommand CheckBoxAvailableFolderCommand
        //{
        //    get
        //    {
        //        return _checkBoxAvailableFolderCommand ??
        //            (_checkBoxAvailableFolderCommand = new RelayCommand(obj =>
        //            {
        //                var tuple = (System.Collections.Generic.KeyValuePair<string, bool>)obj;
        //                this.Settings.AvailableFolders.Remove(this.Settings.AvailableFolders.Where(x => x.Key == tuple.Key).First().Key);
        //                this.Settings.AvailableFolders.Add(tuple.Key, !tuple.Value);
        //            }));
        //    }
        //}

        ///// <summary>
        ///// Выбор отображающихся вкладок
        ///// </summary>
        //public RelayCommand CheckBoxAvailableTabCommand
        //{
        //    get
        //    {
        //        return _checkBoxAvailableFolderCommand ??
        //            (_checkBoxAvailableFolderCommand = new RelayCommand(obj =>
        //            {
        //                var tuple = (System.Collections.Generic.KeyValuePair<string, bool>)obj;
        //                this.Settings.AvailableTab.Remove(this.Settings.AvailableTab.Where(x => x.Key == tuple.Key).First().Key);
        //                this.Settings.AvailableTab.Add(tuple.Key, !tuple.Value);
        //            }));
        //    }
        //}

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
        /// Изменение пути к папке с обновлениями
        /// </summary>
        private void EditPathForUpdat()
        {
            if (!ForRobot.App.EqualsPinCode())
                return;


            //                using (ForRobot.Views.Windows.InputWindow inputWindow = new ForRobot.Views.Windows.InputWindow("Введите путь к новой папке с обновлениями"))
            //                {
            //                    if (inputWindow.ShowDialog() == true && Directory.Exists(inputWindow.Answer))
            //                    {
            //                        Properties.Settings.Default.UpdatePath = inputWindow.Answer;
            //                        Properties.Settings.Default.Save();
            //                    }
            //                }
        }

        /// <summary>
        /// Выбор доступности системных папок
        /// </summary>
        /// <param name="folder"></param>
        public void CheckBoxAvailableFolder(System.Collections.Generic.KeyValuePair<string, bool> folder)
        {
            this.Settings.AvailableFolders.Remove(this.Settings.AvailableFolders.Where(x => x.Key == folder.Key).First().Key);
            this.Settings.AvailableFolders.Add(folder.Key, !folder.Value);
        }

        #region Static

        /// <summary>
        /// Выбор закрытого элемента управления
        /// </summary>
        /// <param name="control"></param>
        private static void SelectClosedControl(System.Windows.Controls.Control control)
        {
            if (ForRobot.App.EqualsPinCode())
                return;

            switch (control)
            {
                case System.Windows.Controls.TreeView tree when tree.ToString() == control.ToString():
                    break;

                case System.Windows.Controls.TreeViewItem treeItem when treeItem.ToString() == control.ToString():
                    var treeViewItem = control as System.Windows.Controls.TreeViewItem;
                    treeViewItem.IsExpanded = false;
                    treeViewItem.IsSelected = false;
                    break;

                case System.Windows.Controls.CheckBox check when check.ToString() == control.ToString():
                    var checkBox = control as System.Windows.Controls.CheckBox;
                    checkBox.IsChecked = !checkBox.IsChecked;
                    break;
            }
        }

        /// <summary>
        /// Сохранение настроек
        /// </summary>
        /// <param name="settings"></param>
        private static void SaveSettings(Libr.Settings.Settings settings)
        {
            settings.Save();
            if (MessageBox.Show("Чтобы изменения вступили в силу, необходимо перезапустить приложение.\n\nПерезапустить интерфейс?", "Сохранение настроек", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes)
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
        }

        /// <summary>
        /// Изменение пин-кода
        /// </summary>
        public static void EditPinCode()
        {
            if (!ForRobot.App.EqualsPinCode("Введите старый пин-код"))
                return;

            using (ForRobot.Views.Windows.InputWindow inputWindow = new ForRobot.Views.Windows.InputWindow("Введите новый пин-код"))
            {
                if (inputWindow.ShowDialog() == true)
                {
                    Properties.Settings.Default.PinCode = ForRobot.App.Sha256(inputWindow.Answer);
                    Properties.Settings.Default.Save();
                }
            }
        }

        /// <summary>
        /// Удаление AvalonDock.config и перезапуск приложения
        /// </summary>
        private static void DeleteLayoutAnchorable()
        {
            if (MessageBox.Show("Для удаления изменений необходим перезапуск интиерфейса!\n\nПерезапустить интерфейс?",
                               "Предупреждение",
                               MessageBoxButton.OKCancel,
                               MessageBoxImage.Warning,
                               MessageBoxResult.Cancel,
                               MessageBoxOptions.DefaultDesktopOnly) != MessageBoxResult.OK)
                return;

            System.Diagnostics.Process process = new System.Diagnostics.Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = @".\",
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = $"/K taskkill /im {Application.ResourceAssembly.GetName().Name}.exe /f& del {App.Current.AvalonConfigPath}& START \"\" \"{Application.ResourceAssembly.Location}\"",
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                }
            };
            new System.Threading.Thread(() => process.Start()).Start();
        }

        #endregion Static

        #endregion Private functions
    }
}
