using Avalonia.Controls;
using NavigationHost.Abstractions;

namespace NavigationHost.Avalonia.Adapters
{
    /// <summary>
    ///     Avalonia-specific implementation of INavigationHostAdapter.
    ///     Wraps Avalonia NavigationHost to provide platform-agnostic access.
    /// </summary>
    internal class AvaloniaNavigationHostAdapter : INavigationHostAdapter
    {
        private readonly NavigationHost _navigationHost;

        public AvaloniaNavigationHostAdapter(NavigationHost navigationHost)
        {
            _navigationHost = navigationHost ?? throw new System.ArgumentNullException(nameof(navigationHost));
        }

        /// <summary>
        ///     Gets the current content adapter being displayed.
        /// </summary>
        public IContentAdapter? CurrentContent
        {
            get
            {
                var current = _navigationHost.CurrentContent;
                return current != null ? new AvaloniaContentAdapter(current) : null;
            }
        }

        /// <summary>
        ///     Gets the DataContext of the navigation host.
        /// </summary>
        public object? DataContext => _navigationHost.DataContext;

        /// <summary>
        ///     Navigates to the specified content.
        /// </summary>
        /// <param name="content">The Avalonia Control to navigate to.</param>
        public void Navigate(object content)
        {
            if (content is Control control)
            {
                _navigationHost.Navigate(control);
            }
            else
            {
                throw new System.ArgumentException(
                    $"Content must be an Avalonia Control, but got {content?.GetType().Name ?? "null"}",
                    nameof(content));
            }
        }

        /// <summary>
        ///     Gets the underlying Avalonia NavigationHost.
        /// </summary>
        public INavigationHost UnderlyingHost => _navigationHost;
    }
}
