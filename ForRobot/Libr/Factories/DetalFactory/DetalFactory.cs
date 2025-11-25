using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;

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

        #region Private functions

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

        private Plita DeserializePlate(string jsonString, JsonSerializerSettings settings, JsonSchema schema)
        {
            if (string.IsNullOrEmpty(jsonString))
                return this.CreateDetal<Plita>(DetalType.Plita);
            else
                return JsonConvert.DeserializeObject<Plita>(jsonString, settings);
        }

        #endregion Private functions

        #region Public functions

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
                    throw new ArgumentException($"Тип детали {DetalTypes.EnumToString(type)} не поддерживается", nameof(type));
            }
        }

        public T Deserialize<T>(string jsonString) where T : Detal
        {
            return (T)Deserialize(jsonString);
        }

        public Detal Deserialize(string jsonString)
        {
            var settings = new JsonSerializerSettings()
            {
                Error = (s, e) => 
                {
                    var obj = e.CurrentObject as Detal;

                    string message = string.Empty;
                    if (obj == null)
                         message = e.ErrorContext.Error.Message;
                    else
                        message = string.Format("Ошибка десериализации объекта {0}: {1}", obj.GetType(), e.ErrorContext.Error.Message);

                    e.ErrorContext.Handled = true;
                    throw new Exception(message);
                }
            };

            string detalType = Newtonsoft.Json.Linq.JObject.Parse(jsonString)[nameof(Detal.DetalType)].ToString();

            switch (detalType)
            {
                case DetalTypes.Plita:
                    return this.DeserializePlate(jsonString, settings, null);

                default:
                    throw new ArgumentException($"Тип детали {detalType} не поддерживается", detalType);
            }
        }

        public void ClearCache()
        {
            if (_configProvider is CachedConfigurationProvider cachedProvider)
            {
                cachedProvider.ClearCache();
            }
        }

        #endregion Public functions
    }
}
