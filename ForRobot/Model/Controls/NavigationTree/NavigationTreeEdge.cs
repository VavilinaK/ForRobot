using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace ForRobot.Model.Controls.NavigationTree
{
    public class NavigationTreeEdge
    {
        /// <summary>
        /// Родительская папка
        /// </summary>
        public File ParentFolder { get; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="parentFolder">Родительская папка</param>
        public NavigationTreeEdge(File parentFolder)
        {
            this.ParentFolder = parentFolder;
        }
    }
}
