using System;

namespace ForRobot.Libr.Messages
{
    /// <summary>
    /// Сообщение нахождения элемента по тэгу
    /// <para/>
    /// Используется для получения фокуса <see cref="System.Windows.Controls.TextBox"/> с привязанным соответствено <see cref="FindElementByTagMessage.TagProperty"/> параметром
    /// </summary>
    public class FindElementByTagMessage
    {
        public string TagProperty { get; private set; }

        public FindElementByTagMessage(string tagProperty)
        {
            this.TagProperty = tagProperty;
        }
    }
}
