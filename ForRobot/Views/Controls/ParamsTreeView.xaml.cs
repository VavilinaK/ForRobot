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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interactivity;

namespace ForRobot.Views.Controls
{
    /// <summary>
    /// Поведение всех <see cref="TreeViewItem"/> для обработки события ToolTipOpening
    /// </summary>
    public class TreeViewItemToolTipBehavior : Behavior<TreeViewItem>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ToolTipOpening += OnToolTipOpening;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ToolTipOpening -= OnToolTipOpening;
            base.OnDetaching();
        }

        private void OnToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var treeView = FindVisualParent<ParamsTreeView>(AssociatedObject);
            if (treeView == null) return;
                        
            // Обрабатываем разные типы ToolTip
            switch (AssociatedObject.ToolTip)
            {
                case string text:
                    treeView.LastToolTip = text;
                    break;

                case ToolTip toolTip:
                    treeView.LastToolTip = toolTip.Content?.ToString() ?? string.Empty;
                    break;

                default:
                    treeView.LastToolTip = AssociatedObject.ToolTip?.ToString() ?? string.Empty;
                    break;
            }
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            return parentObject is T parent ? parent : FindVisualParent<T>(parentObject);
        }

        private string GetToolTipContent(ToolTip toolTip)
        {
            if (toolTip.Content is string text)
                return text;

            if (toolTip.Content is FrameworkElement element)
                return ExtractTextFromElement(element);

            return toolTip.Content?.ToString() ?? string.Empty;
        }

        private string ExtractTextFromElement(FrameworkElement element) => element.ToString();
    }

    /// <summary>
    /// Логика взаимодействия для ParamsTreeView.xaml
    /// </summary>
    public partial class ParamsTreeView : TreeView
    {
        /// <summary>
        /// Свойство для хранения последнего ToolTip
        /// </summary>
        public static readonly DependencyProperty LastToolTipProperty = DependencyProperty.Register(nameof(LastToolTip),
                                                                                                    typeof(string),
                                                                                                    typeof(ParamsTreeView),
                                                                                                    new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string LastToolTip
        {             
            get => (string)GetValue(LastToolTipProperty);
            set => SetValue(LastToolTipProperty, value);
        }

        static ParamsTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ParamsTreeView), new FrameworkPropertyMetadata(typeof(ParamsTreeView)));
        }

        public ParamsTreeView()
        {
            InitializeComponent();
        }
    }
}
