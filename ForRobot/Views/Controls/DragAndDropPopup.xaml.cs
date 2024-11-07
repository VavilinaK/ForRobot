using System;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using ForRobot.Model.Controls;

namespace ForRobot.Views.Controls
{
    public class DragAndDropFile : INotifyPropertyChanged
    {
        /// <summary>
        /// Имя с разрешением
        /// </summary>
        public string FullName { get; private set; }

        public string OldPath { get; private set; }

        //public string NewPath { get; set; }

        //public string IconSVG
        //{
        //    get
        //    {
        //        switch (this.Type)
        //        {
        //            case FileTypes.DataList:
        //                return "M26.204116796875 8.261261718750001 18.552580078124997 0.609725c-0.20516367187499998 -0.20493710937499998 -0.48335976562499994 -0.31995156249999995 -0.7733484375 -0.319725H4.662305078125c-1.20741953125 -0.000056640625 -2.186146875 0.9787386718750001 -2.186146875 2.186158203125v24.047683593749998c0 1.20741953125 0.9787273437499999 2.1862148437499997 2.186146875 2.186158203125h19.67538984375c1.20741953125 0.000056640625 2.186146875 -0.9787386718750001 2.186146875 -2.186158203125V9.034621484375c0.0002265625 -0.29 -0.11478789062500001 -0.568184765625 -0.319725 -0.773359765625Zm-7.33181171875 12.7972015625H10.127694921875c-0.841453125 -0.0000453125 -1.3673726562500002 -0.9109625000000001 -0.946612109375 -1.63965546875 0.195262890625 -0.338178515625 0.556108984375 -0.546502734375 0.946612109375 -0.546502734375h8.74461015625c0.841453125 0 1.3673613281249999 0.9108945312500001 0.946634765625 1.639621484375 -0.1952515625 0.338178515625 -0.556131640625 0.5465140625 -0.946634765625 0.54653671875Zm0 -4.372305078125H10.127694921875c-0.841453125 -0.0000453125 -1.3673726562500002 -0.9109625000000001 -0.946612109375 -1.6396667968749998C9.376345703125 14.70832421875 9.737191796875 14.5 10.127694921875 14.5h8.74461015625c0.841453125 0 1.3673613281249999 0.9108945312500001 0.946634765625 1.63961015625 -0.1952515625 0.33818984375 -0.556131640625 0.5465253906249999 -0.946634765625 0.546548046875Zm-1.0930734375 -7.65153671875V3.022694921875l6.0119265625 6.0119265625Z";

        //            case FileTypes.Program:
        //                return "M26.523841796875 2.476158203125H2.476158203125C1.268738671875 2.4761015625000002 0.29 3.454885546875 0.29 4.662305078125v19.67538984375c0 1.207408203125 0.9787386718750001 2.186203515625 2.186158203125 2.186146875h24.047683593749998c1.207340234375 -0.0000453125 2.186158203125 -0.9788066406249999 2.186158203125 -2.186146875V4.662305078125c0 -1.20737421875 -0.97877265625 -2.186146875 -2.186158203125 -2.186146875Zm-12.43375 12.877812500000001 -5.465378515625 4.372305078125c-0.657382421875 0.525908203125 -1.637559765625 0.1429609375 -1.764321484375 -0.689305078125 -0.058826953125 -0.386255078125 0.09287929687499999 -0.7745605468750001 0.397968359375 -1.0186250000000001L11.656629296875 14.5 7.258360156250001 10.981654296875c-0.657382421875 -0.525908203125 -0.4989359375 -1.566237890625 0.285208203125 -1.872595703125 0.363916015625 -0.142179296875 0.776055859375 -0.07941015624999999 1.081144921875 0.164665625l5.465378515625 4.372305078125c0.547680859375 0.43759414062499996 0.547680859375 1.270347265625 0 1.70794140625Zm6.968371484375 4.611419140625h-5.465389843750001c-0.841441796875 -0.0000453125 -1.3673613281249999 -0.9109625000000001 -0.9466007812499999 -1.6396667968749998 0.195262890625 -0.33816718749999997 0.556108984375 -0.54649140625 0.9466007812499999 -0.54649140625h5.465389843750001c0.841453125 0 1.3673613281249999 0.9108945312500001 0.946634765625 1.63961015625 -0.1952515625 0.33818984375 -0.556131640625 0.5465253906249999 -0.946634765625 0.546548046875Z";

        //            default:
        //                return "M12.57,17.29a1,1,0,0,0-1.41,0,1.06,1.06,0,0,0-.22.33,1.07,1.07,0,0,0,0,.76,1.19,1.19,0,0,0,.22.33,1,1,0,0,0,.32.21,1,1,0,0,0,.39.08,1,1,0,0,0,.92-1.38A.91.91,0,0,0,12.57,17.29ZM20,8.94a1.31,1.31,0,0,0-.06-.27l0-.09a1.07,1.07,0,0,0-.19-.28h0l-6-6h0a1.07,1.07,0,0,0-.28-.19l-.09,0A.88.88,0,0,0,13.05,2H7A3,3,0,0,0,4,5V19a3,3,0,0,0,3,3H17a3,3,0,0,0,3-3V9S20,9,20,8.94ZM14,5.41,16.59,8H15a1,1,0,0,1-1-1ZM18,19a1,1,0,0,1-1,1H7a1,1,0,0,1-1-1V5A1,1,0,0,1,7,4h5V7a3,3,0,0,0,3,3h3Zm-6.13-9a3,3,0,0,0-2.6,1.5,1,1,0,1,0,1.73,1,1,1,0,0,1,.87-.5,1,1,0,0,1,0,2,1,1,0,1,0,0,2,3,3,0,0,0,0-6Z";
        //        }
        //    }
        //}

