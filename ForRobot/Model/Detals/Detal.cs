using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ComponentModel;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ForRobot.Model.File3D;
using ForRobot.Libr;
using ForRobot.Libr.Json;

namespace ForRobot.Model.Detals
{
    public class Detal : ICloneable
    {
        #region Private variables

        /// <summary>
        /// Экземпляр детали из app.config
        /// </summary>
        private ForRobot.Libr.ConfigurationProperties.PlitaConfigurationSection PlitaConfig { get; set; }

        protected readonly JsonSerializerSettings _jsonDeserializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new ForRobot.Libr.Json.SaveAttributesResolver(),
            Formatting = Formatting.Indented,

        };
        protected readonly JsonLoadSettings _jsonLoadSettings = new JsonLoadSettings()
        {
            CommentHandling = CommentHandling.Ignore
        };

        #endregion

        #region Public variables

        public const int MIN_RIB_COUNT = 1;
        
        #region Static

        #endregion Static

        #region Virtual

        [JsonIgnore]
        /// <summary>
        /// Игнорируемы для Undo/Redo свойства
        /// </summary>
        public virtual string[] NotSaveProperties { get; }

        public virtual string Json { get; }
        
        public virtual string JsonForSave { get; }
        
        /// <summary>
        /// Тип детали
        /// </summary>
        public virtual string DetalType { get; }
        
        /// <summary>
        /// Длина настила
        /// </summary>
        public virtual decimal PlateLength { get; set; }

        /// <summary>
        /// Ширина настила
        /// </summary>
        public virtual decimal PlateWidth { get; set; }

        /// <summary>
        /// Толщина настила
        /// </summary>
        public virtual decimal PlateThickness { get; set; }

        /// <summary>
        /// Расстояние до осевой линии первого ребра сверху
        /// </summary>
        public virtual decimal DistanceToFirst { get; set; }
        
        /// <summary>
        /// Расстояние между осевыми линиями рёбер
        /// </summary>
        public virtual decimal DistanceBetween { get; set; }
        
        /// <summary>
        /// Расстояние торца ребра слева
        /// </summary>
        public virtual decimal IdentToLeft { get; set; }
        
        /// <summary>
        /// Расстояние торца ребра справа
        /// </summary>
        public virtual decimal IdentToRight { get; set; }
        
        /// <summary>
        /// Роспуск слева
        /// </summary>
        public virtual decimal DissolutionLeft { get; set; }
        
        /// <summary>
        /// Роспуск справа
        /// </summary>
        public virtual decimal DissolutionRight { get; set; }

        /// <summary>
        /// Высота ребра
        /// </summary>
        public virtual decimal RibHeight { get; set; }

        /// <summary>
        /// Толщина ребра
        /// </summary>
        public virtual decimal RibThickness { get; set; }
        
        /// <summary>
        /// Отступ поиска в начале
        /// </summary>
        public virtual decimal SearchOffsetStart { get; set; }
        
        /// <summary>
        /// Отступ поиска в конце
        /// </summary>
        public virtual decimal SearchOffsetEnd { get; set; }
        
        /// <summary>
        /// Перекрытие швов
        /// </summary>
        public virtual decimal SeamsOverlap { get; set; }

        /// <summary>
        /// Технологический отступ начала шва
        /// </summary>
        public virtual decimal TechOffsetSeamStart { get; set; }

        /// <summary>
        /// Технологический отступ конца шва
        /// </summary>
        public virtual decimal TechOffsetSeamEnd { get; set; }

        /// <summary>
        /// Смещение детали от 0 точки по осям XYZ
        /// </summary>
        public virtual decimal[] XYZOffset { get; set; } = new decimal[3] { 0, 0, 0 };

        /// <summary>
        /// Обратный прогиб
        /// </summary>
        public virtual decimal ReverseDeflection { get; set; }

        /// <summary>
        /// Скорость сварки
        /// </summary>
        public virtual int WildingSpead { get; set; }
        
        /// <summary>
        /// Номер сварочной программы
        /// </summary>
        public virtual int ProgramNom { get; set; }

