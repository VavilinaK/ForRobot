using System;

using Newtonsoft.Json.Serialization;

using ForRobot.Models.Detals;

namespace ForRobot.Libr.Factories.DetalFactory
{
    public interface IDetalFactory
    {
        T CreateDetal<T>(DetalType type) where T : Detal;
        Detal CreateDetal(DetalType type);

        T Deserialize<T>(string jsonString) where T : Detal;
        Detal Deserialize(string jsonString);

        string Serialize<T>(T detal, IContractResolver contractResolver = null) where T : Detal;
        string Serialize(Detal detal, IContractResolver contractResolver = null);

        void ClearCache();
    }
}
