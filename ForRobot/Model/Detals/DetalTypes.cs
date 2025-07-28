using System;

namespace ForRobot.Model.Detals
{
    /// <summary>
    /// Перечень типов деталей
    /// </summary>
    public static class DetalTypes
    {
        /// <summary>
        /// Плита
        /// </summary>
        public const string Plita = "Настил с ребром";

        /// <summary>
        /// Плита со стрингером
        /// </summary>
        public const string Stringer = "Настил со стрингером";

        /// <summary>
        /// Плита треугольником
        /// </summary>
        public const string Treygolnik = "Настил треугольником";

        public static DetalType StringToEnum(string detalType)
        {
            switch (detalType)
            {
                case Plita:
                    return DetalType.Plita;

                case Stringer:
                    return DetalType.Stringer;

                case Treygolnik:
                    return DetalType.Treygolnik;

                default:
                    return DetalType.Plita;
            }
        }

        public static string EnumToString(DetalType detalType)
        {
            switch (detalType)
            {
                case DetalType.Plita:
                    return Plita;

                case DetalType.Stringer:
                    return Stringer;

                case DetalType.Treygolnik:
                    return Treygolnik;

                default:
                    return Plita;
            }
        }
    }
}
