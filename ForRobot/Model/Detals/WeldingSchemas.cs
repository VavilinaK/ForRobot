using System;
using System.Linq;
using System.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;

namespace ForRobot.Model.Detals
{
    /// <summary>
    /// Сторона ребра
    /// </summary>
    public enum SideOfRib
    {
        Left,

        Right
    }

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

        public class RibSide
        {
            public SideOfRib Side { get; set; }

            public string Number { get; set; }
        }

        public class SchemaRib
        {
            //public int RibNumber { private get; set; }

            public ObservableCollection<RibSide> Sides { get; set; } = new ObservableCollection<RibSide>();

            public SchemaRib()
            {
                //this.Line.Add((SideOfRib.Left, String.Empty));
                //this.Line.Add((SideOfRib.Right, String.Empty));

                this.Sides.Add(new RibSide() { Side = SideOfRib.Left, Number = String.Empty });
                this.Sides.Add(new RibSide() { Side = SideOfRib.Right, Number = String.Empty });
            }

            //public SchemaRib(int iRibNumber) : this ()
            //{
            //    this.RibNumber = iRibNumber;
            //}
        }               

        /// <summary>
        /// Сборка схемы варки рёбер
        /// </summary>
        /// <param name="typeSchema">Тип схемы</param>
        public static ObservableCollection<SchemaRib> BuildingSchema(SchemasTypes typeSchema)
        {
            switch (typeSchema)
            {
                case SchemasTypes.LeftEvenOdd_RightEvenOdd:
                    return null;

                default:
                    return null;
            }
        }

        public static string[,] GetSchema(SchemasTypes schemaType, ObservableCollection<SchemaRib> schema)
        {
            switch (schemaType)
            {
                case SchemasTypes.LeftEvenOdd_RightEvenOdd:
                    return null;

                //return GetLeftEvenOddRightEvenOddSchema();

                default:
                    return null;
            }
        }

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
        public static ObservableCollection<SchemaRib> SelectSchemaRib(int iCountOfRibs)
        {
            ObservableCollection<SchemaRib> schemaRibs = new System.Collections.ObjectModel.ObservableCollection<WeldingSchemas.SchemaRib>();
            for (int i = 0; i < iCountOfRibs; i++)
            {
                schemaRibs.Add(new WeldingSchemas.SchemaRib());
            }
            return schemaRibs;
        }

        //private static ObservableCollection<SchemaRib> BuildLeftEvenOddRightEvenOdd()
        //{

        //}

        //private static string[,] GetLeftEvenOddRightEvenOddSchema()
        //{

        //}
    }
}
