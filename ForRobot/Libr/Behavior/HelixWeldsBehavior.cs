using System;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;

using GalaSoft.MvvmLight.Messaging;

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
                                                                                              new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDetalChanged));

        public virtual double Thickness
        {
            get => (double)GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }

        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(nameof(Thickness),
                                                                                                  typeof(double),
                                                                                                  typeof(HelixWeldsBehavior),
                                                                                                  new PropertyMetadata(Services.WeldService.DEFAULT_WELD_THICKNESS, OnThicknessChanged));

        public bool IsDivided
        {
            get => (bool)GetValue(IsDividedProperty);
            set => SetValue(IsDividedProperty, value);
        }

        public static readonly DependencyProperty IsDividedProperty = DependencyProperty.Register(nameof(IsDivided),
                                                                                                  typeof(bool),
                                                                                                  typeof(HelixWeldsBehavior),
                                                                                                  new PropertyMetadata(false, OnIsDividedChanged));

        public HelixWeldsBehavior()
        {
            Messenger.Default.Register<ForRobot.Libr.Messages.UpdateCurrentDetalMessage>(this, message => 
            {
                this.Detal = message.Detal;
                this.UpdateWelds();
            });
        }

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
            helixWeldsBehavior.UpdateWeldsThickness();
        }

        private static void OnIsDividedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HelixWeldsBehavior helixWeldsBehavior = (HelixWeldsBehavior)d;
            helixWeldsBehavior.IsDivided = (bool)e.NewValue;
            helixWeldsBehavior.UpdateWeldsIsDivided();
        }

        private void PropertyChangeHandle(object sender, PropertyChangedEventArgs e) => this.UpdateWelds();

        private void UpdateWelds()
        {
            ForRobot.Services.IWeldService weldService = new ForRobot.Services.WeldService(ForRobot.Model.Settings.Settings.ScaleFactor);

            //if (this.Detal == null)
            //    return;

            var welds = weldService.GetWelds(this.Detal);

            if (this.Items is ObservableCollection<Weld> currentCollection)
            {
                currentCollection.Clear();
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
                    item.IsDivided = this.IsDivided;
                }
                this.Items = newCollection;
            }
        }

        private void UpdateWeldsThickness()
        {
            if (!(this.Items is ObservableCollection<Weld> currentCollection) || currentCollection == null)
                return;

            foreach (var item in currentCollection)
                item.Thickness = this.Thickness;
        }

        private void UpdateWeldsIsDivided()
        {
            if (!(this.Items is ObservableCollection<Weld> currentCollection) || currentCollection == null)
                return;

            foreach (var item in currentCollection)
                item.IsDivided = this.IsDivided;
        }
    }
}
