using System;

using Newtonsoft.Json;

namespace ForRobot.Libr.Converters
{
    /// <summary>
    /// Класс преобразования добавления комментария к <see cref="JsonPropertyAttribute"/>
    /// </summary>
    public class JsonCommentConverter : JsonConverter
    {
        #region Public variables

        private readonly string _comment;

        public override bool CanRead => false;
        public override bool CanConvert(Type objectType) => true;

        #region Override

        #endregion

        #endregion

        #region Constuctor

        public JsonCommentConverter(string comment)
        {
            _comment = comment;
        }

        #endregion

        #region Private functions

        #region Override
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
            writer.WriteComment(_comment);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}
