using System;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;

using AvalonDock.Themes;

using Newtonsoft.Json;

using HelixToolkit.Wpf;

namespace ForRobot.Libr.Settings
{
    public class Settings
    {
        #region Private variables

        private const string _fileName = "interfaceOfRobot_settings.json";

        private static string _path = Path.Combine(Path.GetTempPath(), Settings._fileName);

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented
        };

        private Tuple<string, Theme> _selectedTheme;

        private ForRobot.Libr.ConfigurationProperties.AppConfigurationSection _appConfig = ConfigurationManager.GetSection("app") as ForRobot.Libr.ConfigurationProperties.AppConfigurationSection;
        private ForRobot.Libr.ConfigurationProperties.RobotConfigurationSection _robotConfig = ConfigurationManager.GetSection("robot") as ForRobot.Libr.ConfigurationProperties.RobotConfigurationSection;
        
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
        /// Вход в приложение интерфейса по пин-коду
        /// </summary>
        public bool LoginByPINCode { get; set; } = false;

        /// <summary>
        /// Ограничено ли время ожидания ответа от сервера
        /// </summary>
        public bool LimitedConnectionTimeOut { get; set; } = false;

        /// <summary>
        /// Создаётся ли при открытии файл детли. Не работает, если приложение открывает файл "с помощью"
        /// </summary>
        public bool CreatedDetalFile { get; set; } = true;

        /// <summary>
        /// Время ожидания ответа от сервера, сек.
        /// </summary>
        public double ConnectionTimeOut { get; set; } = 3;

        /// <summary>
        /// Тип детали, для которой создаётся стартовый файл
        /// </summary>
        public string StartedDetalType { get; set; } = Model.Detals.DetalTypes.Plita;

        ///// <summary>
        ///// Режим закрытия приложения. Спрашивает пользователя о закрытии и/или разрыве соединения
        ///// </summary>
        //public ModeClosingApp ModeClosingApp { get; set; } = ModeClosingApp.HaveConnected;

        #endregion Generic

        #region Navigation
            
        /// <summary>
        /// Отображаютя ли файлы с расширением .dat
        /// </summary>
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

        [JsonIgnore]
        public List<Tuple<string, Theme>> Themes { get; } = new List<Tuple<string, Theme>>()
        {
            new Tuple<string, Theme>(nameof(GenericTheme), new GenericTheme()),
            new Tuple<string, Theme>(nameof(AeroTheme),new AeroTheme()),
            new Tuple<string, Theme>(nameof(ExpressionDarkTheme),new ExpressionDarkTheme()),
            new Tuple<string, Theme>(nameof(ExpressionLightTheme),new ExpressionLightTheme()),
            new Tuple<string, Theme>(nameof(MetroTheme),new MetroTheme()),
            //new Tuple<string, Theme>(nameof(VS2010Theme),new VS2010Theme()),
            new Tuple<string, Theme>(nameof(Vs2013BlueTheme),new Vs2013BlueTheme()),
            new Tuple<string, Theme>(nameof(Vs2013DarkTheme),new Vs2013DarkTheme()),
            new Tuple<string, Theme>(nameof(Vs2013LightTheme),new Vs2013LightTheme())
        };        
        [JsonIgnore]
        public Tuple<string, Theme> SelectedTheme
        {
            get => this._selectedTheme;
            set
            {
                this._selectedTheme = value;
                Properties.Settings.Default.SelectedTheme = this._selectedTheme.Item1;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Отображение рёбер
        /// </summary>
        public bool VisibilityPictures { get; set; } = true; // Устареет в версии 3.0

        /// <summary>
        /// Доступность вкладок интерфейса
        /// </summary>
        public SortedDictionary<string, bool> AvailableTab { get; set; } = new SortedDictionary<string, bool>()
                                                                                    {
                                                                                        { "Роботы", true },
                                                                                        { "Управление", true },
                                                                                        { "Программа", true }
                                                                                    }; // Устареет в версии 3.0

        #region 3DView

        /// <summary>
        /// Масштабный коэффициент: 1 единица модели = <see cref="ScaleFactor"/> мм. реальных размеров
        /// </summary>
        public static decimal ScaleFactor { get; set; } = 1.00M / 100.00M;

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

        #endregion 3DView

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

        #endregion Camera

        #endregion View

        #region Generation

        /// <summary>
        /// При каждой генерации будет спрашивать имя генерируемого файла
        /// </summary>
        public bool AskNameFile { get; set; } = false;
        /// <summary>
        /// Отправляются ли сгенерированные файлы на робота/ов
        /// </summary>
        public bool SendingGeneratedFiles { get; set; } = true;

        /// <summary>
        /// Имя сгенерированной программы (настил с рёбрами)
        /// </summary>
        public string PlitaProgramName { get; set; }
        /// <summary>
        /// Имя сгенерированной программы (плита со стрингерами)
        /// </summary>
        public string PlitaStringerProgramName { get; set; }
        /// <summary>
        /// Имя сгенерированной программы (плита с треугольником)
        /// </summary>
        public string PlitaTreugolnikProgramName { get; set; }

        /// <summary>
        /// Имя скрапта-генератора (настил с рёбрами)
        /// </summary>
        public string PlitaScriptName { get; set; }
        /// <summary>
        /// Имя скрапта-генератора (плита со стрингерами)
        /// </summary>
        public string PlitaStringerScriptName { get; set; }
        /// <summary>
        /// Имя скрапта-генератора (плита с треугольником)
        /// </summary>
        public string PlitaTreugolnikScriptName { get; set; }

        /// <summary>
        /// Путь к папке для генерации
        /// </summary>
        public string PathFolderOfGeneration { get; set; }

        #endregion

        #region Robots

        /// <summary>
        /// Стандартный путь к папке на роботе
        /// </summary>
        public string ControlerFolder { get; set; }

        #endregion

        #endregion Public variables

        #region Constructor

        public Settings()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.SelectedTheme))
                this.SelectedTheme = this.Themes.First();
            else
                this.SelectedTheme = this.Themes.Where(t => t.Item1 == Properties.Settings.Default.SelectedTheme).First();

            this.PlitaProgramName = this._appConfig.PlitaProgramName;
            this.PlitaStringerProgramName = this._appConfig.PlitaStringerProgramName;
            this.PlitaTreugolnikProgramName = this._appConfig.PlitaTreugolnikProgramName;

            this.PlitaScriptName = this._appConfig.PlitaGenerator;
            this.PlitaStringerScriptName = this._appConfig.PlitaStringerGenerator;
            this.PlitaTreugolnikScriptName = this._appConfig.PlitaTreugolnikGenerator;

            this.PathFolderOfGeneration = this._robotConfig.PathForGeneration;

            this.ControlerFolder = this._robotConfig.PathControllerFolder;
        }

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
            if (File.Exists(_path))
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_path), _jsonSettings);
            else
                return new Settings();
        }

        #endregion
    }
}
