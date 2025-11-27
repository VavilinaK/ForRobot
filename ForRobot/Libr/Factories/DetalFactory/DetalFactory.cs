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

        private void HandleSerializeringError(object sender, ErrorEventArgs e)
        {
            var obj = e.CurrentObject as Detal;
            string message = string.Empty;
            if (obj == null)
                message = e.ErrorContext.Error.Message;
            else
                message = string.Format("Ошибка десериализации объекта {0}: {1}", obj.GetType(), e.ErrorContext.Error.Message);
            e.ErrorContext.Handled = true;
            throw new JsonSerializationException(message);
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

        /// <summary>
        /// Десериализация и валидация
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public T Deserialize<T>(string jsonString) where T : Detal
        {
            return (T)Deserialize(jsonString);
        }

        /// <summary>
        /// Десериализация и валидация
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public Detal Deserialize(string jsonString)
        {
            var settings = new JsonSerializerSettings()
            {
                Error = HandleSerializeringError
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

        public string Serialize<T>(T detal, IContractResolver contractResolver = null) where T : Detal
        {
            return Serialize(detal as Detal, contractResolver);
        }

        public string Serialize(Detal detal, IContractResolver contractResolver = null)
        {
            string jsonString = string.Empty;

            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = contractResolver ?? new DefaultContractResolver(),
                Error = HandleSerializeringError
            };

            switch (detal.DetalType)
            {
                case DetalTypes.Plita:
                    this.ValidationJsonString<ForRobot.Models.Detals.Plita>(jsonString);
                    //return this.DeserializePlate(jsonString, settings);
                    break;

                default:
                    throw new ArgumentException($"Тип детали {detal.DetalType} не поддерживается", detal.DetalType);
            }

            return JsonConvert.SerializeObject(detal, settings);
        }

        public bool ValidationJsonString<T>(string jsonString) where T : Detal
        {
            string schemaTitle = "Unknown Schema";
            try
            {
                JObject jsonObject = JObject.Parse(jsonString);
                JSchema schema = _jsonSchemaProvider.GetPlitaSchema();
                schemaTitle = schema.Title ?? schemaTitle;
                var validationErrors = new List<ValidationErrorInfo>();
                
                // Сбор ошибок валидации
                jsonObject.Validate(schema, (sender, args) =>
                {
                    validationErrors.Add(new ValidationErrorInfo
                    {
                        Message = args.Message,
                        Path = args.Path,
                        ErrorDetails = args
                    });
                });

                if (validationErrors.Count > 0)
                {
                    throw new JsonSchemaValidationException(schemaTitle, validationErrors, jsonString);
                }
            }
            catch (JsonReaderException ex)
            {
                var errors = new List<ValidationErrorInfo>
                {
                    new ValidationErrorInfo
                    {
                        Message = $"Deserialization failed: {ex.Message}",
                        Path = ex.Path
                    }
                };
                throw new JsonSchemaValidationException(schemaTitle, errors, jsonString, ex);
            }
            return true;
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
