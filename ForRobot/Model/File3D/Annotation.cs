using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using HelixToolkit.Wpf;

namespace ForRobot.Model.File3D
{
    public class Annotation : ScreenSpaceVisual3D
    {
        private readonly MeshVisual3D _arrowLine;
        private readonly MeshVisual3D _arrowHead;
        private readonly TextVisual3D _label;

        public static double DefaultFontSize { get; set; } = 32;
        public static double DefaultThickness { get; set; } = 2.0;
        public static double DefaultHeadSize { get; set; } = 10.0;
        public static Brush DefaultTextForeground { get; set; } = Brushes.Black;
        public static Brush DefaultTextBackground { get; set; } = Brushes.Transparent;
        public static Material DefaultArrowMaterial { get; set; } = new DiffuseMaterial(Brushes.Red);

        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        /// <summary>
        /// Начало стрелки
        /// </summary>
        public Point3D StartPoint { get; set; }
        /// <summary>
        /// Конец стрелки
        /// </summary>
        public Point3D EndPoint { get; set; }

        public string PropertyName { get; set; }

        public Annotation() : this(new Point3D(0, 0, 0), new Point3D(0, 0, 0)) { }

        public Annotation(Point3D startPoint, Point3D endPoint)
        {
            _label = new TextVisual3D
            {
                FontSize = DefaultFontSize,
                Foreground = DefaultTextForeground,
                Background = DefaultTextBackground,
                IsDoubleSided = true,
                Padding = new System.Windows.Thickness(4, 4, 4, 4)
            };

            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            
            //Children.Add(_arrowLine);
            //Children.Add(_arrowHead);
            //Children.Add(_label);

            this.UpdateGeometry();
        }

        ///// <summary>
        ///// Преобразование экранных координат
        ///// </summary>
        ///// <param name="screenPoint"></param>
        ///// <returns></returns>
        //private Point3D To3DPoint(Point screenPoint)
        //{
        //    return new HelixViewport3D().Viewport.UnProject(new Point3D(
        //        screenPoint.X,
        //        screenPoint.Y,
        //        0.9 // Глубина для 2D элементов
        //    ));
        //}

        protected void UpdateGeometry(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((Annotation)d).UpdateGeometry();

        protected override bool UpdateTransforms()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateGeometry()
        {
            // Построение линии
            //var lineMesh = new MeshBuilder();
            //lineMesh.AddPipe(this.StartPoint, this.EndPoint, DefaultThickness, 4, 0);
            //_arrowLine.Mesh = lineMesh.ToMesh();

            // Построение наконечника
            //var direction = (this.EndPoint - this.StartPoint).Normalized();
            //var headBase = this.EndPoint - direction * DefaultHeadSize;

            //var headMesh = new MeshBuilder();
            //headMesh.AddArrow(headBase, end3D, DefaultHeadSize * 2, 0);
            //_arrowHead.Mesh = headMesh.ToMesh();

            // Позиция текста
            //_label.Position = this.StartPoint + (this.EndPoint - this.StartPoint) * 0.5;
        }
    }

    //public class Annotation : TextVisual3D
    //{
    //    public static double DefaultFontSize { get; set; } = 32;
    //    //public const Brush DefaultForeground = new BrushConverter().ConvertFromString("");
    //    //public const System.Windows.Thickness thickness = new System.Windows.Thickness(4, 4, 4, 4);

    //    public string PropertyName { get; set; }

    //    public Annotation()
    //    {
    //        this.FontSize = DefaultFontSize;
    //        this.Foreground = Brushes.Black;
    //        this.Padding = new System.Windows.Thickness(4, 4, 4, 4);
    //    }

    //    ///// <summary>
    //    ///// Начало стрелки
    //    ///// </summary>
    //    //public Point3D StartPoint { get; set; }

    //    ///// <summary>
    //    ///// Конец стрелки
    //    ///// </summary>
    //    //public Point3D EndPoint { get; set; }

    //    public bool DoubleArrows { get; set; } = false;

    //    ///// <summary>
    //    ///// Текст подписи
    //    ///// </summary>
    //    //public string Label { get; set; }

    //    ///// <summary>
    //    ///// Цвет линии
    //    ///// </summary>
    //    //public Color Color { get; set; }
    //}
}
