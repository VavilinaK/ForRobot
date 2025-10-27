using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.Collections.Generic;
using System.Configuration;

using AvalonDock.Themes;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using HelixToolkit.Wpf;

namespace ForRobot.Model.Settings
{
    public class Settings : ICloneable
    {
        #region Private variables

        private const string _fileName = "interfaceOfRobot_settings.json";

        private static string _path = Path.Combine(Path.GetTempPath(), Settings._fileName);

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            //ContractResolver = new ForRobot.Libr.Json.SettingsResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        private Tuple<string, Theme> _selectedTheme;

        private Dictionary<string, System.Windows.Media.Color> _colors = new Dictionary<string, System.Windows.Media.Color>();
        
        private ForRobot.Libr.ConfigurationProperties.AppConfigurationSection _appConfig = ConfigurationManager.GetSection("app") as ForRobot.Libr.ConfigurationProperties.AppConfigurationSection;
        private ForRobot.Libr.ConfigurationProperties.RobotConfigurationSection _robotConfig = ConfigurationManager.GetSection("robot") as ForRobot.Libr.ConfigurationProperties.RobotConfigurationSection;

        #region Properties

        private bool _showCoordinateSystem = true;
        private bool _showViewCube = true;
        private bool _showTriangleCountInfo = false;
        private bool _orthographic = false;
        private bool _showCameraInfo = false;
        private bool _showCameraTarget = true;
        private bool _rotateAroundMouseDownPoint = false;
        private bool _zoomAroundMouseDownPoint = false;
        private bool _isInertiaEnabled = false;
        private bool _isPanEnabled = true;
        private bool _isMoveEnabled = true;
        private bool _isRotationEnabled = true;
        private bool _isZoomEnabled = true;

        #endregion

        #endregion Private variables

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
        /// Создаётся ли при открытии файл детли. Не работает, если приложение открывает файл "с помощью"
        /// </summary>
        public bool CreatedDetalFile { get; set; } = true;

        /// <summary>
        /// Сохраняются ли параметры детали на выходе
        /// </summary>
        public bool SaveDetalProperties { get; set; } = true;

        /// <summary>
        /// Ограничено ли время ожидания ответа от сервера
        /// </summary>
        public bool LimitedConnectionTimeOut { get; set; } = false; // Не используется

        /// <summary>
        /// Время ожидания ответа от сервера, сек.
        /// </summary>
        public double ConnectionTimeOut { get; set; } = 3; // Не используется

        /// <summary>
        /// Тип детали, для которой создаётся стартовый файл
        /// </summary>
        public string StartedDetalType { get; set; } = Model.Detals.DetalTypes.Plita;

        /// <summary>
        /// Режим закрытия приложения. Спрашивает пользователя о закрытии и/или разрыве соединения
        /// </summary>
        public ModeClosingApp ModeClosingApp { get; set; } = ModeClosingApp.HaveConnected; // Устарело

        /// <summary>
        /// Выбранное приложение для открытия файлов
        /// </summary>
        public ApplicationInfo SelectedAppForOpened { get; set; }

        /// <summary>
        /// Сохранённые приложения для открытия файлов
        /// </summary>
        public List<ApplicationInfo> SavedAppsForOpened { get; set; }

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
        public bool ShowCoordinateSystem
        {
            get => this._showCoordinateSystem;
            set
            {
                this._showCoordinateSystem = value;
                this.OnChangeProperty();
            }
        }
        /// <summary>
        /// Показ пространственного куба
        /// </summary>
        public bool ShowViewCube
        {
            get => this._showViewCube;
            set
            {
                this._showViewCube = value;
                this.OnChangeProperty();
            }
        }
        /// <summary>
        /// Доступности нажатия рёбер пространственного куба
        /// </summary>
        public bool IsViewCubeEdgeClicksEnabled { get; set; } = false;
        /// <summary>
        /// Показ кол-ва полигонов
        /// </summary>
        public bool ShowTriangleCountInfo
        {
            get => this._showTriangleCountInfo;
            set
            {
                this._showTriangleCountInfo = value;
                this.OnChangeProperty();
            }
        }

        public bool ShowFieldOfView { get; set; } = false;
        public bool ShowFrameRate { get; set; } = false;

