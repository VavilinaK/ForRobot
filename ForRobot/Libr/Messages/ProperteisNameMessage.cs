using System;

namespace ForRobot.Libr.Messages
{
    /// <summary>
    /// Сообщение передачи имени свойства
    /// <para/>
    /// Используется для получения фокуса <see cref="System.Windows.Controls.TextBox"/> с привязанным соответствено <see cref="FindElementByTagMessage.TagProperty"/> параметром
    /// </summary>
    public class ProperteisNameMessage
    {
        public string PropertyName { get; private set; }

        public ProperteisNameMessage(string propertyName)
        {
            this.PropertyName = propertyName;
        }
    }
}
