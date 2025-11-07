using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace ForRobot.Themes
{
    public partial class MetroStyleResourceDictionary
    {
        /// <summary>
        /// Метод выделения содержимого TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null)
                textBox.SelectAll();

            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Messages.ProperteisNameMessage(textBox.Tag as string));
        }

        /// <summary>
        /// Метод потери фокуса <see cref="TextBox"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Messages.ProperteisNameMessage(string.Empty)); // Отправка сообщения для изменения аннотации в HelixAnnotationsBehavior.
        }

        /// <summary>
        /// Метод выделения содержимого TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null && !textBox.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                textBox.Focus();
            }
        } 
    }
}
