using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

namespace ForRobot.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для SliderButton.xaml
    /// </summary>
    public partial class SliderButton : System.Windows.Controls.Primitives.ToggleButton
    {
        #region Public variables

        #region Properties

        public double ButtonWidth { get => (double)GetValue(ButtonWidthProperty); set => SetValue(ButtonWidthProperty, value); }

        public static readonly DependencyProperty ButtonWidthProperty = DependencyProperty.Register(nameof(ButtonWidth),
                                                                                        typeof(double),
                                                                                        typeof(SliderButton),
                                                                                        new PropertyMetadata(0.0));

        public string OnLabel { get => (string)GetValue(OnLabelProperty); set => SetValue(OnLabelProperty, value); }

        public static readonly DependencyProperty OnLabelProperty = DependencyProperty.Register(nameof(OnLabel),
                                                                                                typeof(string),
                                                                                                typeof(SliderButton),
                                                                                                new PropertyMetadata("Да"));

        public string OffLabel { get => (string)GetValue(OffLabelProperty); set => SetValue(OffLabelProperty, value); }

        public static readonly DependencyProperty OffLabelProperty = DependencyProperty.Register(nameof(OffLabel),
                                                                                                 typeof(string),
                                                                                                 typeof(SliderButton),
                                                                                                 new PropertyMetadata("Нет"));

        #endregion

        #endregion

        public SliderButton()
        {
            InitializeComponent();
        }
    }
}
