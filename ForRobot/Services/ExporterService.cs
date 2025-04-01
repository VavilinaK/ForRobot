using System;
using System.IO;
using System.Windows.Media.Media3D;

using HelixToolkit.Wpf;

namespace ForRobot.Services
{
    public interface IModelExporter
    {
        void Export(Model3DGroup model, string filePath);
    }

    public class ExporterService : IModelExporter
    {
        public void Export(Model3DGroup model, string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".obj":
                    throw new Exception($"Данный формат не поддерживается: {extension}");
                    //string materialsFilePath = Path.ChangeExtension(filePath, ".mtl");
                    //// Создаём пустой файл материалов
                    //File.WriteAllText(materialsFilePath, "# Empty materials file");
                    //var exporter = new HelixToolkit.Wpf.ObjExporter()
                    //{
                    //    MaterialsFile = materialsFilePath // Указываем файл материалов
                    //};
                    //using (var stream = File.Create(filePath))
                    //{
                    //    exporter.Export(model, stream);
                    //}
                    //break;

                default:
                    using (var stream = File.Create(filePath))
                    {
                        Exporters.Create(filePath)?.Export(model, stream);
                    }
                    break;
            }
        }
    }
}
