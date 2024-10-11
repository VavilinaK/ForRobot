using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using System.Windows.Media.Imaging;
using System.Diagnostics;

using ForRobot.Model.Detals;

namespace ForRobot.Views.Pages
{
    public partial class Image2D : System.Windows.Controls.Image
    {
        public static readonly RoutedEvent MouseDoubleClick = EventManager.RegisterRoutedEvent(nameof(MouseDoubleClickEvent), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Image2D));

        public event RoutedEventHandler MouseDoubleClickEvent
        {
            add
            {
                AddHandler(MouseDoubleClick, value);
            }
            remove
            {
                RemoveHandler(MouseDoubleClick, value);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                RaiseEvent(new MouseDoubleClickEventArgs(MouseDoubleClick, this));
            }
            base.OnMouseLeftButtonDown(e);
        }

        public class MouseDoubleClickEventArgs : RoutedEventArgs
        {
            public MouseDoubleClickEventArgs(RoutedEvent routedEvent, object source)
                : base(routedEvent, source)
            {
            }
        }
    }

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
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create((obj as Image).Source as BitmapImage));

                        string filePath = Path.Combine(Path.GetTempPath(), "Параметры_детали.png");

                        using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                        {
                            encoder.Save(fileStream);
                        }

                        ProcessStartInfo Info = new ProcessStartInfo()
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            FileName = "explorer.exe",
                            WindowStyle = ProcessWindowStyle.Normal,
                            Arguments = filePath
                        };
                        Process.Start(Info);
                    }));
            }
        }

        #endregion
    }
}
