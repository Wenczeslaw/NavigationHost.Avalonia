using System.Windows;
using NavigationHost.Abstractions;

namespace NavigationHost.WPF.Adapters
{
    /// <summary>
    ///     WPF-specific implementation of IContentAdapter.
    ///     Wraps WPF FrameworkElement to provide platform-agnostic access.
    /// </summary>
    internal class WpfContentAdapter : IContentAdapter
    {
        private readonly FrameworkElement _element;

        public WpfContentAdapter(FrameworkElement element)
        {
            _element = element ?? throw new System.ArgumentNullException(nameof(element));
        }

        /// <summary>
        ///     Gets the underlying WPF FrameworkElement.
        /// </summary>
        public object Content => _element;

        /// <summary>
        ///     Gets or sets the DataContext of the element.
        /// </summary>
        public object? DataContext
        {
            get => _element.DataContext;
            set => _element.DataContext = value;
        }

        /// <summary>
        ///     Gets the parent DataContext (inherited from parent element).
        /// </summary>
        public object? ParentDataContext => (_element.Parent as FrameworkElement)?.DataContext;
    }
}
