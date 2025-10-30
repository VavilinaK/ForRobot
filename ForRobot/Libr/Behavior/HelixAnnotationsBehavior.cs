using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
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
        #region Private variables

        private readonly ForRobot.Services.IAnnotationService _annotationService = new ForRobot.Services.AnnotationService(ForRobot.Model.Settings.Settings.ScaleFactor);

        #endregion

        #region Public variables
        
        public Detal Detal
        {
            get => (Detal)GetValue(DetalProperty);
            set => SetValue(DetalProperty, value);
        }

        public static readonly DependencyProperty DetalProperty = DependencyProperty.Register(nameof(Detal), typeof(Detal), typeof(HelixAnnotationsBehavior), new PropertyMetadata(null, OnDetalChanged));

        /// <summary>
        /// Gets or sets the size of the font.
        /// </summary>
        /// <value>The size of the font.</value>
        public double FontSize
        {
            get => (double)this.GetValue(FontSizeProperty);
            set => this.SetValue(FontSizeProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(HelixAnnotationsBehavior), new UIPropertyMetadata(20.0, VisualChanged));

        /// <summary>
        /// Gets or sets the font family.
        /// </summary>
        /// <value>The font family.</value>
        public FontFamily FontFamily
        {
            get => (FontFamily)this.GetValue(FontFamilyProperty);
            set => this.SetValue(FontFamilyProperty, value);
        }

        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(nameof(FontFamily), typeof(FontFamily), typeof(HelixAnnotationsBehavior), new UIPropertyMetadata(new FontFamily("GOST Type A"), VisualChanged));

        /// <summary>
        /// Gets or sets the font weight.
        /// </summary>
        /// <value>The font weight.</value>
        public FontWeight FontWeight
        {
            get => (FontWeight)this.GetValue(FontWeightProperty);
            set => this.SetValue(FontWeightProperty, value);
        }

        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(nameof(FontWeight), typeof(FontWeight), typeof(HelixAnnotationsBehavior), new UIPropertyMetadata(FontWeights.Normal, VisualChanged));

        /// <summary>
        /// Gets or sets the foreground brush.
        /// </summary>
        /// <value>The foreground.</value>
        public Brush Foreground
        {
            get => (Brush)this.GetValue(ForegroundProperty);
            set => this.SetValue(ForegroundProperty, value);
        }

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(HelixAnnotationsBehavior), new UIPropertyMetadata(Brushes.Black, VisualChanged));

        /// <summary>
        /// Gets or sets the background.
        /// </summary>
        /// <value>The background.</value>
        public Brush LabelBackground
        {
            get => (Brush)this.GetValue(LabelBackgroundProperty);
            set => this.SetValue(LabelBackgroundProperty, value);
        }
        
        public static readonly DependencyProperty LabelBackgroundProperty = DependencyProperty.Register(nameof(LabelBackground), typeof(Brush), typeof(HelixAnnotationsBehavior), new UIPropertyMetadata(Brushes.Transparent, VisualChanged));

        /// <summary>
        /// Gets or sets the thickness.
        /// </summary>
        /// <value>The thickness.</value>
        public double Thickness
        {
            get => (double)this.GetValue(ThicknessProperty);
            set => this.SetValue(ThicknessProperty, value);
        }

        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(HelixAnnotationsBehavior), new UIPropertyMetadata(5.0, VisualChanged));

        #endregion Public variables

        #region Private functions

        private void VisualChanged()
        {
            if (this.Items == null || this.Items.Count() == 0)
                return;

            foreach(Annotation item in this.Items)
            {
                if (item == null) continue;
                
                item.GetFontSize(this.FontSize);
                item.GetFontFamily(this.FontFamily);
                item.GetFontWeight(this.FontWeight);
                item.GetForeground(this.Foreground);
                item.GetLabelBackground(this.LabelBackground);
                item.GetThickness(this.Thickness);
            }
        }

        #region Handle

        private void PropertyChangeHandle(object sender, PropertyChangedEventArgs e) => this.ChangePropertyAnnotations(sender as Detal, e.PropertyName);
        
        #endregion

        #region Callback

        private static void VisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HelixAnnotationsBehavior)d).VisualChanged();
        }

        private static void OnDetalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HelixAnnotationsBehavior hab = (HelixAnnotationsBehavior)d;
            hab.Detal = (Detal)e.NewValue;

            if (e.OldValue != null && e.OldValue is Detal oldDetal)
                oldDetal.ChangePropertyEvent -= hab.PropertyChangeHandle;

            hab.Detal = (Detal)e.NewValue;

            if (hab.Detal != null)
            {
                hab.Detal.ChangePropertyEvent += hab.PropertyChangeHandle;
                hab.UpdateAnnotations();
            }
        }

        #endregion Callback

        #endregion Private functions

        //private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    HelixAnnotationsBehavior helixAnnotationsBehavior = (HelixAnnotationsBehavior)d;
        //    helixAnnotationsBehavior.FontSize = (double)e.NewValue;

        //    if (helixAnnotationsBehavior.Items == null) return;

        //    foreach(var item in helixAnnotationsBehavior.Items)
        //    {
        //        item.FontSize = helixAnnotationsBehavior.FontSize;
        //    }
        //}


        /// <summary>
        /// Обновление параметров
        /// </summary>
        private void UpdateAnnotations()
        {
            var annotations = this._annotationService.GetAnnotations(this.Detal);
            if (this.Items != null && this.Items is ObservableCollection<Annotation> currentCollection)
            {
                currentCollection.Clear();
                //currentCollection.Union(annotations);
                foreach (var annotation in annotations)
                {
                    //annotation.FontSize = this.FontSize;
                    currentCollection.Add(annotation);
                }
            }
            else
            {
                //var newCollection = new ObservableCollection<Annotation>(annotations);
                //foreach (var item in newCollection)
                //{
                //    item.FontSize = this.FontSize;
                //}
                //this.Items = newCollection;
                this.Items = annotations;
            }
            this.VisualChanged();
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
