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
using ForRobot.Model.Detals;

namespace ForRobot.Libr.Behavior
{
    /// <summary>
    /// Класс-поведение <see cref="HelixViewport3D"/> вывода параметров детали
    /// </summary>
    public class HelixAnnotationsBehavior : HelixAddCollectionBehavior<Annotation>
    {
        private readonly ForRobot.Services.IAnnotationService _annotationService = new ForRobot.Services.AnnotationService(ForRobot.Model.Settings.Settings.ScaleFactor);

        public Detal Detal
        {
            get => (Detal)GetValue(DetalProperty);
            set => SetValue(DetalProperty, value);
        }

        public static readonly DependencyProperty DetalProperty = DependencyProperty.Register(nameof(Detal),
                                                                                              typeof(Detal),
                                                                                              typeof(HelixAnnotationsBehavior),
                                                                                              new PropertyMetadata(null, new PropertyChangedCallback(OnDetalChanged)));
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(nameof(FontSize),
                                                                                                 typeof(double),
                                                                                                 typeof(HelixAnnotationsBehavior),
                                                                                                 new PropertyMetadata(Model.File3D.Annotation.DefaultFontSize, OnFontSizeChanged));

        private static void OnDetalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HelixAnnotationsBehavior helixAnnotationsBehavior = (HelixAnnotationsBehavior)d;
            helixAnnotationsBehavior.Detal = (Detal)e.NewValue;

            if (e.OldValue != null && e.OldValue is Detal oldDetal)
                oldDetal.ChangePropertyEvent -= helixAnnotationsBehavior.PropertyChangeHandle;

            helixAnnotationsBehavior.Detal = (Detal)e.NewValue;

            if (helixAnnotationsBehavior.Detal != null)
            {
                helixAnnotationsBehavior.Detal.ChangePropertyEvent += helixAnnotationsBehavior.PropertyChangeHandle;
                helixAnnotationsBehavior.UpdateAnnotations();
            }
        }

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

        private void PropertyChangeHandle(object sender, PropertyChangedEventArgs e) => this.ChangePropertyAnnotations(sender as Detal, e.PropertyName);

        /// <summary>
        /// Обновление параметров
        /// </summary>
        private void UpdateAnnotations()
        {
            var annotations = this._annotationService.GetAnnotations(this.Detal);
            if (this.Items != null && this.Items is ObservableCollection<Annotation> currentCollection)
            {
                currentCollection.Clear();
                foreach (var annotation in annotations)
                {
                    annotation.FontSize = this.FontSize;
                    currentCollection.Add(annotation);
                }
            }
            else
            {
                var newCollection = new ObservableCollection<Annotation>(annotations);
                foreach (var item in newCollection)
                {
                    item.FontSize = this.FontSize;
                }
                this.Items = newCollection;
            }
        }

        /// <summary>
        /// Изменение подписи одного из параметров
        /// </summary>
        /// <param name="detal"></param>
        /// <param name="propertyName">Наименование параметра</param>
        private void ChangePropertyAnnotations(Detal detal, string propertyName)
        {
            if (detal == null && string.IsNullOrEmpty(propertyName))
                return;

            if (detal is Plita plate && propertyName == nameof(plate.ScoseType))
            {
                this.UpdateAnnotations();
                return;
            }

            Annotation annotation = this.Items.Count(item => item.PropertyName == propertyName) > 0 ? this.Items.Where(item => item.PropertyName == propertyName).First() : null;

            if (annotation == null)
                return;

            var newValue = detal.GetType().GetProperty(propertyName).GetValue(detal, null);
            annotation.Text = _annotationService.ToString(Convert.ToDecimal(newValue));
        }
    }
}