        public FileTypes Type
        {
            get
            {
                switch (Path.GetExtension(this.OldPath))
                {
                    case ".dat":
                        return FileTypes.DataList;

                    case ".src":
                        return FileTypes.Program;

                    default:
                        return FileTypes.Unknow;
                }
            }
        }

        public bool IsCopy { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public DragAndDropFile(string sFilePath)
        {
            this.OldPath = sFilePath;
            this.FullName = Path.GetFileName(sFilePath);
        }
    }

    /// <summary>
    /// Логика взаимодействия для DragAndDropPopup.xaml
    /// </summary>
    public partial class DragAndDropPopup : UserControl, INotifyPropertyChanged
    {
        #region Private variables

        private bool _isAddingFile = false;

        //private ForRobot.Libr.FullyObservableCollection<DragAndDropFile> _copyFiles;

        private ForRobot.Libr.FullyObservableCollection<DragAndDropFile> _copyFiles = new Libr.FullyObservableCollection<DragAndDropFile>(new System.Collections.Generic.List<DragAndDropFile>()
        {
            new DragAndDropFile(@"D:\newPrograms\R2\main_gen.src"),
            new DragAndDropFile(@"D:\newPrograms\R2\edge_4_right_mte.dat"),
            new DragAndDropFile(@"D:\Пути установки.txt")
        });


        #region Commands

        private static RelayCommand _changeWorkingModeCommand;

        private static RelayCommand _selectFilesCommand;

        #endregion

        #endregion

        #region Public variables

        /// <summary>
        /// Допустимые разрешения файлов
        /// </summary>
        public string[] AccopterebulExtension { get; } = new string[]
        {
            ".dat",
            ".src"
        };

        /// <summary>
        /// Добавляется ли в данный момент файл
        /// </summary>
        public bool IsAddingFile
        {
            get => this._isAddingFile;
            private set => Set(ref this._isAddingFile, value);
        }

        public ForRobot.Libr.FullyObservableCollection<DragAndDropFile> CopyFiles
        {
            get => this._copyFiles ?? (this._copyFiles = new Libr.FullyObservableCollection<DragAndDropFile>());
            set
            {
                this._copyFiles = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CopyFiles)));
            }
        }

        #region Properties

        /// <summary>
        /// Конечный путь для копирования файлов
        /// </summary>
        public string FinishPath
        {
            get => (string)GetValue(FinishPathProperty);
            set => SetValue(FinishPathProperty, value);
        }

        public static readonly DependencyProperty FinishPathProperty = DependencyProperty.Register(nameof(FinishPath),
                                                                                           typeof(string),
                                                                                           typeof(DragAndDropPopup));

        #endregion

        #region Events


        #endregion

        #region Commands

        /// <summary>
        /// Команда смены режима работы <see cref="DragAndDropPopup"/>
        /// </summary>
        private static RelayCommand ChangeWorkingModeCommand
        {
            get
            {
                return _changeWorkingModeCommand ??
                    (_changeWorkingModeCommand = new RelayCommand(obj =>
                    {

                    }));
            }
        }

        /// <summary>
        /// Выбор файлов для отправки на роботы
        /// </summary>
        public RelayCommand SelectedFilesCommand
        {
            get
            {
                return _selectFilesCommand ??
                    (_selectFilesCommand = new RelayCommand(obj =>
                    {
                        System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
                        {
                            Filter = "Source Code or Data files (*.src, *.dat)|*.src;*.dat|Data files (*.dat)|*.dat|Source Code File (*.src)|*src",
                            Title = $"Копирование файлов в {this.FinishPath}"
                        };

                        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel && string.IsNullOrEmpty(openFileDialog.FileName))
                            return;



                    }));
            }
        }

        #endregion

        #endregion

        #region Constructor

        public DragAndDropPopup()
        {
            InitializeComponent();
        }

        #endregion

        #region Private functions

        private void Border_Drop(object sender, DragEventArgs e)
        {
            this.IsAddingFile = false;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            for (int i = 0; i < files.Length; i++)
            {
                if (this.AccopterebulExtension.Any(item => item == Path.GetExtension(files[i])))
                    this.CopyFiles.Add(new DragAndDropFile(files[i]));
            }
        }

        private void Border_DragOver(object sender, DragEventArgs e) => this.IsAddingFile = true;

        private void Border_DragLeave(object sender, DragEventArgs e) => this.IsAddingFile = false;

        #region Static

        //private static void OnIsAddingFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    DragAndDropPopup dragAndDropPopup = (DragAndDropPopup)d;
        //    dragAndDropPopup.IsAddingFile = (bool)e.NewValue;
        //    dragAndDropPopup.PropertyChanged?.Invoke(dragAndDropPopup, new PropertyChangedEventArgs(nameof(dragAndDropPopup.IsAddingFile)));
        //}

        #endregion

        #endregion

        #region Public functions

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            foreach (var prop in propertyNames)
            {
                this.RaisePropertyChanged(prop);
            }
        }

        protected void Set<T>(ref T propertyFiled, T newValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!object.Equals(propertyFiled, newValue))
            {
                T oldValue = propertyFiled;
                propertyFiled = newValue;
                RaisePropertyChanged(propertyName);

                OnPropertyChanged(propertyName, oldValue, newValue);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName, object oldValue, object newValue) { }

        #endregion
    }
}
