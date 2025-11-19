using System;
using System.Linq;

using ForRobot.Libr.Services.Providers;
using ForRobot.Models.Detals;
using ForRobot.Libr.Configuration;

namespace ForRobot.Libr.Factories.DetalFactory
{
    public class DetalFactory : IDetalFactory
    {
        private readonly IConfigurationProvider _configProvider;

        public DetalFactory(IConfigurationProvider configProvider)
        {
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        }

        public T CreateDetal<T>(DetalType type) where T : Detal
        {
            return (T)CreateDetal(type);
        }

        public Detal CreateDetal(DetalType type)
        {
            switch (type)
            {
                case DetalType.Plita:
                    return CreatePlita();

                default:
                    throw new ArgumentException($"Неизвестный тип детали: {type}", nameof(type));
            }
        }

        private Plita CreatePlita()
        {
            var plateConfig = _configProvider.GetPlitaConfig();
            return new Plita
            {
                ReverseDeflection = plateConfig.ReverseDeflection,
                PlateWidth = plateConfig.PlateWidth,
                PlateLength = plateConfig.PlateLength,
                PlateThickness = plateConfig.PlateThickness,
                PlateBevelToLeft = plateConfig.PlateBevelToLeft,
                PlateBevelToRight = plateConfig.PlateBevelToRight,

                RibsHeight = plateConfig.RibsHeight,
                RibsThickness = plateConfig.RibsThickness,
                RibsCount = plateConfig.RibsCount,
                DistanceToFirstRib = plateConfig.DistanceToFirstRib,
                DistanceBetweenRibs = plateConfig.DistanceBetweenRibs,
                RibsIdentToLeft = plateConfig.RibsIdentToLeft,
                RibsIdentToRight = plateConfig.RibsIdentToRight,
                WeldsDissolutionLeft = plateConfig.WeldsDissolutionLeft,
                WeldsDissolutionRight = plateConfig.WeldsDissolutionRight
            };
        }

        public void ClearCache()
        {
            if (_configProvider is CachedConfigurationProvider cachedProvider)
            {
                cachedProvider.ClearCache();
            }
        }
    }
}
