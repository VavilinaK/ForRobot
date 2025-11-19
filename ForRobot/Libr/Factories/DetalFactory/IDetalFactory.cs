using System;

using ForRobot.Models.Detals;

namespace ForRobot.Libr.Factories.DetalFactory
{
    public interface IDetalFactory
    {
        T CreateDetal<T>(DetalType type) where T : Detal;
        Detal CreateDetal(DetalType type);
        void ClearCache();
    }
}
