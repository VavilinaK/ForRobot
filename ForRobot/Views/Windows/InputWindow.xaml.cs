using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ForRobot.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window, IDisposable
    {
        #region Public variables

        /// <summary>
        /// Выходной текст
        /// </summary>
        public string Answer { get => this.InputText.Text; }

        #endregion

        #region Constructs

        public InputWindow()
        {
            InitializeComponent();
            this.Closed += (a, b) => this.Dispose();
        }

        public InputWindow(string question, string defaultAnswer = "") : this()
        {
            this.Question.Content = question;
            this.InputText.Text = defaultAnswer;
        }

        #endregion

        #region Private functions

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            this.InputText.Focus();
            this.InputText.SelectAll();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e) => this.DialogResult = true;

        #endregion

        #region IDisposable Support

        private bool _disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing) { }
                _disposedValue = true;
            }
        }

        ~InputWindow() => Dispose(false);

        // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
