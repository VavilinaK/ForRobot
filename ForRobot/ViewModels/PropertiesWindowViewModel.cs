using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
using System.Configuration;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using AvalonDock.Layout;

using ForRobot.Model.Detals;
using ForRobot.Model.Settings;

namespace ForRobot.ViewModels
{
    public class PropertiesWindowViewModel : BaseClass
    {
        #region Private variables

        private Settings _settings;
        //private ForRobot.Libr.Settings.Settings _settings = ForRobot.Libr.Settings.Settings.GetSettings();
        private System.Windows.Controls.TreeViewItem _selectedItem;
        private string _selectedDetalTypeName;
        private string _selectedDetalTypeScript;

        #region Commands
        
        private RelayCommand _standartSettingsCommand;  
        private RelayCommand _editPathForUpdateCommand;
        private RelayCommand _editAppForOpenFile;
        private RelayCommand _checkBoxAvailableFolderCommand;

        #endregion

        #endregion

        #region Public variables

        public Settings Settings { get => this._settings; set => Set(ref this._settings, value); }

        public System.Windows.Controls.TreeViewItem SelectedItem { get => this._selectedItem; set => Set(ref this._selectedItem, value); }

        /// <summary>
        /// Выбранный тип детали для названия программы
        /// </summary>
        public string SelectedDetalTypeName
        {
            get => this._selectedDetalTypeName ?? (this._selectedDetalTypeName = ForRobot.Model.Detals.DetalTypes.Plita);
            set
            {
                Set(ref this._selectedDetalTypeName, value);
                this.RaisePropertyChanged(nameof(this.StandartNameFile));
            }
        }

        /// <summary>
        /// Выбранный тип детали для имени скрипта
        /// </summary>
        public string SelectedDetalTypeScript
        {
            get => this._selectedDetalTypeScript ?? (this._selectedDetalTypeScript = ForRobot.Model.Detals.DetalTypes.Plita);
            set
            {
                Set(ref this._selectedDetalTypeScript, value);
                this.RaisePropertyChanged(nameof(this.ScriptName));
            }
        }

        /// <summary>
        /// Стандарное название генерируемой программы
        /// </summary>
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

        /// <summary>
        /// Название скрипта-генератора
        /// </summary>
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

