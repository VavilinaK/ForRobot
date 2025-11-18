 using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Threading.Tasks;
using System.Collections.Generic;

using HelixToolkit.Wpf;

using ForRobot.Libr.Collections;
using ForRobot.Model.Detals;
using ForRobot.Strategies.ModelingStrategies;

namespace ForRobot.Libr.Services
{
    /// <summary>
    /// Класс сервис для сборки 3д сцены
    /// </summary>
    public sealed class ModelingService
    {
        private const int DEFAULT_DISTANCE = 7;
        /// <summary>
        /// Масштабный коэффициент
        /// </summary>
        private readonly double _scaleFactor;

        /// <summary>
        /// Минимальное кол-во рёбер
        /// </summary>
        public const double MIN_RIB_LENGTH = 1.0;
        /// <summary>
        /// Смещение скоса
        /// </summary>
        public const double SLOPE_OFF_SET = 10;
        /// <summary>
        /// Коэффициент сужения/расширения для трапеций (от 0.1 до 0.9)
        /// </summary>
        public const double TRAPEZOID_RATIO = 0.2;

        private readonly IEnumerable<IDetalModelingStrategy> _strategies;
            
        #region Contructor

        public ModelingService(IEnumerable<IDetalModelingStrategy> strategies, double scaleFactor)
        {
            this._strategies = strategies;
            this._scaleFactor = scaleFactor;
        }

        #endregion

        #region Public Methods

        public Model3DGroup Get3DScene(Detal detal)
        {
            Model3DGroup scene = new Model3DGroup();

            IDetalModelingStrategy strategy = _strategies.FirstOrDefault(s => s.CanHandle(DetalTypes.StringToEnum(detal.DetalType)));
            Model3DGroup detalModel3D = strategy?.CreateModel3D(detal) ?? null;

            double halfModelPlateLength = (double)detal.PlateLength * this._scaleFactor / 2;
            double halfModelPlateWidth = (double)detal.PlateWidth * this._scaleFactor / 2;
            double halfModelPlateHeight = (double)detal.PlateThickness * this._scaleFactor / 2;

            switch (detal.DetalType)
            {
                case DetalTypes.Plita:
                    Plita plate = detal as Plita;
                    detalModel3D.SetName("Plate");

                    double offsetDirection = (plate.ScoseType == ScoseTypes.SlopeLeft || plate.ScoseType == ScoseTypes.SlopeRight) ? SLOPE_OFF_SET : 0;
                    
                    scene.Children.AddRobot(halfModelPlateLength + offsetDirection, -halfModelPlateWidth - DEFAULT_DISTANCE, 0, this._scaleFactor * 10);
                    scene.Children.AddPC(-halfModelPlateLength - offsetDirection, -halfModelPlateWidth - DEFAULT_DISTANCE, 0, this._scaleFactor * 200);

                    Transform3DGroup modelTransformA = new Transform3DGroup();
                    modelTransformA.Children.Add(new ScaleTransform3D(this._scaleFactor, this._scaleFactor, this._scaleFactor));
                    modelTransformA.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), 90), new Point3D(halfModelPlateLength + 7, 0, 0)));
                    Transform3DGroup modelTransformB = modelTransformA.Clone();
                    modelTransformA.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, 1), -90)));

                    scene.Children.AddMan(halfModelPlateLength + DEFAULT_DISTANCE + offsetDirection, 0, 0, this._scaleFactor * 150, modelTransformA);
                    scene.Children.AddMan(0, halfModelPlateWidth + 15, 0, this._scaleFactor * 150, modelTransformB);
                    break;

                default:
                    throw new NotSupportedException($"Ошибка построения модели: тип детали {detal.DetalType} не поддерживается!");
            }

            scene.Children.Add(detalModel3D);

            return scene;
        }

        public async Task<Model3DGroup> Get3DSceneAsync(Detal detal) => await Task.Run(() => this.Get3DScene(detal));

        /// <summary>
        /// Поворот вершин вокруг оси X на заданный угол (в градусах)
        /// </summary>
        /// <param name="vertices">Точки вершин</param>
        /// <param name="angleDegrees">Угол поворота (в градусах)</param>
        public static void RotateVerticesAroundX(Point3D[] vertices, double angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180;
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);

            for (int i = 0; i < vertices.Length; i++)
            {
                double y = vertices[i].Y;
                double z = vertices[i].Z;

                // Матрица поворота вокруг оси X:
                // Y' = Y * cosθ - Z * sinθ
                // Z' = Y * sinθ + Z * cosθ
                double newY = y * cos - z * sin;
                double newZ = y * sin + z * cos;

                vertices[i] = new Point3D(
                    vertices[i].X, // X остаётся без изменений
                    newY,
                    newZ
                );
            }
        }

        /// <summary>
        /// Поворот вершин вокруг оси Y на заданный угол (в градусах)
        /// </summary>
        /// <param name="vertices">Точки вершин</param>
        /// <param name="angleDegrees">Угол поворота (в градусах)</param>
        public static void RotateVerticesAroundY(Point3D[] vertices, double angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180;
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);

            for (int i = 0; i < vertices.Length; i++)
            {
                double x = vertices[i].X;
                double z = vertices[i].Z;

                // Применение матрицы поворота вокруг оси Y:
                // X' = X * cosθ - Z * sinθ
                // Z' = X * sinθ + Z * cosθ
                double newX = x * cos - z * sin;
                double newZ = x * sin + z * cos;

                vertices[i] = new Point3D(newX, vertices[i].Y, newZ);
            }
        }

        /// <summary>
        /// Поворот вершин вокруг оси Z на заданный угол (в градусах)
        /// </summary>
        /// <param name="vertices">Точки вершин</param>
        /// <param name="angleDegrees">Угол поворота (в градусах)</param>
        public static void RotateVerticesAroundZ(Point3D[] vertices, double angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180;
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);

            for (int i = 0; i < vertices.Length; i++)
            {
                double x = vertices[i].X;
                double y = vertices[i].Y;

                // Матрица поворота вокруг оси Z:
                // X' = X * cosθ - Y * sinθ
                // Y' = X * sinθ + Y * cosθ
                double newX = x * cos - y * sin;
                double newY = x * sin + y * cos;

                vertices[i] = new Point3D(
                    newX,
                    newY,
                    vertices[i].Z // Z остаётся без изменений
                );
            }
        }

        #endregion Public Methods
    }
}
