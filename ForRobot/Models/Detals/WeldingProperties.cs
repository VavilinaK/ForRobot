using System;
using System.Runtime.CompilerServices;
using System.ComponentModel;

using Newtonsoft.Json;

using ForRobot.Libr.Converters;
using ForRobot.Libr.Collections;

namespace ForRobot.Models.Detals
{
    public class WeldingProperties
    {
        #region Private variables

        private decimal _searchOffsetStart;
        private decimal _searchOffsetEnd;
        private decimal _techOffsetSeamStart;
        private decimal _techOffsetSeamEnd;
        private decimal _seamsOverlap;
        private int _programNom;
        private int _weldingSpead;
        private decimal _distanceForSearch;
        private decimal _distanceForWelding;
        private WeldingSchemas.SchemasTypes _selectedWeldingSchema = WeldingSchemas.SchemasTypes.Edit;
        private FullyObservableCollection<WeldingSchemas.SchemaItem> _weldingSchema;

        #endregion Private variables

        #region Public variables

        [JsonProperty("search_offset_start")]
        [JsonConverter(typeof(JsonCommentConverter), "Отступ поиска в начале шва")]
        /// <summary>
        /// Отступ поиска в начале шва
        /// </summary>
        public decimal SearchOffsetStart
        {
            get => this._searchOffsetStart;
            set
            {
                this._searchOffsetStart = value;
                this.OnChangeProperty(nameof(this.SearchOffsetStart));
            }
        }

        [JsonProperty("search_offset_end")]
        [JsonConverter(typeof(JsonCommentConverter), "Отступ поиска в конце шва")]
        /// <summary>
        /// Отступ поиска в конце шва
        /// </summary>
        public decimal SearchOffsetEnd
        {
            get => this._searchOffsetEnd;
            set
            {
                this._searchOffsetEnd = value;
                this.OnChangeProperty(nameof(this.SearchOffsetEnd));
            }
        }

        [JsonProperty("weld_tech_offset_start")]
        [JsonConverter(typeof(JsonCommentConverter), "Технологический отступ начала шва")]
        /// <summary>
        /// Технологический отступ начала шва
        /// </summary>
        public decimal TechOffsetSeamStart
        {
            get => this._techOffsetSeamStart;
            set
            {
                this._techOffsetSeamStart = value;
                this.OnChangeProperty(nameof(this.TechOffsetSeamStart));
            }
        }

        [JsonProperty("weld_tech_offset_end")]
        [JsonConverter(typeof(JsonCommentConverter), "Технологический отступ конца шва")]
        /// <summary>
        /// Технологический отступ конца шва
        /// </summary>
        public decimal TechOffsetSeamEnd
        {
            get => this._techOffsetSeamEnd;
            set
            {
                this._techOffsetSeamEnd = value;
                this.OnChangeProperty(nameof(this.TechOffsetSeamEnd));
            }
        }

        [JsonProperty("weld_overlap")]
        [JsonConverter(typeof(JsonCommentConverter), "Перекрытие швов в месте соединения")]
        /// <summary>
        /// Перекрытие швов в месте соединения
        /// </summary>
        public decimal SeamsOverlap
        {
            get => this._seamsOverlap;
            set
            {
                this._seamsOverlap = value;
                this.OnChangeProperty(nameof(this.SeamsOverlap));
            }
        }

        [JsonProperty("weld_job")]
        [JsonConverter(typeof(JsonCommentConverter), "Номер используемого джоба (ячейки) на источнике")]
        /// <summary>
        /// Номер сварочной программы
        /// </summary>
        public int ProgramNom
        {
            get => this._programNom;
            set
            {
                this._programNom = value;
                this.OnChangeProperty(nameof(this.ProgramNom));
            }
        }

        [JsonProperty("weld_velocity")]
        [JsonConverter(typeof(JsonCommentConverter), "Скорость сварки (обратно пропорциональна получаемому катету)")]
        /// <summary>
        /// Скорость сварки
        /// </summary>
        public int WeldingSpead
        {
            get => this._weldingSpead;
            set
            {
                this._weldingSpead = value;
                this.OnChangeProperty(nameof(this.WeldingSpead));
            }
        }

        [JsonProperty("gantry_radius_weld")]
        [JsonConverter(typeof(JsonCommentConverter), "Расстояние между фланцем робота и позиционера на сварке для расчёта положения позиционера")]
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

        [JsonProperty("gantry_radius_weld")]
        [JsonConverter(typeof(JsonCommentConverter), "Расстояние между фланцем робота и позиционера на сварке для расчёта положения позиционера")]
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

        [JsonConverter(typeof(JsonCommentConverter), "Выбранная схема сварки рёбер")]
        /// <summary>
        /// Выбранная схема сварки рёбер
        /// </summary>
        public WeldingSchemas.SchemasTypes SelectedWeldingSchema
        {
            get => this._selectedWeldingSchema;
            set
            {
                this._selectedWeldingSchema = value;
                this.OnChangeProperty(nameof(this.SelectedWeldingSchema));
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Схема сварки (в какой очерёдности будут накладываться сварные швы)
        /// </summary>
        public FullyObservableCollection<WeldingSchemas.SchemaItem> WeldingSchema
        {
            get => this._weldingSchema;
            set
            {
                this._weldingSchema = value;
                this.OnChangeProperty(nameof(this.WeldingSchema));
            }
        }

        #region Events

        /// <summary>
        /// Событие изменения параметра детали
        /// </summary>
        public event PropertyChangedEventHandler ChangePropertyEvent;

        #endregion Events

        public WeldingProperties()
        {
            this.ChangePropertyEvent += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(this.WeldingSchema):
                        this.WeldingSchema.ItemPropertyChanged += HandlerItemPropertyChanged;
                        break;
                }
            };

            this.WeldingSchema = ForRobot.Models.Detals.WeldingSchemas.BuildingSchema(this.SelectedWeldingSchema, Plita.MIN_RIB_COUNT);
        }

        #endregion Public variables

        private void HandlerItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            this.SelectedWeldingSchema = WeldingSchemas.SchemasTypes.Edit;
        }

        /// <summary>
        /// Вызов события изменения свойства
        /// </summary>
        /// <param name="propertyName">Наименование свойства</param>
        private void OnChangeProperty([CallerMemberName] string propertyName = null) => this.ChangePropertyEvent?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        //public static FullyObservableCollection<WeldingSchemas.SchemaItem> FillWeldingSchema(WeldingSchemas.SchemasTypes schemasType, int weldsCount)
        //{
        //    FullyObservableCollection<WeldingSchemas.SchemaItem> schema = ForRobot.Models.Detals.WeldingSchemas.BuildingSchema(schemasType, weldsCount);
        //    schema.ItemPropertyChanged += (s, e) =>
        //    {
        //        if (this.SelectedWeldingSchema != WeldingSchemas.GetDescription(WeldingSchemas.SchemasTypes.Edit))
        //            this.SelectedWeldingSchema = ForRobot.Models.Detals.WeldingSchemas.GetDescription(WeldingSchemas.SchemasTypes.Edit);

        //        this.OnChangeProperty(nameof(this.WeldingSchema));
        //    };
        //    return schema;
        //}
    }
}