        /// <summary>
        /// Коллекция панелей макета интерфейса
        /// </summary>
        public List<LayoutAnchorable> Anchorables
        {
            get
            {
                if ((App.Current.MainWindow as ForRobot.Views.Windows.MainWindow).MainFrame?.Content is Views.Pages.PageMain3)
                {
                    var dockingManager = ((App.Current.MainWindow as ForRobot.Views.Windows.MainWindow).MainFrame?.Content as Views.Pages.PageMain3).DockingManeger;
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

        /// <summary>
        /// Коллекция конфигураций сцен для цехов
        /// </summary>
        public ObservableCollection<SceneConfiguration> SceneConfigurationsCollection
        {
            get
            {
                List<SceneConfiguration> sc = new List<SceneConfiguration>()
                {
                    SceneConfiguration.FirstCehConfiguration,
                    SceneConfiguration.SecondCehConfiguration
                };
                return new ObservableCollection<SceneConfiguration>(sc);
            }
        }

        public ObservableCollection<HorizontalAlignment> HorizontalAlignments { get; } = new ObservableCollection<HorizontalAlignment>(Enum.GetValues(typeof(HorizontalAlignment))
                                                                                                                                                                                  .Cast<HorizontalAlignment>()
                                                                                                                                                                                  .ToList<HorizontalAlignment>());
        public ObservableCollection<VerticalAlignment> VerticalAlignments { get; } = new ObservableCollection<VerticalAlignment>(Enum.GetValues(typeof(VerticalAlignment))
                                                                                                                                                                         .Cast<VerticalAlignment>()
                                                                                                                                                                         .ToList<VerticalAlignment>());

        #region Commands

        /// <summary>
        /// Выбор закрытого элемента управления
        /// </summary>
        public ICommand SelectClosedControlCommand { get; } = new RelayCommand(obj => SelectClosedControl(obj as System.Windows.Controls.Control), _ => _isSelectClosedControl);
        /// <summary>
        /// Возвращение к стандартным настройкам
        /// </summary>
        public ICommand StandartSettingsCommand
        {
            get => this._standartSettingsCommand ?? (this._standartSettingsCommand = new RelayCommand(_ => 
                                                                                                          {
                                                                                                              ForRobot.Themes.Colors.DefaultColors();
                                                                                                              this.Settings = new Settings();
                                                                                                          }));
        }
        /// <summary>
        /// Сохранение настроек
        /// </summary>
        public ICommand SaveSettingsCommand { get; } = new RelayCommand(obj => 
                                                                              {
                                                                                  Settings settings = obj as Settings;
                                                                                  settings.Colors = App.Current.Settings.Colors;
                                                                                  SaveSettings(settings);
                                                                              });
        /// <summary>
        /// Закрытие окна
        /// </summary>
        public ICommand CancelCommand { get; } = new RelayCommand(_ => App.Current.WindowsAppService.ClosePropertiesWindow());
        /// <summary>
        /// Команда изменения директивы каталога с новой версией программы
        /// </summary>
        public ICommand EditPathForUpdateCommand { get => this._editPathForUpdateCommand ?? (this._editPathForUpdateCommand = new RelayCommand(_ => this.EditPathForUpdat())); }
        public ICommand EditAppForOpenFile { get => this._editAppForOpenFile ?? (this._editAppForOpenFile = new RelayCommand(_ => 
        {
            try
            {
                System.Collections.IEnumerable selectedItems = null;
                using (ForRobot.Views.Windows.SelectWindow selectWindow = new ForRobot.Views.Windows.SelectWindow(ForRobot.Services.SelectAppsOnDeviceService.GetAllApplicationsOnDevice(), this.Settings.SavedAppsForOpened))
                {
                    ResourceDictionary resource = (ResourceDictionary)Application.Current.Resources["SelectAppsWindowResource"];

                    if (resource == null)
                        throw new Exception("Не найден словарь ресурсов SelectAppsWindowResource.");

                    selectWindow.Resources.MergedDictionaries.Clear();
                    selectWindow.Resources.MergedDictionaries.Add(resource);

                    if (selectWindow.ShowDialog() == true)
                        selectedItems = selectWindow.SelectedItems;
                }
                if (selectedItems == null)
                    return;

                this.Settings.SavedAppsForOpened = selectedItems.Cast<ApplicationInfo>().ToList<ApplicationInfo>();
                RaisePropertyChanged(nameof(this.Settings));
            }
            catch(Exception ex)
            {

            }
        })); }
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

        #endregion

        #endregion

        #region Constructor

        public PropertiesWindowViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            this.Settings = App.Current.Settings.Clone() as ForRobot.Model.Settings.Settings;
            this.Settings.ChangePropertyEvent -= App.Current.SaveAppSettings;
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

            using (var fbd = new System.Windows.Forms.FolderBrowserDialog() { SelectedPath = Properties.Settings.Default.UpdatePath })
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Properties.Settings.Default.UpdatePath = fbd.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }
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

        private static bool _isSelectClosedControl = true;

        /// <summary>
        /// Выбор закрытого элемента управления
        /// </summary>
        /// <param name="control"></param>
        private static void SelectClosedControl(System.Windows.Controls.Control control)
        {
            if (ForRobot.App.EqualsPinCode())
                return;

            _isSelectClosedControl = false;
            switch (control)
            {
                case System.Windows.Controls.TreeView tree when tree.ToString() == control.ToString():
                    break;

                case System.Windows.Controls.TreeViewItem treeViewItem when treeViewItem.ToString() == control.ToString():
                    treeViewItem = control as System.Windows.Controls.TreeViewItem;
                    treeViewItem.IsExpanded = false;
                    treeViewItem.IsSelected = false;
                    break;

                case System.Windows.Controls.CheckBox checkBox when checkBox.ToString() == control.ToString():
                    checkBox = control as System.Windows.Controls.CheckBox;
                    checkBox.IsChecked = !checkBox.IsChecked;
                    break;

                case System.Windows.Controls.TextBox textBox when textBox.ToString() == control.ToString():
                    textBox = control as System.Windows.Controls.TextBox;
                    if (textBox == null) return;

                    var parent = textBox.Parent as UIElement;
                    if (parent != null && parent.Focusable)
                    {
                        parent.Focus();
                    }
                    else
                    {
                        var page = FindParent<Window>(textBox);
                        if (page != null)
                        {
                            page.Focus();
                        }
                    }
                    Keyboard.ClearFocus();
                    break;

                default:
                    if (control == null) return;

                    // Сброс фокуса для всех областей фокуса
                    var focusScope = FocusManager.GetFocusScope(control);
                    FocusManager.SetFocusedElement(focusScope, null);

                    // Дополнительно: поиск и сброс фокуса во всех дочерних элементах
                    var children = FindVisualChildren<UIElement>(control);
                    foreach (var child in children)
                    {
                        if (child.IsKeyboardFocused)
                        {
                            Keyboard.ClearFocus();
                            break;
                        }
                    }
                    break;
            }
            _isSelectClosedControl = true;
        }

        // Поиск родительского элемента определенного типа
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        // Поиск дочерниго элемента определенного типа
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        /// <summary>
        /// Сохранение настроек
        /// </summary>
        /// <param name="settings"></param>
        private static void SaveSettings(Settings settings)
        {
            settings.Save();
            if (MessageBox.Show("Чтобы изменения вступили в силу, необходимо перезапустить приложение.\n\nПерезапустить интерфейс?", "Сохранение настроек", 
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes)
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
                        Arguments = $"/K taskkill /im {Application.ResourceAssembly.GetName().Name}.exe /f& " +
                                    $"START \"\" /HIGH \"{sAppPath + "\\" + Application.ResourceAssembly.GetName().Name + ".exe"}\"",
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                    }
                };
                new System.Threading.Thread(() => process.Start()).Start();
            }
            App.Current.WindowsAppService.ClosePropertiesWindow();
        }

        /// <summary>
        /// Изменение пин-кода
        /// </summary>
        public static void EditPinCode()
        {
            if (App.Sha256(new Services.WindowsAppService().InputWindowShow("Введите старый пин-код")) != Properties.Settings.Default.PinCode)
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