        /// <summary>
        /// Вертикальное положение системы координат
        /// </summary>
        public VerticalAlignment CoordinateSystemVerticalPosition { get; set; } = VerticalAlignment.Top;
        /// <summary>
        /// Горизонтальное положение системы координат
        /// </summary>
        public HorizontalAlignment CoordinateSystemHorizontalPosition { get; set; } = HorizontalAlignment.Right;

        /// <summary>
        /// Перечень цветов
        /// </summary>
        public Dictionary<string, System.Windows.Media.Color> Colors
        {
            get => this._colors;
            set
            {
                this._colors = value;

                foreach(var c in this._colors) // Установка класса Colors.
                {
                    SetColor(c.Key, c.Value);
                }
            }
        }

        #endregion 3DView

        #region Params

        private double _annotationFontSize = 20;
        private double _weldsThickness = 5;
        private double _annotationThickness = 2.0;
        //private bool _annotationIsVisibale = true;

        /// <summary>
        /// Толщина линий шва на модели
        /// </summary>
        public double WeldsThickness
        {
            get => this._weldsThickness;
            set
            {
                this._weldsThickness = value;
                this.OnChangeProperty();
            }
        }

        /// <summary>
        /// Размер шрифта
        /// </summary>
        public double AnnotationFontSize
        {
            get => this._annotationFontSize;
            set
            {
                this._annotationFontSize = value;
                this.OnChangeProperty();
            }
        }

        /// <summary>
        /// Толщина линий
        /// </summary>
        public double AnnotationThickness
        {
            get => this._annotationThickness;
            set
            {
                this._annotationThickness = value;
                this.OnChangeProperty();
            }
        }

        ///// <summary>
        ///// Видимы ли параметры
        ///// </summary>
        //public bool AnnotationIsVisibale
        //{
        //    get => this._annotationIsVisibale;
        //    set
        //    {
        //        this._paramsIsVisibale = value;
        //        this.OnChangeProperty();
        //    }
        //}

        #endregion Params

        #region Camera

        /// <summary>
        /// Включена ли ортоганальная камера
        /// </summary>
        public bool Orthographic
        {
            get => this._orthographic;
            set
            {
                this._orthographic = value;
                this.OnChangeProperty();
            }
        }
        /// <summary>
        /// Демонстрация информации о камере
        /// </summary>
        public bool ShowCameraInfo
        {
            get => this._showCameraInfo;
            set
            {
                this._showCameraInfo = value;
                this.OnChangeProperty();
            }
        }
        /// <summary>
        /// Демонстрация курсора камеры
        /// </summary>
        public bool ShowCameraTarget
        {
            get => this._showCameraTarget;
            set
            {
                this._showCameraTarget = value;
                this.OnChangeProperty();
            }
        }
        /// <summary>
        /// Поворот вокруг мыши
        /// </summary>
        public bool RotateAroundMouseDownPoint
        {
            get => this._rotateAroundMouseDownPoint;
            set
            {
                this._rotateAroundMouseDownPoint = value;
                this.OnChangeProperty();
            }
        }
        /// <summary>
        /// Приближение около мыши
        /// </summary>
        public bool ZoomAroundMouseDownPoint
        {
            get => this._zoomAroundMouseDownPoint;
            set
            {
                this._zoomAroundMouseDownPoint = value;
                this.OnChangeProperty();
            }
        }
        /// <summary>
        /// Инерция камеры
        /// </summary>
        public bool IsInertiaEnabled
        {
            get => this._isInertiaEnabled;
            set
            {
                this._isInertiaEnabled = value;
                this.OnChangeProperty();
            }
        }
        /// <summary>
        /// Вкличено ли панорамирование
        /// </summary>
        public bool IsPanEnabled
        {
            get => this._isPanEnabled;
            set
            {
                this._isPanEnabled = value;
                this.OnChangeProperty();
            }
        }
        /// <summary>
        /// Вкличено ли перемещение
        /// </summary>
        public bool IsMoveEnabled
        {
            get => this._isMoveEnabled;
            set
            {
                this._isMoveEnabled = value;
                this.OnChangeProperty();
            }
        }
        /// <summary>
        /// Вкличено ли вращение
        /// </summary>
        public bool IsRotationEnabled
        {
            get => this._isRotationEnabled;
            set
            {
                this._isRotationEnabled = value;
                this.OnChangeProperty();
            }
        }
        /// <summary>
        /// Вкличено ли маштабирование
        /// </summary>
        public bool IsZoomEnabled
        {
            get => this._isZoomEnabled;
            set
            {
                this._isZoomEnabled = value;
                this.OnChangeProperty();
            }
        }

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