        /// <summary>
        /// Количество ребер
        /// </summary>
        public virtual int RibCount { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Общее изображение детали
        /// </summary>
        public virtual BitmapImage GenericImage { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Изображение рёбер
        /// </summary>
        public virtual BitmapImage RebraImage { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Продольная привязка
        /// </summary>
        public virtual Privyazka LongitudinalPrivyazka { get; set; } = Privyazka.FromLeftToRight;

        [JsonIgnore]
        /// <summary>
        /// Поперечная привязка
        /// </summary>
        public virtual Privyazka TransversePrivyazka { get; set; } = Privyazka.FromLeftToRight;
       
        #endregion

        #endregion

        #region Event

        /// <summary>
        /// Событие изменения параметра детали
        /// </summary>
        public event PropertyChangedEventHandler ChangePropertyEvent;

        /// <summary>
        /// Событие сохранения параметров детали
        /// </summary>
        public event EventHandler SavePropertiesEvent;

        //public virtual event Func<object, EventArgs, Task> Change;

        #endregion

        #region Constructors

        public Detal() { }

        public Detal(DetalType type)
        {
            switch (type)
            {
                case ForRobot.Model.Detals.DetalType.Plita:
                    this.PlitaConfig = ConfigurationManager.GetSection("plita") as ForRobot.Libr.ConfigurationProperties.PlitaConfigurationSection;
                    this.PlateLength = PlitaConfig.Long;
                    this.PlateWidth = PlitaConfig.Width;
                    this.RibHeight = PlitaConfig.Hight;
                    this.DistanceToFirst = PlitaConfig.DistanceToFirst;
                    this.DistanceBetween = PlitaConfig.DistanceBetween;
                    this.IdentToLeft = PlitaConfig.DistanceToStart;
                    this.IdentToRight = PlitaConfig.DistanceToEnd;
                    this.DissolutionLeft = PlitaConfig.DissolutionStart;
                    this.DissolutionRight = PlitaConfig.DissolutionEnd;
                    this.PlateThickness = PlitaConfig.ThicknessPlita;
                    this.RibThickness = PlitaConfig.ThicknessRebro;
                    this.SearchOffsetStart = PlitaConfig.SearchOffsetStart;
                    this.SearchOffsetEnd = PlitaConfig.SearchOffsetEnd;
                    this.SeamsOverlap = PlitaConfig.SeamsOverlap;
                    this.TechOffsetSeamStart = PlitaConfig.TechOffsetSeamStart;
                    this.TechOffsetSeamEnd = PlitaConfig.TechOffsetSeamEnd;
                    this.ReverseDeflection = PlitaConfig.ReverseDeflection;
                    this.WildingSpead = PlitaConfig.WildingSpead;
                    this.ProgramNom = PlitaConfig.ProgramNom;
                    this.RibCount = PlitaConfig.SumReber;
                    break;
            }
        }

        #endregion

        #region Private functions


        #endregion

        #region Public functions

        public virtual object DeserializeDetal(string sJsonString)
        {
            return null;
            //string detalType = Newtonsoft.Json.Linq.JObject.Parse(sJsonString)["DetalType"].ToString();

            //if (string.IsNullOrEmpty(sJsonString))
            //    return new Detal();

            //switch (detalType)
            //{
            //    case DetalTypes.Plita:
            //        break;

            //    case DetalTypes.Stringer:
            //        break;

            //    case DetalTypes.Treygolnik:
            //        break;
            //}

            //Detal detal = DetalTypes.StringToEnum(detalType)

            //string.IsNullOrEmpty(sJsonString) ? new Plita(DetalType.Plita) : JsonConvert.DeserializeObject<Plita>(JObject.Parse(Properties.Settings.Default.SavePlita, _jsonLoadSettings).ToString(), this._jsonSettings);
        }

        public object Clone() => (Detal)this.MemberwiseClone();

        public static Detal GetDetal(string detalType)
        {
            switch (detalType)
            {
                case string a when a == DetalTypes.Plita:
                    return new Plita(Detals.DetalType.Plita);

                case string b when b == DetalTypes.Stringer:
                    return new PlitaStringer(Detals.DetalType.Stringer);

                case string c when c == DetalTypes.Treygolnik:
                    return new PlitaTreygolnik(Detals.DetalType.Treygolnik);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Вызов события изменения свойства
        /// </summary>
        /// <param name="propertyName">Наименование свойства</param>
        public virtual void OnChangeProperty([CallerMemberName] string propertyName = null) => this.ChangePropertyEvent?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Вызов события сохранения свойств
        /// </summary>
        public void SaveProperties() => this.SavePropertiesEvent?.Invoke(this, null);

        #endregion
    }
}
