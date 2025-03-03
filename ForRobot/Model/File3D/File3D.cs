using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

using HelixToolkit;
using HelixToolkit.Wpf;

namespace ForRobot.Model.File3D
{
    public class File3D
    {
        //MeshElement3D _object3D;

        #region Public variables

        public bool IsOpened { get; private set; } = false;

        public string Path { get; private set; }

        //Model3DGroup Group3D { get; set; }

        //public MeshElement3D Object3D
        //{
        //    get => this._object3D ?? (this._object3D = new CubeVisual3D()
        //                                                   {
        //                                                        SideLength = 1,
        //                                                        Center = new System.Windows.Media.Media3D.Point3D(0, 0, 0),
        //                                                        Fill = System.Windows.Media.Brushes.Gray
        //                                                   });
        //    set
        //    {
        //        this._object3D = value;
        //    }
        //}

        public Detals.Detal Detal { get; set; }

        #endregion

        #region Constructor

        public File3D()
        {

        }

        //public File3D(Detals.DetalType detalType)
        //{
        //    this.Detal;
        //}

        public File3D(string sPath)
        {
            this.Path = sPath;
            ModelImporter importer = new ModelImporter();
            //this.Group3D = importer.Load(Path);
            this.IsOpened = true;
        }

        #endregion

        public static File3D Open()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog() {
                Multiselect = false,
                AddExtension = true,
                Title = "Открытие файла",
                Filter = "3D model files (*.3ds;*.obj;*.off;*.lwo;*.stl;*.ply;)|*.3ds;*.obj;*.objz;*.off;*.lwo;*.stl;*.ply;"})
            {
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return null;

                return new File3D(openFileDialog.FileName);
            }
        }
    }
}