        /// <summary>
        /// Событие изменения свойства настроек
        /// </summary>
        public event EventHandler ChangePropertyEvent;

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

            //if (this.Colors == null || this.Colors.Count == 0) this.Colors = new List<File3D.PropertyColor>();
            if (this.Colors.Count == 0) this.Colors = GetColors();
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
            if (!File.Exists(_path))
                return new Settings();

            try
            {
                string json = File.ReadAllText(_path);
                Settings settings =  JsonConvert.DeserializeObject<Settings>(json, _jsonSettings) ?? new Settings();
                settings.Colors = JObject.Parse(json)["Colors"].ToObject<Dictionary<string, System.Windows.Media.Color>>();
                return settings;
            }
            catch (Exception ex) when (LogException(ex))
            {
                Settings settings = new Settings();
                settings.Save();
                return settings;
            }
        }

        /// <summary>
        /// Выгрузка установленных цветов для 3д сцены
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, System.Windows.Media.Color> GetColors()
        {
            Dictionary<string, System.Windows.Media.Color> colors = new Dictionary<string, System.Windows.Media.Color>();

            foreach (var f in typeof(ForRobot.Model.File3D.Colors).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                var attribute = f.GetCustomAttributes(typeof(ForRobot.Libr.Attributes.PropertyNameAttribute), false).FirstOrDefault() as ForRobot.Libr.Attributes.PropertyNameAttribute;
                if (attribute != null)
                {
                    // Извлекаем название из атрибута
                    string name = attribute.PropertyName;

                    // Получаем значение цвета из свойства экземпляра
                    System.Windows.Media.Color colorValue = (System.Windows.Media.Color)f.GetValue(null);

                    colors.Add(name, colorValue);
                }
            }
            return colors;
        }

        /// <summary>
        /// Установка цвета объекта 3д сцена
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="color">Значение цвета</param>
        public static void SetColor(string propertyName, System.Windows.Media.Color color)
        {
            foreach (var f in typeof(ForRobot.Model.File3D.Colors).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                var attribute = f.GetCustomAttributes(typeof(ForRobot.Libr.Attributes.PropertyNameAttribute), false).FirstOrDefault() as ForRobot.Libr.Attributes.PropertyNameAttribute;
                if (attribute.PropertyName == propertyName)
                    f.SetValue(null, color);
            }
        }

        //public List<ForRobot.Model.File3D.PropertyColor> GetColors()
        //{
        //    //this.Colors = new List<Model.File3D.PropertyColor>();
        //    //this.Colors?.Clear();
        //    List<ForRobot.Model.File3D.PropertyColor> colors = new List<ForRobot.Model.File3D.PropertyColor>();
        //    // Создаем экземпляр класса Colors
        //    //var colorsInstance = new ForRobot.Model.File3D.Colors();

        //    foreach (var f in typeof(ForRobot.Model.File3D.Colors).GetProperties(BindingFlags.Static | BindingFlags.Public))
        //    {
        //        var attribute = f.GetCustomAttributes(typeof(ForRobot.Libr.Attributes.PropertyNameAttribute), false).FirstOrDefault() as ForRobot.Libr.Attributes.PropertyNameAttribute;
        //        if (attribute != null)
        //        {
        //            // Извлекаем название из атрибута
        //            string name = attribute.PropertyName;

        //            // Получаем значение цвета из свойства экземпляра
        //            System.Windows.Media.Color colorValue = (System.Windows.Media.Color)f.GetValue(null);

        //            colors.Add(new Model.File3D.PropertyColor(name, colorValue));
        //        }
        //    }
        //    return colors;
        //}

        public object Clone() => (Settings)this.MemberwiseClone();

        #endregion Public functions

        #region Private functions

        /// <summary>
        /// Вызов события изменения свойства
        /// </summary>
        public void OnChangeProperty() => this.ChangePropertyEvent?.Invoke(this, null);

        /// <summary>
        /// Логирование исключений выгрузки настроек
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private static bool LogException(Exception ex)
        {
            App.Current.Logger.Error(ex, "Ошибка выгрузки настроек приложения");
            return true;
        }

        #endregion Private functions
    }
}
