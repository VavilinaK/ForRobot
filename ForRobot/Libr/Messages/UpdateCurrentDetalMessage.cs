using System;

using ForRobot.Model.Detals;

namespace ForRobot.Libr.Messages
{
    public class UpdateCurrentDetalMessage
    {
        public Detal Detal { get; private set; }

        public UpdateCurrentDetalMessage(Detal detal)
        {
            this.Detal = detal;
        }
    }
}
