using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json.Schema;

namespace ForRobot.Libr.Json.Schemas
{
    public class ValidationErrorInfo
    {
        public string Message { get; set; }
        public string Path { get; set; }
        public SchemaValidationEventArgs ErrorDetails { get; set; }

        public override string ToString()
        {
            return $"Path: {Path}, Message: {Message}";
        }
    }

    public class JsonSchemaValidationException : Exception
    {
        public IList<ValidationErrorInfo> ValidationErrors { get; }
        public string JsonData { get; }
        public string SchemaTitle { get; }

        public JsonSchemaValidationException(string schemaTitle, IList<ValidationErrorInfo> errors, string jsonData)
                : base($"JSON validation failed for schema '{schemaTitle}'. Errors: {string.Join("; ", errors.Select(item => item.Message))}")
        {
            ValidationErrors = errors;
            JsonData = jsonData;
            SchemaTitle = schemaTitle;
        }

        public JsonSchemaValidationException(string schemaTitle, IList<ValidationErrorInfo> errors, string jsonData, Exception innerException)
                : base($"JSON validation failed for schema '{schemaTitle}'. Errors: {string.Join("; ", errors.Select(item => item.Message))}", innerException)
        {
            ValidationErrors = errors;
            JsonData = jsonData;
            SchemaTitle = schemaTitle;
        }
    }
}
