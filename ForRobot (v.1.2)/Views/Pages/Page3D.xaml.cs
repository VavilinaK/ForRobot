using System;
using System.Windows;
using System.Windows.Controls;

using ForRobot.Model.Detals;

namespace ForRobot.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для Page3D.xaml
    /// </summary>
    public partial class Page3D : Page
    {
        #region Properties

        public Detal Detal3D
        {
            get => (Detal)GetValue(DetalProperty);
            set => SetValue(DetalProperty, value);
        }


        public string Logger3D
        {
            get => (string)GetValue(LoggerProperty);
            set => SetValue(LoggerProperty, value);
        }

        #region Static readonly

        public static readonly DependencyProperty DetalProperty = DependencyProperty.Register("Detal3D", typeof(Detal), typeof(Page3D));
       
        public static readonly DependencyProperty LoggerProperty = DependencyProperty.Register("Logger3D", typeof(string), typeof(Page3D));

        #endregion

        #endregion

        #region Construct

        public Page3D()
        {
            InitializeComponent();
        }

        #endregion
    }
}
