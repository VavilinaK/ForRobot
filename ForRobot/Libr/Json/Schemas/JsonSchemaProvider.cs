using System;

using Newtonsoft.Json.Schema;

namespace ForRobot.Libr.Json.Schemas
{
    public class JsonSchemaProvider : ForRobot.Libr.Services.Providers.IJsonSchemaProvider
    {
        public JSchema GetPlitaSchema() => SelectSchema<ForRobot.Models.Detals.Plita>();

        private JSchema SelectSchema<T>()
        {
            var schema = JsonManager.GetSchema<T>();

            if (schema == null)
                throw new JSchemaException(string.Format("Json-схема для '{0}' не найдена", typeof(T).FullName));

            return JSchema.Parse(schema);
        }
    }
}
