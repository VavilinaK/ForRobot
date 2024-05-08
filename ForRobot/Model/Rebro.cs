using System;
using System.Drawing;

namespace ForRobot.Model
{
    public class Rebro
    {
        public Rebro(decimal _wight, decimal _dissolutionStart, decimal _dissolutionEnd)
        {
            _wight = Wight;
            _dissolutionStart = DissolutionStart;
            _dissolutionEnd = DissolutionEnd;
        }

        /// <summary>
        /// Толщина ребра
        /// </summary>
        public decimal Wight { get; set; }

        /// <summary>
        /// Роспуск в начале
        /// </summary>
        public decimal DissolutionStart { get; set; }

        /// <summary>
        /// Роспуск в конце
        /// </summary>
        public decimal DissolutionEnd { get; set; }

        /// <summary>
        /// Изображение ячейки
        /// </summary>
        public Image Image { get; set; }
    }
}
