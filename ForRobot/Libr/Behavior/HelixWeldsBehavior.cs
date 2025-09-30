using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;

using ForRobot.Model.File3D;
using ForRobot.Model.Detals;

namespace ForRobot.Libr.Behavior
{
    public class HelixWeldsBehavior : HelixAddCollectionBehavior<Weld>
    {
        public Detal Detal
        {
            get => (Detal)GetValue(DetalProperty);
            set => SetValue(DetalProperty, value);
        }

        public static readonly DependencyProperty DetalProperty = DependencyProperty.Register(nameof(Detal),
                                                                                              typeof(Detal),
                                                                                              typeof(HelixWeldsBehavior),
                                                                                              new PropertyMetadata(null, new PropertyChangedCallback(OnDetalChanged)));

        public virtual double Thickness
        {
            get => (double)GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }

        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(nameof(Thickness),
                                                                                                  typeof(double),
                                                                                                  typeof(HelixWeldsBehavior),
                                                                                                  new PropertyMetadata(5.0, OnThicknessChanged));

        private static void OnDetalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HelixWeldsBehavior helixWeldsBehavior = (HelixWeldsBehavior)d;

            if (e.OldValue != null && e.OldValue is Detal oldDetal)
                oldDetal.ChangePropertyEvent -= helixWeldsBehavior.PropertyChangeHandle;

            if (helixWeldsBehavior.Items != null && helixWeldsBehavior.Items is ObservableCollection<Weld> currentCollection)
                foreach (var item in currentCollection) item.Children.Clear();

            helixWeldsBehavior.Detal = (Detal)e.NewValue;

            if (helixWeldsBehavior.Detal != null)
                helixWeldsBehavior.Detal.ChangePropertyEvent += helixWeldsBehavior.PropertyChangeHandle;

            helixWeldsBehavior.UpdateWelds();
        }

        private static void OnThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HelixWeldsBehavior helixWeldsBehavior = (HelixWeldsBehavior)d;
            helixWeldsBehavior.Thickness = (double)e.NewValue;

            if (helixWeldsBehavior.Items == null)
                return;

            foreach (var item in helixWeldsBehavior.Items)
                item.Thickness = helixWeldsBehavior.Thickness;
        }

        private void PropertyChangeHandle(object sender, PropertyChangedEventArgs e) => this.UpdateWelds();

        private void UpdateWelds()
        {
            ForRobot.Services.IWeldService weldService = new ForRobot.Services.WeldService(ForRobot.Model.Settings.Settings.ScaleFactor);

            var welds = weldService.GetWelds(this.Detal);

            if (this.Items is ObservableCollection<Weld> currentCollection)
            {
                foreach (var item in currentCollection) item.Children.Clear();
                //currentCollection.Clear();
                //currentCollection.CollectionChanged -= this.HandleCollectionChanged;
                foreach (var weld in welds)
                {
                    weld.Thickness = this.Thickness;
                    currentCollection.Add(weld);
                }
            }
            else
            {
                var newCollection = new ObservableCollection<Weld>(welds);
                foreach (var item in newCollection)
                {
                    item.Thickness = this.Thickness;
                }
                this.Items = newCollection;
            }
        }
    }
}
