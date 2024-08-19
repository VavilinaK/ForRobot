using System;
using System.Windows;
using System.Windows.Media.Imaging;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ForRobot.Model.Detals
{
    public class PlitaStringer : Detal
    {
        [JsonIgnore]
        /// <summary>
        /// Тип детали
        /// </summary>
        public override string DetalType { get => DetalTypes.Stringer; }

        //public override sealed BitmapImage GenericImage { get => (BitmapImage)Application.Current.FindResource("ImagePlitaStringerFull"); }

        #region Constructor

        public PlitaStringer() { }

        public PlitaStringer(DetalType type) : base(type) { }

        #endregion
    }
}
