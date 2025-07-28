using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ForRobot.Model.File3D;

namespace ForRobot.Services
{
    public static class FileService
    {
        public static void SaveFiles(IEnumerable<File3D> files)
        {
            foreach (File3D file in files) file.Save();
        }

        //public static IEnumerable<File3D> LoadFiles(string[] pathes)
        //{

        //}
    }
}
