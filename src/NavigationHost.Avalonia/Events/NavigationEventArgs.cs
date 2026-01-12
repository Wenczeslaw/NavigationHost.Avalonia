using Avalonia.Controls;

namespace NavigationHost.Avalonia.Events
{
    /// <summary>
    ///     Provides data for navigation events in Avalonia.
    /// </summary>
    public sealed class NavigationEventArgs : global::NavigationHost.Abstractions.NavigationEventArgs<Control>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationEventArgs" /> class.
        /// </summary>
        /// <param name="content">The content being navigated to.</param>
        /// <param name="previousContent">The previous content, if any.</param>
        public NavigationEventArgs(Control? content, Control? previousContent)
            : base(content, previousContent)
        {
        }
    }
}