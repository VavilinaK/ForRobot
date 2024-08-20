using System;

namespace ForRobot.Model.Detals
{
    [Flags]
    /// <summary>
    /// Перечень типов деталей
    /// </summary>
    public enum DetalType
    {
        /// <summary>
        /// Плита
        /// </summary>
        Plita = 1,

        /// <summary>
        /// Плита со стрингером
        /// </summary>
        Stringer = 2,

        /// <summary>
        /// Плита треугольником
        /// </summary>
        Treygolnik = 3
    }
}
