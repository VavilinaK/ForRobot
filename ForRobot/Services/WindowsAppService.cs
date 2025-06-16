using System;
using System.Collections.Generic;
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
        private Views.Windows.MainWindow _appMainWindow;

        /// <summary>
        /// Окно создания окна
        /// </summary>
        private ForRobot.Views.Windows.CreateWindow _createWindow { get; set; }

        /// <summary>
        /// Окно настроек
        /// </summary>
        private ForRobot.Views.Windows.PropertiesWindow _propertiesWindow { get; set; }

        public Views.Windows.MainWindow AppMainWindow { get => _appMainWindow ?? (_appMainWindow = new Views.Windows.MainWindow()); }

        public string InputWindowShow(string sInputBoxText = "Введите пин-код")
        {

            ForRobot.Views.Windows.InputWindow inputWindow = new ForRobot.Views.Windows.InputWindow(sInputBoxText);
            inputWindow.ShowDialog();
            string answer = inputWindow.Answer;
            inputWindow.Dispose();
            GC.SuppressFinalize(inputWindow);
            return answer;
        }

        public void OpenCreateWindow()
        {
            if (object.Equals(this._createWindow, null)) // Блокировка открытия 2-ого окна.
            {
                this._createWindow = new ForRobot.Views.Windows.CreateWindow();
                this._createWindow.Closed += (a, b) => this._createWindow = null;
                this._createWindow.Owner = App.Current.MainWindow;
                this._createWindow.Show();
            }
        }
        public void OpenPropertiesWindow()
        {
            if (object.Equals(this._propertiesWindow, null)) // Блокировка открытия 2-ого окна.
            {
                this._propertiesWindow = new ForRobot.Views.Windows.PropertiesWindow();
                this._propertiesWindow.Closed += (a, b) => this._propertiesWindow = null;
                this._propertiesWindow.Owner = App.Current.MainWindow;
                this._propertiesWindow.Show();
            }
        }        

        public void CloseCreateWindow() => this._createWindow.Close();
        public void ClosePropertiesWindow() => this._propertiesWindow.Close();
    }
}
