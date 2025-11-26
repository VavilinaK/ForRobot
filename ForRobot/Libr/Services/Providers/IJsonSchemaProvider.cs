using System;

using ForRobot.Libr.Json.Schemas;

namespace ForRobot.Libr.Services.Providers
{
    public interface IJsonSchemaProvider
    {
        PlateJsonSchemaSection GetPlitaJsonSchema();
    }
}