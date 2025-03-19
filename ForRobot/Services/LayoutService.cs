using System;
using System.IO;

using AvalonDock;
using AvalonDock.Layout.Serialization;

namespace ForRobot.Services
{
    public class LayoutService
    {
        private readonly DockingManager _dockingManager;

        public LayoutService(DockingManager dockingManager)
        {
            _dockingManager = dockingManager;
        }

        public void SaveLayout(string filePath)
        {
            var serializer = new XmlLayoutSerializer(_dockingManager);
            using (var writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer);
            }
        }

        public void LoadLayout(string filePath)
        {
            if (File.Exists(filePath))
            {
                var serializer = new XmlLayoutSerializer(_dockingManager);
                using (var reader = new StreamReader(filePath))
                {
                    serializer.Deserialize(reader);
                }
            }
        }
    }
}
