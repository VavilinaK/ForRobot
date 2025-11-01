using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;

using HelixToolkit.Wpf;

using ForRobot.Model.File3D;

namespace ForRobot.Libr.Collections
{
    public static partial class Model3DCollection
    {
        public const string RobotModelPath = "pack://application:,,,/InterfaceOfRobots;component/3DModels/kukaRobot.stl";
        public const string PCModelPath = "pack://application:,,,/InterfaceOfRobots;component/3DModels/computer_monitor.stl";
        public const string ManModelPath = "pack://application:,,,/InterfaceOfRobots;component/3DModels/stickman.stl";

        /// <summary>
        /// Выгрузка модели из компонентов сборки
        /// </summary>
        /// <param name="modelPath"></param>
        /// <returns></returns>
        public static Model3DGroup LoadModel(string modelPath)
        {
            Model3DGroup robotModel;
            try
            {
                robotModel = new ModelImporter().Load(modelPath);
                if (robotModel == null)
                {
                    throw new System.IO.FileNotFoundException($"Не удалось загрузить модель робота по пути: {modelPath}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка загрузки модели робота: {ex.Message}", ex);
            }
            return robotModel;
        }

        /// <summary>
        /// Добавление модели робота
        /// </summary>
        /// <param name="source"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="scaleFactor">Маштабный коэффициент</param>
        public static void AddRobot(this ICollection<Model3D> source, double x, double y, double z, double scaleFactor = 10)
        {
            Vector3D robotTranslate = new Vector3D(x, y, z);

            Model3DGroup robotModel = LoadModel(RobotModelPath);

            ApplyCustomColor(robotModel, Model.File3D.Colors.RobotColor);

            Transform3DGroup modelTransform = new Transform3DGroup();
            modelTransform.Children.Add(new ScaleTransform3D(scaleFactor, scaleFactor, scaleFactor));
            modelTransform.Children.Add(new TranslateTransform3D(robotTranslate));
            modelTransform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, 1), 90), new Point3D(robotTranslate.X, robotTranslate.Y, robotTranslate.Z)));
            robotModel.Transform = modelTransform;
            robotModel.SetName(string.Format("Robot {0}", source.Count(item => item.GetName().Contains("Robot")) + 1));
            source.Add(robotModel);
        }

        /// <summary>
        /// Добавление модели компьютера
        /// </summary>
        /// <param name="source"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="scaleFactor">Маштабный коэффициент</param>
        public static void AddPC(this ICollection<Model3D> source, double x, double y, double z, double scaleFactor = 10)
        {
            Vector3D pcTranslate = new Vector3D(x, y, z);

            Model3DGroup pcModel = LoadModel(PCModelPath);

            ApplyCustomColor(pcModel, Model.File3D.Colors.PcColor);

            Transform3DGroup modelTransform = new Transform3DGroup();
            modelTransform.Children.Add(new ScaleTransform3D(scaleFactor, scaleFactor, scaleFactor));
            modelTransform.Children.Add(new TranslateTransform3D(pcTranslate));
            modelTransform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), 90), new Point3D(pcTranslate.X, pcTranslate.Y, pcTranslate.Z)));
            modelTransform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, 1), 180), new Point3D(pcTranslate.X, pcTranslate.Y, pcTranslate.Z)));
            pcModel.Transform = modelTransform;
            pcModel.SetName(string.Format("PC {0}", source.Count(item => item.GetName().Contains("PC")) + 1));
            source.Add(pcModel);
        }

        /// <summary>
        /// Добавление модели человека
        /// </summary>
        /// <param name="source"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="scaleFactor">Маштабный коэффициент</param>
        /// <param name="modelTransform"></param>
        public static void AddMan(this ICollection<Model3D> source, double x, double y, double z, double scaleFactor = 10, Transform3DGroup modelTransform = null)
        {
            Vector3D manTranslate = new Vector3D(x, y, z);
            Model3DGroup manModel = LoadModel(ManModelPath);
            ApplyCustomColor(manModel, Model.File3D.Colors.WatcherColor);
            modelTransform?.Children.Add(new ScaleTransform3D(scaleFactor, scaleFactor, scaleFactor));
            modelTransform?.Children.Add(new TranslateTransform3D(manTranslate));
            manModel.Transform = modelTransform;
            manModel.SetName(string.Format("Man {0}", source.Count(item => item.GetName().Contains("Man")) + 1));
            source.Add(manModel);
        }

        private static void ApplyCustomColor(Model3DGroup modelGroup, Color color)
        {
            foreach (var model in modelGroup.Children)
            {
                if (model is GeometryModel3D geometryModel)
                {
                    geometryModel.Material = new DiffuseMaterial(new SolidColorBrush(color));
                    geometryModel.BackMaterial = geometryModel.Material;
                }
                else if (model is Model3DGroup group)
                {
                    ApplyCustomColor(group, color);
                }
            }
        }
    }
}
