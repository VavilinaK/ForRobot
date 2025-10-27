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

        //private static double _defaultFontSize = 20.0;
        //private static double _defaultThickness = 2.0;
        //private static double _defaultHeadSize = 1.0;
        //private static double _defaultAnnotationWidth = 5.0;
        //private static Brush _defaultTextForeground = Brushes.Black;
        //private static Brush _defaultTextBackground = Brushes.Transparent;
        //private Brush _defaultArrowColor = Brushes.Blue;

        private Point3DCollection _points;

        //private readonly BillboardTextVisual3D _label = new BillboardTextVisual3D
        //{
        //    FontSize = _defaultFontSize,
        //    Foreground = _defaultTextForeground,
        //    Background = _defaultTextForeground,
        //    VerticalAlignment = VerticalAlignment.Center,
        //    HorizontalAlignment = HorizontalAlignment.Center
        //};
        private readonly LinesVisual3D _lines;
        private readonly LinesVisual3D _arrows;

        //private readonly BillboardVisual3D _lines;
        //private readonly BillboardVisual3D _arrows;

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

        public BillboardTextVisual3D Label { get; } = new BillboardTextVisual3D();
        
        public static double DefaultHeadSize = 1.0;
        public static double DefaultAnnotationWidth = 5.0;

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

        public string PropertyName { get; set; }
        public string Text
        {
            get => this.Label.Text;
            set
            {
                this.Label.Text = value;
                this.UpdateText();
            }
        }

        /// <summary>
        /// Горизонтальное ли примечание
        /// </summary>
        public bool IsHorizontal { get; set; }

        /* Расположение точек
         * B<------->C
         *  |       |
         *  |_______|
         *  A       D
        */
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

        public Annotation(Point3DCollection points)
        {
            this._lines = new LinesVisual3D()
            {
                Color = ForRobot.Model.File3D.Colors.AnnotationArrowsColor
            };

            this._arrows = new LinesVisual3D()
            {
                Color = ForRobot.Model.File3D.Colors.AnnotationArrowsColor
            };

            points = new Point3DCollection(points.Take(4));
            this.Points = points;

            Children.Add(_lines);
            Children.Add(_arrows);
            Children.Add(this.Label);
        }

        /// <summary>
        /// <para>AAA</para>
        /// <para>BBb</para>
        /// </summary>
        /// <param name="point3Ds">Точки прямоугольника примечания</param>
        /// <param name="arrowSide">Сторона со стрелкой</param>
        /// <param name="horisontalAnnotation">Горизонтальное ли примечание</param>
        public Annotation(Point3DCollection points, ArrowSide arrowSide, bool horizontalAnnotation = true) : this(points)
        {
            this.ArrowsSide = arrowSide;

            this.UpdateGeometry();
            //this.IsHorizontal = horizontalAnnotation;            

            //_lines.Thickness = Thickness;
            //_lines.Color = (DefaultArrowColor as SolidColorBrush)?.Color ?? Colors.AnnotationArrowsColor;

            //_arrows.Thickness = Thickness;
            //_arrows.Color = _lines.Color;

            //_label = new TextVisual3D
            //{
            //    FontSize = FontSize,
            //    Foreground = DefaultTextForeground,
            //    VerticalAlignment = VerticalAlignment.Center,
            //    HorizontalAlignment = HorizontalAlignment.Center
            //};

            //Children.Add(_lines);
            //Children.Add(_arrows);
            //Children.Add(_label);
        }

        #endregion

        #region Private functions

        private void UpdatePoints()
        {
            if (this.Points == null || this.Points.Count < 4) return;

            _lines.Points.Clear();
            _arrows.Points.Clear();

            // Построение линий прямоугольника
            for (int i = 0; i < 4; i++)
            {
                _lines.Points.Add(Points[i]);
                _lines.Points.Add(Points[(i + 1) % 4]);
            }

            // Добавление стрелок на выбранных сторонах
            List<Point3D> arrowPoints = new List<Point3D>();
            var directions = new Dictionary<ArrowSide, (int start, int end)> // Направления стрелок и индексы точек
            {
                { ArrowSide.AB, (0, 1) },
                { ArrowSide.BC, (1, 2) },
                { ArrowSide.CD, (2, 3) },
                { ArrowSide.DA, (3, 0) }
            };

            foreach (var side in directions.Where(x => x.Key == ArrowsSide))
            {
                //var start = Points[side.Value.start];
                //var end = Points[side.Value.end];
                //var direction = (end - start).Normalized();
                //var mid = start + (end - start) * 0.5;

                ////Стрелка в середине линии
                //arrowPoints.Add(mid - direction * DefaultHeadSize);
                //arrowPoints.Add(mid);
                //arrowPoints.Add(mid + direction * DefaultHeadSize);

                //var start = Points[side.Value.start];
                //var end = Points[side.Value.end];
                //var direction = (end - start).Normalized();
                //var perpendicular = new Vector3D(-direction.Y, direction.X, 0).Normalized();
                //var mid = start + (end - start) * 0.5;

                //// Треугольный наконечник
                //arrowPoints.Add(mid - direction * DefaultHeadSize + perpendicular * DefaultHeadSize * 0.5);
                //arrowPoints.Add(mid);
                //arrowPoints.Add(mid - direction * DefaultHeadSize - perpendicular * DefaultHeadSize * 0.5);

                //var start = Points[side.Value.start];
                //var end = Points[side.Value.end];
                //var direction = (end - start).Normalized();
                //var perpendicular = new Vector3D(-direction.Y, direction.X, 0).Normalized();
                //var mid = start + (end - start) * 0.5;

                //// Стрелки по обе стороны линии
                //arrowPoints.Add(mid - direction * DefaultHeadSize + perpendicular * DefaultHeadSize * 0.5);
                //arrowPoints.Add(mid);
                //arrowPoints.Add(mid - direction * DefaultHeadSize - perpendicular * DefaultHeadSize * 0.5);

                //// Вторая стрелка с противоположной стороны
                //arrowPoints.Add(mid + direction * DefaultHeadSize + perpendicular * DefaultHeadSize * 0.5);
                //arrowPoints.Add(mid);
                //arrowPoints.Add(mid + direction * DefaultHeadSize - perpendicular * DefaultHeadSize * 0.5);

                var start = Points[side.Value.start];
                var end = Points[side.Value.end];
                var direction = (end - start).Normalized();
                var perpendicular = new Vector3D(-direction.Y, direction.X, 0).Normalized();

                // Стрелка в начале грани (начальная точка)
                arrowPoints.Add(start);
                arrowPoints.Add(start + direction * DefaultHeadSize + perpendicular * DefaultHeadSize * 0.5);

                arrowPoints.Add(start);
                arrowPoints.Add(start + direction * DefaultHeadSize - perpendicular * DefaultHeadSize * 0.5);

                // Стрелка в конце грани (конечная точка)
                arrowPoints.Add(end);
                arrowPoints.Add(end - direction * DefaultHeadSize + perpendicular * DefaultHeadSize * 0.5);

                arrowPoints.Add(end);
                arrowPoints.Add(end - direction * DefaultHeadSize - perpendicular * DefaultHeadSize * 0.5);
            }
            _arrows.Points = new Point3DCollection(arrowPoints);
        }

        //public List<TextVisual3D> Labels { get; set; } = new List<TextVisual3D>();

        private void UpdateText()
        {
            //// Позиционирование текста
            ////var mainLineDirection = (Points[1] - Points[0]).Normalized();
            //Vector3D textOffset = IsHorizontal ? new Vector3D(0, DefaultHeadSize * 2, 0) : new Vector3D(DefaultHeadSize * 2, 0, 0);

            if (this.Label == null || this.Points == null || this.Points.Count < 4) return;

            var directions = new Dictionary<ArrowSide, (int start, int end)> // Направления стрелок и индексы точек
            {
                { ArrowSide.AB, (0, 1) },
                { ArrowSide.BC, (1, 2) },
                { ArrowSide.CD, (2, 3) },
                { ArrowSide.DA, (3, 0) }
            };

            // Вычисляем середину между Points[0] и Points[1]
            var side = directions.Where(x => x.Key == ArrowsSide).First();
            var point0 = Points[side.Value.start];
            var point1 = Points[side.Value.end];
            var midPoint = new Point3D(
                (point0.X + point1.X) * 0.5,
                (point0.Y + point1.Y) * 0.5,
                (point0.Z + point1.Z) * 0.5
            );
            this.Label.Position = midPoint - new Vector3D(0, 0, DefaultHeadSize * 2);
            //_label.FontSize = FontSize;
            //_label.Position = midPoint + textOffset;
            //_label.Transform = new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, 1), -90));

            //Labels = new List<TextVisual3D>()
            //{
            //    new TextVisual3D() { Position = Points[0], Text = "A" },
            //    new TextVisual3D() { Position = Points[1], Text = "B" },
            //    new TextVisual3D() { Position = Points[2], Text = "C" },
            //    new TextVisual3D() { Position = Points[3], Text = "D" }
            //};

            //foreach (var item in Labels)
            //    Application.Current.Dispatcher.Invoke(() => { Children.Add(item); });
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

        #endregion
    }

    //public class Annotation : ScreenSpaceVisual3D
    //{
    //    //private readonly MeshVisual3D _arrowLine;
    //    //private readonly MeshVisual3D _arrowHead;
    //    private readonly TextVisual3D _label;
    //    //private readonly LinesVisual3D _arrowLine = new LinesVisual3D();
    //    private readonly LinesVisual3D _arrowHead = new LinesVisual3D();
    //    private readonly LinesVisual3D _mainLine = new LinesVisual3D();
    //    private readonly LinesVisual3D _startConnector = new LinesVisual3D();
    //    private readonly LinesVisual3D _endConnector = new LinesVisual3D();

    //    public static double ConnectorOffset { get; set; } = 20.0; // Смещение соединительных линий

    //    public static double DefaultFontSize { get; set; } = 32;
    //    public static double DefaultThickness { get; set; } = 2.0;
    //    public static double DefaultHeadSize { get; set; } = 10.0;
    //    public static Brush DefaultTextForeground { get; set; } = Brushes.Black;
    //    public static Brush DefaultTextBackground { get; set; } = Brushes.Transparent;
    //    public static Brush DefaultArrowColor { get; set; } = Brushes.Blue;
    //    //public static Material DefaultArrowMaterial { get; set; } = new DiffuseMaterial(Brushes.Red);

    //    public string Text
    //    {
    //        get => _label.Text;
    //        set => _label.Text = value;
    //    }

    //    private Point3D _startPoint;
    //    public Point3D StartPoint { get => _startPoint; set { _startPoint = value; UpdateGeometry(); } }

    //    private Point3D _endPoint;
    //    public Point3D EndPoint { get => _endPoint; set { _endPoint = value; UpdateGeometry(); } }

    //    public string PropertyName { get; set; }

    //    public Annotation() : this(new Point3D(0, 0, 0), new Point3D(0, 0, 0)) { }

    //    public Annotation(Point3D startPoint, Point3D endPoint)
    //    {
    //        //Настройка текста
    //        _label = new TextVisual3D
    //        {
    //            FontSize = DefaultFontSize,
    //            Foreground = DefaultTextForeground,
    //            Background = DefaultTextBackground,
    //            IsDoubleSided = true,
    //            Padding = new Thickness(4)
    //        };

    //        //Настройка стрелки
    //        //_arrowLine.Thickness = DefaultThickness;
    //        //_arrowLine.Color = (DefaultArrowColor as SolidColorBrush).Color;
    //        _arrowHead.Thickness = DefaultThickness;
    //        _arrowHead.Color = (DefaultArrowColor as SolidColorBrush).Color;

    //        //Children.Add(_arrowLine);
    //        //Children.Add(_mainLine);
    //        //Children.Add(_startConnector);
    //        //Children.Add(_endConnector);
    //        //Children.Add(_arrowHead);
    //        //Children.Add(_label);

    //        StartPoint = startPoint;
    //        EndPoint = endPoint;

    //        //this.UpdateGeometry();
    //    }

    //    ///// <summary>
    //    ///// Преобразование экранных координат
    //    ///// </summary>
    //    ///// <param name="screenPoint"></param>
    //    ///// <returns></returns>
    //    //private Point3D To3DPoint(Point screenPoint)
    //    //{
    //    //    return new HelixViewport3D().Viewport.UnProject(new Point3D(
    //    //        screenPoint.X,
    //    //        screenPoint.Y,
    //    //        0.9 // Глубина для 2D элементов
    //    //    ));
    //    //}

    //    //protected  void UpdateGeometry(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((Annotation)d).UpdateGeometry();

    //    protected override bool UpdateTransforms()
    //    {
    //        //throw new NotImplementedException();
    //        return true;
    //    }

    //    protected override void UpdateGeometry()
    //    {
    //        Children.Clear();

    //        // Основная линия между точками
    //        _mainLine.Points = new Point3DCollection { StartPoint, EndPoint };
    //        _mainLine.Thickness = DefaultThickness;
    //        _mainLine.Color = (DefaultArrowColor as SolidColorBrush).Color;
    //        Children.Add(_mainLine);

    //        // Соединительные линии к объекту
    //        var startDir = (StartPoint - EndPoint).Normalized() * ConnectorOffset;
    //        var endDir = (EndPoint - StartPoint).Normalized() * ConnectorOffset;

    //        _startConnector.Points = new Point3DCollection { StartPoint, StartPoint + startDir };
    //        _startConnector.Thickness = DefaultThickness;
    //        _startConnector.Color = (DefaultArrowColor as SolidColorBrush).Color;
    //        Children.Add(_startConnector);

    //        _endConnector.Points = new Point3DCollection { EndPoint, EndPoint + endDir };
    //        _endConnector.Thickness = DefaultThickness;
    //        _endConnector.Color = (DefaultArrowColor as SolidColorBrush).Color;
    //        Children.Add(_endConnector);

    //        // Двойной наконечник
    //        var direction = (EndPoint - StartPoint).Normalized();
    //        var perpendicular = new Vector3D(-direction.Y, direction.X, 0).Normalized();

    //        // Наконечник у стартовой точки
    //        var startHead = StartPoint + direction * ConnectorOffset;
    //        _arrowHead.Points.Add(startHead + perpendicular * DefaultHeadSize);
    //        _arrowHead.Points.Add(startHead);
    //        _arrowHead.Points.Add(startHead - perpendicular * DefaultHeadSize);

    //        // Наконечник у конечной точки
    //        var endHead = EndPoint - direction * ConnectorOffset;
    //        _arrowHead.Points.Add(endHead + perpendicular * DefaultHeadSize);
    //        _arrowHead.Points.Add(endHead);
    //        _arrowHead.Points.Add(endHead - perpendicular * DefaultHeadSize);

    //        _arrowHead.Thickness = DefaultThickness;
    //        _arrowHead.Color = (DefaultArrowColor as SolidColorBrush).Color;
    //        Children.Add(_arrowHead);

    //        if (StartPoint == new Point3D(0, 0, 0) && EndPoint == new Point3D(0, 0, 0))
    //            return;

    //        // Позиция текста
    //        _label.Position = this.StartPoint + (this.EndPoint - this.StartPoint) * 0.5;
    //        //_label.Position = (StartPoint + EndPoint) * 0.5 + perpendicular * DefaultHeadSize * 2;
    //        Children.Add(_label);


    //        //if (StartPoint.DistanceTo(EndPoint) < 1e-6) return;

    //        //// Основная линия стрелки
    //        //_arrowLine.Points = new Point3DCollection { StartPoint, EndPoint };

    //        //// Наконечник стрелки (треугольник)
    //        //var direction = (EndPoint - StartPoint).Normalized();
    //        //var perpendicular = new Vector3D(-direction.Y, direction.X, 0).Normalized();
    //        //var headTip = EndPoint;
    //        //var headBase = EndPoint - direction * DefaultHeadSize;

    //        //_arrowHead.Points = new Point3DCollection
    //        //{
    //        //    headBase + perpendicular * DefaultHeadSize * 0.5,
    //        //    headTip,
    //        //    headBase - perpendicular * DefaultHeadSize * 0.5
    //        //};

    //        //// Позиция текста (смещение от центра линии)
    //        //var textPosition = StartPoint + (EndPoint - StartPoint) * 0.5 + perpendicular * DefaultHeadSize;
    //        //_label.Position = textPosition;



    //        //// Построение линии стрелки
    //        //_arrowLine.Points = new Point3DCollection(new List<Point3D>() { StartPoint, EndPoint });

    //        //// Построение наконечника
    //        //var direction = Vector3DExtensions.Normalized(EndPoint - StartPoint);
    //        //var headBase = EndPoint - direction * DefaultHeadSize;
    //        //_arrowHead.Points = new Point3DCollection(new List<Point3D>() { headBase, EndPoint, headBase + direction * DefaultHeadSize * 0.3 });

    //        //if (StartPoint == new Point3D(0, 0, 0) && EndPoint == new Point3D(0, 0, 0))
    //        //    return;

    //        //// Позиция текста (середина стрелки)
    //        //_label.Position = StartPoint + (EndPoint - StartPoint) * 0.5;



    //        // Построение линии
    //        //var lineMesh = new MeshBuilder();
    //        //lineMesh.AddPipe(this.StartPoint, this.EndPoint, DefaultThickness, 4, 0);
    //        //_arrowLine.Mesh = lineMesh.ToMesh();

    //        // Построение наконечника
    //        //var direction = (this.EndPoint - this.StartPoint).Normalized();
    //        //var headBase = this.EndPoint - direction * DefaultHeadSize;

    //        //var headMesh = new MeshBuilder();
    //        //headMesh.AddArrow(headBase, end3D, DefaultHeadSize * 2, 0);
    //        //_arrowHead.Mesh = headMesh.ToMesh();

    //        // Позиция текста
    //        //_label.Position = this.StartPoint + (this.EndPoint - this.StartPoint) * 0.5;
    //    }
    //}

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
