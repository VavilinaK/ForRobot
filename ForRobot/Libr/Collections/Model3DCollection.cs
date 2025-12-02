using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;

using HelixToolkit.Wpf;

namespace ForRobot.Libr.Collections
{
    public static partial class Model3DCollection
    {
        public const string RobotModelPath = "pack://application:,,,/InterfaceOfRobots;component/3DModels/kukaRobot.stl";
        public const string PCModelPath = "pack://application:,,,/InterfaceOfRobots;component/3DModels/computer_monitor.stl";
        public const string ManModelPath = "pack://application:,,,/InterfaceOfRobots;component/3DModels/stickman.stl";

        private static bool HasTranslationApplied(this Transform3D transform)
        {
            switch (transform)
            {
                case Transform3DGroup transform3DGroup:
                    return transform3DGroup.Children.OfType<TranslateTransform3D>().Any();

                default:
                    return false;
            }
        }

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
        public static void AddRobot(this ICollection<Model3D> source, double x = 0, double y = 0, double z = 0, Transform3DGroup transform3DGroup = null)
        {
            Model3DGroup robotModel = LoadModel(RobotModelPath);
            ApplyCustomColor(robotModel, ForRobot.Themes.Colors.RobotColor);

            if (transform3DGroup == null)
                robotModel.Transform = Transform3DBuilder.Create().Translate(x, y, z);
            else if (!transform3DGroup.HasTranslationApplied())
                (robotModel.Transform as Transform3DGroup).Translate(x, y, z);
            else
                robotModel.Transform = transform3DGroup;

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
        public static void AddPC(this ICollection<Model3D> source, double x = 0, double y = 0, double z = 0, Transform3DGroup transform3DGroup = null)
        {
            Model3DGroup pcModel = LoadModel(PCModelPath);
            ApplyCustomColor(pcModel, ForRobot.Themes.Colors.PcColor);

            if (transform3DGroup == null)
                pcModel.Transform = Transform3DBuilder.Create().Translate(x, y, z);
            else if (!transform3DGroup.HasTranslationApplied())
                (pcModel.Transform as Transform3DGroup).Translate(x, y, z);
            else
                pcModel.Transform = transform3DGroup;

            pcModel.Transform = transform3DGroup;
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
        public static void AddMan(this ICollection<Model3D> source, double x = 0, double y = 0, double z = 0, Transform3DGroup transform3DGroup = null)
        {
            Vector3D manTranslate = new Vector3D(x, y, z);
            Model3DGroup manModel = LoadModel(ManModelPath);
            ApplyCustomColor(manModel, ForRobot.Themes.Colors.WatcherColor);

            if (transform3DGroup == null)
                manModel.Transform = Transform3DBuilder.Create().Translate(x, y, z);
            else if (!transform3DGroup.HasTranslationApplied())
                (manModel.Transform as Transform3DGroup).Translate(x, y, z);
            else
                manModel.Transform = transform3DGroup;

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
