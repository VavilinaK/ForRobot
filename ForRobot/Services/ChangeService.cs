using System;
using ForRobot.Model.File3D;
using ForRobot.Model.Detals;
using ForRobot.Libr.UndoRedo;

namespace ForRobot.Services
{
    public class ChangeService : BaseClass
    {
        private readonly ForRobot.Services.IModelingService _modelingService = new ForRobot.Services.ModelingService(ForRobot.Model.Settings.Settings.ScaleFactor);
        //private readonly ForRobot.Services.IWeldService _weldService = new ForRobot.Services.WeldService(ForRobot.Model.Settings.Settings.ScaleFactor);

        /// <summary>
        /// Делегат изменения <see cref="ForRobot.Model.Detals.Detal"/> оповещающий о изменению его свойств
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleDetalChanged_Properties(object sender, Libr.ValueChangedEventArgs<Detal> e)
        {
            Model.File3D.File3D file3D = sender as Model.File3D.File3D;
            RaisePropertyChanged(nameof(file3D.CurrentDetal));

            if (file3D.CurrentDetal is ForRobot.Model.Detals.Plita)
            {
                Plita plita = file3D.CurrentDetal as ForRobot.Model.Detals.Plita;
                RaisePropertyChanged(nameof(plita.SelectedWeldingSchema));
                RaisePropertyChanged(nameof(plita.WeldingSchema));
            }

            if (e.OldValue != null)
            {
                var command = new PropertyChangeCommand<Detal>(file3D,
                                                               nameof(File3D.CurrentDetal),
                                                               e.OldValue,
                                                               e.NewValue,
                                                               $"Изменение детали: {e.OldValue?.GetType().Name} -> {e.NewValue?.GetType().Name}");
                file3D.AddUndoCommand(command);
            }
        }

        /// <summary>
        /// Делегат изменения <see cref="ForRobot.Model.Detals.Detal"/>, изменяющий CurrentModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleDetalChanged_Modeling(object sender, Libr.ValueChangedEventArgs<Detal> e)
        {
            File3D file = sender as File3D;
            try
            {
                Detal detal = file.CurrentDetal;
                System.Windows.Media.Media3D.Model3DGroup model = null;
                switch (detal.DetalType)
                {
                    case DetalTypes.Plita:
                        Plita plita = detal as Plita;
                         model = this._modelingService.ModelBuilding(plita);
                        break;

                    case DetalTypes.Stringer:
                        break;

                    case DetalTypes.Treygolnik:
                        break;
                }
                file.CurrentModel.Children.Clear();
                file.CurrentModel.Children.Add(model);
            }
            catch (Exception ex)
            {
                App.Current.Logger.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Делегат изменения CurrentModel в <see cref="ForRobot.Model.File3D.File3D"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleModelChanged(object sender, EventArgs e)
        {
            Model.File3D.File3D file3D = sender as Model.File3D.File3D;
            RaisePropertyChanged(nameof(file3D.CurrentModel));
        }

        /// <summary>
        /// Делегат изменения свойств <see cref="ForRobot.Model.File3D.File3D"/>, изменяющий CurrentModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleFileChange(object sender, EventArgs e)
        {
            Model.File3D.File3D file3D = sender as Model.File3D.File3D;
            RaisePropertyChanged(nameof(file3D));
        }
    }
}
