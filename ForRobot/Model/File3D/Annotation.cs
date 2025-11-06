using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;

using HelixToolkit.Wpf;

namespace ForRobot.Model.File3D
{
    public static class Vector3DExtensions
    {
        public static Vector3D Normalized(this Vector3D vector)
        {
            double length = vector.Length;
            return length < 1e-6 ? new Vector3D(0, 0, 0) : new Vector3D(vector.X / length, vector.Y / length, vector.Z / length);
        }
    }
    
    public class Annotation : ScreenSpaceVisual3D
    {
        #region Private variables
        
        private readonly BillboardTextVisual3D _label;
        private readonly LinesVisual3D _lines;
        private readonly LinesVisual3D _arrows;

        private double _arrowSize = DEFAULT_ARROW_SIZE;
        private bool _isVisible = true;

        /// <summary>
        /// Направления стрелок и индексы точек
        /// </summary>
        private Dictionary<ArrowSide, (int start, int end)> _directions = new Dictionary<ArrowSide, (int start, int end)>
        {
            { ArrowSide.AB, (0, 1) },
            { ArrowSide.BC, (1, 2) },
            { ArrowSide.CD, (2, 3) },
            { ArrowSide.DA, (3, 0) }
        };
        private Point3DCollection _points;

        #endregion Private variables

        #region Public variables

        [Flags]
        /// <summary>
        /// Перечисление сторон, на которых может быть стрелка
        /// </summary>
        public enum ArrowSide
        {
            AB = 0,

            BC = 1,

            CD = 2,

            DA = 3,

            All = AB | BC | CD | DA
        }
                
        const double DEFAULT_ARROW_SIZE = 1.0;
        public static double DefaultAnnotationWidth = 5.0;

        public double ArrowSize
        {
            get => _arrowSize;
            set
            {
                this._arrowSize = value;
                this.UpdateGeometry();
            }
        }

        /// <summary>
        /// Сторона со стрелкой
        /// </summary>
        public ArrowSide ArrowsSide { get; set; } = ArrowSide.BC;

        //public double FontSize
        //{
        //    get => _label.FontSize;
        //    set
        //    {
        //        _label.FontSize = value;
        //        this.UpdateText();
        //    }
        //}
        //public double Thickness
        //{
        //    get => this._thickness;
        //    set
        //    {
        //        this._thickness = value;
        //        this._lines.Thickness = this._thickness;
        //        this._arrows.Thickness = this._thickness;
        //    }
        //}

        /// <summary>
        /// Наименование свойства
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// Текст параметра
        /// </summary>
        public string Text
        {
            get => this._label.Text;
            set
            {
                this._label.Text = value;
                this.UpdateText();
            }
        }

        /// <summary>
        /// Свойство видимости аннотации
        /// </summary>
        public bool IsVisible
        {
            get => this._isVisible;
            set
            {
                this._isVisible = value;
                this.UpdateVisibility();
            }
        }

        //public double Thickness
        //{
        //    get => this._thickness;
        //    set
        //    {
        //        this._thickness = value;
        //        this.UpdateGeometry();
        //    }
        //}

        /* Расположение точек
         * B<------->C
         *  |       |
         *  |_______|
         *  A       D
        */
        /// <summary>
        /// Точки вершин прямоугольника параметров
        /// </summary>
        public new Point3DCollection Points
        {
            get => this._points;
            set
            {
                this._points = value;
                this.UpdatePoints();
            }
        }

