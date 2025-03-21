using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Win32;

using ForRobot.Model.Detals;

namespace ForRobot.ViewModels
{
    public class CreateWindowViewModel : BaseClass
    {
        #region Private variables

        private string _selectedDetalType;
        private string _filePath;

        private string _plitaProgramName = App.Current.Settings.PlitaProgramName;
        private string _plitaStringerProgramName = App.Current.Settings.PlitaStringerProgramName;
        private string _plitaTreugolnikProgramName = App.Current.Settings.PlitaTreugolnikProgramName;
        private string _fileName;
        private Detal _detalObject;

        #region Commands

        private RelayCommand _reviewCommand;
        private RelayCommand _createCommand;
        private RelayCommand _cancelCommand;

        #endregion

        #endregion

        #region Public variables

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

        /// <summary>
        /// Выбранный тип детали
        /// </summary>
        public string SelectedDetalType
        {
            get => this._selectedDetalType ?? (this._selectedDetalType = DetalType.Plita.ToString());
            set
            {
                this._selectedDetalType = value;
                this.DetalObject = Detal.GetDetal(this._selectedDetalType);
                RaisePropertyChanged(nameof(this.SelectedDetalType), nameof(this.FileName));
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
                RaisePropertyChanged(nameof(this.FileName));
            }
        }

        public string FilePath { get => this._filePath; set => Set(ref this._filePath, value); }

        /// <summary>
        /// Объект детали
        /// </summary>
        public Detal DetalObject { get => this._detalObject; set => Set(ref this._detalObject, value); }

        #region Commands

        /// <summary>
        /// Выбор пути для файла
        /// </summary>
        public RelayCommand ReviewCommand
        {
            get
            {
                return _reviewCommand ??
                    (_reviewCommand = new RelayCommand(obj =>
                    {
                        using (var fbd = new FolderBrowserDialog())
                        {
                            DialogResult result = fbd.ShowDialog();
                            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                                this.FilePath = fbd.SelectedPath;
                        }
                    }));
            }
        }

        /// <summary>
        /// Создание файла
        /// </summary>
        public RelayCommand CreateCommand
        {
            get
            {
                return _createCommand ??
                    (_createCommand = new RelayCommand(obj =>
                    {
                        if (string.IsNullOrEmpty(this.FilePath) || string.IsNullOrEmpty(this.FileName))
                            return;

                        App.Current.OpenedFiles.Add(new Model.File3D.File3D(this.DetalObject, Path.Combine(this.FilePath, this.FileName)));
                        App.Current.CreateWindow.Close();
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
                        App.Current.CreateWindow.Close();
                    }));
            }
        }

        #endregion

        #endregion

        #region Constructor

        public CreateWindowViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
        }

        #endregion

        #region Private functions

        ///// <summary>
        ///// Сохранение изменений Detal
        ///// </summary>
        //private void SaveDetal(Detal detal)
        //{
        //    switch (detal)
        //    {
        //        case Plita plita:
        //            Properties.Settings.Default.SavePlita = detal.JsonForSave;
        //            break;

        //        case PlitaStringer plitaStringer:
        //            Properties.Settings.Default.SavePlitaStringer = "";
        //            break;

        //        case PlitaTreygolnik plitaTreygolnik:
        //            Properties.Settings.Default.SavePlita = "";
        //            break;
        //    }
        //    Properties.Settings.Default.Save();
        //}

        #endregion
    }
}
