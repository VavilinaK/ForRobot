using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForRobot.Libr.Behavior
{
    public class SelectLayoutDocumentPane
    {
        public ForRobot.Model.File3D.File3D SelectedFile { get; private set; }

        public SelectLayoutDocumentPane(ForRobot.Model.File3D.File3D file)
        {
            this.SelectedFile = file;
        }

    }
}
