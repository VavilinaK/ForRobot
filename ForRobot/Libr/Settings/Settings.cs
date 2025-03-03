using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;

using HelixToolkit.Wpf;

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

        #region Generic

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

        #region Navigation

        public bool AccessDataFile { get; set; } = false;

        /// <summary>
        /// Доступность системных папок в дереве файлов
        /// </summary>
        public SortedDictionary<string, bool> AvailableFolders { get; set; } = new SortedDictionary<string, bool>()
                                                                                    {
                                                                                        { "System", false },
                                                                                        { "Mada", false },
                                                                                        { "TP", false },
                                                                                        { "STEU", false }
                                                                                    };

        #endregion

        #region View

        /// <summary>
        /// Отображение рёбер
        /// </summary>
        public bool VisibilityPictures { get; set; } = true;

        /// <summary>
        /// Доступность системных папок в дереве файлов
        /// </summary>
        public SortedDictionary<string, bool> AvailableTab { get; set; } = new SortedDictionary<string, bool>()
                                                                                    {
                                                                                        { "Роботы", true },
                                                                                        { "Управление", true },
                                                                                        { "Программа", true }
                                                                                    };

        #region 3DView

        /// <summary>
        /// Показ системы координат
        /// </summary>
        public bool ShowCoordinateSystem { get; set; } = true;
        /// <summary>
        /// Показ пространственного куба
        /// </summary>
        public bool ShowViewCube { get; set; } = true;
        /// <summary>
        /// Показ кол-ва полигонов
        /// </summary>
        public bool ShowTriangleCountInfo { get; set; } = false;

        public bool ShowFieldOfView { get; set; } = false;
        public bool ShowFrameRate { get; set; } = false;

        #endregion

        #region Camera

        /// <summary>
        /// Включена ли ортоганальная камера
        /// </summary>
        public bool Orthographic { get; set; } = false;
        /// <summary>
        /// Демонстрация информации о камере
        /// </summary>
        public bool ShowCameraInfo { get; set; } = false;
        /// <summary>
        /// Демонстрация курсора камеры
        /// </summary>
        public bool ShowCameraTarget { get; set; } = true;
        /// <summary>
        /// Поворот вокруг мыши
        /// </summary>
        public bool RotateAroundMouseDownPoint { get; set; } = false;
        /// <summary>
        /// Приближение около мыши
        /// </summary>
        public bool ZoomAroundMouseDownPoint { get; set; } = false;
        /// <summary>
        /// Инерция камеры
        /// </summary>
        public bool IsInertiaEnabled { get; set; } = false;
        /// <summary>
        /// Вкличено ли панорамирование
        /// </summary>
        public bool IsPanEnabled { get; set; } = true;
        /// <summary>
        /// Вкличено ли перемещение
        /// </summary>
        public bool IsMoveEnabled { get; set; } = true;
        /// <summary>
        /// Вкличено ли вращение
        /// </summary>
        public bool IsRotationEnabled { get; set; } = true;
        /// <summary>
        /// Вкличено ли маштабирование
        /// </summary>
        public bool IsZoomEnabled { get; set; } = true;

        ///// <summary>
        ///// Включено ли панаромирование
        ///// </summary>
        //public bool IsChangeFieldOfViewEnabled { get; set; } = true;
        
        /// <summary>
        /// Режим поворота камеры
        /// <example>
        /// <para/>Turntable - is constrained to two axes of rotation (model up and right direction).
        /// <para/>Turnball - using three axes (look direction, right direction and up direction (on the left/right edges)).
        /// <para/>Trackball - using a virtual trackball.
        /// </example>
        /// </summary>
        public CameraRotationMode CameraRotationMode { get; set; } = CameraRotationMode.Turntable;

        /// <summary>
        /// Режим камеры
        /// <example>
        /// <para/>Inspect - orbits around a point (fixed target position, move closer target when zooming).
        /// <para/>WalkAround - walk around (fixed camera position when rotating, move in camera direction when zooming).
        /// <para/>FixedPosition - fixed camera target, change field of view when zooming.
        /// </example>
        /// </summary>
        public CameraMode CameraMode { get; set; } = CameraMode.Inspect;

        /// <summary>
        /// Чувствительность вращения
        /// </summary>
        public double RotationSensitivity { get; set; } = 1;
        /// <summary>
        /// Чувствительность маштабирования
        /// </summary>
        public double ZoomSensitivity { get; set; } = 1;
        /// <summary>
        /// Степень инерции камеры
        /// </summary>
        public double CameraInertiaFactor { get; set; } = 0.930;

        #endregion

        #endregion

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
