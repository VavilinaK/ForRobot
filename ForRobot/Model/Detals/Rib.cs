using System;
//using System.Text.Json.Serialization;l
using Newtonsoft.Json;

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
        private decimal _distanceToStart;
        private decimal _distanceToEnd;

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
        public decimal DistanceToStart { get => this._distanceToStart; set => Set(ref this._distanceToStart, value); }

        [JsonProperty("d_l2")]
        /// <summary>
        /// Отступ справа
        /// </summary>
        public decimal DistanceToEnd { get => this._distanceToEnd; set => Set(ref this._distanceToEnd, value); }

        public Rib()
        {
            //this.DistanceLeft = this.Distance;
            //this.DistanceRight = this.Distance;
        }
    }
}
