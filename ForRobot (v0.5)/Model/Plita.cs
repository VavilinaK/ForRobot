using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

using ForRobot.Libr;

namespace ForRobot.Model
{
    public class Plita : Detal
    {
        #region Private variables

        private BitmapImage _rebraImage;

        private BitmapImage _plitaImage;

        #endregion

        #region Public variables

        public override sealed DetalType DetalType { get => DetalType.Plita; }

        public sealed override int SumReber
        {
            get => base.SumReber;
            set
            {
                base.SumReber = value;
                RebraImage = JoinRebra(GetStartRebraImage(), GetBodyRebraImage(), GetEndRebraImage());
                GenericImage = JoinPlita(GetStartPlitaImage(), GetBodyPlitaImage(), GetEndPlitaImage());
            }
        }

        public sealed override decimal Long
        {
            get => Math.Round(base.Long, 2);
            set
            {
                base.Long = value;
                RebraImage = JoinRebra(GetStartRebraImage(), GetBodyRebraImage(), GetEndRebraImage());
                GenericImage = JoinPlita(GetStartPlitaImage(), GetBodyPlitaImage(), GetEndPlitaImage());
                this.Change.Invoke(this, null);
            }
        }

        public sealed override decimal Hight
        {
            get => Math.Round(base.Hight, 2);
            set
            {
                base.Hight = value;
                RebraImage = JoinRebra(GetStartRebraImage(), GetBodyRebraImage(), GetEndRebraImage());
                this.Change.Invoke(this, null);
            }
        }

        public sealed override decimal Wight
        {
            get => Math.Round(base.Wight, 2);
            set
            {
                base.Wight = value;
                RebraImage = JoinRebra(GetStartRebraImage(), GetBodyRebraImage(), GetEndRebraImage());
                GenericImage = JoinPlita(GetStartPlitaImage(), GetBodyPlitaImage(), GetEndPlitaImage());
                this.Change.Invoke(this, null);
            }
        }

        public sealed override decimal IndentionStart
        {
            get
            {
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                    return Math.Round(base.IndentionStart, 2);
                else
                    return Math.Round(base.IndentionEnd, 2);
            }
            set
            {
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                    base.IndentionStart = value;
                else
                    base.IndentionEnd = value;

                RebraImage = JoinRebra(GetStartRebraImage(), GetBodyRebraImage(), GetEndRebraImage());
                this.Change.Invoke(this, null);
            }
        }

        public sealed override decimal IndentionEnd
        {
            get
            {
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                    return Math.Round(base.IndentionEnd, 2);
                else
                    return Math.Round(base.IndentionStart, 2);
            }
            set
            {
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                    base.IndentionEnd = value;
                else
                    base.IndentionStart = value;

                RebraImage = JoinRebra(GetStartRebraImage(), GetBodyRebraImage(), GetEndRebraImage());
                this.Change.Invoke(this, null);
            }
        }

        public sealed override decimal DissolutionStart
        {
            get
            {
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                    return Math.Round(base.DissolutionStart, 2);
                else
                    return Math.Round(base.DissolutionEnd, 2);
            }
            set
            {
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                    base.DissolutionStart = value;
                else
                    base.DissolutionEnd = value;

                GenericImage = JoinPlita(GetStartPlitaImage(), GetBodyPlitaImage(), GetEndPlitaImage());
                this.Change.Invoke(this, null);
            }
        }

        public sealed override decimal DissolutionEnd
        {
            get
            {
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                    return Math.Round(base.DissolutionEnd, 2);
                else
                    return Math.Round(base.DissolutionStart, 2);
            }
            set
            {
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                    base.DissolutionEnd = value;
                else
                    base.DissolutionStart = value;

                GenericImage = JoinPlita(GetStartPlitaImage(), GetBodyPlitaImage(), GetEndPlitaImage());
                this.Change.Invoke(this, null);
            }
        }

        public sealed override decimal DistanceToFirst
        {
            get => Math.Round(base.DistanceToFirst, 2);
            set
            {
                base.DistanceToFirst = value;
                this.Change.Invoke(this, null);
            }
        }

        public sealed override decimal DistanceBetween
        {
            get => Math.Round(base.DistanceBetween, 2);
            set
            {
                base.DistanceBetween = value;
                RebraImage = JoinRebra(GetStartRebraImage(), GetBodyRebraImage(), GetEndRebraImage());
                this.Change.Invoke(this, null);
            }
        }

