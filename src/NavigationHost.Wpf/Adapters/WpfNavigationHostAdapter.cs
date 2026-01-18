using System.Windows;
using NavigationHost.Abstractions;

namespace NavigationHost.WPF.Adapters
{
    /// <summary>
    ///     WPF-specific implementation of INavigationHostAdapter.
    ///     Wraps WPF NavigationHost to provide platform-agnostic access.
    /// </summary>
    internal class WpfNavigationHostAdapter : INavigationHostAdapter
    {
        private readonly NavigationHost _navigationHost;

        public WpfNavigationHostAdapter(NavigationHost navigationHost)
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
                return current != null ? new WpfContentAdapter(current) : null;
            }
        }

        /// <summary>
        ///     Gets the DataContext of the navigation host.
        /// </summary>
        public object? DataContext => _navigationHost.DataContext;

        /// <summary>
        ///     Navigates to the specified content.
        /// </summary>
        /// <param name="content">The WPF FrameworkElement to navigate to.</param>
        public void Navigate(object content)
        {
            if (content is FrameworkElement element)
            {
                _navigationHost.Navigate(element);
            }
            else
            {
                throw new System.ArgumentException(
                    $"Content must be a WPF FrameworkElement, but got {content?.GetType().Name ?? "null"}",
                    nameof(content));
            }
        }

        /// <summary>
        ///     Gets the underlying WPF NavigationHost.
        /// </summary>
        public INavigationHost UnderlyingHost => _navigationHost;
    }
}
