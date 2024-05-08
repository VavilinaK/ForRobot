using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.ComponentModel;

using ForRobot.Model;

namespace ForRobot.ViewModels
{
    public class Page2DViewModel : BaseClass
    {
        #region Private variables

        private Detal _detal;
        #endregion

        #region Public variables

        public Detal DetalObject { get => this._detal; set => Set(ref this._detal, value); }

        #endregion

        #region Constructors

        public Page2DViewModel() : this(null) { }

        public Page2DViewModel(Detal detal)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            this.DetalObject = detal;
        }

        #endregion
    }
}
