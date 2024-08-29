using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ForRobot.Libr
{
    public class Settings
    {
        public bool AutoUpdate { get; set; } = true;

        public bool InformUser { get; set; } = true;

        /// <summary>
        /// Время ожидания ответа от сервера, сек.
        /// </summary>
        public double ConnectionTimeOut { get; set; } = 10;
    }
}
