using System;
using System.Windows;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForRobot.Views.Windows;

namespace ForRobot.Services
{
    public interface IWindowsAppService
    {
        /// <summary>
        /// Главное окно приложения
        /// </summary>
        ForRobot.Views.Windows.MainWindow AppMainWindow { get; }

        /// <summary>
        /// Вывод окна ввода
        /// </summary>
        /// <param name="sInputBoxText">Question, текст в InputBox</param>
        /// <returns>Введённый пользователем текст</returns>
        string InputWindowShow(string sInputBoxText = "Введите пин-код");

        /// <summary>
        /// Вывод окна выбора
        /// </summary>
        /// <param name="itemsSource"></param>
        /// <param name="selectedItems">Выбранные элементы</param>
        /// <returns></returns>
        IEnumerable SelectWindowShow(IEnumerable itemsSource, IEnumerable selectedItems = null);

        /// <summary>
        /// Открытие окна создание файла
        /// </summary>
        void OpenCreateWindow();

        /// <summary>
        /// Открытие окна настроек
        /// </summary>
        void OpenPropertiesWindow();

        /// <summary>
        /// Закрытие окна создание файла
        /// </summary>
        void CloseCreateWindow();

        /// <summary>
        /// Закрытие окна настроек
        /// </summary>
        void ClosePropertiesWindow();
    }

    /// <summary>
    /// Сервис открытия окон приложения
    /// </summary>
    public sealed class WindowsAppService : IWindowsAppService
    {
        /// <summary>
        /// Главное окно приложения
        /// </summary>
        private Views.Windows.MainWindow _appMainWindow;
        /// <summary>
        /// Окно создания окна
        /// </summary>
        private ForRobot.Views.Windows.CreateWindow _createWindow { get; set; }
        /// <summary>
        /// Окно настроек
        /// </summary>
        private ForRobot.Views.Windows.PropertiesWindow _propertiesWindow { get; set; }

        private ForRobot.Views.Windows.SelectWindow _selectedAppsForOpenedFile { get; set; }

        public Views.Windows.MainWindow AppMainWindow { get => _appMainWindow ?? (_appMainWindow = new Views.Windows.MainWindow()); }

        public string InputWindowShow(string sInputBoxText = "Введите пин-код")
        {
            string answer = null;
            using (ForRobot.Views.Windows.InputWindow inputWindow = new ForRobot.Views.Windows.InputWindow(sInputBoxText))
            {
                if(inputWindow.ShowDialog() == true)
                    answer = inputWindow.Answer;
            }
            return answer;
        }

        public IEnumerable SelectWindowShow(IEnumerable itemsSource, IEnumerable selectedItems = null)
        {
            using (ForRobot.Views.Windows.SelectWindow selectWindow = new ForRobot.Views.Windows.SelectWindow(itemsSource, selectedItems))
            {
                if(selectWindow.ShowDialog() == true)
                    selectedItems = selectWindow.SelectedItems;
            }
            return selectedItems;
        }

        public void OpenCreateWindow()
        {
            if (!object.Equals(this._createWindow, null)) // Блокировка открытия 2-ого окна.
            {
                FocusedWindow(this._createWindow);
                return;
            }
            this._createWindow = new ForRobot.Views.Windows.CreateWindow();
            this._createWindow.Closed += (a, b) => this._createWindow = null;
            this._createWindow.Owner = App.Current.MainWindow;
            this._createWindow.Show();
        }
        public void OpenPropertiesWindow()
        {
            if (!object.Equals(this._propertiesWindow, null)) // Блокировка открытия 2-ого окна.
            {
                FocusedWindow(this._propertiesWindow);
                return;
            }

            this._propertiesWindow = new ForRobot.Views.Windows.PropertiesWindow();
            this._propertiesWindow.Closed += (a, b) =>
            {
                this._propertiesWindow = null;
                App.Current.SelectAppMainWindow();
            };
            this._propertiesWindow.Owner = App.Current.MainWindow;
            this._propertiesWindow.Show();
        }

        public void CloseCreateWindow() => this._createWindow.Close();
        public void ClosePropertiesWindow() => this._propertiesWindow.Close();

        private void FocusedWindow(Window window)
        {
            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;

            window.Topmost = true;
            window.Topmost = false;
            window.Activate();
            window.Focus();
        }
    }
}
