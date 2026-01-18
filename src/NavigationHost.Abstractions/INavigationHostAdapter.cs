namespace NavigationHost.Abstractions
{
    /// <summary>
    ///     Adapter interface for platform-specific NavigationHost implementations.
    ///     Provides a platform-agnostic way to interact with navigation host controls.
    /// </summary>
    public interface INavigationHostAdapter
    {
        /// <summary>
        ///     Gets the current content adapter being displayed.
        /// </summary>
        IContentAdapter? CurrentContent { get; }

        /// <summary>
        ///     Gets the DataContext of the navigation host.
        /// </summary>
        object? DataContext { get; }

        /// <summary>
        ///     Navigates to the specified content.
        /// </summary>
        /// <param name="content">The platform-specific content to navigate to.</param>
        void Navigate(object content);

        /// <summary>
        ///     Gets the underlying platform-specific navigation host.
        /// </summary>
        INavigationHost UnderlyingHost { get; }
    }
}
