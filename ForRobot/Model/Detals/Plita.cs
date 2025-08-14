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
using ForRobot.Libr.Converters;
using ForRobot.Libr.ConfigurationProperties;

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

        private decimal _bevelToLeft;
        private decimal _bevelToRight;
        private decimal _distanceForSearch;
        private decimal _distanceForWelding;

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

        [JsonIgnore]
        [SaveAttribute]
        /// <summary>
        /// Выбранная схема сварки рёбер
        /// </summary>
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
                            this.RibsCollection[i].DistanceLeft = this.DistanceToFirst;
                            continue;
                        }
                        this.RibsCollection[i].DistanceLeft = this.DistanceBetween;
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
                        this.RibsCollection[i].OnChangeDistanceEvent(this.RibsCollection[i], null);
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
        //                this.RibsCollection[i].HightLeft = this.RibHeight;
        //                this.RibsCollection[i].HightRight = this.RibHeight;
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

        [JsonProperty("l")]
        [JsonConverter(typeof(JsonCommentConverter), "Длина настила")]
        /// <summary>
        /// Длина настила
        /// </summary>
        public override decimal PlateLength
        {
            get => base.PlateLength;
            set
            {
                base.PlateLength = value;
                this.OnChangeProperty(nameof(this.PlateLength));
            }
        }

        [JsonProperty("w")]
        [JsonConverter(typeof(JsonCommentConverter), "Ширина настила")]
        /// <summary>
        /// Ширина настила
        /// </summary>
        public override decimal PlateWidth
        {
            get => base.PlateWidth;
            set
            {
                base.PlateWidth = value;
                this.OnChangeProperty(nameof(this.PlateWidth));
            }
        }

        [JsonProperty("t_p")]
        [JsonConverter(typeof(JsonCommentConverter), "Толщина настила")]
        /// <summary>
        /// Толщина настила
        /// </summary>
        public override decimal PlateThickness
        {
            get => base.PlateThickness;
            set
            {
                base.PlateThickness = value;
                this.OnChangeProperty(nameof(this.PlateThickness));
            }
        }

        [JsonProperty("h")]
        [JsonConverter(typeof(JsonCommentConverter), "Высота ребра")]
        /// <summary>
        /// Высота ребра
        /// </summary>
        public override decimal RibHeight
        {
            get => base.RibHeight;
            set
            {
                base.RibHeight = value;
                //if (this.RibsCollection?.Count > 0)
                //{
                //    for (int i = 0; i < this.RibsCollection.Count; i++)
                //    {
                //        this.RibsCollection[i].HightLeft = base.RibHeight;
                //        this.RibsCollection[i].HightRight = base.RibHeight;
                //    }
                //}
                this.OnChangeProperty(nameof(this.RibHeight));
            }
        }

        [JsonIgnore]
        [JsonProperty("d_w1"), SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Расстояние по ширине до осевой линии первого ребра")]
        /// <summary>
        /// Расстояние по ширине до осевой линии первого ребра
        /// </summary>
        public override decimal DistanceToFirst
        {
            get => base.DistanceToFirst;
            set
            {
                base.DistanceToFirst = value;
                if (this.RibsCollection?.Count > 0)
                {
                    this.RibsCollection[0].DistanceLeft = base.DistanceToFirst;
                    this.RibsCollection[0].DistanceRight = base.DistanceToFirst;
                }
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        [JsonProperty("d_w2"), SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Расстояние между осевыми линиями рёбер")]
        /// <summary>
        /// Расстояние между осевыми линиями рёбер
        /// </summary>
        public override decimal DistanceBetween
        {
            get => base.DistanceBetween;
            set
            {
                base.DistanceBetween = value;
                if (this.RibsCollection?.Count > 0)
                    for (int i = 1; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].DistanceLeft = base.DistanceBetween;
                        this.RibsCollection[i].DistanceRight = base.DistanceBetween;
                    }
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        [JsonProperty("d_l1"), SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Расстояние торца ребра слева")]
        /// <summary>
        /// Расстояние торца ребра слева
        /// </summary>
        public override decimal IdentToLeft
        {
            get => base.IdentToLeft;
            set
            {
                base.IdentToLeft = value;
                if (this.RibsCollection?.Count > 0)
                    for (int i = 0; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].IdentToLeft = base.IdentToLeft;
                    }
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        [JsonProperty("d_l2"), SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Расстояние торца справа")]
        /// <summary>
        /// Расстояние торца ребра справа
        /// </summary>
        public override decimal IdentToRight
        {
            get => base.IdentToRight;
            set
            {
                base.IdentToRight = value;
                if (this.RibsCollection?.Count > 0)
                    for (int i = 0; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].IdentToRight = base.IdentToRight;
                    }
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        [JsonProperty("l_r1"), SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Роспуск слева")]
        /// <summary>
        /// Роспуск в начале
        /// </summary>
        public override decimal DissolutionLeft
        {
            get => base.DissolutionLeft;
            set
            {
                base.DissolutionLeft = value;
                if (this.RibsCollection?.Count > 0)
                    for (int i = 0; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].DissolutionLeft = base.DissolutionLeft;
                    }
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        [JsonProperty("l_r2"), SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Роспуск справа")]
        /// <summary>
        /// Роспуск в конце
        /// </summary>
        public override decimal DissolutionRight
        {
            get => base.DissolutionRight;
            set
            {
                base.DissolutionRight = value;
                if (this.RibsCollection?.Count > 0)
                    for (int i = 0; i < this.RibsCollection.Count; i++)
                    {
                        this.RibsCollection[i].DissolutionRight = base.DissolutionRight;
                    }
                this.OnChangeProperty();
            }
        }

        [JsonProperty("t_r")]
        [JsonConverter(typeof(JsonCommentConverter), "Толщина рёбер")]
        /// <summary>
        /// Толщина ребра
        /// </summary>
        public override decimal RibThickness
        {
            get => base.RibThickness;
            set
            {
                base.RibThickness = value;
                this.OnChangeProperty(nameof(this.RibThickness));
            }
        }

        [JsonProperty("d_s1")]
        [JsonConverter(typeof(JsonCommentConverter), "Отступ поиска в начале")]
        /// <summary>
        /// Отступ поиска в начале
        /// </summary>
        public override decimal SearchOffsetStart
        {
            get => base.SearchOffsetStart;
            set
            {
                base.SearchOffsetStart = value;
                this.OnChangeProperty(nameof(this.SearchOffsetStart));
            }
        }

        [JsonProperty("d_s2")]
        [JsonConverter(typeof(JsonCommentConverter), "Отступ поиска в конце")]
        /// <summary>
        /// Отступ поиска в конце
        /// </summary>
        public override decimal SearchOffsetEnd
        {
            get => base.SearchOffsetEnd;
            set
            {
                base.SearchOffsetEnd = value;
                this.OnChangeProperty(nameof(this.SearchOffsetEnd));
            }
        }

        [JsonProperty("l_overlap")]
        [JsonConverter(typeof(JsonCommentConverter), "Перекрытие швов")]
        /// <summary>
        /// Перекрытие швов
        /// </summary>
        public override decimal SeamsOverlap
        {
            get => base.SeamsOverlap;
            set
            {
                base.SeamsOverlap = value;
                this.OnChangeProperty(nameof(this.SeamsOverlap));
            }
        }

        [JsonProperty("d_t1")]
        [JsonConverter(typeof(JsonCommentConverter), "Технологический отступ начала шва")]
        /// <summary>
        /// Технологический отступ начала шва
        /// </summary>
        public override decimal TechOffsetSeamStart
        {
            get => base.TechOffsetSeamStart;
            set
            {
                base.TechOffsetSeamStart = value;
                this.OnChangeProperty(nameof(this.TechOffsetSeamStart));
            }
        }

        [JsonProperty("d_t2")]
        [JsonConverter(typeof(JsonCommentConverter), "Технологический отступ конца шва")]
        /// <summary>
        /// Технологический отступ конца шва
        /// </summary>
        public override decimal TechOffsetSeamEnd
        {
            get => base.TechOffsetSeamEnd;
            set
            {
                base.TechOffsetSeamEnd = value;
                this.OnChangeProperty(nameof(this.TechOffsetSeamEnd));
            }
        }

        [JsonProperty("d_b1")]
        [JsonConverter(typeof(JsonCommentConverter), "Скос слева")]
        /// <summary>
        /// Скос слева
        /// </summary>
        public decimal BevelToLeft
        {
            get => this._bevelToLeft;
            set
            {
                this._bevelToLeft = value;
                this.OnChangeProperty(nameof(this.BevelToLeft));
            }
        }

        [JsonProperty("d_b2")]
        [JsonConverter(typeof(JsonCommentConverter), "Скос справа")]
        /// <summary>
        /// Скос справа
        /// </summary>
        public decimal BevelToRight
        {
            get => this._bevelToRight;
            set
            {
                this._bevelToRight = value;
                this.OnChangeProperty(nameof(this.BevelToRight));
            }
        }

        [JsonProperty("velocity")]
        [JsonConverter(typeof(JsonCommentConverter), "Скорость сварки")]
        /// <summary>
        /// Скорость сварки
        /// </summary>
        public override int WildingSpead
        {
            get => base.WildingSpead;
            set
            {
                base.WildingSpead = value;
                this.OnChangeProperty(nameof(this.WildingSpead));
            }
        }

        [JsonProperty("job")]
        [JsonConverter(typeof(JsonCommentConverter), "Номер сварочной программы")]
        /// <summary>
        /// Номер сварочной программы
        /// </summary>
        public override int ProgramNom
        {
            get => base.ProgramNom;
            set
            {
                base.ProgramNom = value;
                this.OnChangeProperty(nameof(this.ProgramNom));
            }
        }

        [JsonProperty("weld_gantry_radius")]
        [JsonConverter(typeof(JsonCommentConverter), "Дистанция до позиционера для сварки")]
        /// <summary>
        /// Дистанция до позиционера для сварки
        /// </summary>
        public decimal DistanceForWelding
        {
            get => this._distanceForWelding;
            set
            {
                this._distanceForWelding = value;
                this.OnChangeProperty(nameof(this.DistanceForWelding));
            }
        }

        [JsonProperty("search_gantry_radius")]
        [JsonConverter(typeof(JsonCommentConverter), "Дистанция до позиционера для поиска")]
        /// <summary>
        /// Дистанция до позиционера для поиска
        /// </summary>
        public decimal DistanceForSearch
        {
            get => this._distanceForSearch;
            set
            {
                this._distanceForSearch = value;
                this.OnChangeProperty(nameof(this.DistanceForSearch));
            }
        }

        [JsonProperty("base_displace")]
        /// <summary>
        /// Смещение детали от 0 точки по осям XYZ
        /// </summary>
        public override decimal[] XYZOffset
        {
            get => base.XYZOffset;
            set
            {
                base.XYZOffset = value;
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Смещение детали от 0 точки по X
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
        /// Смещение детали от 0 точки по Y
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
        /// Смещение детали от 0 точки по Z
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

        [JsonProperty("reverse_deflection")]
        [JsonConverter(typeof(JsonCommentConverter), "Обратный прогиб")]
        /// <summary>
        /// Обратный прогиб
        /// </summary>
        public override decimal ReverseDeflection
        {
            get => base.ReverseDeflection;
            set
            {
                base.ReverseDeflection = value;
                this.OnChangeProperty(nameof(this.ReverseDeflection));
            }
        }

        [JsonProperty("edge_count")]
        [JsonConverter(typeof(JsonCommentConverter), "Кол-во рёбер")]
        /// <summary>
        /// Количество ребер
        /// </summary>
        public override int RibCount
        {
            get => base.RibCount;
            set
            {
                if (value < MIN_RIB_COUNT)
                    return;

                base.RibCount = value;

                if (this.RibsCollection != null)
                    this.ChangeRibCollection(this.RibsCollection, RibCount);

                if (this.WeldingSchema != null)
                    this.ChangeWeldingSchema(this.WeldingSchema, this.SelectedWeldingSchema, RibCount);

                this.OnChangeProperty(nameof(this.RibCount));
            }
        }
        
        [JsonProperty("d_W2")]
        /// <summary>
        /// Коллекция рёбер
        /// </summary>
        public FullyObservableCollection<Rib> RibsCollection
        {
            get => this._ribsCollection;
            private set
            {
                this._ribsCollection = value;

                foreach (var rib in this._ribsCollection)
                    rib.ChangeDistance += (s, e) =>
                    {
                        if (this.ParalleleRibs)
                            (s as Rib).DistanceRight = (s as Rib).DistanceLeft;
                    };
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        public FullyObservableCollection<WeldingSchemas.SchemaRib> WeldingSchema { get; private set; }

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

        #region Constructors

        //public Plita() { }

        public Plita(DetalType type) : base(type)
        {
            this.PlateWidth = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).Width;
            this.BevelToLeft = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).BevelToStart;
            this.BevelToRight = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).BevelToEnd;
            this.DistanceForWelding = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).DistanceForWelding;
            this.DistanceForSearch = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).DistanceForSearch;

            this.RibsCollection = this.FillRibsCollection();
            this.WeldingSchema = this.FillWeldingSchema();
        }

        #endregion

        #region Public functions

        //public async Task OnChange() => this.Change;

        //public async Task OnChange() => await base.OnChange(this.Change);

        #endregion

        #region Private functions

        /// <summary>
        /// Заполнение коллекции расстояний
        /// </summary>
        /// <returns></returns>
        private FullyObservableCollection<Rib> FillRibsCollection()
        {
            Rib rib;
            List<Rib> ribsList = new List<Rib>();

            for (int i = 0; i < this.RibCount; i++)
            {
                rib = new Rib()
                {
                    IdentToLeft = this.IdentToLeft,
                    IdentToRight = this.IdentToRight,
                    DissolutionLeft = this.DissolutionLeft,
                    DissolutionRight = this.DissolutionRight
                    //HightLeft = this.RibHeight,
                    //HightRight = this.RibHeight
                };

                if (i == 0)
                {
                    rib.DistanceLeft = this.DistanceToFirst;
                    rib.DistanceRight = this.DistanceToFirst;
                }
                else
                {
                    rib.DistanceLeft = this.DistanceBetween;
                    rib.DistanceRight = this.DistanceBetween;
                }

                ribsList.Add(rib);
            }
            FullyObservableCollection<Rib> ribs = new FullyObservableCollection<Rib>(ribsList);
            ribs.ItemPropertyChanged += (s, e) => this.OnChangeProperty();
            return ribs;
        }

        private FullyObservableCollection<WeldingSchemas.SchemaRib> FillWeldingSchema()
        {
            if (string.IsNullOrEmpty(this.SelectedWeldingSchema))
                return null;

            FullyObservableCollection<WeldingSchemas.SchemaRib> schema = ForRobot.Model.Detals.WeldingSchemas.BuildingSchema(ForRobot.Model.Detals.WeldingSchemas.GetSchemaType(this.SelectedWeldingSchema), base.RibCount);
            schema.ItemPropertyChanged += (a, e) =>
            {
                if (this.SelectedWeldingSchema != WeldingSchemas.GetDescription(WeldingSchemas.SchemasTypes.Edit))
                    this.SelectedWeldingSchema = ForRobot.Model.Detals.WeldingSchemas.GetDescription(WeldingSchemas.SchemasTypes.Edit);

                this.OnChangeProperty();
            };
            //foreach (var rib in schema)
            //{
            //    rib.Change += (o, e) => {
            //        if (this.SelectedWeldingSchema != ForRobot.Model.Detals.WeldingSchemas.GetDescription(ForRobot.Model.Detals.WeldingSchemas.SchemasTypes.Edit))
            //            this.SelectedWeldingSchema = ForRobot.Model.Detals.WeldingSchemas.GetDescription(ForRobot.Model.Detals.WeldingSchemas.SchemasTypes.Edit);
            //        this.OnChangeProperty();
            //    };
            //}
            return schema;
        }

        private void ChangeRibCollection(FullyObservableCollection<Rib> collection, int ribCount)
        {
            if (collection.Count == 0)
                return;

            if (ribCount > collection.Count)
                for (int i = collection.Count; i < ribCount; i++)
                {
                    collection.Add(collection.Last<Rib>().Clone() as Rib);
                }
            else
            {
                for(int i = collection.Count - 1; i >= ribCount; i--)
                    collection.RemoveAt(i);
            }
        }

        private void ChangeWeldingSchema(FullyObservableCollection<WeldingSchemas.SchemaRib> collection, string selectedWeldingSchema, int ribCount)
        {
            if (collection?.Count == 0)
                return;

            if (ribCount > collection.Count)
                for (int i = collection.Count; i < ribCount; i++)
                    collection.Add(new WeldingSchemas.SchemaRib());
            else
            {
                for (int i = collection.Count - 1; i >= ribCount; i--)
                    collection.RemoveAt(i);
            }

            this.SelectedWeldingSchema = selectedWeldingSchema;
        }

        #region Рёбра

        /// <summary>
        /// Отрисовка первого ребра
        /// </summary>
        /// <returns></returns>
        private Bitmap GetStartRebraImage()
        {
            Bitmap image = new Bitmap(300, 310);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.Clear(Color.White);
                Pen pen = new Pen(Color.Black, 7);
                PointF[] points =
                {
                 new PointF(135,  177),
                 new PointF(146, 177),
                 new PointF(146,  61),
                 new PointF(136, 61)
                };

                graphics.DrawLines(pen, points);
                graphics.DrawLine(pen, new PointF(145, 191), new PointF(145, 294));

                // Плита
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(148, 180, 151, 11));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(171, 171, 171)), new Rectangle(149, 182, 151, 7));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(135, 135, 135)), new Rectangle(149, 184, 151, 5));

                // Верхняя стрелка
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(149, 58, 151, 3));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(157, 56, 21, 7));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(178, 54, 17, 11));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(195, 51, 6, 17));

                //// Нижняя стрелка
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(149, 258, 151, 3));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(157, 256, 21, 7));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(178, 254, 17, 11));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(195, 251, 6, 17));

                // Ребро
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(284, 58, 17, 125));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(85, 255, 66)), new Rectangle(287, 61, 11, 122));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(36, 110, 27)), new Rectangle(291, 61, 7, 122));
                graphics.DrawEllipse(new Pen(Color.Black, 3), 284, 180, 17, 6);

                //// Текст
                //Font font = new Font("Lucida Console", image.PlateWidth / 14, System.Drawing.FontStyle.Regular);
                //StringFormat stringFormat = new StringFormat(StringFormatFlags.DirectionVertical);
                //graphics.DrawString(IndentionStart.ToString(), font, new SolidBrush(Color.Black), 206, 29);
                //graphics.DrawString(RibHeight.ToString(), font, new SolidBrush(Color.Black), new PointF(102, 126 - RibHeight.ToString().Length * (font.Size - 8)), stringFormat);
            }
            return image;
        }

        /// <summary>
        /// Отрисовка всех рёбер, кроме первого и последнего
        /// </summary>
        /// <returns></returns>
        private Bitmap GetBodyRebraImage()
        {
            Bitmap image = new Bitmap(150, 310);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.Clear(Color.White);

                // Плита
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 180, 150, 11));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(171, 171, 171)), new Rectangle(0, 182, 150, 7));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(135, 135, 135)), new Rectangle(0, 184, 150, 5));

                //// Нижняя стрелка
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 258, 150, 3));

                // Ребро
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(135, 58, 17, 125));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(85, 255, 66)), new Rectangle(137, 61, 11, 122));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(36, 110, 27)), new Rectangle(141, 61, 7, 122));
                graphics.DrawEllipse(new Pen(Color.Black, 3), 135, 180, 16, 6);
            }
            return image;
        }

        /// <summary>
        /// Отрисовка последнего ребра
        /// </summary>
        /// <returns></returns>
        private Bitmap GetEndRebraImage()
        {
            Bitmap image = new Bitmap(450, 310);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.Clear(Color.White);
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(300, 47, 7, 250));

                // Плита
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 180, 301, 11));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(171, 171, 171)), new Rectangle(0, 182, 300, 7));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(135, 135, 135)), new Rectangle(0, 184, 300, 5));

                //// Верхняя стрелка
                //if (RibCount == 1)
                //{
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 58, 301, 3));

                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(274, 56, 21, 7));
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(257, 54, 17, 11));
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(251, 51, 6, 17));

                //}
                //else
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(150, 58, 151, 3));

                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(274, 56, 21, 7));
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(257, 54, 17, 11));
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(251, 51, 6, 17));

                // Нижняя стрелка
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 258, 301, 3));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(274, 256, 21, 7));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(257, 254, 17, 11));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(251, 251, 6, 17));

                //if (RibCount == 1)
                //{
                //    // Нижняя стрелка в начале
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(149, 258, 151, 3));
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(157, 256, 21, 7));
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(178, 254, 17, 11));
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(195, 251, 6, 17));
                //}

                // Ребро
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(150, 58, 17, 125));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(85, 255, 66)), new Rectangle(154, 61, 11, 122));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(36, 110, 27)), new Rectangle(158, 61, 7, 122));
                graphics.DrawEllipse(new Pen(Color.Black, 3), 150, 180, 16, 6);

                // Толщина
                graphics.DrawRectangle(new Pen(Color.FromArgb(27, 214, 242), 4), 309, 180, 12, 12);
                graphics.DrawLine(new Pen(Color.FromArgb(27, 214, 242), 4), 309, 192, 442, 192);

                //// Текст
                //Font font = new Font("Lucida Console", image.PlateWidth / 22, System.Drawing.FontStyle.Regular);
                //graphics.DrawString(IndentionEnd.ToString(), font, new SolidBrush(Color.Black), 205, 15);
                //graphics.DrawString(PlateThickness.ToString(), font, new SolidBrush(Color.Black), 334, 156);
            }
            return image;
        }

        /// <summary>
        /// Добавление дистанции междурёбрами и ширены плиты
        /// </summary>
        /// <param name="oldImage"></param>
        /// <returns></returns>
        private Bitmap PaintDistanceBetween_And_WightImage(Bitmap oldImage)
        {
            Bitmap image = new Bitmap(oldImage);
            //using (Graphics graphics = Graphics.FromImage(image))
            //{
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 59, 135, 3));

            //    // Стрелка левая
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(3, 58, 4, 5));
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(7, 57, 5, 7));
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(12, 56, 4, 9));
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(16, 54, 4, 13));

            //    // Стрелка правая
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(128, 58, 4, 5));
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(123, 57, 5, 7));
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(119, 56, 4, 9));
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(115, 54, 4, 13));

            //    // Текст
            //    Font font = new Font("Lucida Console", image.PlateWidth / 8, System.Drawing.FontStyle.Regular);
            //    graphics.DrawString(DistanceBetween.ToString(), font, new SolidBrush(Color.Black), 51, 33);
            //    graphics.DrawString(PlateLength.ToString(), font, new SolidBrush(Color.Black), 33, 234);
            //}
            return image;
        }

        private Bitmap GetArrowsPlita(Bitmap bitmap)
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                Pen pen = new Pen(Color.Black, 2);

                #region Стрелка ширины

                PointF[] point =
                {
                    new PointF(149, 258),
                    new PointF(bitmap.Width - 145, 258)
                };
                graphics.DrawLines(pen, point);

                point = new PointF[]
                {
                    new PointF(149, 258),
                    new PointF(149, 259),
                    new PointF(157, 259),
                    new PointF(157, 261),
                    new PointF(175, 261),
                    new PointF(175, 264),
                    new PointF(187, 264),
                    new PointF(187, 266),
                    new PointF(194, 266),
                    new PointF(194, 251),
                    new PointF(187, 251),
                    new PointF(187, 253),
                    new PointF(175, 253),
                    new PointF(175, 256),
                    new PointF(157, 256),
                    new PointF(157, 258),
                    new PointF(149, 258)
                };
                graphics.FillPolygon(new SolidBrush(Color.Black), point);

                point = new PointF[]
{
                    new PointF(300 + 150 * (RibCount - 2) + 256, 251),
                    new PointF(300 + 150 * (RibCount - 2) + 256, 266),
                    new PointF(300 + 150 * (RibCount - 2) + 262, 266),
                    new PointF(300 + 150 * (RibCount - 2) + 262, 264),
                    new PointF(300 + 150 * (RibCount - 2) + 275, 264),
                    new PointF(300 + 150 * (RibCount - 2) + 275, 261),
                    new PointF(300 + 150 * (RibCount - 2) + 293, 261),
                    new PointF(300 + 150 * (RibCount - 2) + 293, 259),
                    new PointF(300 + 150 * (RibCount - 2) + 299, 259),
                    new PointF(300 + 150 * (RibCount - 2) + 299, 258),
                    new PointF(300 + 150 * (RibCount - 2) + 293, 258),
                    new PointF(300 + 150 * (RibCount - 2) + 293, 256),
                    new PointF(300 + 150 * (RibCount - 2) + 275, 256),
                    new PointF(300 + 150 * (RibCount - 2) + 275, 253),
                    new PointF(300 + 150 * (RibCount - 2) + 263, 253),
                    new PointF(300 + 150 * (RibCount - 2) + 263, 251),
};
                graphics.FillPolygon(new SolidBrush(Color.Black), point);

                if (RibCount == 1)
                {
                    pen.Width = 3;
                    graphics.DrawLine(pen, new PointF(149, 59), new PointF(304, 59));

                    pen.Width = 1;
                    point = new PointF[]
                    {
                        new PointF(149, 59),
                        new PointF(149, 61),
                        new PointF(157, 61),
                        new PointF(157, 63),
                        new PointF(178, 63),
                        new PointF(178, 65),
                        new PointF(195, 65),
                        new PointF(195, 67),
                        new PointF(200, 67),
                        new PointF(200, 52),
                        new PointF(195, 52),
                        new PointF(195, 55),
                        new PointF(178, 55),
                        new PointF(178, 57),
                        new PointF(157, 57),
                        new PointF(157, 58)
                    };

                    graphics.FillPolygon(new SolidBrush(Color.Black), point);
                }

                #endregion

                #region Расстояние между рёбрами

                if (RibCount > 1)
                {
                    pen.Width = 3;
                    graphics.DrawLine(pen, new PointF(bitmap.Width - 300, 59), new PointF(bitmap.Width - 450, 59));

                    point = new PointF[]
                    {
                            new PointF(300 + 150 * (RibCount - 2) + 1, 59),
                            new PointF(300 + 150 * (RibCount - 2) + 1, 61),
                            new PointF(300 + 150 * (RibCount - 2) + 5, 61),
                            new PointF(300 + 150 * (RibCount - 2) + 5, 63),
                            new PointF(300 + 150 * (RibCount - 2) + 9, 63),
                            new PointF(300 + 150 * (RibCount - 2) + 9, 65),
                            new PointF(300 + 150 * (RibCount - 2) + 13, 65),
                            new PointF(300 + 150 * (RibCount - 2) + 13, 67),
                            new PointF(300 + 150 * (RibCount - 2) + 16, 67),
                            new PointF(300 + 150 * (RibCount - 2) + 16, 54),
                            new PointF(300 + 150 * (RibCount - 2) + 13, 54),
                            new PointF(300 + 150 * (RibCount - 2) + 13, 55),
                            new PointF(300 + 150 * (RibCount - 2) + 9, 55),
                            new PointF(300 + 150 * (RibCount - 2) + 9, 57),
                            new PointF(300 + 150 * (RibCount - 2) + 5, 57),
                            new PointF(300 + 150 * (RibCount - 2) + 5, 58)
                    };
                    graphics.FillPolygon(new SolidBrush(Color.Black), point);

                    point = new PointF[]
                    {
                            new PointF(300 + 150 * (RibCount - 2) + 149, 59),
                            new PointF(300 + 150 * (RibCount - 2) + 149, 61),
                            new PointF(300 + 150 * (RibCount - 2) + 145, 61),
                            new PointF(300 + 150 * (RibCount - 2) + 145, 63),
                            new PointF(300 + 150 * (RibCount - 2) + 141, 63),
                            new PointF(300 + 150 * (RibCount - 2) + 141, 65),
                            new PointF(300 + 150 * (RibCount - 2) + 137, 65),
                            new PointF(300 + 150 * (RibCount - 2) + 137, 67),
                            new PointF(300 + 150 * (RibCount - 2) + 134, 67),
                            new PointF(300 + 150 * (RibCount - 2) + 134, 54),
                            new PointF(300 + 150 * (RibCount - 2) + 137, 54),
                            new PointF(300 + 150 * (RibCount - 2) + 137, 55),
                            new PointF(300 + 150 * (RibCount - 2) + 141, 55),
                            new PointF(300 + 150 * (RibCount - 2) + 141, 57),
                            new PointF(300 + 150 * (RibCount - 2) + 145, 57),
                            new PointF(300 + 150 * (RibCount - 2) + 145, 58)
                    };
                    graphics.FillPolygon(new SolidBrush(Color.Black), point);
                }

                #endregion
            }
            return bitmap;
        }

        /// <summary>
        /// Добавление текста на изображение рёбер
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private Bitmap GetTextPlita(Bitmap bitmap)
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                StringFormat stringFormat1 = new StringFormat(StringFormatFlags.DirectionVertical)
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                StringFormat stringFormat2 = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                using (Font font = new Font("Lucida Console", 18, System.Drawing.FontStyle.Regular))
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                    graphics.DrawString(RibHeight.ToString(),
                                        FontLibr.FindFont(graphics, RibHeight.ToString(), new System.Drawing.Size(80, 30), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(120, 126),
                                        stringFormat1);

                    graphics.DrawString(DistanceToFirst.ToString(),
                                        FontLibr.FindFont(graphics, DistanceToFirst.ToString(), new System.Drawing.Size(60, 25), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(300 + 150 * (RibCount - 2) + 205, 40),
                                        stringFormat2);

                    graphics.DrawString(PlateThickness.ToString(),
                                        FontLibr.FindFont(graphics, PlateThickness.ToString(), new System.Drawing.Size(60, 25), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(300 + 150 * (RibCount - 2) + 360, 180),
                                        stringFormat2);

                    if (RibCount > 1)
                        graphics.DrawString(DistanceBetween.ToString(),
                                            FontLibr.FindFont(graphics, DistanceBetween.ToString(), new System.Drawing.Size(90, 25), font),
                                            new SolidBrush(Color.Black),
                                            new PointF(bitmap.Width - 375, 45),
                                            stringFormat2);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Объединение изображений
        /// </summary>
        /// <param name="png1">Изображение первого ребра</param>
        /// <param name="png2">Все рёбра кроме первого и последнего</param>
        /// <param name="png3">Последнее ребро</param>
        /// <returns></returns>
        private BitmapImage JoinRebra(Bitmap png1, Bitmap png2, Bitmap png3)
        {
            BitmapImage imageSource = new BitmapImage();
            using (Bitmap result = new Bitmap(png1.Width + png2.Width * (this.RibCount - 2) + png3.Width, 310))
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(png1, 0, 0);

                    for (int i = 1; i < RibCount; i++)
                    {
                        if (i == 1)
                            g.DrawImage(png2, 300, 0);
                        else
                            g.DrawImage(png2, i * 150, 0);
                    }
                    g.DrawImage(png3, (RibCount - 2) * 150 + 300, 0);
                }
                imageSource = ForRobot.Libr.Converters.ImageConverter.BitmapToBitmapImage(GetTextPlita(GetArrowsPlita(result)));
            }
            return imageSource;
        }

        #endregion

        #region Плита

        /// <summary>
        /// Вывод изображения плиты исхотя из типа настила
        /// </summary>
        /// <returns></returns>
        private BitmapImage GetGenericImage()
        {
            switch (this.ScoseType)
            {
                case ScoseTypes.Rect:
                    return JoinPlita(new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImagePlitaStart"))),
                                     new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImagePlitaBody"))),
                                     new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImagePlitaEnd"))));
                //new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap(new BitmapImage(new Uri("pack://application:,,,/InterfaceOfRobots;component/Themes/Images/EndPlita.png")))));
                //new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImagePlitaEnd"))));

                case ScoseTypes.SlopeLeft:
                    return ForRobot.Libr.Converters.ImageConverter.BitmapToBitmapImage(GetTextSlopeLeft(new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImageSlopeLeft")), new System.Drawing.Size(1280, 720))));

                case ScoseTypes.SlopeRight:
                    return ForRobot.Libr.Converters.ImageConverter.BitmapToBitmapImage(GetTextSlopeRight(new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImageSlopeRight")), new System.Drawing.Size(1280, 720))));

                case ScoseTypes.TrapezoidTop:
                    return ForRobot.Libr.Converters.ImageConverter.BitmapToBitmapImage(GetTextTrapezoidTop(new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImageTrapezoidTopParam")), new System.Drawing.Size(1280, 720))));

                case ScoseTypes.TrapezoidBottom:
                    return ForRobot.Libr.Converters.ImageConverter.BitmapToBitmapImage(GetTextTrapezoidBottom(new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImageTrapezoidBottomParam")), new System.Drawing.Size(1280, 720))));

                default:
                    return this._plitaImage;
            }
        }

        /// <summary>
        /// Добавление текста на изображение плиты прямоугольной формы
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private Bitmap GetText(Bitmap bitmap)
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                StringFormat stringFormat1 = new StringFormat(StringFormatFlags.DirectionVertical)
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                StringFormat stringFormat2 = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                using (Font font = new Font("Lucida Console", 18, System.Drawing.FontStyle.Regular))
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                    graphics.DrawString(PlateLength.ToString(),
                                        FontLibr.FindFont(graphics, PlateLength.ToString(), new System.Drawing.Size(100, 30), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(420, 475),
                                        stringFormat2);

                    //graphics.DrawString(PlateWidth.ToString(),
                    //                    FontLibr.FindFont(graphics, PlateWidth.ToString(), new System.Drawing.Size(100, 30), font),
                    //                    new SolidBrush(Color.Black), 
                    //                    new PointF(840, bitmap.Height/2),
                    //                    stringFormat1);

                    graphics.DrawString(DissolutionLeft.ToString(),
                                        FontLibr.FindFont(graphics, DissolutionLeft.ToString(), new System.Drawing.Size(48, 22), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(95, 420),
                                        stringFormat2);

                    graphics.DrawString(DissolutionRight.ToString(),
                                        FontLibr.FindFont(graphics, DissolutionRight.ToString(), new System.Drawing.Size(48, 22), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(710, 420),
                                        stringFormat2);

                    graphics.DrawString(RibThickness.ToString(),
                                        FontLibr.FindFont(graphics, RibThickness.ToString(), new System.Drawing.Size(40, 25), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(795, 140),
                                        stringFormat1);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Добавление текста на изображение плиты со скосом влево
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private Bitmap GetTextSlopeLeft(Bitmap bitmap)
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                StringFormat stringFormatHorizont = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                StringFormat stringFormatVertical = new StringFormat(StringFormatFlags.DirectionVertical)
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                using (Font font = new Font("Lucida Console", 18, System.Drawing.FontStyle.Regular))
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                    graphics.DrawString(PlateLength.ToString(),
                        FontLibr.FindFont(graphics, PlateLength.ToString(), new System.Drawing.Size(250, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(550, 70),
                        stringFormatHorizont);

                    graphics.DrawString(DistanceToFirst.ToString(),
                        FontLibr.FindFont(graphics, DistanceToFirst.ToString(), new System.Drawing.Size(125, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1220, 240),
                        stringFormatVertical);

                    graphics.DrawString(DistanceBetween.ToString(),
                        FontLibr.FindFont(graphics, DistanceBetween.ToString(), new System.Drawing.Size(100, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1220, 365),
                        stringFormatVertical);

                    graphics.DrawString(BevelToLeft.ToString(),
                        FontLibr.FindFont(graphics, BevelToLeft.ToString(), new System.Drawing.Size(100, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(105, 680),
                        stringFormatHorizont);

                    graphics.DrawString(BevelToRight.ToString(),
                        FontLibr.FindFont(graphics, BevelToRight.ToString(), new System.Drawing.Size(80, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1125, 150),
                        stringFormatHorizont);

                    graphics.DrawString(IdentToLeft.ToString(),
                        FontLibr.FindFont(graphics, IdentToLeft.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(215, 600),
                        stringFormatHorizont);

                    graphics.DrawString(IdentToRight.ToString(),
                        FontLibr.FindFont(graphics, IdentToRight.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1080, 600),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionLeft.ToString(),
                        FontLibr.FindFont(graphics, DissolutionLeft.ToString(), new System.Drawing.Size(80, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(320, 600),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionRight.ToString(),
                        FontLibr.FindFont(graphics, DissolutionRight.ToString(), new System.Drawing.Size(50, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(995, 600),
                        stringFormatHorizont);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Добавление текста на изображение плиты со скосом вправо
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private Bitmap GetTextSlopeRight(Bitmap bitmap)
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                StringFormat stringFormatHorizont = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                StringFormat stringFormatVertical = new StringFormat(StringFormatFlags.DirectionVertical)
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                using (Font font = new Font("Lucida Console", 18, System.Drawing.FontStyle.Regular))
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                    graphics.DrawString(PlateLength.ToString(),
                        FontLibr.FindFont(graphics, PlateLength.ToString(), new System.Drawing.Size(250, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(700, 60),
                        stringFormatHorizont);

                    graphics.DrawString(DistanceToFirst.ToString(),
                        FontLibr.FindFont(graphics, DistanceToFirst.ToString(), new System.Drawing.Size(125, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(80, 220),
                        stringFormatVertical);

                    graphics.DrawString(DistanceBetween.ToString(),
                        FontLibr.FindFont(graphics, DistanceBetween.ToString(), new System.Drawing.Size(100, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(80, 340),
                        stringFormatVertical);

                    graphics.DrawString(BevelToLeft.ToString(),
                        FontLibr.FindFont(graphics, BevelToLeft.ToString(), new System.Drawing.Size(70, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(190, 130),
                        stringFormatHorizont);

                    graphics.DrawString(BevelToRight.ToString(),
                        FontLibr.FindFont(graphics, BevelToRight.ToString(), new System.Drawing.Size(100, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1220, 680),
                        stringFormatHorizont);

                    graphics.DrawString(IdentToLeft.ToString(),
                        FontLibr.FindFont(graphics, IdentToLeft.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(240, 600),
                        stringFormatHorizont);

                    graphics.DrawString(IdentToRight.ToString(),
                        FontLibr.FindFont(graphics, IdentToRight.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1105, 600),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionLeft.ToString(),
                        FontLibr.FindFont(graphics, DissolutionLeft.ToString(), new System.Drawing.Size(43, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(322, 600),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionRight.ToString(),
                        FontLibr.FindFont(graphics, DissolutionRight.ToString(), new System.Drawing.Size(140, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(965, 600),
                        stringFormatHorizont);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Добавление текста на изображение плиты трапецией
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private Bitmap GetTextTrapezoidTop(Bitmap bitmap)
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                StringFormat stringFormatHorizont = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                StringFormat stringFormatVertical = new StringFormat(StringFormatFlags.DirectionVertical)
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                using (Font font = new Font("Lucida Console", 18, System.Drawing.FontStyle.Regular))
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                    graphics.DrawString(PlateLength.ToString(),
                        FontLibr.FindFont(graphics, PlateLength.ToString(), new System.Drawing.Size(250, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(600, 60),
                        stringFormatHorizont);

                    graphics.DrawString(DistanceToFirst.ToString(),
                        FontLibr.FindFont(graphics, DistanceToFirst.ToString(), new System.Drawing.Size(150, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(60, 300),
                        stringFormatVertical);

                    graphics.DrawString(DistanceBetween.ToString(),
                        FontLibr.FindFont(graphics, DistanceBetween.ToString(), new System.Drawing.Size(140, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(60, 465),
                        stringFormatVertical);

                    graphics.DrawString(BevelToLeft.ToString(),
                        FontLibr.FindFont(graphics, BevelToLeft.ToString(), new System.Drawing.Size(80, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(150, 190),
                        stringFormatHorizont);

                    graphics.DrawString(BevelToRight.ToString(),
                        FontLibr.FindFont(graphics, BevelToRight.ToString(), new System.Drawing.Size(100, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1120, 140),
                        stringFormatHorizont);

                    graphics.DrawString(IdentToLeft.ToString(),
                        FontLibr.FindFont(graphics, IdentToLeft.ToString(), new System.Drawing.Size(125, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(200, 620),
                        stringFormatHorizont);

                    graphics.DrawString(IdentToRight.ToString(),
                        FontLibr.FindFont(graphics, IdentToRight.ToString(), new System.Drawing.Size(130, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1060, 620),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionLeft.ToString(),
                        FontLibr.FindFont(graphics, DissolutionLeft.ToString(), new System.Drawing.Size(50, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(295, 620),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionRight.ToString(),
                        FontLibr.FindFont(graphics, DissolutionRight.ToString(), new System.Drawing.Size(80, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(940, 620),
                        stringFormatHorizont);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Добавление текста на изображение плиты перевернутой трапецией
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private Bitmap GetTextTrapezoidBottom(Bitmap bitmap)
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                StringFormat stringFormatHorizont = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                StringFormat stringFormatVertical = new StringFormat(StringFormatFlags.DirectionVertical)
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                using (Font font = new Font("Lucida Console", 18, System.Drawing.FontStyle.Regular))
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                    graphics.DrawString(PlateLength.ToString(),
                        FontLibr.FindFont(graphics, PlateLength.ToString(), new System.Drawing.Size(250, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(600, 60),
                        stringFormatHorizont);

                    graphics.DrawString(DistanceToFirst.ToString(),
                        FontLibr.FindFont(graphics, DistanceToFirst.ToString(), new System.Drawing.Size(130, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(100, 260),
                        stringFormatVertical);

                    graphics.DrawString(DistanceBetween.ToString(),
                        FontLibr.FindFont(graphics, DistanceBetween.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(100, 390),
                        stringFormatVertical);

                    graphics.DrawString(BevelToLeft.ToString(),
                        FontLibr.FindFont(graphics, BevelToLeft.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(185, 150),
                        stringFormatHorizont);

                    graphics.DrawString(BevelToRight.ToString(),
                        FontLibr.FindFont(graphics, BevelToRight.ToString(), new System.Drawing.Size(100, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1130, 150),
                        stringFormatHorizont);

                    graphics.DrawString(IdentToLeft.ToString(),
                        FontLibr.FindFont(graphics, IdentToLeft.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(260, 640),
                        stringFormatHorizont);

                    graphics.DrawString(IdentToRight.ToString(),
                        FontLibr.FindFont(graphics, IdentToRight.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1055, 640),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionLeft.ToString(),
                        FontLibr.FindFont(graphics, DissolutionLeft.ToString(), new System.Drawing.Size(45, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(343, 640),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionRight.ToString(),
                        FontLibr.FindFont(graphics, DissolutionRight.ToString(), new System.Drawing.Size(80, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(955, 640),
                        stringFormatHorizont);
                }
            }
            return bitmap;
        }

        private Bitmap GetArrows(Bitmap bitmap)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                Pen pen = new Pen(Color.FromArgb(0, 183, 239), 1);

                PointF[] points1 =
                    {
                        new PointF(804,  460),
                        new PointF(804, 462),
                        new PointF(806,  462),
                        new PointF(806, 464),
                        new PointF(808, 464),
                        new PointF(808, 466),
                        new PointF(810, 466),
                        new PointF(810, 468),
                        new PointF(811, 468),
                        new PointF(811, 469),
                        new PointF(812, 469),
                        new PointF(812, 468),
                        new PointF(813, 468),
                        new PointF(813, 466),
                        new PointF(815, 466),
                        new PointF(815, 464),
                        new PointF(817, 464),
                        new PointF(817, 462),
                        new PointF(819, 462),
                        new PointF(819, 460),
                    };

                PointF[] points2 =
                    {
                        new PointF(804,  39),
                        new PointF(804, 38),
                        new PointF(806,  38),
                        new PointF(806, 36),
                        new PointF(808, 36),
                        new PointF(808, 34),
                        new PointF(810, 34),
                        new PointF(810, 32),
                        new PointF(811, 32),
                        new PointF(811, 31),
                        new PointF(812, 31),
                        new PointF(812, 32),
                        new PointF(813, 32),
                        new PointF(813, 34),
                        new PointF(815, 34),
                        new PointF(815, 36),
                        new PointF(817, 36),
                        new PointF(817, 38),
                        new PointF(819, 38),
                        new PointF(819, 39),
                    };

                if (TransversePrivyazka == Privyazka.FromRightToLeft)
                {
                    g.DrawPolygon(pen, points1);
                    g.FillPolygon(new SolidBrush(Color.FromArgb(0, 183, 239)), points1);
                }
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                {
                    g.DrawPolygon(pen, points2);
                    g.FillPolygon(new SolidBrush(Color.FromArgb(0, 183, 239)), points2);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Объединение изображений плиты
        /// </summary>
        /// <param name="png1"></param>
        /// <param name="png2"></param>
        /// <param name="png3"></param>
        /// <returns></returns>
        private BitmapImage JoinPlita(Bitmap png1, Bitmap png2, Bitmap png3)
        {
            BitmapImage imageSource = new BitmapImage();
            using (Bitmap result = new Bitmap(980, png1.Height + png2.Height * 2 + png3.Height ))
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(png1, 0, 0);

                    for (int i = 1; i < 4; i++)
                    {
                        if (i == 1)
                            g.DrawImage(png2, 0, 150);
                        else
                            g.DrawImage(png2, 0, (i - 1) * 88 + 150);
                    }

                    g.DrawImage(png3, 0, 2 * 88 + 150);
                }
                imageSource = ForRobot.Libr.Converters.ImageConverter.BitmapToBitmapImage(GetText(result));
            }
            return imageSource;
        }

        #endregion

        #endregion Private functions

        #region Public functions

        /// <summary>
        /// Десериализация класса <see cref="Plita"/> из JSON-строки
        /// </summary>
        /// <param name="sJsonString">JSON-строка</param>
        /// <returns></returns>
        public override object DeserializeDetal(string sJsonString)
        {
            if (string.IsNullOrEmpty(sJsonString))
                return new Plita(Detals.DetalType.Plita);
            else
                return JsonConvert.DeserializeObject<Plita>(JObject.Parse(sJsonString, this._jsonLoadSettings).ToString(), this._jsonDeserializerSettings);
        }

        public FullyObservableCollection<Rib> SetRibsCollection(FullyObservableCollection<Rib> collection) => this.RibsCollection = collection;
        
        #endregion Public functions
    }
}
