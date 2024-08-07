using System;
using System.Text;
using System.Collections.Generic;

namespace ForRobot.Model
{
    ///// <summary>
    ///// Статус запроса
    ///// </summary>
    //public static class DataStatus
    //{
    //    public const string None = "";

    //    /// <summary>
    //    /// Данные передаются на сервер
    //    /// </summary>
    //    public const string DataTransfer = "Передача данных . . . .";

    //    /// <summary>
    //    /// Данные переданы
    //    /// </summary>
    //    public const string DataTransmitted = "Данные переданы !";
    //}


    [Flags]
    /// <summary>
    /// Состояние робота
    /// </summary>
    public enum RobotStatus
    {
        /// <summary>
        /// Программа не выбрана
        /// </summary>
        ProgrammNotSelect,

        /// <summary>
        /// Программа выбрана, но не запущена
        /// </summary>
        ProgrammSelected,

        /// <summary>
        /// Ппрограмма запущена, но остановлена
        /// </summary>
        StopProgramm,

        /// <summary>
        /// Программа в работе
        /// </summary>
        ProgrammInWork,

        /// <summary>
        /// Программа завершила работу
        /// </summary>
        EndProgramm
    }
}
