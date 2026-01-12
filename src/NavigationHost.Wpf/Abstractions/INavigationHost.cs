using System;
using System.Windows;
using NavigationHost.WPF.Events;

namespace NavigationHost.WPF.Abstractions
{
    /// <summary>
    ///     Defines the basic navigation service interface for WPF.
    ///     Provides methods for navigating between views.
    ///     Note: Stack functionality has been removed. CanGoBack always returns false and StackCount always returns 0.
    ///     This is the WPF-specific version. For the framework-agnostic version, see NavigationHost.Abstractions.INavigationHost&lt;TContent&gt;.
    /// </summary>
    public interface INavigationHost
    {
        /// <summary>
        ///     Gets the current content being displayed.
        /// </summary>
        FrameworkElement? CurrentContent { get; }

        /// <summary>
        ///     Occurs when navigation has completed.
        /// </summary>
        event EventHandler<NavigationEventArgs>? Navigated;

        /// <summary>
        ///     Navigates to the specified content.
        /// </summary>
        /// <param name="content">The content to navigate to.</param>
        void Navigate(FrameworkElement content);
    }
}
