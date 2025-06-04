using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ForRobot.Model.File3D
{
    public class Materials
    {
        public static Brush DefaultPlateBrush => new System.Windows.Media.BrushConverter().ConvertFromString("#17e64b") as System.Windows.Media.Brush;
        public static Brush DefaultRibBrush => new System.Windows.Media.BrushConverter().ConvertFromString("#17e64b") as System.Windows.Media.Brush;
        public static Brush DefaultWeldBrush => new System.Windows.Media.BrushConverter().ConvertFromString("#ff8e14") as System.Windows.Media.Brush;
        public static Brush DefaultArrowBrush => Brushes.Blue;

        public static Material Plate { get; set; }
        public static Material Rib { get; set; }
        public static Material Weld { get; set; }
        public static Material Arrow { get; set; }

        static Materials()
        {
            var brushConverter = new BrushConverter();
            Plate = new DiffuseMaterial(DefaultPlateBrush) { AmbientColor = System.Windows.Media.Colors.White };
            Rib = new DiffuseMaterial(DefaultRibBrush) { AmbientColor = System.Windows.Media.Colors.White };
            Weld = new DiffuseMaterial(DefaultWeldBrush);
            Arrow = new DiffuseMaterial(DefaultArrowBrush);
        }

        ////public static Material Plate => new DiffuseMaterial(new System.Windows.Media.BrushConverter().ConvertFromString("#6cc3e6") as System.Windows.Media.Brush) { AmbientColor = Colors.White };
        //public static Material Plate => new DiffuseMaterial(new System.Windows.Media.BrushConverter().ConvertFromString("#17e64b") as System.Windows.Media.Brush) { AmbientColor = Colors.White };
        //public static Material Rib => new DiffuseMaterial(new System.Windows.Media.BrushConverter().ConvertFromString("#17e64b") as System.Windows.Media.Brush) { AmbientColor = Colors.White };
        //public static Material Weld => new DiffuseMaterial(new System.Windows.Media.BrushConverter().ConvertFromString("#ff8e14") as System.Windows.Media.Brush) { AmbientColor = Colors.White };
        //public static Material Arrow => new DiffuseMaterial(new System.Windows.Media.BrushConverter().ConvertFromString("#6cc3e6") as System.Windows.Media.Brush);
        //public static Material Steel => new DiffuseMaterial(Brushes.Silver);
        //public static Material Hole => new DiffuseMaterial(Brushes.DarkGray);
    }
}
