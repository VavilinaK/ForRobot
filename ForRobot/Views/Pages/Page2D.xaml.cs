using System;
using System.Windows;
using System.Windows.Controls;

using ForRobot.Model.Detals;

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

        #region Commands

        public RelayCommand OpenImageCommand
        {
            get { return (RelayCommand)GetValue(OpenImageCommandProperty); }
            set { SetValue(OpenImageCommandProperty, value); }
        }

        #endregion

        #region Static readonly

        public static readonly DependencyProperty DetalProperty = DependencyProperty.Register("Detal2D", typeof(Detal), typeof(Page2D));

        public static readonly DependencyProperty OpenImageCommandProperty = DependencyProperty.Register(nameof(OpenImageCommand), typeof(RelayCommand), typeof(Page2D), new PropertyMetadata(OnOpenImageCommand));

        #endregion

        #endregion

        #region Construct

        public Page2D()
        {
            InitializeComponent();
        }

        #endregion

        #region Private function

        private static RelayCommand _openImageCommand;

        /// <summary>
        /// Открытие изображения детали
        /// </summary>
        public static RelayCommand OnOpenImageCommand
        {
            get
            {
                return _openImageCommand ??
                    (_openImageCommand = new RelayCommand(obj =>
                    {

                    }));
            }
        }

        #endregion
    }
}
