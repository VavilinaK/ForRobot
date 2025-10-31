using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using ForRobot.Model.File3D;

using HelixToolkit.Wpf;

namespace ForRobot.Model.File3D
{
    /// <summary>
    /// Модель представления шва
    /// </summary>
    public class Weld : ScreenSpaceVisual3D
    {
        #region Private variables
        
        private readonly LinesVisual3D _line1;
        private readonly LinesVisual3D _line2;
        private Point3D _startPoint;
        private Point3D _endPoint;
        private Point3D _centerPoint;
        private double _thickness = 2.0;
        private Color _color;
        private Color _leftLineColor;
        private Color _rightLineColor;
        private bool _isDivided = false;

        #endregion Private variables

        #region Public variables

        public string Name { get; set; }

        /// <summary>
        /// Точка начала шва
        /// </summary>
        public Point3D StartPoint
        {
            get => this._startPoint;
            set
            {
                this._startPoint = value;
                this.UpdateCenterPoint();
                this.UpdateGeometry();
            }
        }

        /// <summary>
        /// Точка конца шва
        /// </summary>
        public Point3D EndPoint
        {
            get => this._endPoint;
            set
            {
                this._endPoint = value;
                this.UpdateCenterPoint();
                this.UpdateGeometry();
            }
        }

        /// <summary>
        /// Центральная точка, где сходятся два сегмента
        /// </summary>
        public Point3D CenterPoint
        {
            get => this._centerPoint;
            set
            {
                this._centerPoint = value;
                this.UpdateGeometry();
            }
        }

        /// <summary>
        /// Толщина линии шва
        /// </summary>
        public double Thickness
        {
            get => this._thickness;
            set
            {
                this._thickness = value;
                if (this._line1 != null) this._line1.Thickness = this._thickness;
                if (this._line2 != null) this._line2.Thickness = this._thickness;
            }
        }

        /// <summary>
        /// Общий цвет шва
        /// </summary>
        public new Color Color
        {
            get => this._color;
            set
            {
                this._color = value;
                this.UpdateColor();
            }
        }
        /// <summary>
        /// Цвет левой полавины шва
        /// </summary>
        public Color LeftLineColor
        {
            get => this._leftLineColor;
            set
            {
                this._leftLineColor = value;
                this.UpdateColor();
            }
        }
        /// <summary>
        /// Цвет правой полавины шва
        /// </summary>
        public Color RightLineColor
        {
            get => this._rightLineColor;
            set
            {
                this._rightLineColor = value;
                this.UpdateColor();
            }
        }

        /// <summary>
        /// Разделён ли шов по цвету попалам
        /// </summary>
        public bool IsDivided
        {
            get => this._isDivided;
            set
            {
                this._isDivided = value;
                this.UpdateColor();
            }
        }

        #endregion Public variables

        #region Constructor

        public Weld()
        {
            this._line1 = new LinesVisual3D()
            {
                Thickness = this.Thickness
            };

            this._line2 = new LinesVisual3D()
            {
                Thickness = this.Thickness
            };

            Children.Add(this._line1);
            Children.Add(this._line2);
        }

        public Weld(Color color, Color? leftLineColor, Color? rightLineColor) : this()
        {
            this.Color = color;
            this.LeftLineColor = leftLineColor ?? this.Color;
            this.RightLineColor = rightLineColor ?? this.Color;
        }

        #endregion

        /// <summary>
        /// Обновение центральной точки как середины между StartPoint и EndPoint
        /// </summary>
        private void UpdateCenterPoint()
        {
            if (this.StartPoint != null && this.EndPoint != null)
            {
                this.CenterPoint = new Point3D(
                    (this.StartPoint.X + this.EndPoint.X) / 2,
                    (this.StartPoint.Y + this.EndPoint.Y) / 2,
                    (this.StartPoint.Z + this.EndPoint.Z) / 2
                );
            }
        }

        /// <summary>
        /// Обновление цвета двух сегментов шва
        /// </summary>
        private void UpdateColor()
        {
            if (this.IsDivided)
            {
                this._line1.Color = this.LeftLineColor;
                this._line2.Color = this.RightLineColor;
            }
            else
            {
                this._line1.Color = this.Color;
                this._line2.Color = this.Color;
            }
        }

        protected override bool UpdateTransforms() => true;

        protected override void UpdateGeometry()
        {
            if (this._line1 == null || this._line2 == null) return;
            
            this._line1.Points = new Point3DCollection { this.StartPoint, this.CenterPoint };
            
            this._line2.Points = new Point3DCollection { this.CenterPoint, this.EndPoint };
        }
    }
}
