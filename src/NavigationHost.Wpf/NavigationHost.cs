using System;
using System.Windows;
using System.Windows.Controls;
using NavigationHost.WPF.Abstractions;
using NavigationHost.WPF.Events;

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
        ///     Occurs when navigation has completed.
        /// </summary>
        public event EventHandler<NavigationEventArgs>? Navigated;

        /// <summary>
        ///     Navigates to the specified content.
        /// </summary>
        /// <param name="content">The content to navigate to.</param>
        public void Navigate(FrameworkElement content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var previousContent = CurrentContent;
            CurrentContent = content;
            
            Navigated?.Invoke(this, new NavigationEventArgs(content, previousContent));
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

