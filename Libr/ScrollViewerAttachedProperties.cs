using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace ForRobot.Libr
{
    public static class ScrollViewerAttachedProperties
    {
        #region Private variables

        static readonly Dictionary<TextBox, TextBoxScrollingTrigger> _associations = new Dictionary<TextBox, TextBoxScrollingTrigger>();

        #endregion

        #region Public variables

        public static bool GetScrollOnTextChanged(DependencyObject dependencyObject) => (bool)dependencyObject.GetValue(ScrollOnTextChangedProperty);

        public static void SetScrollOnTextChanged(DependencyObject dependencyObject, bool value) => dependencyObject.SetValue(ScrollOnTextChangedProperty, value);

        #region Readonly

        public static readonly DependencyProperty ScrollOnTextChangedProperty = DependencyProperty.RegisterAttached("ScrollOnTextChanged", 
            typeof(bool), typeof(ScrollViewerAttachedProperties), new UIPropertyMetadata(false, OnScrollOnTextChanged));

        #endregion

        #endregion

        #region Private function

        private static void OnScrollOnTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var textBox = dependencyObject as TextBox;
            if (textBox == null)
                return;

            bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
            if (newValue == oldValue)
                return;

            if (newValue)
            {
                textBox.Loaded += TextBoxLoaded;
                textBox.Unloaded += TextBoxUnloaded;
            }
            else
            {
                textBox.Loaded -= TextBoxLoaded;
                textBox.Unloaded -= TextBoxUnloaded;
                if (_associations.ContainsKey(textBox))
                {
                    _associations[textBox].Dispose();
                }
            }
        }

        private static void TextBoxUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var textBox = (TextBox)sender;
            _associations[textBox].Dispose();
            textBox.Unloaded -= TextBoxUnloaded;
        }

        private static void TextBoxLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var textBox = (TextBox)sender;
            textBox.Loaded -= TextBoxLoaded;
            _associations[textBox] = new TextBoxScrollingTrigger(textBox);
        }

        #endregion

        class TextBoxScrollingTrigger : IDisposable
        {
            private TextBox TextBox { get; set; }

            public TextBoxScrollingTrigger(TextBox textBox)
            {
                TextBox = textBox;
                TextBox.TextChanged += OnTextBoxOnTextChanged;
            }

            private void OnTextBoxOnTextChanged(object sender, TextChangedEventArgs args) => TextBox.ScrollToEnd();

            public void Dispose()
            {
                TextBox.TextChanged -= OnTextBoxOnTextChanged;
            }
        }
    }
}
