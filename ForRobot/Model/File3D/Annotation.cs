using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using HelixToolkit.Wpf;

namespace ForRobot.Model.File3D
{
    public class Annotation : TextVisual3D
    {
        public const double DefaultFontSize = 32;
        //public const Brush DefaultForeground = new BrushConverter().ConvertFromString("");
        //public const System.Windows.Thickness thickness = new System.Windows.Thickness(4, 4, 4, 4);


        public string PropertyName { get; set; }

        public Annotation()
        {
            this.FontSize = DefaultFontSize;
            this.Foreground = Brushes.Black;
            this.Padding = new System.Windows.Thickness(4, 4, 4, 4);
        }

        ///// <summary>
        ///// Начало стрелки
        ///// </summary>
        //public Point3D StartPoint { get; set; }

        ///// <summary>
        ///// Конец стрелки
        ///// </summary>
        //public Point3D EndPoint { get; set; }

        ///// <summary>
        ///// Текст подписи
        ///// </summary>
        //public string Label { get; set; }

        ///// <summary>
        ///// Цвет линии
        ///// </summary>
        //public Color Color { get; set; }
    }
}
