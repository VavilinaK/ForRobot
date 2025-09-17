using System;
using ForRobot.Model.File3D;
using ForRobot.Model.Detals;

namespace ForRobot.Services
{
    public class ChangeService : BaseClass
    {
        private readonly ForRobot.Services.IModelingService _modelingService = new ForRobot.Services.ModelingService(ForRobot.Model.Settings.Settings.ScaleFactor);
        private readonly ForRobot.Services.IWeldService _weldService = new ForRobot.Services.WeldService(ForRobot.Model.Settings.Settings.ScaleFactor);

        /// <summary>
        /// Делегат изменения <see cref="ForRobot.Model.Detals.Detal"/> оповещающий о изменению его свойств
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleDetalChanged_Properties(object sender, Libr.ValueChangedEventArgs<Detal> e)
        {
            Model.File3D.File3D file3D = sender as Model.File3D.File3D;
            RaisePropertyChanged(nameof(file3D.Detal));
            //RaisePropertyChanged(nameof(file3D.CurrentModel));

            if (file3D.Detal is ForRobot.Model.Detals.Plita)
            {
                Plita plita = file3D.Detal as ForRobot.Model.Detals.Plita;
                RaisePropertyChanged(nameof(plita.SelectedWeldingSchema));
                RaisePropertyChanged(nameof(plita.WeldingSchema));
            }

            //if(e.OldValue != null)
            //    this.TrackUndo(e.OldValue, e.NewValue);
        }

        //private void FillWeldsCollection(Detal detal)
        //{
        //    foreach (var item in this.WeldsCollection) item.Children.Clear();
        //    this.WeldsCollection.Clear();
        //    foreach (var item in this._weldService.GetWelds(detal)) this.WeldsCollection.Add(item);
        //}

        /// <summary>
        /// Делегат изменения <see cref="ForRobot.Model.Detals.Detal"/>, изменяющий CurrentModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleDetalChanged_Modeling(object sender, Libr.ValueChangedEventArgs<Detal> e)
        {
            File3D file = sender as File3D;
            file.CurrentModel.Children.Clear();
            Detal detal = file.Detal;
            switch (detal.DetalType)
            {
                case DetalTypes.Plita:
                    Plita plita = detal as Plita;
                    file.CurrentModel.Children.Add(this._modelingService.ModelBuilding(plita));
                    //file.FillWeldsCollection(plita);
                    break;

                case DetalTypes.Stringer:
                    break;

                case DetalTypes.Treygolnik:
                    break;
            }
        }

        public void HandleModelChanged(object sender, EventArgs e)
        {
            Model.File3D.File3D file3D = sender as Model.File3D.File3D;
            RaisePropertyChanged(nameof(file3D.CurrentModel));
        }

        public void HandleFileChange(object sender, EventArgs e)
        {
            Model.File3D.File3D file3D = sender as Model.File3D.File3D;
            RaisePropertyChanged(nameof(file3D));
        }
    }
}
