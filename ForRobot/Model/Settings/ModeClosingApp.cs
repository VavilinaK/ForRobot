using System;

namespace ForRobot.Model.Settings
{
    [Flags]
    /// <summary>
    /// Переключатель. спрашивает о закрытии приложения
    /// </summary>
    public enum ModeClosingApp
    {
        /// <summary>
        /// Всегда
        /// </summary>
        Ever,

        /// <summary>
        /// Никогда
        /// </summary>
        Never,

        /// <summary>
        /// Если хоть один робот имеет открытое соединение
        /// </summary>
        HaveConnected
    }
}
