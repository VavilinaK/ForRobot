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

using ForRobot.Models.File3D;
using ForRobot.Libr;
using ForRobot.Libr.Json;
using ForRobot.Libr.Converters;
using HelixToolkit.Wpf.SharpDX;
using System.Threading;

namespace ForRobot.Models.Detals
{
    public class Detal : ICloneable, IDisposable
    {
        #region Private variables

        private string _scoseType = ScoseTypes.Rect;
        private decimal _reverseDeflection;
        private decimal _plateWidth;
        private decimal _plateLength;
        private decimal _plateThickness;
        private decimal _plateBevelToLeft;
        private decimal _plateBevelToRight;
        private decimal _plateBevelToLeftSave;
        private decimal _plateBevelToRightSave;
        private decimal[] _XYZOffset = new decimal[3] { 0, 0, 0 };
        
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

        [JsonProperty("d_type")]
        [JsonConverter(typeof(JsonCommentConverter), "Тип скоса")]
        /// <summary>
        /// Тип скоса
        /// </summary>
        public string ScoseType
        {
            get => this._scoseType;
            set
            {
                this._scoseType = value;
                this.OnChangeProperty(nameof(this.ScoseType));
            }
        }

        [JsonProperty("reverse_deflection")]
        [JsonConverter(typeof(JsonCommentConverter), "Обратный прогиб детали")]
        /// <summary>
        /// Обратный прогиб детали
        /// </summary>
        public decimal ReverseDeflection
        {
            get => this._reverseDeflection;
            set
            {
                this._reverseDeflection = value;
                this.OnChangeProperty(nameof(this.ReverseDeflection));
            }
        }

        [JsonProperty("base_width")]
        [JsonConverter(typeof(JsonCommentConverter), "Ширина настила")]
        /// <summary>
        /// Ширина настила
        /// </summary>
        public decimal PlateWidth
        {
            get => this._plateWidth;
            set
            {
                this._plateWidth = value;
                this.OnChangeProperty(nameof(this.PlateWidth));
            }
        }

        [JsonProperty("base_length")]
        [JsonConverter(typeof(JsonCommentConverter), "Длина настила")]
        /// <summary>
        /// Длина настила
        /// </summary>
        public decimal PlateLength
        {
            get => this._plateWidth;
            set
            {
                this._plateWidth = value;
                this.OnChangeProperty(nameof(this.PlateLength));
            }
        }

        [JsonProperty("base_thickness")]
        [JsonConverter(typeof(JsonCommentConverter), "Толщина настила")]
        /// <summary>
        /// Толщина настила
        /// </summary>
        public decimal PlateThickness
        {
            get => this._plateThickness;
            set
            {
                this._plateThickness = value;
                this.OnChangeProperty(nameof(this.PlateThickness));
            }
        }

        [JsonProperty("base_bevel_left")]
        [JsonConverter(typeof(JsonCommentConverter), "Скос настила слева")]
        /// <summary>
        /// Скос настила слева
        /// </summary>
        public decimal PlateBevelToLeft
        {
            get => this._plateBevelToLeft;
            set
            {
                this._plateBevelToLeft = value;
                this.OnChangeProperty(nameof(this.PlateBevelToLeft));
            }
        }

        [JsonProperty("base_bevel_right")]
        [JsonConverter(typeof(JsonCommentConverter), "Скос настила справа")]
        /// <summary>
        /// Скос настила справа
        /// </summary>
        public decimal PlateBevelToRight
        {
            get => this._plateBevelToRight;
            set
            {
                this._plateBevelToRight = value;
                this.OnChangeProperty(nameof(this.PlateBevelToRight));
            }
        }

        [JsonProperty("base_displace")]
        [JsonConverter(typeof(JsonCommentConverter), "Смещение настила от нулевой точки стола")]
        /// <summary>
        /// Смещение детали от 0 точки по осям XYZ
        /// </summary>
        public decimal[] XYZOffset
        {
            get => this._XYZOffset;
            set
            {
                this._XYZOffset = value;
                this.OnChangeProperty(nameof(this.XYZOffset));
            }
        }            

        [JsonIgnore]
        /// <summary>
        /// Смещение детали от 0 точки по оси X
        /// </summary>
        public decimal XOffset
        {
            get => this.XYZOffset[0];
            set
            {
                this.XYZOffset[0] = value;
                this.OnChangeProperty(nameof(this.XOffset));
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Смещение детали от 0 точки по оси Y
        /// </summary>
        public decimal YOffset
        {
            get => this.XYZOffset[1];
            set
            {
                this.XYZOffset[1] = value;
                this.OnChangeProperty(nameof(this.YOffset));
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Смещение детали от 0 точки по оси Z
        /// </summary>
        public decimal ZOffset
        {
            get => this.XYZOffset[2];
            set
            {
                this.XYZOffset[2] = value;
                this.OnChangeProperty(nameof(this.ZOffset));
            }
        }

        //[JsonIgnore]
        ///// <summary>
        ///// Продольная привязка
        ///// </summary>
        //public virtual Privyazka LongitudinalPrivyazka { get; set; } = Privyazka.FromLeftToRight;

        //[JsonIgnore]
        ///// <summary>
        ///// Поперечная привязка
        ///// </summary>
        //public virtual Privyazka TransversePrivyazka { get; set; } = Privyazka.FromLeftToRight;
       
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

        #endregion

        #region Constructors

        public Detal()
        {
            this.ChangePropertyEvent += this.HandleChangeProperty;
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Делегат изменения свойства детали
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleChangeProperty(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(this.ScoseType))
            {
                if(this.ScoseType == ScoseTypes.Rect)
                {
                    (this._plateBevelToLeftSave, this._plateBevelToRightSave) = (this.PlateBevelToLeft, this.PlateBevelToRight);
                    (this.PlateBevelToLeft, this.PlateBevelToRight) = (0, 0);
                }
                else if(this.PlateBevelToLeft == 0 || this.PlateBevelToRight == 0)
                {
                    (this.PlateBevelToLeft, this.PlateBevelToRight) = (this._plateBevelToLeftSave, this._plateBevelToRightSave);
                }
            }
        }

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
        //{
        //    var json = JsonConvert.SerializeObject(this);
        //    return JsonConvert.DeserializeObject(json, this.GetType());
        //}

        public bool Equals(Detal detal)
        {
            var detals = new System.Collections.Generic.HashSet<Detal>();
            detals.Add(this);
            return detals.Contains(detal);
        }

        //public static Detal GetDetal(string detalType)
        //{
        //    switch (detalType)
        //    {
        //        case string a when a == DetalTypes.Plita:
        //            return new Plita();

        //        case string b when b == DetalTypes.Stringer:
        //            return new PlitaStringer();

        //        case string c when c == DetalTypes.Treygolnik:
        //            return new PlitaTreygolnik();

        //        default:
        //            return null;
        //    }
        //}

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

        #region Implementations of IDisposable

        private volatile int _disposed;

        ~Detal() => Dispose(false);

        public void Dispose() => this.Dispose(true);

        public void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                if (disposing)
                {
                    this.ChangePropertyEvent -= this.HandleChangeProperty;
                    GC.SuppressFinalize(this);
                }
            }
        }

        #endregion
    }
}