        public sealed override decimal ThicknessPlita
        {
            get => Math.Round(base.ThicknessPlita, 2);
            set
            {
                base.ThicknessPlita = value;
                RebraImage = JoinRebra(GetStartRebraImage(), GetBodyRebraImage(), GetEndRebraImage());
                this.Change.Invoke(this, null);
            }
        }

        public sealed override decimal ThicknessRebro
        {
            get => Math.Round(base.ThicknessRebro, 2);
            set
            {
                base.ThicknessRebro = value;
                GenericImage = JoinPlita(GetStartPlitaImage(), GetBodyPlitaImage(), GetEndPlitaImage());
                this.Change.Invoke(this, null);
            }
        }

        public sealed override decimal SearchOffsetStart
        {
            get => base.SearchOffsetStart;
            set
            {
                base.SearchOffsetStart = value;
                this.Change.Invoke(this, null);
            }
        }

        public sealed override decimal SearchOffsetEnd
        {
            get => base.SearchOffsetEnd;
            set
            {
                base.SearchOffsetEnd = value;
                this.Change.Invoke(this, null);
            }
        }

        public override Privyazka LongitudinalPrivyazka
        {
            get => base.LongitudinalPrivyazka;
            set => base.LongitudinalPrivyazka = value;
        }

        public override Privyazka TransversePrivyazka
        {
            get => base.TransversePrivyazka;
            set
            {
                base.TransversePrivyazka = value;
                GenericImage = JoinPlita(GetStartPlitaImage(), GetBodyPlitaImage(), GetEndPlitaImage());
            }
        }

        /// <summary>
        /// Изображение рёбер плиты
        /// </summary>
        public override sealed BitmapImage RebraImage
        {
            get => this._rebraImage ?? (this._rebraImage = JoinRebra(GetStartRebraImage(), GetBodyRebraImage(), GetEndRebraImage()));
            set => Set(ref this._rebraImage, value);
        }

