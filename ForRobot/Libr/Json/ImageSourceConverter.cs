using System;
using System.IO;
using System.Xaml;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

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

        public override ImageSource ReadJson(JsonReader reader, Type objectType, ImageSource existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            //if (reader.TokenType == JsonToken.Null)
            //    return null;

            //try
            //{
            //    ImageSourceData data = serializer.Deserialize<ImageSourceData>(reader);

            //    if (data == null || string.IsNullOrEmpty(data.Data))
            //        return null;

            //    //// Восстанавливаем изображение в зависимости от типа
            //    //if (data.Type == "Raster")
            //    //{
            //    //    return ConvertBase64ToImageSource(data.Data);
            //    //}
            //    //else if (data.Type == "Vector")
            //    //{
            //    //    return ConvertXamlToDrawingImage(data.Data);
            //    //}
            //    //else
            //    //{
            //    //    // По умолчанию пытаемся обработать как растровое изображение
            //    //    return ConvertBase64ToImageSource(data.Data);
            //    //}
            //}
            //catch(Exception ex)
            //{
                throw new JsonSerializationException($"Ошибка при десериализации ImageSource: ");
            //}
        }

        public override void WriteJson(JsonWriter writer, ImageSource value, JsonSerializer serializer)
        {
            if(value == null)
            {
                writer.WriteNull();
            }
            try
            {
                ImageSourceData result = new ImageSourceData();
                switch (value)
                {
                    case BitmapImage bitmapSource:
                        result.Type = ImageType.Raster;
                        result.Data = ConvertBitmapSourceToBase64(bitmapSource);
                        break;

                    case DrawingImage drawingImage:
                        result.Type = ImageType.Vector;
                        //result.Data = ConvertDrawingImageToBase64(drawingImage);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                writer.WriteNull();
                throw new JsonSerializationException($"Ошибка при сериализации ImageSource: {ex.Message}");
            }
        }

        #region Private functions

        private string ConvertBitmapSourceToBase64(BitmapSource bitmapSource)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            using(MemoryStream memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }

        //private string ConvertDrawingImageToBase64(DrawingImage drawingImage)
        //{
        //    try
        //    {
        //        return XamlWriter
        //    }
        //    catch ()
        //    {

        //    }
        //}

        #endregion Private functions
    }
}
