using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ForRobot.Model
{
    public class PlitaWithBevels : Detal
    {
        #region Private variables

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            WriteIndented = true
        };

        private string _scoseType = ScoseTypes.Rect;

        #endregion

        #region Public variables

        [JsonIgnore]
        public sealed override string Json { get => JsonSerializer.Serialize<PlitaWithBevels>(this, options); }

        [JsonIgnore]
        /// <summary>
        /// Тип детали
        /// </summary>
        public DetalType DetalType { get => DetalType.WithBevels; }

        #region Общие

        [JsonPropertyName("edge_count")]
        /// <summary>
        /// Количество ребер
        /// </summary>
        public sealed override int SumReber { get; set; }

        [JsonPropertyName("l")]
        /// <summary>
        /// Длина настила
        /// </summary>
        public sealed override decimal Long { get; set; }

        [JsonPropertyName("h")]
        /// <summary>
        /// Высота ребра
        /// </summary>
        public sealed override decimal Hight { get; set; }

        [JsonPropertyName("d_w1")]
        /// <summary>
        /// Расстояние по ширине до осевой линии первого ребра
        /// </summary>
        public sealed override decimal DistanceToFirst { get; set; }

        [JsonPropertyName("d_w2")]
        /// <summary>
        /// Расстояние между осевыми линиями рёбер
        /// </summary>
        public sealed override decimal DistanceBetween { get; set; }

        [JsonPropertyName("d_l1")]
        /// <summary>
        /// Расстояние торца ребра в начале
        /// </summary>
        public sealed override decimal DistanceToStart { get; set; }

        [JsonPropertyName("d_l2")]
        /// <summary>
        /// Расстояние торца ребра в конце
        /// </summary>
        public sealed override decimal DistanceToEnd { get; set; }

        [JsonPropertyName("l_r1")]
        /// <summary>
        /// Роспуск вначале
        /// </summary>
        public sealed override decimal DissolutionStart { get; set; }

        [JsonPropertyName("l_r2")]
        /// <summary>
        /// Роспуск вконце
        /// </summary>
        public sealed override decimal DissolutionEnd { get; set; }


        [JsonPropertyName("t_p")]
        /// <summary>
        /// Толщина настила
        /// </summary>
        public sealed override decimal ThicknessPlita { get; set; }

        [JsonPropertyName("t_r")]
        /// <summary>
        /// Толщина ребра
        /// </summary>
        public sealed override decimal ThicknessRebro { get; set; }

        [JsonPropertyName("d_s1")]
        /// <summary>
        /// Отступ поиска в начале
        /// </summary>
        public sealed override decimal SearchOffsetStart { get; set; }

        [JsonPropertyName("d_s2")]
        /// <summary>
        /// Отступ поиска в конце
        /// </summary>
        public sealed override decimal SearchOffsetEnd { get; set; }

        [JsonPropertyName("l_overlap")]
        /// <summary>
        /// Перекрытие швов
        /// </summary>
        public sealed override decimal SeamsOverlap { get; set; }

        [JsonPropertyName("d_t1")]
        /// <summary>
        /// Технологический отступ начала шва
        /// </summary>
        public sealed override decimal TechOffsetSeamStart { get; set; }

        [JsonPropertyName("d_t2")]
        /// <summary>
        /// Технологический отступ конца шва
        /// </summary>
        public sealed override decimal TechOffsetSeamEnd { get; set; }

        #endregion

        #region Спец.
        
        [JsonPropertyName("d_type")]
        /// <summary>
        /// Тип скоса
        /// </summary>
        public string ScoseType
        {
            get => this._scoseType;
            set => Set(ref this._scoseType, value);
        }

        [JsonPropertyName("w")]
        /// <summary>
        /// Ширина настила
        /// </summary>
        public decimal Wight { get; set; } = 2260;

        [JsonPropertyName("d_b1")]
        /// <summary>
        /// Скос слева
        /// </summary>
        public decimal BevelToStart { get; set; } = 243;
        
        [JsonPropertyName("d_b2")]
        /// <summary>
        /// Скос справа
        /// </summary>
        public decimal BevelToEnd { get; set; } = 243;

        #endregion

        #region Сварка

        [JsonPropertyName("velocity")]
        /// <summary>
        /// Скорость сварки
        /// </summary>
        public sealed override int WildingSpead { get; set; }

        [JsonPropertyName("job")]
        /// <summary>
        /// Номер сварачной программы
        /// </summary>
        public sealed override int ProgramNom { get; set; }

        #endregion

        [JsonIgnore]
        public override Privyazka LongitudinalPrivyazka
        {
            get => base.LongitudinalPrivyazka;
            set => base.LongitudinalPrivyazka = value;
        }

        [JsonIgnore]
        public override Privyazka TransversePrivyazka
        {
            get => base.TransversePrivyazka;
            set
            {
                base.TransversePrivyazka = value;
                //GenericImage = JoinPlita(GetStartPlitaImage(), GetBodyPlitaImage(), GetEndPlitaImage());
            }
        }

        #endregion

        #region Constructors

        public PlitaWithBevels() { }

        public PlitaWithBevels(DetalType type) : base(type) { }

        #endregion

        #region Public functions

        #endregion

        #region Private functions

        #endregion
    }
}
