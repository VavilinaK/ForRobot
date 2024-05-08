using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ForRobot.Model
{
    public class PlitaTreygolnik : Detal
    {
        public override sealed DetalType DetalType { get => DetalType.Treygolnik; }

        //public override sealed BitmapImage GenericImage { get => (BitmapImage)Application.Current.FindResource("ImagePlitaTreygolnikFull"); }
    }
}
