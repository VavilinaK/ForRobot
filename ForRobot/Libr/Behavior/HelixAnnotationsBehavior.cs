using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Interactivity;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using HelixToolkit.Wpf;

using ForRobot.Model.File3D;

namespace ForRobot.Libr.Behavior
{
    /// <summary>
    /// Класс-поведение <see cref="HelixViewport3D"/> вывода параметров детали
    /// </summary>
    public class HelixAnnotationsBehavior : HelixAddCollectionBehavior<Annotation>
    {
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(nameof(FontSize),
                                                                                                 typeof(double),
                                                                                                 typeof(HelixAnnotationsBehavior),
                                                                                                 new PropertyMetadata(Model.File3D.Annotation.DefaultFontSize, OnFontSizeChanged));

        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HelixAnnotationsBehavior helixAnnotationsBehavior = (HelixAnnotationsBehavior)d;
            double fontSize = helixAnnotationsBehavior.FontSize = (double)e.NewValue;

            if (helixAnnotationsBehavior.Items == null) return;

            foreach(var item in helixAnnotationsBehavior.Items)
            {
                item.FontSize = fontSize;
            }
        }
    }
}
