using System;
using System.Windows.Media;

using Newtonsoft.Json;

namespace ForRobot.Model.Settings
{
    public class ApplicationInfo
    {
        [JsonConverter(typeof(ForRobot.Libr.Json.ImageSourceConverter))]
        public ImageSource Icon { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }
    }
}
