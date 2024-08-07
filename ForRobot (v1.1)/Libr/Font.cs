using System;
using System.Drawing;

namespace ForRobot.Libr
{
    public static class FontLibr
    {
        /// <summary>
        /// Вычисление размера шрифта
        /// </summary>
        /// <param name="g"></param>
        /// <param name="longString"></param>
        /// <param name="Room"></param>
        /// <param name="PreferedFont"></param>
        /// <returns></returns>
        public static Font FindFont(System.Drawing.Graphics g, string longString, Size Room, Font PreferedFont)
        {
            SizeF RealSize = g.MeasureString(longString, PreferedFont);
            float HeightScaleRatio = Room.Height / RealSize.Height;
            float WidthScaleRatio = Room.Width / RealSize.Width;

            float ScaleRatio = (HeightScaleRatio < WidthScaleRatio) ? ScaleRatio = HeightScaleRatio : ScaleRatio = WidthScaleRatio;

            float ScaleFontSize = PreferedFont.Size * ScaleRatio;

            return new Font(PreferedFont.FontFamily, ScaleFontSize);
        }
    }
}
