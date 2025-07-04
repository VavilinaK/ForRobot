using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using ForRobot.Model.File3D;

using HelixToolkit.Wpf;

namespace ForRobot.Model.File3D
{
    public class Weld : ScreenSpaceVisual3D
    {
        #region Private variables

        private readonly LinesVisual3D _line;
        private Point3D _startPoint;
        private Point3D _endPoint;

        #endregion Private variables

        #region Public variables

        public string Name { get => this._line.GetName(); set => this._line.SetName(value); }

        /// <summary>
        /// Начало шва
        /// </summary>
        public Point3D StartPoint
        {
            get => this._startPoint;
            set
            {
                this._startPoint = value;
                this.UpdateGeometry();
            }
        }

        /// <summary>
        /// Конец шва
        /// </summary>
        public Point3D EndPoint
        {
            get => this._endPoint;
            set
            {
                this._endPoint = value;
                this.UpdateGeometry();
            }
        }

        /// <summary>
        /// Толщина линии шва
        /// </summary>
        public double Thickness { get; set; } = 5;

        #endregion Public variables

        #region Constructor

        public Weld()
        {
            this._line = new LinesVisual3D()
            {
                Color = ForRobot.Model.File3D.Colors.WeldColor,
                Thickness = this.Thickness
            };

            Children.Add(this._line);
        }

        #endregion

        protected override bool UpdateTransforms() => true;

        protected override void UpdateGeometry()
        {
            if (this._line == null) return;

            this._line.Points = new Point3DCollection { this.StartPoint, this.EndPoint };
        }
    }
}
