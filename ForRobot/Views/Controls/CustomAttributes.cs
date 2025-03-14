using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.ComponentModel;

using HelixToolkit.Wpf;

namespace ForRobot.Views.Controls
{
    public partial class CustomAttributes
    {
        public static readonly DependencyProperty VisibleProperty = DependencyProperty.RegisterAttached("IsVisible",
                                                                                                         typeof(bool),
                                                                                                         typeof(CustomAttributes),
                                                                                                         new UIPropertyMetadata(true));

        public static bool GetIsVisible(System.Windows.UIElement element)
        {
            return (bool)element.GetValue(VisibleProperty);
            //return (bool)obj.GetValue(VisibleProperty);
        }

        public static void SetIsVisible(System.Windows.UIElement element, bool value)
        {
            element.SetValue(VisibleProperty, value); 
            //obj.SetValue(VisibleProperty, value);
        }

        //public static bool GetIsVisible(DependencyObject obj)
        //{
        //    return (bool)obj.GetValue(VisibleProperty);
        //}

        //public static void SetIsVisible(DependencyObject obj, bool value)
        //{
        //    obj.SetValue(VisibleProperty, value);
        //}

        //public static void OnVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    //var element = (ModelVisual3D)d;

        //    //if (element != null && (bool)e.NewValue)
        //    //    element.IsVisibleChanged += OnVisiblePropertyChanged;
        //    //else
        //    //    element.IsVisibleChanged -= OnVisiblePropertyChanged;

        //    //d.SetValue(Control.VisibilityProperty, e);

        //    if (d == null)
        //        return;

        //    //d.SetValue(Control.IsVisibleProperty, e);

        //    //bool newValue = (bool)e.NewValue;
        //    //var behaviours = Interaction.GetBehaviors(d);

        //    //if (d.GetType() == typeof(CoordinateSystemVisual3D))
        //    //{
        //    //    var c = d as CoordinateSystemVisual3D;
        //    //    c.XAxisColor = Colors.Transparent;
        //    //    c.YAxisColor = Colors.Transparent;
        //    //    c.ZAxisColor = Colors.Transparent;

        //    //}
        //}


        //private static void VisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    bool newValue = (bool)e.NewValue;
        //}
    }
}
