using System;
using System.Linq;
using System.Drawing;
using System.Windows;
//using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using HelixToolkit.Wpf;

using ForRobot.Libr;
using ForRobot.Libr.Json;
using ForRobot.Libr.Attributes;
using ForRobot.Libr.Converters;
using ForRobot.Libr.Collections;
using ForRobot.Libr.ConfigurationProperties;
using System.ComponentModel;
using System.Threading;

namespace ForRobot.Model.Detals
{
    public class Plita : Detal
    {
        #region Private variables

        private string _scoseType;
        private string _selectedWeldingSchema = WeldingSchemas.GetDescription(WeldingSchemas.SchemasTypes.LeftEvenOdd_RightEvenOdd);

        private bool _diferentDistance = false;
        private bool _paralleleRibs = true;
        private bool _diferentDissolutionLeft = false;
        private bool _diferentDissolutionRight = false;
        //private bool _diferentHight = false;
        //private bool _diferentHightLeftToRight = false;

        private decimal _reverseDeflection;
        private decimal _ribsHeight;
        private decimal _ribsThickness;
        private int _ribCount;
        private decimal _distanceToFirstRib;
        private decimal _distanceBetweenRibs;
        private decimal _identToLeft;
        private decimal _identToRight;
        private decimal _dissolutionLeft;
        private decimal _dissolutionRight;
        private decimal _bevelToLeft;
        private decimal _bevelToRight;

        private BitmapImage _rebraImage;
        private BitmapImage _plitaImage;

        private FullyObservableCollection<Rib> _ribsCollection;
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented
        };

        #endregion

        #region Public variables

        [JsonIgnore]
        /// <summary>
        /// Игнорируемы для Undo/Redo свойства
        /// </summary>
        public override string[] NotSaveProperties { get; } = new string[] { nameof(SelectedWeldingSchema) };

        /// <summary>
        /// Json-строка для передачи
        /// </summary>
        [JsonIgnore]
        public override string Json
        {
            get
            {
                this._jsonSettings.ContractResolver = new PlitaWithRibsAttributesResolver(this.ScoseType, this.DiferentDistance, this.ParalleleRibs, this.DiferentDissolutionLeft, this.DiferentDissolutionRight);
                return JsonConvert.SerializeObject(this, _jsonSettings).Replace("d_W2", "d_w");
            }
        }

