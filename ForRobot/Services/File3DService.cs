using System;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;

using ForRobot.Model.Detals;
using ForRobot.Model.File3D;

namespace ForRobot.Services
{
    public static class File3DService
    {
        public static event PropertyChangedEventHandler PropertyChanged;

        private static void NotifyStaticPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(name));
        }

        public static void SaveFiles(params File3D[] files)
        {
            foreach (File3D file in files) file.Save();
        }

        //public static void Change3DModel(File3D file)
        //{
        //    //Detal detal = file.Detal;
        //    //detal.D
        //}

        //public static IEnumerable<File3D> LoadFiles(string[] pathes)
        //{

        //}
    }
}
