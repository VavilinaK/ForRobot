using System;

namespace ForRobot.Model
{
    public static class ProcessStatuses
    {
        /// <summary>
        /// Прогамма не выбрана.
        /// </summary>
        public const string Free = "#P_FREE";

        /// <summary>
        /// Программа выбрана, но не запущена/сброшена.
        /// </summary>
        public const string Reset = "#P_RESET";

        /// <summary>
        /// Программа запущена.
        /// </summary>
        public const string Active = "#P_ACTIVE";

        /// <summary>
        /// Программа запущена, но остановлена.
        /// </summary>
        public const string Stop = "#P_STOP";

        /// <summary>
        /// Программа завершила работу
        /// </summary>
        public const string End = "#P_END";
    }
}