        /// <summary>
        /// Json-строка для сохранения
        /// </summary>
        [JsonIgnore]
        public override string JsonForSave
        {
            get
            {
                this._jsonSettings.ContractResolver = new SaveAttributesResolver();
                return JsonConvert.SerializeObject(this, _jsonSettings);
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        /// <inheritdoc cref="Detal.DetalType"/>
        public override string DetalType { get => DetalTypes.Plita; }

        [JsonProperty("d_type")]
        [JsonConverter(typeof(JsonCommentConverter), "Тип скоса")]
        /// <summary>
        /// Тип скоса
        /// </summary>
        public string ScoseType
        {
            get => this._scoseType ?? (this._scoseType = ScoseTypes.Rect);
            set
            {
                this._scoseType = value;
                this.OnChangeProperty(nameof(this.ScoseType));
            }
        }

        //[JsonIgnore]
        //[SaveAttribute]
        ///// <summary>
        ///// Выбранная схема сварки рёбер
        ///// </summary>
        public string SelectedWeldingSchema
        {
            get => this._selectedWeldingSchema;
            set
            {
                this._selectedWeldingSchema = value;

                if (this._selectedWeldingSchema != ForRobot.Model.Detals.WeldingSchemas.GetDescription(ForRobot.Model.Detals.WeldingSchemas.SchemasTypes.Edit))
                    this.WeldingSchema = this.FillWeldingSchema();

                this.OnChangeProperty(nameof(this.SelectedWeldingSchema));
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Разное ли рассояние между рёбрами")]
        /// <summary>
        /// Различно ли расстояние между рёбрами => отступы и т.д.
        /// </summary>
        public bool DiferentDistance
        {
            get => this._diferentDistance;
            set
            {
                this._diferentDistance = value;
                if (!this._diferentDistance && this.RibsCollection?.Count > 0)
                {
                    for (int i = 0; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].IdentToLeft = this.IdentToLeft;
                        this.RibsCollection[i].IdentToRight = this.IdentToRight;

                        if (i == 0)
                        {
                            this.RibsCollection[i].DistanceLeft = this.DistanceToFirstRib;
                            continue;
                        }
                        this.RibsCollection[i].DistanceLeft = this.DistanceBetweenRibs;
                    }
                }
                //else
                //    foreach(var rib in this.RibsCollection)
                //        rib.IsSave = true;
                        
                this.OnChangeProperty(nameof(this.DiferentDistance));
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Параллельлны ли рёбра")]
        /// <summary>
        /// Паралельны ли рёбра друг к другу
        /// </summary>
        public bool ParalleleRibs
        {
            get => this.DiferentDistance ? this._paralleleRibs : true;
            set
            {
                this._paralleleRibs = value;
                if (this._paralleleRibs && this.RibsCollection?.Count > 0) // Изменение расстояния для параллельных рёбер.
                    for (int i = 0; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].OnChangeDistanceLeftEvent();
                    }
                this.OnChangeProperty(nameof(this.ParalleleRibs));
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Разный ли роспуск слева")]
        /// <summary>
        /// Разный ли роспуск слева
        /// </summary>
        public bool DiferentDissolutionLeft
        {
            get => this._diferentDissolutionLeft;
            set
            {
                this._diferentDissolutionLeft = value;
                if (!_diferentDissolutionLeft && this.RibsCollection?.Count > 0)
                    for (int i = 0; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].DissolutionLeft = this.DissolutionLeft;
                    }
                this.OnChangeProperty(nameof(this.DiferentDissolutionLeft));
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Разный ли роспуск справа")]
        /// <summary>
        /// Разный ли роспуск справа
        /// </summary>
        public bool DiferentDissolutionRight
        {
            get => this._diferentDissolutionRight;
            set
            {
                this._diferentDissolutionRight = value;
                if (!_diferentDissolutionRight && this.RibsCollection?.Count > 0)
                    for (int i = 0; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].DissolutionRight = this.DissolutionRight;
                    }
                this.OnChangeProperty(nameof(this.DiferentDissolutionRight));
            }
        }

        //[JsonIgnore]
        //[SaveAttribute]
        //[JsonConverter(typeof(JsonCommentConverter), "Разная ли высота рёбер")]
        ///// <summary>
        ///// Разная ли высота рёбер
        ///// </summary>
        //public bool DiferentHight
        //{
        //    get => this._diferentHight;
        //    set
        //    {
        //        Set(ref this._diferentHight, value);
        //        if (!value && this.RibsCollection?.Count > 0)
        //        {
        //            for (int i = 0; i < this.RibsCollection.Count; i++)
        //            {
        //                this.RibsCollection[i].HightLeft = this.RibsHeight;
        //                this.RibsCollection[i].HightRight = this.RibsHeight;
        //            }
        //        }
        //        this.OnChangeProperty();
        //    }
        //}

        //[JsonIgnore]
        //[SaveAttribute]
        //[JsonConverter(typeof(JsonCommentConverter), "Разная ли высота рёбер с 2-х сторон")]
        ///// <summary>
        ///// Разная ли высота рёбер с 2-х сторон
        ///// </summary>
        //public bool DiferentHightLeftToRight
        //{
        //    get => this._diferentHightLeftToRight;
        //    set
        //    {
        //        Set(ref this._diferentHightLeftToRight, value);
        //        if (!value && this.RibsCollection?.Count > 0)
        //            foreach (var rib in this.RibsCollection)
        //            {
        //                rib.OnChangeHightEvent(rib, null);
        //            }
        //        this.OnChangeProperty();
        //    }
        //}

        [JsonIgnore]
        [SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Высота рёбер (вертикальной стенки)")]
        /// <summary>
        /// Высота ребра
        /// </summary>
        public decimal RibsHeight
        {
            get => this._ribsHeight;
            set
            {
                this._ribsHeight = value;
                this.OnChangeProperty(nameof(this.RibsHeight));
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Толщина рёбер")]
        /// <summary>
        /// Толщина ребра
        /// </summary>
        public decimal RibsThickness
        {
            get => this._ribsThickness;
            set
            {
                this._ribsThickness = value;
                this.OnChangeProperty(nameof(this.RibsThickness));
            }
        }

        [JsonProperty("wall_count")]
        [JsonConverter(typeof(JsonCommentConverter), "Кол-во рёбер")]
        /// <summary>
        /// Количество ребер
        /// </summary>
        public int RibsCount
        {
            get => this._ribCount;
            set
            {
                if (value < MIN_RIB_COUNT)
                    return;

                this._ribCount = value;

                this.ChangeRibCollection();
                this.ChangeWeldingSchema();
                this.OnChangeProperty(nameof(this.RibsCount));
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Расстояние по ширине до осевой линии первого ребра")]
        /// <summary>
        /// Поперечное расстояние по ширине до осевой линии первого ребра
        /// </summary>
        public decimal DistanceToFirstRib
        {
            get => this._distanceToFirstRib;
            set
            {
                this._distanceToFirstRib = value;
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Расстояние между осевыми линиями рёбер")]
        /// <summary>
        /// Поперечное расстояние между осевыми линиями рёбер
        /// </summary>
        public decimal DistanceBetweenRibs
        {
            get => this._distanceBetweenRibs;
            set
            {
                this._distanceBetweenRibs = value;
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Продольное расстояние до ребра по левому краю")]
        /// <summary>
        /// Продольное расстояние до ребер по левому краю
        /// </summary>
        public decimal IdentToLeft
        {
            get => this._identToLeft;
            set
            {
                this._identToLeft = value;
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Продольное расстояние до ребра по правому краю")]
        /// <summary>
        /// Продольное расстояние до ребер по правому краю
        /// </summary>
        public decimal IdentToRight
        {
            get => this._identToRight;
            set
            {
                this._identToRight = value;
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Роспуск слева")]
        /// <summary>
        /// Отступ шва от левого края ребер (роспуск, выкружка)
        /// </summary>
        public decimal DissolutionLeft
        {
            get => this._dissolutionLeft;
            set
            {
                this._dissolutionLeft = value;
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Роспуск справа")]
        /// <summary>
        /// Отступ шва от правого края ребер (роспуск, выкружка)
        /// </summary>
        public decimal DissolutionRight
        {
            get => this._dissolutionRight;
            set
            {
                this._dissolutionRight = value;
                this.OnChangeProperty();
            }
        }

        #region Weld's Properties
        
        [JsonProperty("d_W2")]
        /// <summary>
        /// Коллекция рёбер
        /// </summary>
        public FullyObservableCollection<Rib> RibsCollection
        {
            get => this._ribsCollection;
            private set
            {
                if(this._ribsCollection != null)
                {
                    this._ribsCollection.ItemPropertyChanged -= (s, e) => this.OnChangeProperty();

                    foreach (var rib in this._ribsCollection)
                        rib.ChangeDistanceLeft -= HandleChangeDistanceLeft;
                }

                this._ribsCollection = value;

                if (this._ribsCollection != null)
                {
                    this._ribsCollection.ItemPropertyChanged += (s, e) => this.OnChangeProperty();

                    foreach (var rib in this._ribsCollection)
                        rib.ChangeDistanceLeft += HandleChangeDistanceLeft;
                }
            }
        }

        #endregion Weld's Properties
        
        [JsonIgnore]
        /// <summary>
        /// Изображение рёбер плиты
        /// </summary>
        public override sealed BitmapImage RebraImage
        {
            get => Equals(this.ScoseType, ScoseTypes.Rect) ? (this._rebraImage = JoinRebra(GetStartRebraImage(), GetBodyRebraImage(), GetEndRebraImage())) : null;
            set => this._rebraImage = value;
        }

        [JsonIgnore]
        /// <summary>
        /// Общее изображение плиты
        /// </summary>
        public override sealed BitmapImage GenericImage { get => this.GetGenericImage(); set => this._plitaImage = value; }

        #endregion

        #region Constructor

        public Plita(DetalType type) : base(type)
        {
            this.ChangePropertyEvent += this.HandleChangeProperty;
            this.SelectDefoultPlateProperties();

            //this.PlateWidth = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).Width;
            //this.PlateBevelToLeft = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).BevelToStart;
            //this.PlateBevelToRight = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).BevelToEnd;
            //this.DistanceForWelding = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).DistanceForWelding;
            //this.DistanceForSearch = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).DistanceForSearch;

            //this.RibsCollection = this.FillRibsCollection();
            //this.WeldingSchema = this.FillWeldingSchema();
        }

        #endregion

        #region Public functions

        //public async Task OnChange() => this.Change;

        //public async Task OnChange() => await base.OnChange(this.Change);

        #endregion

        #region Private functions

        #region Handle

        /// <summary>
        /// Делегат изменения расстояния до следующего ребра слева
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleChangeDistanceLeft(object sender, EventArgs e)
        {
            if (sender == null || !(sender is Rib rib))
                return;

            if (this.ParalleleRibs)
                rib.DistanceRight = rib.DistanceLeft;
        }

        /// <summary>
        /// Делегат изменения свойства детали
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleChangeProperty(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.DistanceToFirstRib):
                    if (this.RibsCollection?.Count == 0)
                        return;

                    this.RibsCollection[0].DistanceLeft = base.DistanceToFirstRib;
                    this.RibsCollection[0].DistanceRight = base.DistanceToFirstRib;
                    break;

                case nameof(this.DistanceBetweenRibs):
                    if (this.RibsCollection?.Count == 0)
                        return;

                    for (int i = 1; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].DistanceLeft = base.DistanceBetweenRibs;
                        this.RibsCollection[i].DistanceRight = base.DistanceBetweenRibs;
                    }
                    break;

                case nameof(this.DissolutionRight):
                    if (this.RibsCollection?.Count == 0)
                        return;

                    for (int i = 0; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].DissolutionRight = base.DissolutionRight;
                    }
                    break;

                case nameof(this.DissolutionLeft):
                    if (this.RibsCollection?.Count == 0)
                        return;

                    for (int i = 0; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].DissolutionLeft = base.DissolutionLeft;
                    }
                    break;

                case nameof(this.IdentToRight):
                    if (this.RibsCollection?.Count == 0)
                        return;

                    for (int i = 0; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].IdentToRight = base.IdentToRight;
                    }
                    break;
                    
                case nameof(this.IdentToLeft):
                    if (this.RibsCollection?.Count == 0)
                        return;

                    for (int i = 0; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].IdentToLeft = base.IdentToLeft;
                    }
                    break;

                case nameof(this.RibsHeight):
                    //if (this.RibsCollection?.Count > 0)
                    //{
                    //    for (int i = 0; i < this.RibsCollection.Count; i++)
                    //    {
                    //        this.RibsCollection[i].HightLeft = base.RibsHeight;
                    //        this.RibsCollection[i].HightRight = base.RibsHeight;
                    //    }
                    //}
                    break;
            }
        }

        #endregion Handle

        /// <summary>
        /// Выгрузка стандартных для <see cref="ForRobot.Model.Detals.Plita"/> параметров
        /// </summary>
        private void SelectDefoultPlateProperties()
        {
            ForRobot.Libr.ConfigurationProperties.PlateConfigurationSection plateConfig = ConfigurationManager.GetSection("plate") as ForRobot.Libr.ConfigurationProperties.PlateConfigurationSection;
            this.ReverseDeflection = plateConfig.ReverseDeflection;
            this.PlateWidth = plateConfig.PlateWidth;
            this.PlateLength = plateConfig.PlateLength;
            this.PlateThickness = plateConfig.PlateThickness;
            this.PlateBevelToLeft = plateConfig.PlateBevelToLeft;
            this.PlateBevelToRight = plateConfig.PlateBevelToRight;

            this.RibsHeight = plateConfig.RibsHeight;
            this.RibsThickness = plateConfig.RibsThickness;
            this.RibsCount = plateConfig.RibsCount;
            this.DistanceToFirstRib = plateConfig.DistanceToFirstRib;
            this.DistanceBetweenRibs = plateConfig.DistanceBetweenRibs;
            this.IdentToLeft = plateConfig.IdentToLeft;
            this.IdentToRight = plateConfig.IdentToRight;
            this.DissolutionLeft = plateConfig.DissolutionLeft;
            this.DissolutionRight = plateConfig.DissolutionRight;

            //this.PlateLength = PlitaConfig.Long;
            //this.PlateWidth = PlitaConfig.Width;
            //this.RibsHeight = PlitaConfig.Hight;
            //this.DistanceToFirstRib = PlitaConfig.DistanceToFirstRib;
            //this.DistanceBetweenRibs = PlitaConfig.DistanceBetweenRibs;
            //this.IdentToLeft = PlitaConfig.DistanceToStart;
            //this.IdentToRight = PlitaConfig.DistanceToEnd;
            //this.DissolutionLeft = PlitaConfig.DissolutionStart;
            //this.DissolutionRight = PlitaConfig.DissolutionEnd;
            //this.PlateThickness = PlitaConfig.ThicknessPlita;
            //this.RibsThickness = PlitaConfig.ThicknessRebro;
            //this.SearchOffsetStart = PlitaConfig.SearchOffsetStart;
            //this.SearchOffsetEnd = PlitaConfig.SearchOffsetEnd;
            //this.SeamsOverlap = PlitaConfig.SeamsOverlap;
            //this.TechOffsetSeamStart = PlitaConfig.TechOffsetSeamStart;
            //this.TechOffsetSeamEnd = PlitaConfig.TechOffsetSeamEnd;
            //this.ReverseDeflection = PlitaConfig.ReverseDeflection;
            //this.WildingSpead = PlitaConfig.WildingSpead;
            //this.ProgramNom = PlitaConfig.ProgramNom;
            //this.RibsCount = PlitaConfig.SumReber;
        }

        /// <summary>
        /// Заполнение коллекции расстояний
        /// </summary>
        /// <returns></returns>
        private FullyObservableCollection<Rib> FillRibsCollection()
        {
            Rib rib;
            List<Rib> ribsList = new List<Rib>();

            for (int i = 0; i < this.RibsCount; i++)
            {
                rib = new Rib()
                {
                    IdentToLeft = this.IdentToLeft,
                    IdentToRight = this.IdentToRight,
                    DissolutionLeft = this.DissolutionLeft,
                    DissolutionRight = this.DissolutionRight
                    //HightLeft = this.RibsHeight,
                    //HightRight = this.RibsHeight
                };

                if (i == 0)
                {
                    rib.DistanceLeft = this.DistanceToFirstRib;
                    rib.DistanceRight = this.DistanceToFirstRib;
                }
                else
                {
                    rib.DistanceLeft = this.DistanceBetweenRibs;
                    rib.DistanceRight = this.DistanceBetweenRibs;
                }

                ribsList.Add(rib);
            }
            return new FullyObservableCollection<Rib>(ribsList);
        }

        //private FullyObservableCollection<WeldingSchemas.SchemaRib> FillWeldingSchema()
        //{
        //    if (string.IsNullOrEmpty(this.SelectedWeldingSchema))
        //        return null;

        //    FullyObservableCollection<WeldingSchemas.SchemaRib> schema = ForRobot.Model.Detals.WeldingSchemas.BuildingSchema(ForRobot.Model.Detals.WeldingSchemas.GetSchemaType(this.SelectedWeldingSchema), base.RibsCount);
        //    schema.ItemPropertyChanged += (s, e) =>
        //    {
        //        if (this.SelectedWeldingSchema != WeldingSchemas.GetDescription(WeldingSchemas.SchemasTypes.Edit))
        //            this.SelectedWeldingSchema = ForRobot.Model.Detals.WeldingSchemas.GetDescription(WeldingSchemas.SchemasTypes.Edit);

        //        this.OnChangeProperty(nameof(this.WeldingSchema));
        //    };
        //    return schema;
        //}

        private void ChangeRibCollection()
        {
            if (this.RibsCollection == null || this.RibsCollection?.Count == 0)
                return;

            if (this.RibsCount > this.RibsCollection.Count)
                for (int i = this.RibsCollection.Count; i < this.RibsCount; i++)
                {
                    this.RibsCollection.Add(this.RibsCollection.Last<Rib>().Clone() as Rib);
                }
            else
            {
                for(int i = this.RibsCollection.Count - 1; i >= this.RibsCount; i--)
                    this.RibsCollection.RemoveAt(i);
            }
        }

        //private void ChangeWeldingSchema()
        //{
        //    if (this.WeldingSchema == null || this.WeldingSchema?.Count == 0)
        //        return;

        //    if (this.RibsCount > this.WeldingSchema.Count)
        //        for (int i = this.WeldingSchema.Count; i < this.RibsCount; i++)
        //            this.WeldingSchema.Add(new WeldingSchemas.SchemaRib());
        //    else
        //    {
        //        for (int i = this.WeldingSchema.Count - 1; i >= this.RibsCount; i--)
        //            this.WeldingSchema.RemoveAt(i);
        //    }

        //    this.SelectedWeldingSchema = this.SelectedWeldingSchema;
        //}

        #endregion Private functions

        #region Public functions

        /// <summary>
        /// Десериализация класса <see cref="Plita"/> из JSON-строки
        /// </summary>
        /// <param name="sJsonString">JSON-строка</param>
        /// <returns></returns>
        public new Plita DeserializeDetal(string sJsonString = null)
        {
            if (string.IsNullOrEmpty(sJsonString))
                return new Plita(Detals.DetalType.Plita);
            else
                return JsonConvert.DeserializeObject<Plita>(JObject.Parse(sJsonString, this._jsonLoadSettings).ToString(), this._jsonDeserializerSettings);
        }

        public FullyObservableCollection<Rib> SetRibsCollection(FullyObservableCollection<Rib> collection) => this.RibsCollection = collection;

        #endregion Public functions

        #region Implementations of IDisposable

        private volatile int _disposed;

        ~Plita() => Dispose(false);

        public new void Dispose() => this.Dispose(true);

        public new void Dispose(bool disposing)
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
