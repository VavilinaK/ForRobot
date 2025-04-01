using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Specialized;

using AvalonDock.Layout;

namespace ForRobot.Libr
{
    public static class DocumentPaneHelper
    {
        public static readonly DependencyProperty EmptyBackgroundProperty = DependencyProperty.RegisterAttached("EmptyBackground",
                                                                                                                typeof(Brush),
                                                                                                                typeof(DocumentPaneHelper),
                                                                                                                new PropertyMetadata(Brushes.LightGray, OnEmptyBackgroundChanged));

        public static Brush GetEmptyBackground(DependencyObject obj) => (Brush)obj.GetValue(EmptyBackgroundProperty);

        public static void SetEmptyBackground(DependencyObject obj, Brush value) => obj.SetValue(EmptyBackgroundProperty, value);

        private static void OnEmptyBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LayoutDocumentPane pane)
            {
                var parent = VisualTreeHelper.GetParent(pane) as FrameworkElement;

                while (parent != null && !(parent is Border))
                    parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;

                if (parent is Border border)
                {
                    UpdateBackground(border, pane);
                    var childrenProperty = typeof(LayoutDocumentPane).GetProperty("Children");
                    if (childrenProperty?.GetValue(pane) is INotifyCollectionChanged collection)
                    {
                        collection.CollectionChanged += (s, args) => UpdateBackground(border, pane);
                    }
                }
            }
        }

        private static void UpdateBackground(Border border, LayoutDocumentPane pane)
        {
            var children = pane.Children;
            bool hasDocuments = children != null && children.Count > 0;
            border.Background = hasDocuments ? Brushes.Transparent : GetEmptyBackground(border);
        }
    }
}
