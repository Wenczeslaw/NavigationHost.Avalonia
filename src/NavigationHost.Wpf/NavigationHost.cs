using System;
using System.Windows;
using System.Windows.Controls;
using NavigationHost.Abstractions;

namespace NavigationHost.WPF
{
    /// <summary>
    ///     A control that provides navigation services for WPF applications.
    ///     Displays different views dynamically.
    /// </summary>
    public class NavigationHost : ContentControl, INavigationHost
    {
        /// <summary>
        ///     Defines the <see cref="CurrentContent" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty CurrentContentProperty =
            DependencyProperty.Register(
                nameof(CurrentContent),
                typeof(FrameworkElement),
                typeof(NavigationHost),
                new PropertyMetadata(null, OnCurrentContentChanged));

        /// <summary>
        ///     Defines the <see cref="DefaultContent" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultContentProperty =
            DependencyProperty.Register(
                nameof(DefaultContent),
                typeof(FrameworkElement),
                typeof(NavigationHost),
                new PropertyMetadata(null, OnDefaultContentChanged));

        static NavigationHost()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigationHost), 
                new FrameworkPropertyMetadata(typeof(NavigationHost)));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationHost" /> class.
        /// </summary>
        public NavigationHost()
        {
        }

        /// <summary>
        ///     Gets or sets the default content to display when no content is set.
        /// </summary>
        public FrameworkElement? DefaultContent
        {
            get => (FrameworkElement?)GetValue(DefaultContentProperty);
            set => SetValue(DefaultContentProperty, value);
        }

        /// <summary>
        ///     Gets or sets the current content being displayed.
        /// </summary>
        public FrameworkElement? CurrentContent
        {
            get => (FrameworkElement?)GetValue(CurrentContentProperty);
            set => SetValue(CurrentContentProperty, value);
        }

        /// <summary>
        ///     Gets the current content as object (INavigationHost implementation).
        /// </summary>
        object? INavigationHost.CurrentContent => CurrentContent;

        /// <summary>
        ///     Sets the content (INavigationHost implementation).
        /// </summary>
        void INavigationHost.SetContent(object content)
        {
            if (content is FrameworkElement element)
            {
                Navigate(element);
            }
            else
            {
                throw new ArgumentException("Content must be a WPF FrameworkElement", nameof(content));
            }
        }

        /// <summary>
        ///     Navigates to the specified content.
        /// </summary>
        /// <param name="content">The content to navigate to.</param>
        public void Navigate(FrameworkElement content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            
            CurrentContent = content;
        }

        private static void OnCurrentContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NavigationHost host)
            {
                host.Content = e.NewValue;
            }
        }

        private static void OnDefaultContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NavigationHost host && host.CurrentContent == null && e.NewValue is FrameworkElement defaultContent)
            {
                host.CurrentContent = defaultContent;
            }
        }
    }
}

