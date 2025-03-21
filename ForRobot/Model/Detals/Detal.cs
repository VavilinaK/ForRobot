using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Configuration;

using Newtonsoft.Json;

using ForRobot.Libr;
using ForRobot.Libr.Json;

namespace ForRobot.Model.Detals
{
    public class Detal : BaseClass
    {
        #region Private variables

        /// <summary>
        /// Экземпляр детали из app.config
        /// </summary>
        private ForRobot.Libr.ConfigurationProperties.PlitaConfigurationSection PlitaConfig { get; set; }

        #endregion

        #region Public variables

        #region Virtual

        public virtual string Json { get; }
        
        public virtual string JsonForSave { get; }
        
        /// <summary>
        /// Тип детали
        /// </summary>
        public virtual string DetalType { get; }
        
        /// <summary>
        /// Длина настила
        /// </summary>
        public virtual decimal Long { get; set; }
        
        /// <summary>
        /// Высота ребра
        /// </summary>
        public virtual decimal Hight { get; set; }
        
        /// <summary>
        /// Ширина настила
        /// </summary>
        public virtual decimal Width { get; set; }
        
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
        /// Роспуск в начале
        /// </summary>
        public virtual decimal DissolutionStart { get; set; }
        
        /// <summary>
        /// Роспуск в конце
        /// </summary>
        public virtual decimal DissolutionEnd { get; set; }
        
        /// <summary>
        /// Толщина настила
        /// </summary>
        public virtual decimal ThicknessPlita { get; set; }
        
        /// <summary>
        /// Толщина ребра
        /// </summary>
        public virtual decimal ThicknessRebro { get; set; }
        
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
        /// Выбранная схема сварки рёбер
        /// </summary>
        public virtual string SelectedWeldingSchema { get; set; } = WeldingSchemas.GetDescription(WeldingSchemas.SchemasTypes.LeftEvenOdd_RightEvenOdd);

        /// <summary>
        /// Схема сварки рёбер
        /// </summary>
        public virtual FullyObservableCollection<WeldingSchemas.SchemaRib> WeldingSchema { get; set; }

        /// <summary>
        /// Количество ребер
        /// </summary>
        public virtual int SumReber { get; set; }

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

        public virtual event EventHandler Change;

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
                    this.Long = PlitaConfig.Long;
                    this.Width = PlitaConfig.Width;
                    this.Hight = PlitaConfig.Hight;
                    this.DistanceToFirst = PlitaConfig.DistanceToFirst;
                    this.DistanceBetween = PlitaConfig.DistanceBetween;
                    this.IdentToLeft = PlitaConfig.DistanceToStart;
                    this.IdentToRight = PlitaConfig.DistanceToEnd;
                    this.DissolutionStart = PlitaConfig.DissolutionStart;
                    this.DissolutionEnd = PlitaConfig.DissolutionEnd;
                    this.ThicknessPlita = PlitaConfig.ThicknessPlita;
                    this.ThicknessRebro = PlitaConfig.ThicknessRebro;
                    this.SearchOffsetStart = PlitaConfig.SearchOffsetStart;
                    this.SearchOffsetEnd = PlitaConfig.SearchOffsetEnd;
                    this.SeamsOverlap = PlitaConfig.SeamsOverlap;
                    this.TechOffsetSeamStart = PlitaConfig.TechOffsetSeamStart;
                    this.TechOffsetSeamEnd = PlitaConfig.TechOffsetSeamEnd;
                    this.ReverseDeflection = PlitaConfig.ReverseDeflection;
                    this.WildingSpead = PlitaConfig.WildingSpead;
                    this.ProgramNom = PlitaConfig.ProgramNom;
                    this.SumReber = PlitaConfig.SumReber;
                    break;
            }

            if (this.WeldingSchema == null)
                this.WeldingSchema = ForRobot.Model.Detals.WeldingSchemas.BuildingSchema(WeldingSchemas.GetSchemaType(this.SelectedWeldingSchema), this.SumReber);
        }

        #endregion

        #region Private functions

        #endregion

        #region Public functions

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

        #endregion
    }
}
