using System;

using Newtonsoft.Json.Schema;

namespace ForRobot.Libr.Services.Providers
{
    public interface IJsonSchemaProvider
    {
        JSchema GetPlitaSchema();
    }
}