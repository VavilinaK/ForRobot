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

        private RelayCommand _editPathForUpdateCommand;

        private RelayCommand _editPinCodeCommand;
        
        #endregion

        #endregion

        #region Public variables

        public bool AutoUpdate
        {
            get => Properties.Settings.Default.AutoUpdate;
            set
            {
                Properties.Settings.Default.AutoUpdate = value;
                Properties.Settings.Default.Save();
            }
        }

        public bool InformUser
        {
            get => Properties.Settings.Default.InformUser;
            set
            {
                Properties.Settings.Default.InformUser = value;
                Properties.Settings.Default.Save();
            }
        }

        #region Commands

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
