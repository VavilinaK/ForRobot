using System;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ForRobot.Model.Detals;

namespace ForRobot.ViewModels
{
    public class MainPageViewModel2 : BaseClass
    {
        #region Private variables

        private Detal _detalObject;

        #endregion

        #region Public variables

        public Version Version { get => System.Reflection.Assembly.GetEntryAssembly().GetName().Version; }

        public Detal DetalObject
        {
            get => this._detalObject;
            set => Set(ref this._detalObject, value);
        }

        /// <summary>
        /// выбранный тип детали
        /// </summary>
        public string SelectedDetalType
        {
            get => this.DetalObject.DetalType;
            set
            {
                //switch (value)
                //{
                //    case DetalTypes.Plita:
                //        break;
                //}
                if (value == DetalTypes.Plita)
                {
                    this.DetalObject = new Plita();
                    //DetalObject = GetSavePlita();
                    //((Plita)this.DetalObject).RibsCollection.ItemPropertyChanged += (o, e) => this.SaveDetal();
                }
                //if (value == DetalTypes.Stringer) { DetalObject = GetSavePlitaStringer(); }
                //if (value == DetalTypes.Treygolnik) { DetalObject = GetSavePlitaTreygolnik(); }

                //this.DetalObject.Change += ChangeProperiesDetal; // Обределение события изменения свойств

                //RaisePropertyChanged("SelectedType");
                //RaisePropertyChanged("PathGenerator");
                //RaisePropertyChanged("ProgrammName");
            }
        }

        /// <summary>
        /// Коллекция видов деталей
        /// </summary>
        public ObservableCollection<string> DetalTypeCollection
        {
            get
            {
                List<string> detalTypesList = new List<string>();
                foreach(var f in typeof(ForRobot.Model.Detals.DetalTypes).GetFields())
                {
                    detalTypesList.Add(f.GetValue(null).ToString());
                }
                return new ObservableCollection<string>(detalTypesList);
            }
        }

        #endregion

        #region Constructor

        public MainPageViewModel2()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
        }

        #endregion

        #region Private functions

        #endregion
    }
}
