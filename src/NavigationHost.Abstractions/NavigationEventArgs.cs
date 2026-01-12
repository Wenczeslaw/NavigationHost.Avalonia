using System;

namespace NavigationHost.Abstractions
{
    /// <summary>
    ///     Provides data for navigation events.
    /// </summary>
    /// <typeparam name="TContent">The type of content being navigated.</typeparam>
    public class NavigationEventArgs<TContent> : EventArgs where TContent : class
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationEventArgs{TContent}" /> class.
        /// </summary>
        /// <param name="content">The content being navigated to.</param>
        /// <param name="previousContent">The previous content, if any.</param>
        public NavigationEventArgs(TContent? content, TContent? previousContent)
        {
            Content = content;
            PreviousContent = previousContent;
        }

        /// <summary>
        ///     Gets the content being navigated to.
        /// </summary>
        public TContent? Content { get; }

        /// <summary>
        ///     Gets the previous content.
        /// </summary>
        public TContent? PreviousContent { get; }
    }
}
