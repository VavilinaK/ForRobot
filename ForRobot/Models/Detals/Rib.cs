using System;
//using System.Text.Json.Serialization;l
using Newtonsoft.Json;

using ForRobot.Libr.Json;

namespace ForRobot.Models.Detals
{
    /// <summary>
    /// Модель ребра настила
    /// </summary>
    public class Rib : BaseClass, ICloneable
    {
        private decimal _height;
        private decimal _thickness;
        private decimal _distanceLeft;
        private decimal _distanceRight;
        private decimal _identToLeft;
        private decimal _identToRight;
        private decimal _dissolutionLeft;
        private decimal _dissolutionRight;
        //private decimal _hightLeft;
        //private decimal _hightRight;

        [JsonProperty("wall_height")]
        /// <summary>
        /// Высота ребра
        /// </summary>
        public decimal Height { get => this._height; set => Set(ref this._height, value); }

        [JsonProperty("wall_thickness")]
        /// <summary>
        /// Толщина ребра
        /// </summary>
        public decimal Thickness { get => this._thickness; set => Set(ref this._thickness, value); }

        [JsonProperty("wall_cross_dist_left")]
        /// <summary>
        /// Поперечное расстояние до следующего ребра по левому краю
        /// </summary>
        public decimal DistanceLeft  { get => this._distanceLeft; set => Set(ref this._distanceLeft, value); }

        [JsonProperty("wall_cross_dist_right")]
        /// <summary>
        /// Поперечное расстояние до ребра по правому краю
        /// </summary>
        public decimal DistanceRight { get => this._distanceRight; set => Set(ref this._distanceRight, value); }

        [JsonProperty("wall_long_dist_left")]
        /// <summary>
        /// Продольное расстояние до ребра по левому краю
        /// </summary>
        public decimal IdentToLeft { get => this._identToLeft; set => Set(ref this._identToLeft, value); }

        [JsonProperty("wall_long_dist_right")]
        /// <summary>
        /// Продольное расстояние до ребра по правому краю
        /// </summary>
        public decimal IdentToRight { get => this._identToRight; set => Set(ref this._identToRight, value); }

        [JsonProperty("weld_offset_left")]
        /// <summary>
        /// Отступ шва от левого края ребра
        /// </summary>
        public decimal DissolutionLeft { get => this._dissolutionLeft; set => Set(ref this._dissolutionLeft, value); }

        [JsonProperty("weld_offset_right")]
        /// <summary>
        /// Отступ шва от правого края ребра
        /// </summary>
        public decimal DissolutionRight { get => this._dissolutionRight; set => Set(ref this._dissolutionRight, value); }

        //[JsonProperty("h1")]
        ///// <summary>
        ///// высота ребра (общая или слева)
        ///// </summary>
        //public decimal HightLeft
        //{
        //    get => this._hightLeft;
        //    set
        //    {
        //        Set(ref this._hightLeft, value);
        //        this.ChangeHight?.Invoke(this, null);
        //    }
        //}

        //[JsonProperty("h2")]
        ///// <summary>
        ///// высота ребра справа
        ///// </summary>
        //public decimal HightRight { get => this._hightRight; set => Set(ref this._hightRight, value); }

        public Rib() { }

        public object Clone() => (Rib)this.MemberwiseClone();
    }
}
