using System;
using System.Runtime.CompilerServices;
using System.ComponentModel;

using Newtonsoft.Json;

using ForRobot.Libr.Converters;
using ForRobot.Libr.Collections;

namespace ForRobot.Model.Detals
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
        private string _selectedWeldingSchema = WeldingSchemas.GetDescription(WeldingSchemas.SchemasTypes.Edit);

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

        [JsonIgnore]
        //[SaveAttribute]
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
                    this.WeldingSchema = this.FillWeldingSchema(this.SelectedWeldingSchema);

                this.OnChangeProperty(nameof(this.SelectedWeldingSchema));
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Схема сварки (в какой очерёдности будут накладываться сварные швы)
        /// </summary>
        public FullyObservableCollection<WeldingSchemas.SchemaRib> WeldingSchema { get; private set; }

        #region Events

        /// <summary>
        /// Событие изменения параметра детали
        /// </summary>
        public event PropertyChangedEventHandler ChangePropertyEvent;

        #endregion Events

        #endregion Public variables

        //public FullyObservableCollection<WeldingSchemas.SchemaRib> FillWeldingSchema()
        //{
        //    if (string.IsNullOrEmpty(this.SelectedWeldingSchema) || )
        //        throw new ArgumentNullException(nameof(this.SelectedWeldingSchema), "Неверный формат схемы сварки");

        //    FullyObservableCollection<WeldingSchemas.SchemaRib> schema = ForRobot.Model.Detals.WeldingSchemas.BuildingSchema(ForRobot.Model.Detals.WeldingSchemas.GetSchemaType(this.SelectedWeldingSchema), base.RibsCount);
        //    schema.ItemPropertyChanged += (s, e) =>
        //    {
        //        if (this.SelectedWeldingSchema != WeldingSchemas.GetDescription(WeldingSchemas.SchemasTypes.Edit))
        //            this.SelectedWeldingSchema = ForRobot.Model.Detals.WeldingSchemas.GetDescription(WeldingSchemas.SchemasTypes.Edit);

        //        this.OnChangeProperty(nameof(this.WeldingSchema));
        //    };
        //    return schema;
        //}

        /// <summary>
        /// Вызов события изменения свойства
        /// </summary>
        /// <param name="propertyName">Наименование свойства</param>
        public virtual void OnChangeProperty([CallerMemberName] string propertyName = null) => this.ChangePropertyEvent?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