        #endregion Public variables

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points">Точки прямоугольника примечания</param>
        /// <param name="arrowSide">Сторона со стрелкой</param>
        public Annotation(Point3DCollection points, ArrowSide arrowSide = ArrowSide.BC)
        {
            this._label = new BillboardTextVisual3D()
            {
                Padding = new Thickness(4),
                Background = Brushes.Transparent,
                Foreground = new SolidColorBrush(Colors.AnnotationForegroundColor)
            };
            this._lines = new LinesVisual3D()
            {
                Color = ForRobot.Model.File3D.Colors.AnnotationArrowsColor,
                DepthOffset = 0.05
            };
            this._arrows = new LinesVisual3D()
            {
                Color = ForRobot.Model.File3D.Colors.AnnotationArrowsColor,
                DepthOffset = 0.05
            };

            points = new Point3DCollection(points.Take(4));
            this.Points = points;
            this.ArrowsSide = arrowSide;

            Children.Add(this._label);
            Children.Add(this._lines);
            Children.Add(this._arrows);

            this.UpdateGeometry();
        }

        ///// <summary>
        ///// <para>AAA</para>
        ///// <para>BBb</para>
        ///// </summary>
        ///// <param name="point3Ds">Точки прямоугольника примечания</param>
        ///// <param name="arrowSide">Сторона со стрелкой</param>
        //public Annotation(Point3DCollection points, ArrowSide arrowSide) : this(points)
        //{
        //    this.ArrowsSide = arrowSide;

        //    this.UpdateGeometry();
        //}

        #endregion

        #region Private functions

        private void UpdatePoints()
        {
            if (this.Points == null || this.Points.Count < 4) return;
            
            var linePoints = new Point3DCollection();
            for (int i = 0; i < 4; i++)
            {
                linePoints.Add(Points[i]);
                linePoints.Add(Points[(i + 1) % 4]);
            }
            _lines.Points = linePoints;

            // Добавление стрелок на выбранных сторонах
            List <Point3D> arrowPoints = new List<Point3D>();

            foreach (var side in this._directions.Where(x => x.Key == ArrowsSide))
            {
                var start = Points[side.Value.start];
                var end = Points[side.Value.end];
                var direction = (end - start).Normalized();
                var perpendicular = new Vector3D(-direction.Y, direction.X, 0).Normalized();

                // Стрелка в начале грани (начальная точка)
                arrowPoints.Add(start);
                arrowPoints.Add(start + direction * ArrowSize + perpendicular * ArrowSize * 0.5);

                arrowPoints.Add(start);
                arrowPoints.Add(start + direction * ArrowSize - perpendicular * ArrowSize * 0.5);

                // Стрелка в конце грани (конечная точка)
                arrowPoints.Add(end);
                arrowPoints.Add(end - direction * ArrowSize + perpendicular * ArrowSize * 0.5);

                arrowPoints.Add(end);
                arrowPoints.Add(end - direction * ArrowSize - perpendicular * ArrowSize * 0.5);
            }
            _arrows.Points = new Point3DCollection(arrowPoints);
        }

        private void UpdateText()
        {
            if (this._label == null || this.Points == null || this.Points.Count < 4) return;
            
            // Вычисляем середину между Points[0] и Points[1]
            var side = this._directions.Where(x => x.Key == ArrowsSide).First();
            var point0 = Points[side.Value.start];
            var point1 = Points[side.Value.end];
            var midPoint = new Point3D(
                (point0.X + point1.X) * 0.5,
                (point0.Y + point1.Y) * 0.5,
                (point0.Z + point1.Z) * 0.5
            );
            this._label.Position = midPoint - new Vector3D(0, 0, -ArrowSize * 2);
        }

        private void UpdateVisibility()
        {
            if (_isVisible)
            {
                if (!Children.Contains(_label))
                    Children.Add(_label);
                if (!Children.Contains(_lines))
                    Children.Add(_lines);
                if (!Children.Contains(_arrows))
                    Children.Add(_arrows);
            }
            else
            {
                Children.Remove(_label);
                Children.Remove(_lines);
                Children.Remove(_arrows);
            }
        }

        protected override bool UpdateTransforms() => true;

