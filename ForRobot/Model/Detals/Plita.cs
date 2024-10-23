using System;
using System.Linq;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

using System.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

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

        private bool _diferentDistance = false;
        private bool _paralleleRibs = true;

        private decimal _bevelToStart;
        private decimal _bevelToEnd;
        private decimal _distanceForSearch;
        private decimal _distanceForWelding;

        private BitmapImage _rebraImage;
        private BitmapImage _plitaImage;

        /// <summary>
        /// Коллекция рёбер
        /// </summary>
        private FullyObservableCollection<Rib> _ribsCollection;

        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented
        };

        #endregion

        #region Public variables

        /// <summary>
        /// Json-строка для передачи
        /// </summary>
        [JsonIgnore]
        public override string Json
        {
            get
            {
                if (this.ScoseType == ScoseTypes.Rect)
                {
                    if (this.DiferentDistance)
                    {
                        if (this.ParalleleRibs)
                        {
                            this._jsonSettings.ContractResolver = new StraightPlitaDifferentDistanceBetweenParallelRibsAttributesResolver();
                            return JsonConvert.SerializeObject(this, _jsonSettings).Replace("d_W2", "d_w2");
                        }
                        else
                        {
                            this._jsonSettings.ContractResolver = new StraightPlitaDifferentDistanceBetweenNotParallelRibsAttributesResolver();
                            return JsonConvert.SerializeObject(this, _jsonSettings).Replace("d_W2", "d_w2").Replace("d_dis1", "d_dis");
                        }
                    }
                    else
                    {
                        this._jsonSettings.ContractResolver = new StraightPlitaEqualDistanceBetweenRibsAttributesResolver();
                    }
                }
                else
                {
                    if (this.DiferentDistance)
                    {
                        if (this.ParalleleRibs)
                        {
                            this._jsonSettings.ContractResolver = new BeveledPlitaDifferentDistanceBetweenParallelRibsAttributesResolver();
                            return JsonConvert.SerializeObject(this, _jsonSettings).Replace("d_W2", "d_w2");
                        }
                        else
                        {
                            this._jsonSettings.ContractResolver = new BeveledPlitaDifferentDistanceBetweenNotParallelRibsAttributesResolver();
                            return JsonConvert.SerializeObject(this, _jsonSettings).Replace("d_W2", "d_w2").Replace("d_dis1", "d_dis");
                        }
                    }
                    else
                    {
                        this._jsonSettings.ContractResolver = new BeveledPlitaEqualDistanceBetweenRibsAttributesResolver();
                    }
                }

                return JsonConvert.SerializeObject(this, _jsonSettings);
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
                //var js = JObject.Parse(JsonConvert.SerializeObject(this.Save(), _jsonSettings).Replace(ScoseTypes.SlopeLeft, this.ScoseType));
                ////js[nameof(this.DiferentDistance)] = this._diferentDistance;
                //return js.ToString();
                return JsonConvert.SerializeObject(this.Save(), _jsonSettings).Replace(ScoseTypes.SlopeLeft, this.ScoseType);
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
                Set(ref this._diferentDistance, value);
                this.Change?.Invoke(this, null);
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
            get => this._paralleleRibs;
            set
            {
                Set(ref this._paralleleRibs, value);
                this.Change?.Invoke(this, null);
            }
        }

        [JsonIgnore]
        [SaveAttribute]
        /// <summary>
        /// Тип детали
        /// </summary>
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
                Set(ref this._scoseType, value);
                RaisePropertyChanged(nameof(this.DiferentDistance), nameof(this.Wight), nameof(this.BevelToStart), nameof(this.BevelToEnd), nameof(this.GenericImage), nameof(this.RebraImage));
                this.Change?.Invoke(this, null);
            }
        }

        [JsonProperty("l")]
        [JsonConverter(typeof(JsonCommentConverter), "Длина настила")]
        /// <summary>
        /// Длина настила
        /// </summary>
        public override decimal Long
        {
            get => base.Long;
            set
            {
                base.Long = value;
                RaisePropertyChanged(nameof(this.RebraImage), nameof(this.GenericImage));
                this.Change?.Invoke(this, null);
            }
        }

        [JsonIgnore]
        [JsonProperty("w"), BeveledPlitaEqualDistanceBetweenRibsAttribute, BeveledPlitaDifferentDistanceBetweenParallelRibsAttribute, BeveledPlitaDifferentDistanceBetweenNotParallelRibsAttribute, SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Ширина настила")]
        /// <summary>
        /// Ширина настила
        /// </summary>
        public override decimal Wight
        {
            get
            {
                if (this.ScoseType == ScoseTypes.Rect)
                    return 0;
                else
                    return base.Wight;
            }
            set
            {
                base.Wight = value;
                this.Change?.Invoke(this, null);
            }
        }

        [JsonProperty("h")]
        [JsonConverter(typeof(JsonCommentConverter), "Высота ребра")]
        /// <summary>
        /// Высота ребра
        /// </summary>
        public override decimal Hight
        {
            get => base.Hight;
            set
            {
                base.Hight = value;
                RaisePropertyChanged(nameof(this.RebraImage));
                this.Change?.Invoke(this, null);
            }
        }

        [JsonProperty("d_w1")]
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
                RaisePropertyChanged(nameof(this.GenericImage));
                this.Change?.Invoke(this, null);
            }
        }

        [JsonIgnore]
        [JsonProperty("d_w2"), StraightPlitaEqualDistanceBetweenRibsAttribute, BeveledPlitaEqualDistanceBetweenRibsAttribute, SaveAttribute]
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
                RaisePropertyChanged(nameof(this.RebraImage), nameof(this.GenericImage));
                this.Change?.Invoke(this, null);
            }
        }

        [JsonIgnore]
        [JsonProperty("d_l1"), StraightPlitaEqualDistanceBetweenRibsAttribute, BeveledPlitaEqualDistanceBetweenRibsAttribute, SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Расстояние торца ребра в начале")]
        /// <summary>
        /// Расстояние торца ребра в начале
        /// </summary>
        public override decimal DistanceToStart
        {
            get => base.DistanceToStart;
            set
            {
                base.DistanceToStart = value;
                RaisePropertyChanged(nameof(this.GenericImage));
                this.Change?.Invoke(this, null);
            }
        }

        [JsonIgnore]
        [JsonProperty("d_l2"), StraightPlitaEqualDistanceBetweenRibsAttribute, BeveledPlitaEqualDistanceBetweenRibsAttribute, SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Расстояние торца ребра в конце")]
        /// <summary>
        /// Расстояние торца ребра в конце
        /// </summary>
        public override decimal DistanceToEnd
        {
            get => base.DistanceToEnd;
            set
            {
                base.DistanceToEnd = value;
                RaisePropertyChanged(nameof(this.GenericImage));
                this.Change?.Invoke(this, null);
            }
        }

        [JsonProperty("l_r1")]
        [JsonConverter(typeof(JsonCommentConverter), "Роспуск в начале")]
        /// <summary>
        /// Роспуск в начале
        /// </summary>
        public override decimal DissolutionStart
        {
            get
            {
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                    return base.DissolutionStart;
                else
                    return base.DissolutionEnd;
            }
            set
            {
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                    base.DissolutionStart = value;
                else
                    base.DissolutionEnd = value;

                RaisePropertyChanged(nameof(this.GenericImage));
                this.Change?.Invoke(this, null);
            }
        }

        [JsonProperty("l_r2")]
        [JsonConverter(typeof(JsonCommentConverter), "Роспуск в конце")]
        /// <summary>
        /// Роспуск в конце
        /// </summary>
        public override decimal DissolutionEnd
        {
            get
            {
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                    return base.DissolutionEnd;
                else
                    return base.DissolutionStart;
            }
            set
            {
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                    base.DissolutionEnd = value;
                else
                    base.DissolutionStart = value;

                RaisePropertyChanged(nameof(this.GenericImage));
                this.Change?.Invoke(this, null);
            }
        }

        [JsonProperty("t_p")]
        [JsonConverter(typeof(JsonCommentConverter), "Толщина настила")]
        /// <summary>
        /// Толщина настила
        /// </summary>
        public override decimal ThicknessPlita
        {
            get => base.ThicknessPlita;
            set
            {
                base.ThicknessPlita = value;
                RaisePropertyChanged(nameof(this.RebraImage));
                this.Change?.Invoke(this, null);
            }
        }

        [JsonProperty("t_r")]
        [JsonConverter(typeof(JsonCommentConverter), "Толщина рёбер")]
        /// <summary>
        /// Толщина ребра
        /// </summary>
        public override decimal ThicknessRebro
        {
            get => base.ThicknessRebro;
            set
            {
                base.ThicknessRebro = value;
                RaisePropertyChanged(nameof(this.GenericImage));
                this.Change?.Invoke(this, null);
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
                RaisePropertyChanged(nameof(this.RebraImage));
                this.Change?.Invoke(this, null);
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
                RaisePropertyChanged(nameof(this.RebraImage));
                this.Change?.Invoke(this, null);
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
                this.Change?.Invoke(this, null);
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
                this.Change?.Invoke(this, null);
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
                this.Change?.Invoke(this, null);
            }
        }

        [JsonIgnore]
        [JsonProperty("d_b1"), BeveledPlitaEqualDistanceBetweenRibsAttribute, BeveledPlitaDifferentDistanceBetweenParallelRibsAttribute, BeveledPlitaDifferentDistanceBetweenNotParallelRibsAttribute, SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Скос слева")]
        /// <summary>
        /// Скос слева
        /// </summary>
        public decimal BevelToStart
        {
            get
            {
                if (Equals(this.ScoseType, ScoseTypes.Rect))
                    return 0;
                else
                    return this._bevelToStart;
            }
            set
            {
                this._bevelToStart = value;
                RaisePropertyChanged(nameof(this.GenericImage));
                this.Change?.Invoke(this, null);
            }
        }

        [JsonIgnore]
        [JsonProperty("d_b2"), BeveledPlitaEqualDistanceBetweenRibsAttribute, BeveledPlitaDifferentDistanceBetweenParallelRibsAttribute, BeveledPlitaDifferentDistanceBetweenNotParallelRibsAttribute, SaveAttribute]
        [JsonConverter(typeof(JsonCommentConverter), "Скос справа")]
        /// <summary>
        /// Скос справа
        /// </summary>
        public decimal BevelToEnd
        {
            get
            {
                if (Equals(this.ScoseType, ScoseTypes.Rect))
                    return 0;
                else
                    return this._bevelToEnd;
            }
            set
            {
                this._bevelToEnd = value;
                RaisePropertyChanged(nameof(this.GenericImage));
                this.Change?.Invoke(this, null);
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
                this.Change?.Invoke(this, null);
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
                this.Change?.Invoke(this, null);
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
                this.Change?.Invoke(this, null);
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
                this.Change?.Invoke(this, null);
            }
        }

        [JsonIgnore]
        [JsonProperty("d_W2"), StraightPliteDifferentDistanceBetweenParallelRibsAttribute, StraightPliteDifferentDistanceBetweenNotParallelRibsAttribute, 
            BeveledPlitaDifferentDistanceBetweenParallelRibsAttribute, BeveledPlitaDifferentDistanceBetweenNotParallelRibsAttribute, SaveAttribute]
        /// <summary>
        /// Коллекция рёбер
        /// </summary>
        public FullyObservableCollection<Rib> RibsCollection
        {
            get => _ribsCollection ?? (this._ribsCollection = this.FillRibsCollection());
            set => Set(ref this._ribsCollection, value);
        }

        [JsonProperty("edge_count")]
        [JsonConverter(typeof(JsonCommentConverter), "Кол-во рёбер")]
        /// <summary>
        /// Количество ребер
        /// </summary>
        public override int SumReber
        {
            get => base.SumReber;
            set
            {
                base.SumReber = value;

                if (this.RibsCollection != null)
                {
                    if (this.SumReber < this.RibsCollection.Count)
                        this.RibsCollection = new FullyObservableCollection<Rib>(this.RibsCollection.Take(this.SumReber).ToList<Rib>());
                    else
                        for (int i = this.RibsCollection.Count; i < this.SumReber; i++)
                        {
                            this.RibsCollection.Add(new Rib()
                            {
                                Distance = this.DistanceBetween,
                                DistanceToStart = this.DistanceToStart,
                                DistanceToEnd = this.DistanceToEnd
                            });
                        }
                }

                //this.RibsCollection = this.FillRibsCollection();

                RaisePropertyChanged(nameof(this.RebraImage), nameof(this.GenericImage));
                this.Change?.Invoke(this, null);
            }
        }

        [JsonIgnore]
        public override Privyazka LongitudinalPrivyazka
        {
            get => base.LongitudinalPrivyazka;
            set
            {
                base.LongitudinalPrivyazka = value;
                this.Change?.Invoke(this, null);
            }
        }

        [JsonIgnore]
        public override Privyazka TransversePrivyazka
        {
            get => base.TransversePrivyazka;
            set
            {
                base.TransversePrivyazka = value;
                RaisePropertyChanged(nameof(this.GenericImage));
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Изображение рёбер плиты
        /// </summary>
        public override sealed BitmapImage RebraImage
        {
            get => Equals(this.ScoseType, "d_rect") ? (this._rebraImage = JoinRebra(GetStartRebraImage(), GetBodyRebraImage(), GetEndRebraImage())) : null;
            set => Set(ref this._rebraImage, value);
        }

        [JsonIgnore]
        /// <summary>
        /// Общее изображение плиты
        /// </summary>
        public override sealed BitmapImage GenericImage { get => this.GetGenericImage(); set => Set(ref this._plitaImage, value); }

        #endregion

        #region Events

        /// <summary>
        /// Событие изменения свойства класса
        /// </summary>
        public override event EventHandler Change;

        //public override event Func<object, EventArgs, Task> Change;

        #endregion

        #region Constructors

        public Plita() { }

        public Plita(DetalType type) : base(type)
        {
            this.Wight = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).Wight;
            this.BevelToStart = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).BevelToStart;
            this.BevelToEnd = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).BevelToEnd;
            this.DistanceForWelding = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).DistanceForWelding;
            this.DistanceForSearch = (ConfigurationManager.GetSection("plita") as PlitaConfigurationSection).DistanceForSearch;
        }

        #endregion

        #region Public functions

        //public async Task OnChange() => this.Change;

        //public async Task OnChange() => await base.OnChange(this.Change);

        #endregion

        #region Private functions

        /// <summary>
        /// Экземпляр для сохранения
        /// </summary>
        /// <returns></returns>
        private Plita Save() => new Plita()
        {
            ScoseType = ScoseTypes.SlopeLeft,
            Long = this.Long,
            Wight = base.Wight,
            Hight = this.Hight,
            DistanceToFirst = this.DistanceToFirst,
            DistanceBetween = this.DistanceBetween,
            DistanceToStart = this.DistanceToStart,
            DistanceToEnd = this.DistanceToEnd,
            DissolutionStart = this.DissolutionStart,
            DissolutionEnd = this.DissolutionEnd,
            ThicknessPlita = this.ThicknessPlita,
            ThicknessRebro = this.ThicknessRebro,
            SearchOffsetStart = this.SearchOffsetStart,
            SearchOffsetEnd = this.SearchOffsetEnd,
            SeamsOverlap = this.SeamsOverlap,
            TechOffsetSeamStart = this.TechOffsetSeamStart,
            TechOffsetSeamEnd = this.TechOffsetSeamEnd,
            BevelToStart = this._bevelToStart,
            BevelToEnd = this._bevelToEnd,
            DistanceForWelding = this.DistanceForWelding,
            DistanceForSearch = this.DistanceForSearch,
            WildingSpead = this.WildingSpead,
            ProgramNom = this.ProgramNom,
            DiferentDistance = this.DiferentDistance,
            ParalleleRibs = this.ParalleleRibs,
            SumReber = this.SumReber,
            RibsCollection = this.RibsCollection
        };

        /// <summary>
        /// Заполнение коллекции расстояний
        /// </summary>
        /// <returns></returns>
        private FullyObservableCollection<Rib> FillRibsCollection()
        {
            Rib rib;
            List<Rib> ribs = new List<Rib>();

            for (int i = 0; i < SumReber; i++)
            {
                rib = new Rib()
                {
                    DistanceToStart = this.DistanceToStart,
                    DistanceToEnd = this.DistanceToEnd,
                };

                if (i == 0)
                {
                    rib.Distance = this.DistanceToFirst;
                    rib.DistanceLeft = this.DistanceToFirst;
                    rib.DistanceRight = this.DistanceToFirst;
                }
                else
                {
                    rib.Distance = this.DistanceBetween;
                    rib.DistanceLeft = this.DistanceBetween;
                    rib.DistanceRight = this.DistanceBetween;
                }

                ribs.Add(rib);
            }

            return new FullyObservableCollection<Rib>(ribs);
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
                //Font font = new Font("Lucida Console", image.Width / 14, System.Drawing.FontStyle.Regular);
                //StringFormat stringFormat = new StringFormat(StringFormatFlags.DirectionVertical);
                //graphics.DrawString(IndentionStart.ToString(), font, new SolidBrush(Color.Black), 206, 29);
                //graphics.DrawString(Hight.ToString(), font, new SolidBrush(Color.Black), new PointF(102, 126 - Hight.ToString().Length * (font.Size - 8)), stringFormat);
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
                //if (SumReber == 1)
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

                //if (SumReber == 1)
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
                //Font font = new Font("Lucida Console", image.Width / 22, System.Drawing.FontStyle.Regular);
                //graphics.DrawString(IndentionEnd.ToString(), font, new SolidBrush(Color.Black), 205, 15);
                //graphics.DrawString(ThicknessPlita.ToString(), font, new SolidBrush(Color.Black), 334, 156);
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
            //    Font font = new Font("Lucida Console", image.Width / 8, System.Drawing.FontStyle.Regular);
            //    graphics.DrawString(DistanceBetween.ToString(), font, new SolidBrush(Color.Black), 51, 33);
            //    graphics.DrawString(Long.ToString(), font, new SolidBrush(Color.Black), 33, 234);
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
                    new PointF(300 + 150 * (SumReber - 2) + 256, 251),
                    new PointF(300 + 150 * (SumReber - 2) + 256, 266),
                    new PointF(300 + 150 * (SumReber - 2) + 262, 266),
                    new PointF(300 + 150 * (SumReber - 2) + 262, 264),
                    new PointF(300 + 150 * (SumReber - 2) + 275, 264),
                    new PointF(300 + 150 * (SumReber - 2) + 275, 261),
                    new PointF(300 + 150 * (SumReber - 2) + 293, 261),
                    new PointF(300 + 150 * (SumReber - 2) + 293, 259),
                    new PointF(300 + 150 * (SumReber - 2) + 299, 259),
                    new PointF(300 + 150 * (SumReber - 2) + 299, 258),
                    new PointF(300 + 150 * (SumReber - 2) + 293, 258),
                    new PointF(300 + 150 * (SumReber - 2) + 293, 256),
                    new PointF(300 + 150 * (SumReber - 2) + 275, 256),
                    new PointF(300 + 150 * (SumReber - 2) + 275, 253),
                    new PointF(300 + 150 * (SumReber - 2) + 263, 253),
                    new PointF(300 + 150 * (SumReber - 2) + 263, 251),
};
                graphics.FillPolygon(new SolidBrush(Color.Black), point);

                if (SumReber == 1)
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

                if (SumReber > 1)
                {
                    pen.Width = 3;
                    graphics.DrawLine(pen, new PointF(bitmap.Width - 300, 59), new PointF(bitmap.Width - 450, 59));

                    point = new PointF[]
                    {
                            new PointF(300 + 150 * (SumReber - 2) + 1, 59),
                            new PointF(300 + 150 * (SumReber - 2) + 1, 61),
                            new PointF(300 + 150 * (SumReber - 2) + 5, 61),
                            new PointF(300 + 150 * (SumReber - 2) + 5, 63),
                            new PointF(300 + 150 * (SumReber - 2) + 9, 63),
                            new PointF(300 + 150 * (SumReber - 2) + 9, 65),
                            new PointF(300 + 150 * (SumReber - 2) + 13, 65),
                            new PointF(300 + 150 * (SumReber - 2) + 13, 67),
                            new PointF(300 + 150 * (SumReber - 2) + 16, 67),
                            new PointF(300 + 150 * (SumReber - 2) + 16, 54),
                            new PointF(300 + 150 * (SumReber - 2) + 13, 54),
                            new PointF(300 + 150 * (SumReber - 2) + 13, 55),
                            new PointF(300 + 150 * (SumReber - 2) + 9, 55),
                            new PointF(300 + 150 * (SumReber - 2) + 9, 57),
                            new PointF(300 + 150 * (SumReber - 2) + 5, 57),
                            new PointF(300 + 150 * (SumReber - 2) + 5, 58)
                    };
                    graphics.FillPolygon(new SolidBrush(Color.Black), point);

                    point = new PointF[]
                    {
                            new PointF(300 + 150 * (SumReber - 2) + 149, 59),
                            new PointF(300 + 150 * (SumReber - 2) + 149, 61),
                            new PointF(300 + 150 * (SumReber - 2) + 145, 61),
                            new PointF(300 + 150 * (SumReber - 2) + 145, 63),
                            new PointF(300 + 150 * (SumReber - 2) + 141, 63),
                            new PointF(300 + 150 * (SumReber - 2) + 141, 65),
                            new PointF(300 + 150 * (SumReber - 2) + 137, 65),
                            new PointF(300 + 150 * (SumReber - 2) + 137, 67),
                            new PointF(300 + 150 * (SumReber - 2) + 134, 67),
                            new PointF(300 + 150 * (SumReber - 2) + 134, 54),
                            new PointF(300 + 150 * (SumReber - 2) + 137, 54),
                            new PointF(300 + 150 * (SumReber - 2) + 137, 55),
                            new PointF(300 + 150 * (SumReber - 2) + 141, 55),
                            new PointF(300 + 150 * (SumReber - 2) + 141, 57),
                            new PointF(300 + 150 * (SumReber - 2) + 145, 57),
                            new PointF(300 + 150 * (SumReber - 2) + 145, 58)
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

                    graphics.DrawString(Hight.ToString(),
                                        FontLibr.FindFont(graphics, Hight.ToString(), new System.Drawing.Size(80, 30), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(120, 126),
                                        stringFormat1);

                    graphics.DrawString(DistanceToFirst.ToString(),
                                        FontLibr.FindFont(graphics, DistanceToFirst.ToString(), new System.Drawing.Size(60, 25), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(300 + 150 * (SumReber - 2) + 205, 40),
                                        stringFormat2);

                    graphics.DrawString(ThicknessPlita.ToString(),
                                        FontLibr.FindFont(graphics, ThicknessPlita.ToString(), new System.Drawing.Size(60, 25), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(300 + 150 * (SumReber - 2) + 360, 180),
                                        stringFormat2);

                    if (SumReber > 1)
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
            using (Bitmap result = new Bitmap(png1.Width + png2.Width * (SumReber - 2) + png3.Width, 310))
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(png1, 0, 0);

                    for (int i = 1; i < SumReber; i++)
                    {
                        if (i == 1)
                            g.DrawImage(png2, 300, 0);
                        else
                            g.DrawImage(png2, i * 150, 0);
                    }
                    g.DrawImage(png3, (SumReber - 2) * 150 + 300, 0);
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
                case "d_rect":
                    return JoinPlita(new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImagePlitaStart"))),
                                     new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImagePlitaBody"))),
                                     new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImagePlitaEnd"))));

                case "d_slope_left":
                    return ForRobot.Libr.Converters.ImageConverter.BitmapToBitmapImage(GetTextSlopeLeft(new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImageSlopeLeft")), new System.Drawing.Size(1280, 720))));

                case "d_slope_right":
                    return ForRobot.Libr.Converters.ImageConverter.BitmapToBitmapImage(GetTextSlopeRight(new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImageSlopeRight")), new System.Drawing.Size(1280, 720))));

                case "d_trapezoid_top":
                    return ForRobot.Libr.Converters.ImageConverter.BitmapToBitmapImage(GetTextTrapezoidTop(new Bitmap(ForRobot.Libr.Converters.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImageTrapezoidTopParam")), new System.Drawing.Size(1280, 720))));

                case "d_trapezoid_bottom":
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

                    graphics.DrawString(Long.ToString(),
                                        FontLibr.FindFont(graphics, Long.ToString(), new System.Drawing.Size(100, 30), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(420, 440),
                                        stringFormat2);

                    //graphics.DrawString(Wight.ToString(),
                    //                    FontLibr.FindFont(graphics, Wight.ToString(), new System.Drawing.Size(100, 30), font),
                    //                    new SolidBrush(Color.Black), 
                    //                    new PointF(840, bitmap.Height/2),
                    //                    stringFormat1);

                    graphics.DrawString(DissolutionStart.ToString(),
                                        FontLibr.FindFont(graphics, DissolutionStart.ToString(), new System.Drawing.Size(48, 22), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(95, 430),
                                        stringFormat2);

                    graphics.DrawString(DissolutionEnd.ToString(),
                                        FontLibr.FindFont(graphics, DissolutionEnd.ToString(), new System.Drawing.Size(48, 22), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(710, 430),
                                        stringFormat2);

                    graphics.DrawString(ThicknessRebro.ToString(),
                                        FontLibr.FindFont(graphics, ThicknessRebro.ToString(), new System.Drawing.Size(40, 25), font),
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

                    graphics.DrawString(Long.ToString(),
                        FontLibr.FindFont(graphics, Long.ToString(), new System.Drawing.Size(250, 30), font),
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

                    graphics.DrawString(BevelToStart.ToString(),
                        FontLibr.FindFont(graphics, BevelToStart.ToString(), new System.Drawing.Size(100, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(105, 680),
                        stringFormatHorizont);

                    graphics.DrawString(BevelToEnd.ToString(),
                        FontLibr.FindFont(graphics, BevelToEnd.ToString(), new System.Drawing.Size(80, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1125, 150),
                        stringFormatHorizont);

                    graphics.DrawString(DistanceToStart.ToString(),
                        FontLibr.FindFont(graphics, DistanceToStart.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(215, 600),
                        stringFormatHorizont);

                    graphics.DrawString(DistanceToEnd.ToString(),
                        FontLibr.FindFont(graphics, DistanceToEnd.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1080, 600),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionStart.ToString(),
                        FontLibr.FindFont(graphics, DissolutionStart.ToString(), new System.Drawing.Size(80, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(320, 600),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionEnd.ToString(),
                        FontLibr.FindFont(graphics, DissolutionEnd.ToString(), new System.Drawing.Size(50, 30), font),
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

                    graphics.DrawString(Long.ToString(),
                        FontLibr.FindFont(graphics, Long.ToString(), new System.Drawing.Size(250, 30), font),
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

                    graphics.DrawString(BevelToStart.ToString(),
                        FontLibr.FindFont(graphics, BevelToStart.ToString(), new System.Drawing.Size(70, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(190, 130),
                        stringFormatHorizont);

                    graphics.DrawString(BevelToEnd.ToString(),
                        FontLibr.FindFont(graphics, BevelToEnd.ToString(), new System.Drawing.Size(100, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1220, 680),
                        stringFormatHorizont);

                    graphics.DrawString(DistanceToStart.ToString(),
                        FontLibr.FindFont(graphics, DistanceToStart.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(240, 600),
                        stringFormatHorizont);

                    graphics.DrawString(DistanceToEnd.ToString(),
                        FontLibr.FindFont(graphics, DistanceToEnd.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1105, 600),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionStart.ToString(),
                        FontLibr.FindFont(graphics, DissolutionStart.ToString(), new System.Drawing.Size(43, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(322, 600),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionEnd.ToString(),
                        FontLibr.FindFont(graphics, DissolutionEnd.ToString(), new System.Drawing.Size(140, 30), font),
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

                    graphics.DrawString(Long.ToString(),
                        FontLibr.FindFont(graphics, Long.ToString(), new System.Drawing.Size(250, 30), font),
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

                    graphics.DrawString(BevelToStart.ToString(),
                        FontLibr.FindFont(graphics, BevelToStart.ToString(), new System.Drawing.Size(80, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(150, 190),
                        stringFormatHorizont);

                    graphics.DrawString(BevelToEnd.ToString(),
                        FontLibr.FindFont(graphics, BevelToEnd.ToString(), new System.Drawing.Size(100, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1120, 140),
                        stringFormatHorizont);

                    graphics.DrawString(DistanceToStart.ToString(),
                        FontLibr.FindFont(graphics, DistanceToStart.ToString(), new System.Drawing.Size(125, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(200, 620),
                        stringFormatHorizont);

                    graphics.DrawString(DistanceToEnd.ToString(),
                        FontLibr.FindFont(graphics, DistanceToEnd.ToString(), new System.Drawing.Size(130, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1060, 620),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionStart.ToString(),
                        FontLibr.FindFont(graphics, DissolutionStart.ToString(), new System.Drawing.Size(50, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(295, 620),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionEnd.ToString(),
                        FontLibr.FindFont(graphics, DissolutionEnd.ToString(), new System.Drawing.Size(80, 30), font),
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

                    graphics.DrawString(Long.ToString(),
                        FontLibr.FindFont(graphics, Long.ToString(), new System.Drawing.Size(250, 30), font),
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

                    graphics.DrawString(BevelToStart.ToString(),
                        FontLibr.FindFont(graphics, BevelToStart.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(185, 150),
                        stringFormatHorizont);

                    graphics.DrawString(BevelToEnd.ToString(),
                        FontLibr.FindFont(graphics, BevelToEnd.ToString(), new System.Drawing.Size(100, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1130, 150),
                        stringFormatHorizont);

                    graphics.DrawString(DistanceToStart.ToString(),
                        FontLibr.FindFont(graphics, DistanceToStart.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(260, 640),
                        stringFormatHorizont);

                    graphics.DrawString(DistanceToEnd.ToString(),
                        FontLibr.FindFont(graphics, DistanceToEnd.ToString(), new System.Drawing.Size(110, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(1055, 640),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionStart.ToString(),
                        FontLibr.FindFont(graphics, DissolutionStart.ToString(), new System.Drawing.Size(45, 30), font),
                        new SolidBrush(Color.Black),
                        new PointF(343, 640),
                        stringFormatHorizont);

                    graphics.DrawString(DissolutionEnd.ToString(),
                        FontLibr.FindFont(graphics, DissolutionEnd.ToString(), new System.Drawing.Size(80, 30), font),
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
            using (Bitmap result = new Bitmap(980, png1.Height + png2.Height * 2 + png3.Height))
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

                        //    else if (SumReber % 2 == 0 && i == (SumReber / 2) + 1)
                        //        g.DrawImage(PaintDistanceBetween_And_WightImage(png2), i * 150, 0);
                        //    else if (SumReber % 2 == 1 && i == Math.Ceiling((decimal)SumReber / 2))
                        //        g.DrawImage(PaintDistanceBetween_And_WightImage(png2), i * 150, 0);
                        //    else
                        //        g.DrawImage(png2, i * 150, 0);
                    }

                    g.DrawImage(png3, 0, 2 * 88 + 150);

                    //Font font = new Font("Lucida Console", 18, System.Drawing.FontStyle.Regular);
                    //StringFormat stringFormat = new StringFormat(StringFormatFlags.DirectionVertical);
                    //g.DrawString(Long.ToString(), font, new SolidBrush(Color.Black), new PointF(835, result.Height/2 - Long.ToString().Length * (font.Size - 8)), stringFormat);
                }
                imageSource = ForRobot.Libr.Converters.ImageConverter.BitmapToBitmapImage(GetText(GetArrows(result)));
            }
            return imageSource;
        }

        #endregion

        #endregion
    }
}
