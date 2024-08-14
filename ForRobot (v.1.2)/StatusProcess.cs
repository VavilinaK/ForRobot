using System;

namespace ForRobot.Libr.Client
{
    public static class StatusProcess
    {
        public static readonly string None = "";

        public static readonly string Error = "Ошибка!";

        public static readonly string ErrorGeneration = "Ошибка генерации программы!";

        public static readonly string RobotBusy = "Уже запущен процесс";

        public static readonly string ProgrammReady = "Готов к работе";

        public static readonly string ProgrammStart = "Программа запущена";

        public static readonly string ProgrammPause = "Программа остановлена";

        public static readonly string ProgrammStop = "Программа сброшена";

        public static readonly string ProgrammEnd = "Программа завершена";

        public static readonly string ProgrammNotSelected = "Не выбрана программа";
    }
}
