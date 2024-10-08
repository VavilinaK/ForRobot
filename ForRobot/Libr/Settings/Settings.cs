using System;
using System.IO;
using System.Diagnostics;

using Newtonsoft.Json;

namespace ForRobot.Libr.Settings
{
    public class Settings
    {
        #region Private variables

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented
        };

        #endregion

        #region Public variables
        
        /// <summary>
        /// Прилежение обновляется автоматически
        /// </summary>
        public bool AutoUpdate { get; set; } = true;

        /// <summary>
        /// Спрашивать пользователя об обновлении
        /// </summary>
        public bool InformUser { get; set; } = true;

        /// <summary>
        /// Ограничено ли время ожидания ответа от сервера
        /// </summary>
        public bool LimitedConnectionTimeOut { get; set; } = false;

        /// <summary>
        /// Время ожидания ответа от сервера, сек.
        /// </summary>
        public double ConnectionTimeOut { get; set; } = 3;

        /// <summary>
        /// Режим закрытия приложения. Спрашивает пользователя о закрытии и/или разрыве соединения
        /// </summary>
        public ModeClosingApp ModeClosingApp { get; set; } = ModeClosingApp.HaveConnected;

        #endregion

        #region Constructor

        public Settings() { }

        #endregion

        #region Public functions

        /// <summary>
        /// Сохранение настроек в json-файл во временных файлах
        /// </summary>
        public void Save()
        {
            string sPathForSave = Path.Combine(Path.GetTempPath(), "interfaceOfRobot_settings.json");
            File.WriteAllText(sPathForSave, JsonConvert.SerializeObject(this, _jsonSettings));
        }

        /// <summary>
        /// Инициализация настроек (при первой загрузки) или выгрузка из временных файлов
        /// </summary>
        /// <returns></returns>
        public static Settings GetSettings()
        {
            if (File.Exists(Path.Combine(Path.GetTempPath(), "interfaceOfRobot_settings.json")))
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Combine(Path.GetTempPath(), "interfaceOfRobot_settings.json")), _jsonSettings);
            else
                return new Settings();
        }

        #endregion
    }
}
