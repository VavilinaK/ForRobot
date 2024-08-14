using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace ForRobot.Model
{
    public class FileData
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public FileTypes Type { get; set; } = FileTypes.Folder;

        public FileData(string path)
        {
            this.Path = path;
            this.Name = this.Path.Split(new char[] { '\\' }).Last();
            switch (this.Name)
            {
                case string a when a.Contains(".dat"):
                    this.Type = FileTypes.DataList;
                    break;

                case string b when b.Contains(".src"):
                    this.Type = FileTypes.Program;
                    break;
            }
        }

        public ObservableCollection<FileData> Files { get; set; }
    }
}
