using System;
//using System.Text.Json.Serialization;l
using Newtonsoft.Json;

using ForRobot.Libr.Json;

namespace ForRobot.Model.Detals
{
    /// <summary>
    /// Модель ребра настила
    /// </summary>
    public class Rib : BaseClass, ICloneable
    {
        private decimal _distanceLeft;
        private decimal _distanceRight;
        private decimal _identToLeft;
        private decimal _identToRight;
        private decimal _dissolutionLeft;
        private decimal _dissolutionRight;
        private decimal _hightLeft;
        private decimal _hightRight;

        [JsonProperty("d_dis1")]
        /// <summary>
        /// Расстояние до следующего ребра (общее или по левому краю)
        /// </summary>
        public decimal DistanceLeft
        {
            get => this._distanceLeft;
            set
            {
                Set(ref this._distanceLeft, value);
                this.ChangeDistance?.Invoke(this, null);
            }
        }

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

        /// <summary>
        /// Событие изменения расстояниия между рёбрами, при их параллельности
        /// </summary>
        public event EventHandler ChangeDistance;

        ///// <summary>
        ///// Событие изменения высоты ребра, если она одинаковая с 2-х сторон
        ///// </summary>
        //public event EventHandler ChangeHight;

        public Rib() { }

        public void OnChangeDistanceEvent(object sender, EventArgs e) => this.ChangeDistance?.Invoke(sender, e);
        //public void OnChangeHightEvent(object sender, EventArgs e) => this.ChangeHight?.Invoke(sender, e);

        public object Clone() => (Rib)this.MemberwiseClone();
    }
}
