using System;
using System.Windows;
using ForRobot.Model.File3D;

namespace ForRobot.Libr.Behavior
{
    public class HelixWeldsBehavior : HelixAddCollectionBehavior<Weld>
    {
        public virtual double Thickness
        {
            get => (double)GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }

        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(nameof(Thickness),
                                                                                                  typeof(double),
                                                                                                  typeof(HelixWeldsBehavior),
                                                                                                  new PropertyMetadata(5.0, OnThicknessChanged));

        private static void OnThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HelixWeldsBehavior helixWeldsBehavior = (HelixWeldsBehavior)d;
            helixWeldsBehavior.Thickness = (double)e.NewValue;

            foreach (var item in helixWeldsBehavior.Items)
                item.Thickness = helixWeldsBehavior.Thickness;
        }
    }
}