        /// <summary>
        /// Общее изображение плиты
        /// </summary>
        public override sealed BitmapImage GenericImage
        {
            get => _plitaImage ?? (_plitaImage = JoinPlita(GetStartPlitaImage(), GetBodyPlitaImage(), GetEndPlitaImage()));
            set => Set(ref this._plitaImage, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// Событие изменения свойства класса
        /// </summary>
        //public override event EventHandler Change;

        public override event Func<object, EventArgs, Task> Change;

        #endregion

        #region Public functions

        public async Task OnChange() => await base.OnChange(this.Change);

        #endregion

        #region Private functions

        #region Рёбра

        /// <summary>
        /// Отрисовка первого ребра
        /// </summary>
        /// <returns></returns>
        private Bitmap GetStartRebraImage()
        {
            Bitmap image = new Bitmap(300, 310);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.Clear(Color.White);
                Pen pen = new Pen(Color.Black, 7);
                PointF[] points =
                {
                 new PointF(135,  177),
                 new PointF(146, 177),
                 new PointF(146,  61),
                 new PointF(136, 61)
                };

                graphics.DrawLines(pen, points);
                graphics.DrawLine(pen, new PointF(145, 191), new PointF(145, 294));

                // Плита
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(148, 180, 151, 11));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(171, 171, 171)), new Rectangle(149, 182, 151, 7));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(135, 135, 135)), new Rectangle(149, 184, 151, 5));

                // Верхняя стрелка
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(149, 58, 151, 3));
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(157, 56, 21, 7));
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(178, 54, 17, 11));
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(195, 51, 6, 17));

                //// Нижняя стрелка
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(149, 258, 151, 3));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(157, 256, 21, 7));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(178, 254, 17, 11));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(195, 251, 6, 17));

                // Ребро
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(284, 58, 17, 125));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(85, 255, 66)), new Rectangle(287, 61, 11, 122));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(36, 110, 27)), new Rectangle(291, 61, 7, 122));
                graphics.DrawEllipse(new Pen(Color.Black, 3), 284, 180, 17, 6);

                //// Текст
                //Font font = new Font("Lucida Console", image.Width / 14, System.Drawing.FontStyle.Regular);
                //StringFormat stringFormat = new StringFormat(StringFormatFlags.DirectionVertical);
                //graphics.DrawString(IndentionStart.ToString(), font, new SolidBrush(Color.Black), 206, 29);
                //graphics.DrawString(Hight.ToString(), font, new SolidBrush(Color.Black), new PointF(102, 126 - Hight.ToString().Length * (font.Size - 8)), stringFormat);
            }
            return image;
        }

        /// <summary>
        /// Отрисовка всех рёбер, кроме первого и последнего
        /// </summary>
        /// <returns></returns>
        private Bitmap GetBodyRebraImage()
        {
            Bitmap image = new Bitmap(150, 310);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.Clear(Color.White);

                // Плита
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 180, 150, 11));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(171, 171, 171)), new Rectangle(0, 182, 150, 7));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(135, 135, 135)), new Rectangle(0, 184, 150, 5));

                //// Нижняя стрелка
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 258, 150, 3));

                // Ребро
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(135, 58, 17, 125));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(85, 255, 66)), new Rectangle(137, 61, 11, 122));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(36, 110, 27)), new Rectangle(141, 61, 7, 122));
                graphics.DrawEllipse(new Pen(Color.Black, 3), 135, 180, 16, 6);
            }
            return image;
        }

        /// <summary>
        /// Отрисовка последнего ребра
        /// </summary>
        /// <returns></returns>
        private Bitmap GetEndRebraImage()
        {
            Bitmap image = new Bitmap(450, 310);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.Clear(Color.White);
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(300, 47, 7, 250));

                // Плита
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 180, 301, 11));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(171, 171, 171)), new Rectangle(0, 182, 300, 7));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(135, 135, 135)), new Rectangle(0, 184, 300, 5));

                //// Верхняя стрелка
                //if (SumReber == 1)
                //{
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 58, 301, 3));

                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(274, 56, 21, 7));
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(257, 54, 17, 11));
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(251, 51, 6, 17));

                //}
                //else
                    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(150, 58, 151, 3));

                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(274, 56, 21, 7));
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(257, 54, 17, 11));
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(251, 51, 6, 17));

                // Нижняя стрелка
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 258, 301, 3));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(274, 256, 21, 7));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(257, 254, 17, 11));
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(251, 251, 6, 17));

                //if (SumReber == 1)
                //{
                //    // Нижняя стрелка в начале
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(149, 258, 151, 3));
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(157, 256, 21, 7));
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(178, 254, 17, 11));
                //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(195, 251, 6, 17));
                //}

                // Ребро
                graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(150, 58, 17, 125));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(85, 255, 66)), new Rectangle(154, 61, 11, 122));
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(36, 110, 27)), new Rectangle(158, 61, 7, 122));
                graphics.DrawEllipse(new Pen(Color.Black, 3), 150, 180, 16, 6);

                // Толщина
                graphics.DrawRectangle(new Pen(Color.FromArgb(27, 214, 242), 4), 309, 180, 12, 12);
                graphics.DrawLine(new Pen(Color.FromArgb(27, 214, 242), 4), 309, 192, 442, 192);

                //// Текст
                //Font font = new Font("Lucida Console", image.Width / 22, System.Drawing.FontStyle.Regular);
                //graphics.DrawString(IndentionEnd.ToString(), font, new SolidBrush(Color.Black), 205, 15);
                //graphics.DrawString(ThicknessPlita.ToString(), font, new SolidBrush(Color.Black), 334, 156);
            }
            return image;
        }

        /// <summary>
        /// Добавление дистанции междурёбрами и ширены плиты
        /// </summary>
        /// <param name="oldImage"></param>
        /// <returns></returns>
        private Bitmap PaintDistanceBetween_And_WightImage(Bitmap oldImage)
        {
            Bitmap image = new Bitmap(oldImage);
            //using (Graphics graphics = Graphics.FromImage(image))
            //{
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 59, 135, 3));

            //    // Стрелка левая
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(3, 58, 4, 5));
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(7, 57, 5, 7));
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(12, 56, 4, 9));
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(16, 54, 4, 13));

            //    // Стрелка правая
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(128, 58, 4, 5));
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(123, 57, 5, 7));
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(119, 56, 4, 9));
            //    graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(115, 54, 4, 13));

            //    // Текст
            //    Font font = new Font("Lucida Console", image.Width / 8, System.Drawing.FontStyle.Regular);
            //    graphics.DrawString(DistanceBetween.ToString(), font, new SolidBrush(Color.Black), 51, 33);
            //    graphics.DrawString(Long.ToString(), font, new SolidBrush(Color.Black), 33, 234);
            //}
            return image;
        }

        private Bitmap GetArrowsPlita(Bitmap bitmap)
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                Pen pen = new Pen(Color.Black, 2);

                #region Стрелка ширины

                PointF[] point =
                {
                    new PointF(149, 258),
                    new PointF(bitmap.Width - 145, 258)
                };
                graphics.DrawLines(pen, point);

                point = new PointF[]
                {
                    new PointF(149, 258),
                    new PointF(149, 259),
                    new PointF(157, 259),
                    new PointF(157, 261),
                    new PointF(175, 261),
                    new PointF(175, 264),
                    new PointF(187, 264),
                    new PointF(187, 266),
                    new PointF(194, 266),
                    new PointF(194, 251),
                    new PointF(187, 251),
                    new PointF(187, 253),
                    new PointF(175, 253),
                    new PointF(175, 256),
                    new PointF(157, 256),
                    new PointF(157, 258),
                    new PointF(149, 258)
                };
                graphics.FillPolygon(new SolidBrush(Color.Black), point);

                point = new PointF[]
{
                    new PointF(300 + 150 * (SumReber - 2) + 256, 251),
                    new PointF(300 + 150 * (SumReber - 2) + 256, 266),
                    new PointF(300 + 150 * (SumReber - 2) + 262, 266),
                    new PointF(300 + 150 * (SumReber - 2) + 262, 264),
                    new PointF(300 + 150 * (SumReber - 2) + 275, 264),
                    new PointF(300 + 150 * (SumReber - 2) + 275, 261),
                    new PointF(300 + 150 * (SumReber - 2) + 293, 261),
                    new PointF(300 + 150 * (SumReber - 2) + 293, 259),
                    new PointF(300 + 150 * (SumReber - 2) + 299, 259),
                    new PointF(300 + 150 * (SumReber - 2) + 299, 258),
                    new PointF(300 + 150 * (SumReber - 2) + 293, 258),
                    new PointF(300 + 150 * (SumReber - 2) + 293, 256),
                    new PointF(300 + 150 * (SumReber - 2) + 275, 256),
                    new PointF(300 + 150 * (SumReber - 2) + 275, 253),
                    new PointF(300 + 150 * (SumReber - 2) + 263, 253),
                    new PointF(300 + 150 * (SumReber - 2) + 263, 251),
};
                graphics.FillPolygon(new SolidBrush(Color.Black), point);

                if (SumReber == 1)
                {
                    pen.Width = 3;
                    graphics.DrawLine(pen, new PointF(149, 59), new PointF(304, 59));

                    pen.Width = 1;
                    point = new PointF[] 
                    {
                        new PointF(149, 59),
                        new PointF(149, 61),
                        new PointF(157, 61),
                        new PointF(157, 63),
                        new PointF(178, 63),
                        new PointF(178, 65),
                        new PointF(195, 65),
                        new PointF(195, 67),
                        new PointF(200, 67),
                        new PointF(200, 52),
                        new PointF(195, 52),
                        new PointF(195, 55),
                        new PointF(178, 55),
                        new PointF(178, 57),
                        new PointF(157, 57),
                        new PointF(157, 58)
                    };

                    graphics.FillPolygon(new SolidBrush(Color.Black), point);
                }

                #endregion

                #region Расстояние между рёбрами

                if (SumReber > 1)
                {
                    pen.Width = 3;
                    graphics.DrawLine(pen, new PointF(bitmap.Width - 300, 59), new PointF(bitmap.Width - 450, 59));

                    point = new PointF[]
                    {
                            new PointF(300 + 150 * (SumReber - 2) + 1, 59),
                            new PointF(300 + 150 * (SumReber - 2) + 1, 61),
                            new PointF(300 + 150 * (SumReber - 2) + 5, 61),
                            new PointF(300 + 150 * (SumReber - 2) + 5, 63),
                            new PointF(300 + 150 * (SumReber - 2) + 9, 63),
                            new PointF(300 + 150 * (SumReber - 2) + 9, 65),
                            new PointF(300 + 150 * (SumReber - 2) + 13, 65),
                            new PointF(300 + 150 * (SumReber - 2) + 13, 67),
                            new PointF(300 + 150 * (SumReber - 2) + 16, 67),
                            new PointF(300 + 150 * (SumReber - 2) + 16, 54),
                            new PointF(300 + 150 * (SumReber - 2) + 13, 54),
                            new PointF(300 + 150 * (SumReber - 2) + 13, 55),
                            new PointF(300 + 150 * (SumReber - 2) + 9, 55),
                            new PointF(300 + 150 * (SumReber - 2) + 9, 57),
                            new PointF(300 + 150 * (SumReber - 2) + 5, 57),
                            new PointF(300 + 150 * (SumReber - 2) + 5, 58)
                    };
                    graphics.FillPolygon(new SolidBrush(Color.Black), point);

                    point = new PointF[]
                    {
                            new PointF(300 + 150 * (SumReber - 2) + 149, 59),
                            new PointF(300 + 150 * (SumReber - 2) + 149, 61),
                            new PointF(300 + 150 * (SumReber - 2) + 145, 61),
                            new PointF(300 + 150 * (SumReber - 2) + 145, 63),
                            new PointF(300 + 150 * (SumReber - 2) + 141, 63),
                            new PointF(300 + 150 * (SumReber - 2) + 141, 65),
                            new PointF(300 + 150 * (SumReber - 2) + 137, 65),
                            new PointF(300 + 150 * (SumReber - 2) + 137, 67),
                            new PointF(300 + 150 * (SumReber - 2) + 134, 67),
                            new PointF(300 + 150 * (SumReber - 2) + 134, 54),
                            new PointF(300 + 150 * (SumReber - 2) + 137, 54),
                            new PointF(300 + 150 * (SumReber - 2) + 137, 55),
                            new PointF(300 + 150 * (SumReber - 2) + 141, 55),
                            new PointF(300 + 150 * (SumReber - 2) + 141, 57),
                            new PointF(300 + 150 * (SumReber - 2) + 145, 57),
                            new PointF(300 + 150 * (SumReber - 2) + 145, 58)
                    };
                    graphics.FillPolygon(new SolidBrush(Color.Black), point);
                }

                #endregion
            }
            return bitmap;
        }

        /// <summary>
        /// Добавление текста на изображение рёбер
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private Bitmap GetTextPlita(Bitmap bitmap)
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                StringFormat stringFormat1 = new StringFormat(StringFormatFlags.DirectionVertical)
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                StringFormat stringFormat2 = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                using (Font font = new Font("Lucida Console", 18, System.Drawing.FontStyle.Regular))
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                    graphics.DrawString(Hight.ToString(),
                                        FontLibr.FindFont(graphics, Hight.ToString(), new System.Drawing.Size(80, 30), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(120, 126),
                                        stringFormat1);

                    graphics.DrawString(IndentionStart.ToString(),
                                        FontLibr.FindFont(graphics, IndentionStart.ToString(), new System.Drawing.Size(60, 25), font),
                                        new SolidBrush(Color.Black),
                                        new PointF(240, 40),
                                        stringFormat2);

                    graphics.DrawString(IndentionEnd.ToString(),
                                        FontLibr.FindFont(graphics, IndentionEnd.ToString(), new System.Drawing.Size(60, 25), font),
                                        new SolidBrush(Color.Black), 
                                        new PointF(300 + 150 * (SumReber - 2) + 205, 40),
                                        stringFormat2);

                    graphics.DrawString(ThicknessPlita.ToString(),
                                        FontLibr.FindFont(graphics, ThicknessPlita.ToString(), new System.Drawing.Size(60, 25), font),
                                        new SolidBrush(Color.Black), 
                                        new PointF(300 + 150 * (SumReber - 2) + 360, 180),
                                        stringFormat2);

                    graphics.DrawString(Wight.ToString(),
                                        FontLibr.FindFont(graphics, Wight.ToString(), new System.Drawing.Size(120, 30), font),
                                        new SolidBrush(Color.Black), 
                                        new PointF(bitmap.Width / 2, 245),
                                        stringFormat2);

                    if (SumReber > 1)
                        graphics.DrawString(DistanceBetween.ToString(),
                                            FontLibr.FindFont(graphics, DistanceBetween.ToString(), new System.Drawing.Size(90, 25), font),
                                            new SolidBrush(Color.Black),
                                            new PointF(bitmap.Width - 375, 45),
                                            stringFormat2);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Объединение изображений
        /// </summary>
        /// <param name="png1">Изображение первого ребра</param>
        /// <param name="png2">Все рёбра кроме первого и последнего</param>
        /// <param name="png3">Последнее ребро</param>
        /// <returns></returns>
        private BitmapImage JoinRebra(Bitmap png1, Bitmap png2, Bitmap png3)
        {
            BitmapImage imageSource = new BitmapImage();
            using (Bitmap result = new Bitmap(png1.Width + png2.Width * (SumReber - 2) + png3.Width, 310))
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(png1, 0, 0);

                    for (int i = 1; i < SumReber; i++)
                    {
                        if (i == 1)
                            g.DrawImage(png2, 300, 0);

                        //else if (SumReber % 2 == 0 && i == (SumReber / 2) + 1)
                        //    g.DrawImage(PaintDistanceBetween_And_WightImage(png2), i * 150, 0);
                        //else if (SumReber % 2 == 1 && i == Math.Ceiling((decimal)SumReber / 2))
                        //    g.DrawImage(PaintDistanceBetween_And_WightImage(png2), i * 150, 0);
                        else
                            g.DrawImage(png2, i * 150, 0);
                    }
                        g.DrawImage(png3, (SumReber - 2) * 150 + 300, 0);
                }
                imageSource = LibraryConvertClass.ImageConverter.BitmapToBitmapImage(GetTextPlita(GetArrowsPlita(result)));
            }
            return imageSource;
        }

        #endregion

        #region Плита

        /// <summary>
        /// Выгрузка изображения начала плиты из ресурсов
        /// </summary>
        /// <returns></returns>
        private Bitmap GetStartPlitaImage()
        {
            Bitmap image = new Bitmap(980, 250);
            image = new Bitmap(LibraryConvertClass.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImagePlitaStart")));
            //if (LongitudinalPrivyazka == Privyazka.FromLeftToRight)
            //{
            //    image = new Bitmap(LibraryConvertClass.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImagePlitaStart")), new System.Drawing.Size(880, 150));
            //}
            //if (TransversePrivyazka == Privyazka.FromRightToLeft)
            //{
            //    image = new Bitmap(LibraryConvertClass.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImagePlitaStartDown")), new System.Drawing.Size(880, 150));
            //}
            //using (Graphics graphics = Graphics.FromImage(image))
            //{
                //graphics.Clear(Color.White);

                //// Плита
                //graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(49, 62, 813, 88));
                //graphics.FillRectangle(new SolidBrush(Color.FromArgb(171, 171, 171)), new Rectangle(50, 63, 811, 87));

                //// Толщина
                //graphics.DrawRectangle(new Pen(Color.FromArgb(27, 214, 242), 2), 50, 52, 9, 9);
                //graphics.DrawLine(new Pen(Color.FromArgb(27, 214, 242), 2), 50, 1, 50, 62);

                //// Ребро
                //Pen pen = new Pen(Color.Black, 3);
                //PointF[] points = 
                //{
                // new PointF(81,  150),
                // new PointF(81, 128),
                // new PointF(133,  128),
                // new PointF(133, 121),
                // new PointF(772, 121),
                // new PointF(772, 128),
                // new PointF(823, 128),
                // new PointF(823, 150)
                //};

                //graphics.DrawLines(pen, points);
                //graphics.FillPolygon(new SolidBrush(Color.FromArgb(105, 237, 78)), points);

                //graphics.DrawLine(new Pen(new SolidBrush(Color.Black)), 35, 127, 81, 127);
                //graphics.DrawLine(new Pen(new SolidBrush(Color.Black)), 35, 149, 823, 149);
                //graphics.DrawLine(new Pen(new SolidBrush(Color.Black)), 35, 128, 35, 149);

                //graphics.DrawEllipse(new Pen(Color.Black, 1), 89, 131, 11, 4);
                //graphics.DrawEllipse(new Pen(Color.Black, 1), 89, 143, 11, 4);
                //graphics.DrawEllipse(new Pen(Color.Black, 1), 108, 131, 11, 4);
                //graphics.DrawEllipse(new Pen(Color.Black, 1), 108, 143, 11, 4);

                ////graphics.DrawEllipse(new Pen(Color.Black, 1), 89, 131, 11, 5);
                ////graphics.DrawEllipse(new Pen(Color.Black, 1), 89, 131, 11, 5);
                ////graphics.DrawEllipse(new Pen(Color.Black, 1), 89, 131, 11, 5);
                ////graphics.DrawEllipse(new Pen(Color.Black, 1), 89, 131, 11, 5);

                ////Верхняя стрелка
                //pen.Width = 1;
                //PointF[] points2 =
                //{
                // new PointF(31,  133),
                // new PointF(32, 132),
                // new PointF(33, 131),
                // new PointF(34, 130),
                // new PointF(36, 130),
                // new PointF(37, 131),
                // new PointF(38, 132),
                // new PointF(39, 133),
                // new PointF(31, 133)
                //};

                //graphics.DrawLines(pen, points2);
                //graphics.FillPolygon(new SolidBrush(Color.Black), points2);

                //// Нижняя стрелка
                //PointF[] points3 = {
                // new PointF(31, 145),
                // new PointF(32, 146),
                // new PointF(33, 147),
                // new PointF(34, 148),
                // new PointF(36, 148),
                // new PointF(37, 147),
                // new PointF(38, 146),
                // new PointF(39, 145),
                // new PointF(31, 145)
                //};

                //graphics.DrawLines(pen, points3);
                //graphics.FillPolygon(new SolidBrush(Color.Black), points3);

                //Текст
                //Font font = new Font("Lucida Console", image.Height / 11, System.Drawing.FontStyle.Regular);
                ////StringFormat stringFormat = new StringFormat(StringFormatFlags.DirectionVertical);
                ////graphics.DrawString(DistanceToFirst.ToString(), font, new SolidBrush(Color.Black), new PointF(6, 146 - DistanceToFirst.ToString().Length * (font.Size - 2)), stringFormat);

                //SizeF textImageSize = graphics.MeasureString(ThicknessRebro.ToString(), font);
                //Bitmap str = new Bitmap((int)textImageSize.Width + 30, (int)textImageSize.Height + 20);
                //Graphics strgr = Graphics.FromImage(str);
                //strgr.DrawString(ThicknessRebro.ToString(), font, Brushes.Black, 1, 1);
                //strgr.Save();
                //str.RotateFlip(RotateFlipType.Rotate270FlipNone);
                //graphics.DrawImage(str, new PointF(6, 114 - ThicknessRebro.ToString().Length * (font.Size - 2)));
            //}
            return image;
        }

        /// <summary>
        /// Выгрузка изображения тела плиты из ресурсов
        /// </summary>
        /// <returns></returns>
        private Bitmap GetBodyPlitaImage()
        {
            Bitmap image = new Bitmap(980, 188);
            image = new Bitmap(LibraryConvertClass.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImagePlitaBody")));
            //using (Graphics graphics = Graphics.FromImage(image))
            //{
            //    graphics.Clear(Color.Orange);
            //}
            return image;
        }

        /// <summary>
        /// Выгрузка изображения окончания плиты из ресурсов
        /// </summary>
        /// <returns></returns>
        private Bitmap GetEndPlitaImage()
        {
            Bitmap image = new Bitmap(980, 150);
            //if (TransversePrivyazka == Privyazka.FromLeftToRight)
            //{
                image = new Bitmap(LibraryConvertClass.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImagePlitaEnd")));
            //}
            //if (TransversePrivyazka == Privyazka.FromRightToLeft)
            //{
            //    image = new Bitmap(LibraryConvertClass.ImageConverter.BitmapImagetoBitmap((BitmapImage)Application.Current.TryFindResource("ImagePlitaEndRight")), new System.Drawing.Size(250, 980));
            //}
            //using (Graphics graphics = Graphics.FromImage(image))
            //{
            //    // Текст
            //    Font font = new Font("Lucida Console", 18, System.Drawing.FontStyle.Regular);
            //    graphics.DrawString(DissolutionStart.ToString(), font, new SolidBrush(Color.Black), 95 - (DissolutionStart.ToString().Length - 1) * 7, 96);
            //    graphics.DrawString(DissolutionEnd.ToString(), font, new SolidBrush(Color.Black), 710 - (DissolutionEnd.ToString().Length - 1) * 7, 96);
            //    graphics.DrawString(Wight.ToString(), font, new SolidBrush(Color.Black), 416 - (Wight.ToString().Length - 1) * 7, 110);
            //}
            return image;
        }

        /// <summary>
        /// Добавление текста на изображение плиты
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private Bitmap GetText(Bitmap bitmap)
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                StringFormat stringFormat1 = new StringFormat(StringFormatFlags.DirectionVertical)
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                StringFormat stringFormat2 = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                using (Font font = new Font("Lucida Console", 18, System.Drawing.FontStyle.Regular))
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                    graphics.DrawString(Long.ToString(), 
                                        FontLibr.FindFont(graphics, Long.ToString(), new System.Drawing.Size(100, 30), font), 
                                        new SolidBrush(Color.Black), 
                                        new PointF(420, 440),
                                        stringFormat2);

                    graphics.DrawString(Wight.ToString(),
                                        FontLibr.FindFont(graphics, Wight.ToString(), new System.Drawing.Size(100, 30), font),
                                        new SolidBrush(Color.Black), 
                                        new PointF(840, bitmap.Height/2),
                                        stringFormat1);

                    graphics.DrawString(DissolutionStart.ToString(),
                                        FontLibr.FindFont(graphics, DissolutionStart.ToString(), new System.Drawing.Size(48, 22), font),
                                        new SolidBrush(Color.Black), 
                                        new PointF(95, 430), 
                                        stringFormat2);

                    graphics.DrawString(DissolutionEnd.ToString(),
                                        FontLibr.FindFont(graphics, DissolutionEnd.ToString(), new System.Drawing.Size(48, 22), font),
                                        new SolidBrush(Color.Black), 
                                        new PointF(710, 430),
                                        stringFormat2);

                    graphics.DrawString(ThicknessRebro.ToString(),
                                        FontLibr.FindFont(graphics, ThicknessRebro.ToString(), new System.Drawing.Size(40, 25), font),
                                        new SolidBrush(Color.Black), 
                                        new PointF(795,140),
                                        stringFormat1);
                }
            }
            return bitmap;
        }

        private Bitmap GetArrows(Bitmap bitmap)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                Pen pen = new Pen(Color.FromArgb(0, 183, 239), 1);

                PointF[] points1 = 
                    {
                        new PointF(804,  460),
                        new PointF(804, 462),
                        new PointF(806,  462),
                        new PointF(806, 464),
                        new PointF(808, 464),
                        new PointF(808, 466),
                        new PointF(810, 466),
                        new PointF(810, 468),
                        new PointF(811, 468),
                        new PointF(811, 469),
                        new PointF(812, 469),
                        new PointF(812, 468),
                        new PointF(813, 468),
                        new PointF(813, 466),
                        new PointF(815, 466),
                        new PointF(815, 464),
                        new PointF(817, 464),
                        new PointF(817, 462),
                        new PointF(819, 462),
                        new PointF(819, 460),
                    };

                PointF[] points2 =
                    {
                        new PointF(804,  39),
                        new PointF(804, 38),
                        new PointF(806,  38),
                        new PointF(806, 36),
                        new PointF(808, 36),
                        new PointF(808, 34),
                        new PointF(810, 34),
                        new PointF(810, 32),
                        new PointF(811, 32),
                        new PointF(811, 31),
                        new PointF(812, 31),
                        new PointF(812, 32),
                        new PointF(813, 32),
                        new PointF(813, 34),
                        new PointF(815, 34),
                        new PointF(815, 36),
                        new PointF(817, 36),
                        new PointF(817, 38),
                        new PointF(819, 38),
                        new PointF(819, 39),
                    };

                if (TransversePrivyazka == Privyazka.FromRightToLeft)
                {
                    g.DrawPolygon(pen, points1);
                    g.FillPolygon(new SolidBrush(Color.FromArgb(0, 183, 239)), points1);
                }
                if (TransversePrivyazka == Privyazka.FromLeftToRight)
                {
                    g.DrawPolygon(pen, points2);
                    g.FillPolygon(new SolidBrush(Color.FromArgb(0, 183, 239)), points2);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Объединение изображений плиты
        /// </summary>
        /// <param name="png1"></param>
        /// <param name="png2"></param>
        /// <param name="png3"></param>
        /// <returns></returns>
        private BitmapImage JoinPlita(Bitmap png1, Bitmap png2, Bitmap png3)
        {
            BitmapImage imageSource = new BitmapImage();
            using (Bitmap result = new Bitmap(980, png1.Height + png2.Height * 2 + png3.Height))
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(png1, 0, 0);

                    for (int i = 1; i < 4; i++)
                    {
                        if (i == 1)
                            g.DrawImage(png2, 0, 150);
                        else
                            g.DrawImage(png2, 0, (i - 1) * 88 + 150);

                        //    else if (SumReber % 2 == 0 && i == (SumReber / 2) + 1)
                        //        g.DrawImage(PaintDistanceBetween_And_WightImage(png2), i * 150, 0);
                        //    else if (SumReber % 2 == 1 && i == Math.Ceiling((decimal)SumReber / 2))
                        //        g.DrawImage(PaintDistanceBetween_And_WightImage(png2), i * 150, 0);
                        //    else
                        //        g.DrawImage(png2, i * 150, 0);
                    }

                    g.DrawImage(png3, 0, 2 * 88 + 150);

                    //Font font = new Font("Lucida Console", 18, System.Drawing.FontStyle.Regular);
                    //StringFormat stringFormat = new StringFormat(StringFormatFlags.DirectionVertical);
                    //g.DrawString(Long.ToString(), font, new SolidBrush(Color.Black), new PointF(835, result.Height/2 - Long.ToString().Length * (font.Size - 8)), stringFormat);
                }
                imageSource = LibraryConvertClass.ImageConverter.BitmapToBitmapImage(GetText(GetArrows(result)));
            }
            return imageSource;
        }

        #endregion

        #endregion
    }
}
