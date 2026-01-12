using System;

namespace NavigationHost.Abstractions
{
    /// <summary>
    ///     Defines the basic navigation service interface.
    ///     Provides methods for navigating between views.
    ///     Note: Stack functionality has been removed. CanGoBack always returns false and StackCount always returns 0.
    /// </summary>
    /// <typeparam name="TContent">The type of content that can be displayed (e.g., Control, UserControl).</typeparam>
    public interface INavigationHost<TContent> where TContent : class
    {
        /// <summary>
        ///     Gets the current content being displayed.
        /// </summary>
        TContent? CurrentContent { get; }

        /// <summary>
        ///     Occurs when navigation has completed.
        /// </summary>
        event EventHandler<NavigationEventArgs<TContent>>? Navigated;

        /// <summary>
        ///     Navigates to the specified content.
        /// </summary>
        /// <param name="content">The content to navigate to.</param>
        void Navigate(TContent content);
    }
}

