using System;

using ForRobot.Models.Detals;

namespace ForRobot.Libr.Messages
{
    /// <summary>
    /// Сообщение обновления/передачи детали
    /// <para/>
    /// Используется для обновления свойства <see cref="ForRobot.Models.Detals.Detal"/> в HelixAnnotationsBehavior и HelixWeldsBehavior
    /// </summary>
    public class UpdateCurrentDetalMessage
    {
        public Detal Detal { get; private set; }

        public UpdateCurrentDetalMessage(Detal detal)
        {
            this.Detal = detal;
        }
    }
}
