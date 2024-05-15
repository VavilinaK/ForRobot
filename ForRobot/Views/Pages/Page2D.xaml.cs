using System;
using System.Windows;
using System.Windows.Controls;

using ForRobot.Model;

namespace ForRobot.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для Page2D.xaml
    /// </summary>
    public partial class Page2D : Page
    {
        #region Properties

        public Detal Detal2D
        {
            get => (Detal)GetValue(DetalProperty);
            set => SetValue(DetalProperty, value);
        }

        #region Static readonly

        public static readonly DependencyProperty DetalProperty = DependencyProperty.Register("Detal2D", typeof(Detal), typeof(Page2D));
               
        #endregion

        #endregion

        #region Construct

        public Page2D()
        {
            InitializeComponent();
        }

        #endregion
    }
}
