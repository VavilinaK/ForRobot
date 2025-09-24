using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ForRobot.Model.File3D;
using ForRobot.Model.Detals;

namespace ForRobot.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для CreateWindow.xaml
    /// </summary>
    public partial class CreateWindow : Window, IDisposable
    {
        private string _selectedDetalType = DetalTypes.Plita;

        private string _plitaProgramName = App.Current.Settings.PlitaProgramName;
        private string _plitaStringerProgramName = App.Current.Settings.PlitaStringerProgramName;
        private string _plitaTreugolnikProgramName = App.Current.Settings.PlitaTreugolnikProgramName;

        /// <summary>
        /// Выбранный тип детали
        /// </summary>
        public string SelectedDetalType
        {
            get => this._selectedDetalType;
            set
            {
                this._selectedDetalType = value;
                this.CreationFile.Detal = Detal.GetDetal(this._selectedDetalType);
            }
        }

        public string FileName
        {
            get
            {
                switch (this.SelectedDetalType)
                {
                    case string a when a == DetalTypes.Plita:
                        return this._plitaProgramName;

                    case string b when b == DetalTypes.Stringer:
                        return this._plitaStringerProgramName;

                    case string c when c == DetalTypes.Treygolnik:
                        return this._plitaTreugolnikProgramName;

                    default:
                        return string.Empty;
                }
            }
            set
            {
                switch (this.SelectedDetalType)
                {
                    case string a when a == DetalTypes.Plita:
                        this._plitaProgramName = value;
                        break;

                    case string b when b == DetalTypes.Stringer:
                        this._plitaStringerProgramName = value;
                        break;

                    case string c when c == DetalTypes.Treygolnik:
                        this._plitaTreugolnikProgramName = value;
                        break;
                }
            }
        }

        public string Path { get; set; }

        public File3D CreationFile { get; set; } = new File3D();

        /// <summary>
        /// Коллекция видов деталей
        /// </summary>
        public ObservableCollection<string> DetalTypeCollection
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

        public CreateWindow()
        {
            InitializeComponent();
            this.Closed += (a, b) => this.Dispose();
        }

        public CreateWindow(DetalType detalType, string path = null) : this()
        {
            Detal detal = Detal.GetDetal(DetalTypes.EnumToString(detalType));
            this.CreationFile = new File3D(detal, path);
        }

        /// <summary>
        /// Метод выделения содержимого TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Text_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null)
                textBox.SelectAll();
        }

        /// <summary>
        /// Метод выделения содержимого TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null && !textBox.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                textBox.Focus();
            }
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.CreationFile.Path = System.IO.Path.Combine(this.Path, this.FileName);
            this.DialogResult = true;
        }

        #region IDisposable Support

        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing) { }
                _disposedValue = true;
            }
        }

        ~CreateWindow() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
