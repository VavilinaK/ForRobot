using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Media3D;
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
        private string _filePath = string.Empty;

        private string _plitaProgramName = App.Current.Settings.PlitaProgramName;
        private string _plitaStringerProgramName = App.Current.Settings.PlitaStringerProgramName;
        private string _plitaTreugolnikProgramName = App.Current.Settings.PlitaTreugolnikProgramName;
        private Model.File3D.File3D _file3D;

        #endregion Private variables

        #region Public variables

        /// <summary>
        /// Выбранный тип детали
        /// </summary>
        public string SelectedDetalType
        {
            get => this._selectedDetalType ?? (this._selectedDetalType = DetalType.Plita.ToString());
            set
            {
                this._selectedDetalType = value;
                this.File3D = new Model.File3D.File3D(Detal.GetDetal(this._selectedDetalType));
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

        public Model.File3D.File3D File3D { get => this._file3D; set => Set(ref this._file3D, value); }
        
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

        #region Commands

        /// <summary>
        /// Выбор пути для файла
        /// </summary>
        public ICommand ReviewCommand { get => new RelayCommand(_ => this.SelectFilePath()); }

        ///// <summary>
        ///// Создание файла
        ///// </summary>
        //public ICommand CreateCommand { get => new RelayCommand(_ => this.CreatedFile()); }

        /// <summary>
        /// Закрытие окна
        /// </summary>
        //public ICommand CancelCommand { get; } = new RelayCommand(_ => App.Current.WindowsAppService.CloseCreateWindow());

        #endregion Commands

        #endregion Public variables

        #region Constructor

        public CreateWindowViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
        }

        #endregion

        #region Private functions
        
        private void SelectFilePath()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    this.FilePath = fbd.SelectedPath;
            }
        }

        private void CreatedFile()
        {
            if (string.IsNullOrEmpty(this.FilePath) || string.IsNullOrEmpty(this.FileName))
                return;

            this.File3D.Path = Path.Combine(this.FileName, this.FilePath);

            App.Current.OpenedFiles.Add(this.File3D);
            //App.Current.WindowsAppService.CloseCreateWindow();
        }

        #endregion Private functions
    }
}
