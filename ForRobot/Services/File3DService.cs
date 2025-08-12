using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using ForRobot.Model.File3D;

namespace ForRobot.Services
{
    public static class File3DService
    {
        public static void SaveFiles(IEnumerable<File3D> files)
        {
            foreach (File3D file in files) file.Save();
        }

        public static void Change3DModel(File3D file)
        {

        }

        //public static IEnumerable<File3D> LoadFiles(string[] pathes)
        //{

        //}
    }
}
