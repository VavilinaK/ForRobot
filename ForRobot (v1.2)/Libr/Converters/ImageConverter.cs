using System;
using System.IO;

namespace ForRobot.Libr.Converters
{
    /// <summary>
    /// Класс преобразования изображений
    /// </summary>
    public static class ImageConverter
    {
        /// <summary>
        /// Преобразование из <see cref="System.Windows.Media.Imaging.BitmapImage"/> в <see cref="System.Drawing.Bitmap"/>
        /// </summary>
        /// <param name="bitmapImage"> Объект <see cref="System.Windows.Media.Imaging.BitmapImage"/> для преобразования</param>
        /// <returns></returns>
        public static System.Drawing.Bitmap BitmapImagetoBitmap(System.Windows.Media.Imaging.BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                System.Windows.Media.Imaging.BitmapEncoder enc = new System.Windows.Media.Imaging.BmpBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new System.Drawing.Bitmap(bitmap);
            }
        }

        /// <summary>
        /// Преобразование из <see cref="System.Windows.Controls.Image"/> в <see cref="System.Drawing.Bitmap"/>
        /// </summary>
        /// <param name="image"> Объект <see cref="System.Windows.Controls.Image"/> для преобразования</param>
        /// <returns></returns>
        public static System.Drawing.Bitmap ImageToBitmap(System.Windows.Controls.Image image)
        {
            System.Windows.Media.Imaging.RenderTargetBitmap rtBmp = new System.Windows.Media.Imaging.RenderTargetBitmap((int)image.ActualWidth, (int)image.ActualHeight, 96.0, 96.0, System.Windows.Media.PixelFormats.Pbgra32);

            image.Measure(new System.Windows.Size((int)image.ActualWidth, (int)image.ActualHeight));
            image.Arrange(new System.Windows.Rect(new System.Windows.Size((int)image.ActualWidth, (int)image.ActualHeight)));

            rtBmp.Render(image);

            System.Windows.Media.Imaging.PngBitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            MemoryStream stream = new MemoryStream();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(rtBmp));

            encoder.Save(stream);
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(stream);

            return bitmap;
        }

        /// <summary>
        /// Преобразование из <see cref="System.Drawing.Bitmap"/> в <see cref="System.Windows.Media.Imaging.BitmapImage"/>
        /// </summary>
        /// <param name="bitmap"> Объект <see cref="System.Drawing.Bitmap"/> для преобразования</param>
        /// <returns></returns>
        public static System.Windows.Media.Imaging.BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}
