using System;
using System.IO;
using System.Text;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ForRobot.Libr.Json.Schemas
{
    /// <summary>
    /// Предоставляет доступ к .json файлам сборки
    /// </summary>
    public static class JsonManager
    {
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
            return null;
        }

        public static string GetSchemaTargetType(string schemaJson)
        {
            JObject schema = JObject.Parse(schemaJson);
            
            string targetType = schema["title"]?.ToString() ??
                                schema["x-class-name"]?.ToString() ??
                                schema["x-full-name"]?.ToString();

            return targetType;
        }

    }
}
