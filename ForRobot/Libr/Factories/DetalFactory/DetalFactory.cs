using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;

using ForRobot.Libr.Json.Schemas;
using ForRobot.Libr.Services.Providers;
using ForRobot.Libr.Configuration;
using ForRobot.Models.Detals;

namespace ForRobot.Libr.Factories.DetalFactory
{
    public class DetalFactory : IDetalFactory, IDisposable
    {
        private readonly IConfigurationProvider _configProvider;
        private readonly IJsonSchemaProvider _jsonSchemaProvider;

        public DetalFactory(IConfigurationProvider configProvider, IJsonSchemaProvider jsonSchemaProvider)
        {
            this._configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            this._jsonSchemaProvider = jsonSchemaProvider ?? throw new ArgumentNullException(nameof(jsonSchemaProvider));
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

        private Plita DeserializePlate(string jsonString, JsonSerializerSettings settings)
        {
            if (string.IsNullOrEmpty(jsonString))
                return this.CreateDetal<Plita>(DetalType.Plita);
            else
                return JsonConvert.DeserializeObject<Plita>(jsonString, settings);
        }
                
        //private string GetJsonShemaPlate()
        //{
        //    var assembly = Assembly.GetExecutingAssembly();

        //    return string.Empty;
        //}

        //private bool ValidationJsonStringPlate(string jsonString)
        //{
        //    string schemaJson = this.GetJsonShemaPlate(); 
        //    return false;
        //}

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

        /// <summary>
        /// Десериализация и валидация
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public T Deserialize<T>(string jsonString) where T : Detal
        {
            this.ValidationJsonString<T>(jsonString);
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
                    this.ValidationJsonString<ForRobot.Models.Detals.Plita>(jsonString);
                    return this.DeserializePlate(jsonString, settings);

                default:
                    throw new ArgumentException($"Тип детали {detalType} не поддерживается", detalType);
            }
        }

        public string GetJsonShema<T>(DetalType type)
        {
            switch (type)
            {
                case DetalType.Plita:
                    return GetJsonShemaPlate();

                default:
                    throw new ArgumentException($"Тип детали {DetalTypes.EnumToString(type)} не поддерживается", nameof(type));
            }
        }

        public bool ValidationJsonString<T>(string jsonString) where T : Detal
        {
            JObject jsonObject = JObject.Parse(jsonString);
            JSchema schema = _jsonSchemaProvider.GetPlitaSchema();

            try
            {
                if (!jsonObject.IsValid(schema, out IList<string> errors))
                {
                    throw new JsonSchemaValidationException(schema.Title ?? "Unknown Schema", errors, jsonString);
                }
                //switch (detalType)
                //{
                //    case DetalTypes.Plita:
                //        return this.ValidationJsonStringPlate(jsonString);

                //    default:
                //        throw new ArgumentException($"Тип детали {detalType} не поддерживается", detalType);
                //}
            }
            catch (JsonSchemaValidationException) { throw; }
            catch (JsonReaderException ex)
            {
                var errors = new List<ValidationErrorInfo>
                {
                    new ValidationErrorInfo
                    {
                        Message = $"Deserialization failed: {ex.Message}",
                        Path = "ROOT"
                    }
                };
                throw new JsonSchemaValidationException(schema.Title ?? "Unknown Schema", errors, json, ex);
            }
        }

        public void ClearCache()
        {
            if (_configProvider is CachedConfigurationProvider cachedConfigProvider)
            {
                cachedConfigProvider.ClearCache();
            }

            if (_jsonSchemaProvider is ForRobot.Libr.Json.Schemas.CachedJsonSchemaProvider cachedJsonProvider)
            {
                cachedJsonProvider.ClearCache();
            }
        }

        public void Dispose() => this.ClearCache();

        #endregion Public functions

        ~DetalFactory() => this.Dispose();
    }
}
