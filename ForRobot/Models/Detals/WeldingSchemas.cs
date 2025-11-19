using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ForRobot.Libr.Collections;

namespace ForRobot.Models.Detals
{
    public static class WeldingSchemas
    {
        /// <summary>
        /// Перечисление схем сварки
        /// </summary>
        public enum SchemasTypes
        {
            [Description("Левые четные-нечётные, правые чётные-нечетные")]
            /// <summary>
            /// Схема сварки сначала слева чётных-нечётных, потом справа чётных-неяётных рёбер
            /// </summary>
            LeftEvenOdd_RightEvenOdd = 0,

            [Description("Редактировать")]
            /// <summary>
            /// Пользовательская схема
            /// </summary>
            Edit = 1
        }

        /// <summary>
        /// Коллекция описаний схем сварки
        /// </summary>
        public static IEnumerable<string> WeldingSchemasCollection { get; } = GetSchemasCollection();

        private static IEnumerable<string> GetSchemasCollection()
        {
            var Descriptions = typeof(SchemasTypes).GetFields().Select(field => field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).SingleOrDefault() as System.ComponentModel.DescriptionAttribute);
            List<string> DescriptionList = Descriptions.Where(item => item != null).Select(item => item.Description).ToList<string>();
            return DescriptionList;
        }

        public class SchemaRib : INotifyPropertyChanged
        {
            #region Private variables

            private string _leftSide;

            private string _rightSide;

            #endregion

            #region Private variables

            public string LeftSide
            {
                get => this._leftSide ?? (this._leftSide = "-");
                set
                {
                    this._leftSide = value;
                    this.Change?.Invoke(this, null);
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LeftSide)));
                }
            }

            public string RightSide
            {
                get => this._rightSide ?? (this._rightSide = "-");
                set
                {
                    this._rightSide = value;
                    this.Change?.Invoke(this, null);
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RightSide)));
                }
            }

            #endregion

            #region Constructor

            public SchemaRib() { }

            #endregion

            #region Events

            public event EventHandler Change;

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion

            #region Public functions

            #endregion
        }

        /// <summary>
        /// Сборка схемы варки рёбер
        /// </summary>
        /// <param name="typeSchema">Тип схемы</param>
        public static FullyObservableCollection<SchemaRib> BuildingSchema(SchemasTypes typeSchema, int iSumRib)
        {
            switch (typeSchema)
            {
                case SchemasTypes.LeftEvenOdd_RightEvenOdd:
                    return BuildLeftEvenOddRightEvenOdd(iSumRib);

                default:
                    return SelectSchemaRib(iSumRib);
            }
        }

        /// <summary>
        /// Вывод схемы для передачи
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="schema">Схема сварки</param>
        /// <returns></returns>
        public static object[,] GetSchema(ObservableCollection<SchemaRib> schema)
        {
            object[,] finishSchema = new object[schema.Count * 2, 2];
            for (int i = 1; i <= (schema.Count * 2); i++)
            {
                SchemaRib rib = schema.Where(item => item.LeftSide == i.ToString() || item.RightSide == i.ToString())?.Count() == 0 ? null
                                : schema.Where(item => item.LeftSide == i.ToString() || item.RightSide == i.ToString())?.First();
                if (rib == null)
                    throw new Exception(string.Format("При составлении схемы сварки не найдена очерёдность №{0}", i));
                finishSchema[i - 1, 0] = schema.IndexOf(rib) + 1;
                finishSchema[i - 1, 1] = (rib.LeftSide == i.ToString()) ? "left_side" : "right_side";
            }
            return finishSchema;
        }

        /// <summary>
        /// Возвращение элемента <see cref="SchemasTypes"/>
        /// </summary>
        /// <param name="description">Атрибут описания элемента перечисления <see cref="SchemasTypes"/></param>
        /// <returns></returns>
        public static SchemasTypes GetSchemaType(string description)
        {            
            var enums = typeof(WeldingSchemas.SchemasTypes).GetFields();
            var descriptions = enums.Select(field => new { Name = field.Name,  Description = (field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).SingleOrDefault() as System.ComponentModel.DescriptionAttribute)?.Description });

            return (WeldingSchemas.SchemasTypes)Enum.Parse(typeof(WeldingSchemas.SchemasTypes), descriptions.Where(item => item.Description == description).First().Name);
        }

        /// <summary>
        /// Возврат атрибута <see cref="System.ComponentModel.DescriptionAttribute"/> перечисления <see cref="ShemasTypes"/>
        /// </summary>
        /// <param name="shemasTypes">Элемент перечисления <see cref="ShemasTypes"/></param>
        /// <returns></returns>
        public static string GetDescription(SchemasTypes shemaType)
        {
            var enums = typeof(WeldingSchemas.SchemasTypes).GetFields();
            var descriptions = enums.Select(field => new { field.Name, Description = (field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).SingleOrDefault() as System.ComponentModel.DescriptionAttribute)?.Description });

            return descriptions.Where(item => item.Name == shemaType.ToString()).First().Description;
        }

        /// <summary>
        /// Заполнение схемы рёбрами
        /// </summary>
        /// <param name="iCountOfRibs">Кол-во рёбер</param>
        /// <returns></returns>
        public static FullyObservableCollection<SchemaRib> SelectSchemaRib(int iCountOfRibs)
        {
            FullyObservableCollection<SchemaRib> schemaRibs = new FullyObservableCollection<WeldingSchemas.SchemaRib>();
            for (int i = 0; i < iCountOfRibs; i++)
            {
                schemaRibs.Add(new WeldingSchemas.SchemaRib());
            }
            return schemaRibs;
        }

        /// <summary>
        /// Заполнение схемы: сначала слева четные - нечетные, справа четные - нечетные
        /// </summary>
        /// <param name="iSumRib">Кол-во рёбер</param>
        /// <returns></returns>
        private static FullyObservableCollection<SchemaRib> BuildLeftEvenOddRightEvenOdd(int iSumRib)
        {
            List<SchemaRib> ribs = SelectSchemaRib(iSumRib).ToList<SchemaRib>() ;
            int i = 1;
            while (i <= (iSumRib * 2))
            {
                foreach (var rib in ribs.Where(item => (ribs.IndexOf(item) + 1) % 2 == 0))
                {
                    rib.LeftSide = i.ToString();
                    i++;
                }

                foreach (var rib in ribs.Where(item => (ribs.IndexOf(item) + 1) % 2 != 0))
                {
                    rib.LeftSide = i.ToString();
                    i++;
                }

                foreach (var rib in ribs.Where(item => (ribs.IndexOf(item) + 1) % 2 == 0))
                {
                    rib.RightSide = i.ToString();
                    i++;
                }

                foreach (var rib in ribs.Where(item => (ribs.IndexOf(item) + 1) % 2 != 0))
                {
                    rib.RightSide = i.ToString();
                    i++;
                }
            }
            return new FullyObservableCollection<SchemaRib>(ribs);
        }
    }
}
