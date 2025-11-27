using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;

namespace ForRobot.Libr.Json.Schemas
{
    /// <summary>
    /// Предоставляет доступ к .json файлам сборки
    /// </summary>
    public static class JsonManager
    {
        /// <summary>
        /// Свойства json-schema, которые могут содержать тип
        /// </summary>
        private static string[] _titleProperties = new string[] { "meta:targetClass", "meta:namespace", "className", "fullName", "x-target-type", "x-class-name", "x-full-name" };

        /// <summary>
        /// Возвращает схему для класса, если её класс является Embedded Resource
        /// </summary>
        /// <returns></returns>
        public static string GetSchema<T>()
        {
            Type targetType = typeof(T);
            return GetSchema(targetType);
        }

        /// <summary>
        /// Возвращает схему для класса, если её класс является Embedded Resource
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetSchema(Type type)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcesNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var name in resourcesNames)
            {
                using (Stream stream = assembly.GetManifestResourceStream(name))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string schemaJson = reader.ReadToEnd();
                    string schemaTarget = GetSchemaTargetType(schemaJson);

                    if (type.FullName == schemaTarget || type.Name == schemaTarget)
                        return schemaJson;
                }
            }
            throw new Exception(string.Format("В сборке не найдена json-схема для типа {0}.", type));
        }

        public static string GetSchemaTargetType(string schemaJson)
        {
            JSchema schema = JSchema.Parse(schemaJson);

            List<string> allSchemaValues = new List<string>();
            if (schema.Properties != null)
                allSchemaValues.AddRange(schema.Properties.Keys);
            if (schema.Required != null)
                allSchemaValues.AddRange(schema.Required);

            //JObject schema = JObject.Parse(schemaJson);

            //string targetType = schema["title"]?.ToString() ??
            //                    schema["x-class-name"]?.ToString() ??
            //                    schema["x-full-name"]?.ToString();

            string targetType = allSchemaValues.Where(item => _titleProperties.Contains(item)).FirstOrDefault();

            if (string.IsNullOrEmpty(targetType))
                throw new Exception("В json-схеме не найдено свойство обозначающее класс объекта.");
            else
                return targetType;
        }

    }
}
