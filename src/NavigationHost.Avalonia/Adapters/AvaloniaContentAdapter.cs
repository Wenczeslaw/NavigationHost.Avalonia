using Avalonia.Controls;
using NavigationHost.Abstractions;

namespace NavigationHost.Avalonia.Adapters
{
    /// <summary>
    ///     Avalonia-specific implementation of IContentAdapter.
    ///     Wraps Avalonia Control to provide platform-agnostic access.
    /// </summary>
    internal class AvaloniaContentAdapter : IContentAdapter
    {
        private readonly Control _control;

        public AvaloniaContentAdapter(Control control)
        {
            _control = control ?? throw new System.ArgumentNullException(nameof(control));
        }

        /// <summary>
        ///     Gets the underlying Avalonia Control.
        /// </summary>
        public object Content => _control;

        /// <summary>
        ///     Gets or sets the DataContext of the control.
        /// </summary>
        public object? DataContext
        {
            get => _control.DataContext;
            set => _control.DataContext = value;
        }

        /// <summary>
        ///     Gets the parent DataContext (inherited from parent control).
        /// </summary>
        public object? ParentDataContext => _control.Parent?.DataContext;
    }
}
