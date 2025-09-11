using System;
using System.IO;
using System.Xaml;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ForRobot.Libr.Json
{
    public class ImageSourceConverter : JsonConverter<System.Windows.Media.ImageSource>
    {
        public enum ImageType
        {
            Raster,
            Vector
        }

        public class ImageSourceData
        {
            public ImageType Type { get; set; }
            public string Data { get; set; }
        }

        public override void WriteJson(JsonWriter writer, ImageSource value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            try
            {
                // Определяем тип изображения
                var result = new ImageSourceData();

                if (value is BitmapSource bitmapSource)
                {
                    // Растровое изображение
                    result.Type = ImageType.Raster;
                    result.Data = ConvertBitmapSourceToBase64(bitmapSource);
                }
                else if (value is DrawingImage drawingImage)
                {
                    // Векторное изображение (XAML)
                    result.Type = ImageType.Vector;
                    result.Data = ConvertDrawingImageToXaml(drawingImage);
                }
                else
                {
                    // Другие типы - попробуем преобразовать в растровое
                    result.Type = ImageType.Raster;
                    result.Data = ConvertImageSourceToBase64(value);
                }

                // Сериализуем объект с информацией о типе и данных
                serializer.Serialize(writer, result);
            }
            catch (Exception ex)
            {
                writer.WriteNull();
                Console.WriteLine($"Ошибка при сериализации ImageSource: {ex.Message}");
            }
        }

        public override ImageSource ReadJson(JsonReader reader, Type objectType, ImageSource existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            try
            {
                // Десериализуем объект с информацией о типе и данных
                var data = serializer.Deserialize<ImageSourceData>(reader);

                if (data == null || string.IsNullOrEmpty(data.Data))
                    return null;

                // Восстанавливаем изображение в зависимости от типа
                if (data.Type == ImageType.Raster)
                {
                    return ConvertBase64ToImageSource(data.Data);
                }
                else if (data.Type == ImageType.Vector)
                {
                    return ConvertXamlToDrawingImage(data.Data);
                }
                else
                {
                    // По умолчанию пытаемся обработать как растровое изображение
                    return ConvertBase64ToImageSource(data.Data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при десериализации ImageSource: {ex.Message}");
                return null;
            }
        }
        
        #region Методы для работы с растровыми изображениями

        private string ConvertBitmapSourceToBase64(BitmapSource bitmapSource)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using (var memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }

        private string ConvertImageSourceToBase64(ImageSource imageSource)
        {
            if (imageSource is BitmapSource bitmapSource)
            {
                return ConvertBitmapSourceToBase64(bitmapSource);
            }
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(imageSource, new Rect(0, 0, imageSource.Width, imageSource.Height));
            }

            var renderTargetBitmap = new RenderTargetBitmap(
                (int)imageSource.Width, (int)imageSource.Height,
                96, 96, PixelFormats.Pbgra32);

            renderTargetBitmap.Render(drawingVisual);
            return ConvertBitmapSourceToBase64(renderTargetBitmap);
        }

        private ImageSource ConvertBase64ToImageSource(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (var memoryStream = new MemoryStream(imageBytes))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        #endregion

        #region Методы для работы с векторными изображениями

        private string ConvertDrawingImageToXaml(DrawingImage drawingImage)
        {
            try
            {
                return System.Windows.Markup.XamlWriter.Save(drawingImage);
            }
            catch
            {
                return ConvertImageSourceToBase64(drawingImage);
            }
        }

        private ImageSource ConvertXamlToDrawingImage(string xamlString)
        {
            try
            {
                return (ImageSource)System.Windows.Markup.XamlReader.Parse(xamlString);
            }
            catch
            {
                return ConvertBase64ToImageSource(xamlString);
            }
        }

        #endregion
    }
    
    public class UniversalImageSourceContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            
            if (typeof(ImageSource).IsAssignableFrom(property.PropertyType))
            {
                property.Converter = new ImageSourceConverter();
                property.ShouldSerialize = instance =>
                {
                    var value = member is PropertyInfo pi
                        ? pi.GetValue(instance)
                        : ((FieldInfo)member).GetValue(instance);
                    return value != null;
                };
            }

            return property;
        }
    }
}
