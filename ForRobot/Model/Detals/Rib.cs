using System;
//using System.Text.Json.Serialization;l
using Newtonsoft.Json;

using ForRobot.Libr.Json;

namespace ForRobot.Model.Detals
{
    /// <summary>
    /// Модель ребра
    /// </summary>
    public class Rib : BaseClass
    {
        private decimal _distance;
        private decimal _distanceLeft;
        private decimal _distanceRight;
        private decimal _identToLeft;
        private decimal _identToRight;
        private decimal _dissolutionLeft;
        private decimal _dissolutionRight;
        
        [JsonProperty("d_dis")]
        /// <summary>
        /// Расстояние до следующего ребра
        /// </summary>
        public decimal Distance { get => this._distance; set => Set(ref this._distance, value); }

        [JsonProperty("d_dis1")]
        /// <summary>
        /// Расстояние до ребра по левому краю
        /// </summary>
        public decimal DistanceLeft { get => this._distanceLeft; set => Set(ref this._distanceLeft, value); }

        [JsonProperty("d_dis2")]
        /// <summary>
        /// Расстояние до ребра по правому краю
        /// </summary>
        public decimal DistanceRight { get => this._distanceRight; set => Set(ref this._distanceRight, value); }

        [JsonProperty("d_l1")]
        /// <summary>
        /// Отступ слева
        /// </summary>
        public decimal IdentToLeft { get => this._identToLeft; set => Set(ref this._identToLeft, value); }

        [JsonProperty("d_l2")]
        /// <summary>
        /// Отступ справа
        /// </summary>
        public decimal IdentToRight { get => this._identToRight; set => Set(ref this._identToRight, value); }

        [JsonProperty("l_r1")]
        /// <summary>
        /// Роспуск слева
        /// </summary>
        public decimal DissolutionLeft { get => this._dissolutionLeft; set => Set(ref this._dissolutionLeft, value); }

        [JsonProperty("l_r2")]
        /// <summary>
        /// Роспуск справа
        /// </summary>
        public decimal DissolutionRight { get => this._dissolutionRight; set => Set(ref this._dissolutionRight, value); }

        public Rib() { }
    }
}
