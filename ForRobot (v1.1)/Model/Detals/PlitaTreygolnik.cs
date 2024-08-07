using System;
using System.Windows;
using System.Windows.Media.Imaging;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ForRobot.Model.Detals
{
    public class PlitaTreygolnik : Detal
    {
        [JsonIgnore]
        /// <summary>
        /// Тип детали
        /// </summary>
        public override DetalType DetalType { get => DetalType.Treygolnik; }

        //public override sealed BitmapImage GenericImage { get => (BitmapImage)Application.Current.FindResource("ImagePlitaTreygolnikFull"); }

        #region Constructors

        public PlitaTreygolnik() { }

        public PlitaTreygolnik(DetalType type) : base(type) { }

        #endregion
    }
}