        protected override void UpdateGeometry()
        {
            this.UpdatePoints();
            this.UpdateText();

            //if (Points == null || Points.Count < 4) return;

            //_lines.Points.Clear();
            //_arrows.Points.Clear();

            //// Построение линий прямоугольника
            //for (int i = 0; i < 4; i++)
            //{
            //    _lines.Points.Add(Points[i]);
            //    _lines.Points.Add(Points[(i + 1) % 4]);
            //}

            //// Добавление стрелок на выбранных сторонах
            //var arrowPoints = new List<Point3D>();
            //var directions = new Dictionary<ArrowSide, (int start, int end)>
            //{
            //    { ArrowSide.AB, (0, 1) },
            //    { ArrowSide.BC, (1, 2) },
            //    { ArrowSide.CD, (2, 3) },
            //    { ArrowSide.DA, (3, 0) }
            //};

            //foreach (var side in directions.Where(x => ArrowsSide.HasFlag(x.Key)))
            //{
            //    //var start = Points[side.Value.start];
            //    //var end = Points[side.Value.end];
            //    //var direction = (end - start).Normalized();
            //    //var mid = start + (end - start) * 0.5;

            //    //// Стрелка в середине линии
            //    //arrowPoints.Add(mid - direction * DefaultHeadSize);
            //    //arrowPoints.Add(mid);
            //    //arrowPoints.Add(mid + direction * DefaultHeadSize);

            //    var start = Points[side.Value.start];
            //    var end = Points[side.Value.end];
            //    var direction = (end - start).Normalized();
            //    var perpendicular = new Vector3D(-direction.Y, direction.X, 0).Normalized();
            //    var mid = start + (end - start) * 0.5;

            //    // Треугольный наконечник
            //    arrowPoints.Add(mid - direction * DefaultHeadSize + perpendicular * DefaultHeadSize * 0.5);
            //    arrowPoints.Add(mid);
            //    arrowPoints.Add(mid - direction * DefaultHeadSize - perpendicular * DefaultHeadSize * 0.5);
            //}

            //_arrows.Points = new Point3DCollection(arrowPoints);

            //// Позиционирование текста
            ////var center = new Point3D(Points.Average(p => p.X),
            ////                         Points.Average(p => p.Y),
            ////                         Points.Average(p => p.Z));

            ////if (_label == null) return;
            //////_label.Position = IsHorizontal ? center + new Vector3D(0, DefaultHeadSize * 2, 0) : center + new Vector3D(DefaultHeadSize * 2, 0, 0);
            ////_label.Position = center + new Vector3D(DefaultHeadSize * 2, 0, 0);

            //// Позиционирование текста
            //var mainLineDirection = (Points[1] - Points[0]).Normalized();
            //var textOffset = IsHorizontal
            //    ? new Vector3D(0, DefaultHeadSize * 2, 0)
            //    : new Vector3D(DefaultHeadSize * 2, 0, 0);

            //if (_label == null) return;
            //// Вычисляем середину между Points[0] и Points[1]
            //var point0 = Points[0];
            //var point1 = Points[1];
            //var midPoint = new Point3D(
            //    (point0.X + point1.X) * 0.5,
            //    (point0.Y + point1.Y) * 0.5,
            //    (point0.Z + point1.Z) * 0.5
            //);
            //_label.Position = midPoint + textOffset;

            ////if (this.IsHorizontal)
            ////    _label.Transform = new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), 90));

            //_label.Transform = new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, 1), -90));
        }

        #endregion Private functions

        #region Public functions

        public void GetFontSize(double fontSize) => this._label.FontSize = fontSize;
        public void GetFontFamily(FontFamily fontFamily) => this._label.FontFamily = fontFamily;
        public void GetFontWeight(FontWeight fontWeight) => this._label.FontWeight = fontWeight;
        public void GetForeground(Brush brush) => this._label.Foreground = brush;
        public void GetLabelBackground(Brush brush) => this._label.Background = brush;
        public void GetThickness(double thickness)
        {
            this._lines.Thickness = thickness;
            this._arrows.Thickness = thickness;
        }

        #endregion Public functions
    }
}
