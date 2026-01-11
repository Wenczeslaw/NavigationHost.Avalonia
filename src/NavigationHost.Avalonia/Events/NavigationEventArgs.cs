using System;
using Avalonia.Controls;

namespace NavigationHost.Avalonia.Events
{
    /// <summary>
    ///     Provides data for navigation events.
    /// </summary>
    public sealed class NavigationEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationEventArgs" /> class.
        /// </summary>
        /// <param name="content">The content being navigated to.</param>
        /// <param name="previousContent">The previous content, if any.</param>
        public NavigationEventArgs(Control? content, Control? previousContent)
        {
            Content = content;
            PreviousContent = previousContent;
        }

        /// <summary>
        ///     Gets the content being navigated to.
        /// </summary>
        public Control? Content { get; }

        /// <summary>
        ///     Gets the previous content.
        /// </summary>
        public Control? PreviousContent { get; }
    }
}