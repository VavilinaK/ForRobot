using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace ForRobot.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageMain2.xaml
    /// </summary>
    public partial class PageMain2 : Page
    {
        #region Private variables

        private ViewModels.MainPageViewModel2 _viewModel;

        private GridLengthConverter converter = new GridLengthConverter();

        #endregion

        #region Public variables

        public ViewModels.MainPageViewModel2 ViewModel
        {
            get { return _viewModel ?? (ViewModels.MainPageViewModel2)this.DataContext ?? (_viewModel = new ViewModels.MainPageViewModel2()); }
        }

        #endregion

        #region Constructr

        public PageMain2()
        {
            InitializeComponent();
            
            if (Properties.Settings.Default.LeftColumnWidth != string.Empty)
                this.leftColumn.Width = (GridLength)converter.ConvertFromString(Properties.Settings.Default.LeftColumnWidth);
            if (Properties.Settings.Default.BottomRowHeight != string.Empty)
                this.bottomRow.Height = (GridLength)converter.ConvertFromString(Properties.Settings.Default.BottomRowHeight);

            if (this.DataContext == null) { this.DataContext = ViewModel; }
        }

        #endregion

        #region Private functions

        private void ColumnSplitterDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Properties.Settings.Default.LeftColumnWidth = converter.ConvertToString(leftColumn.Width);
            Properties.Settings.Default.Save();
        }

        private void RowSplitterDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Properties.Settings.Default.BottomRowHeight = converter.ConvertToString(bottomRow.Height);
            Properties.Settings.Default.Save();
        }

        #endregion
    }
}
