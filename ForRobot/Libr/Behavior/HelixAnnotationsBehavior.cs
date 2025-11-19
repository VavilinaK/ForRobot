using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using GalaSoft.MvvmLight.Messaging;

using ForRobot.Models.File3D;
using ForRobot.Models.Detals;
using ForRobot.Libr.Strategies.AnnotationStrategies;

namespace ForRobot.Libr.Behavior
{
    /// <summary>
    /// Класс-поведение <see cref="HelixToolkit.Wpf.HelixViewport3D"/> вывода параметров детали
    /// </summary>
    public class HelixAnnotationsBehavior : HelixAddCollectionBehavior<Annotation>
    {
        #region Private variables

        private readonly ForRobot.Libr.Services.AnnotationService _annotationService;
        private readonly List<IDetalAnnotationStrategy> _strategies;

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

        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(HelixAnnotationsBehavior), new UIPropertyMetadata(2.0, VisualChanged));

        #endregion Public variables

        public HelixAnnotationsBehavior()
        {
            double scaleFactor = (double)ForRobot.Models.Settings.Settings.ScaleFactor;
            this._strategies = new List<IDetalAnnotationStrategy>
            {
                new PlateAnnotationStrategy(scaleFactor)
            };
            this._annotationService = new Services.AnnotationService(_strategies);

            Messenger.Default.Register<ForRobot.Libr.Messages.ProperteisNameMessage>(this, message => this.UpdateAnnotationsIsVisibale(message.PropertyName));
            Messenger.Default.Register<ForRobot.Libr.Messages.UpdateCurrentDetalMessage>(this, message =>
            {
                this.Detal = message.Detal;
            });
        }

        #region Private functions

        private void VisualChanged()
        {
            if (this.Items == null || this.Items.Count() == 0)
                return;

            foreach(Annotation item in this.Items)
            {
                if (item == null) continue;
                
                item.FontSize = this.FontSize;
                item.FontFamily = this.FontFamily;
                item.FontWeight = this.FontWeight;
                item.Foreground = this.Foreground;
                item.Background = this.LabelBackground;
                item.Thickness = this.Thickness;
            }
        }

        #region Handle

        private void PropertyChangeHandle(object sender, PropertyChangedEventArgs e) => this.ChangePropertyAnnotation(sender as Detal, e.PropertyName);

        #endregion

        #region Callback

        private static void VisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((HelixAnnotationsBehavior)d).VisualChanged();

        private static void OnDetalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HelixAnnotationsBehavior hab = (HelixAnnotationsBehavior)d;
            hab.Detal = (Detal)e.NewValue;

            if (e.OldValue != null && e.OldValue is Detal oldDetal)
                oldDetal.ChangePropertyEvent -= hab.PropertyChangeHandle;

            hab.Detal = (Detal)e.NewValue;

            if (hab.Detal == null)
            {
                if (hab.Items is ObservableCollection<Annotation> currentCollection)
                    currentCollection.Clear();
            }
            else
            {
                hab.Detal.ChangePropertyEvent += hab.PropertyChangeHandle;
                hab.UpdateAnnotations();
            }
        }

        #endregion Callback

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
                    currentCollection.Add(annotation);
                }
            }
            else
            {
                this.Items = annotations;
            }
            this.VisualChanged();
        }

        /// <summary>
        /// Изменение параметров
        /// </summary>
        /// <param name="detal"></param>
        /// <param name="propertyName">Наименование параметра</param>
        private void ChangePropertyAnnotation(Detal detal, string propertyName)
        {
            if (detal == null || string.IsNullOrEmpty(propertyName))
                return;

            this.UpdateAnnotations();
        }

        /// <summary>
        /// Форматирование значения для отображения
        /// </summary>
        /// <param name="value">Значение</param>
        /// <returns>Отформатированная строка</returns>
        private string FormatValue(object value)
        {
            if (value is decimal decimalValue)
                return string.Format("{0} mm", decimalValue);
            else if (value is double doubleValue)
                return string.Format("{0:F2} mm", doubleValue);
            else
                return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Обновление видимости <see cref="Annotation"/>
        /// </summary>
        /// <param name="propertyName">Имя свойства <see cref="Models.Detals.Detal"/></param>
        private void UpdateAnnotationsIsVisibale(string propertyName)
        {
            if (this.Items == null)
                return;

            foreach (var item in Items.Where(x => x != null))
                item.IsVisible = false;

            switch (propertyName)
            {
                case nameof(ForRobot.Models.Detals.Plita.DistanceToFirstRib):
                case nameof(ForRobot.Models.Detals.Plita.DistanceBetweenRibs):
                case nameof(Rib.DistanceLeft):
                case nameof(Rib.DistanceRight):
                    foreach (var item in this.Items.Where(x => x != null && x.PropertyName.Contains("Distance")))
                        item.IsVisible = true;
                    break;

                case nameof(ForRobot.Models.Detals.Plita.RibsIdentToLeft):
                case nameof(ForRobot.Models.Detals.Plita.RibsIdentToRight):
                    foreach (var item in this.Items.Where(x => x != null && x.PropertyName.Contains("Ident")))
                        item.IsVisible = true;
                    break;

                case nameof(Rib.DissolutionLeft):
                case nameof(Rib.DissolutionRight):
                    foreach (var item in this.Items.Where(x => x != null && x.PropertyName.Contains("Dissolution")))
                        item.IsVisible = true;
                    break;

                default:
                    foreach (var item in this.Items.Where(x => x != null && new List<string> { nameof(Plita.PlateLength), nameof(Plita.PlateWidth), nameof(Plita.PlateBevelToLeft), nameof(Plita.PlateBevelToRight) }.Contains(x.PropertyName)))
                        item.IsVisible = true;
                    break;
            }
        }

        #endregion Private functions

        ~HelixAnnotationsBehavior()
        {
            Messenger.Default.Unregister<ForRobot.Libr.Messages.ProperteisNameMessage>(this);
            Messenger.Default.Unregister<ForRobot.Libr.Messages.UpdateCurrentDetalMessage>(this);
        }
    }
}
