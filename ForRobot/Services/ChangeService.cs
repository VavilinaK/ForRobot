using System;
using ForRobot.Model.Detals;

namespace ForRobot.Services
{
    public class ChangeService : BaseClass
    {
        public void HandleDetalPropertyChange(object sender, Libr.ValueChangedEventArgs<Detal> e)
        {
            Model.File3D.File3D file3D = sender as Model.File3D.File3D;
            RaisePropertyChanged(nameof(file3D.Detal));

            if (file3D.Detal is ForRobot.Model.Detals.Plita)
            {
                Plita plita = file3D.Detal as ForRobot.Model.Detals.Plita;
                RaisePropertyChanged(nameof(plita.SelectedWeldingSchema));
                RaisePropertyChanged(nameof(plita.WeldingSchema));
            }


        }

        public void HandleFileChange(object sender, EventArgs e)
        {
            Model.File3D.File3D file3D = sender as Model.File3D.File3D;
            RaisePropertyChanged(nameof(file3D));
        }
    }
}
