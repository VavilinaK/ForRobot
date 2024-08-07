using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ForRobot.Model
{
    public class PlitaStringer : Detal
    {
        public override sealed DetalType DetalType { get => DetalType.Stringer; }

        //public override sealed BitmapImage GenericImage { get => (BitmapImage)Application.Current.FindResource("ImagePlitaStringerFull"); }
    }
}
