using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;

using HelixToolkit.Wpf;

namespace ForRobot.Models.File3D
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

        private Color _color = Colors.Black;
        private Color _selectedColor = Color.FromRgb(255, 218, 33);
        private double _arrowSize = DEFAULT_ARROW_SIZE;
        private double _thickness = 2.0;
        private bool _isVisible = true;
        private bool _isSelect = false;

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

        /// <summary>
        /// Сторона со стрелкой
        /// </summary>
        public ArrowSide ArrowsSide { get; set; } = ArrowSide.BC;

        public double ArrowSize
        {
            get => _arrowSize;
            set
            {
                this._arrowSize = value;
                this.UpdateGeometry();
            }
        }
        public double FontSize {  get => this._label.FontSize; set => this._label.FontSize = value;  }
        public double Thickness
        {
            get => this._thickness;
            set
            {
                this._thickness = value;
                this._lines.Thickness = this._thickness;
                this._arrows.Thickness = this._thickness;
            }
        }
        
        public FontFamily FontFamily { get => this._label.FontFamily; set => this._label.FontFamily = value; }
        public FontWeight FontWeight { get => this._label.FontWeight; set => this._label.FontWeight = value; }
        public Brush Foreground { get => this._label.Foreground; set => this._label.Foreground = value; }
        public Brush Background { get => this._label.Background; set => this._label.Background = value; }
        public new Color Color
        {
            get => this._color;
            set
            {
                this._color = value;
                this.UpdateColor();
            }
        }
        public Color SelectedColor
        {
            get => this._selectedColor;
            set
            {
                this._selectedColor = value;
                this.UpdateColor();
            }
        }
        
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
        public bool IsSelect
        {
            get => this._isSelect;
            set
            {
                this._isSelect = value;
                this.UpdateColor();
            }
        }

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
            };
            this._lines = new LinesVisual3D()
            {
                Thickness = this._thickness,
                DepthOffset = 0.05
            };
            this._arrows = new LinesVisual3D()
            {
                Thickness = this._thickness,
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

        private void UpdateColor()
        {
            if (_lines == null || _arrows == null) return;

            if (this.IsSelect)
            {
                this._lines.Color = this.SelectedColor;
                this._arrows.Color = this.SelectedColor;
            }
            else{
                this._lines.Color = this.Color;
                this._arrows.Color = this.Color;
            }
        }

        protected override bool UpdateTransforms() => true;

        protected override void UpdateGeometry()
        {
            this.UpdatePoints();
            this.UpdateText();
        }

        #endregion Private functions

        #region Public functions
        
        
        #endregion Public functions
    }
}
