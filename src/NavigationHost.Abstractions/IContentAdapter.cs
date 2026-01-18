namespace NavigationHost.Abstractions
{
    /// <summary>
    ///     Adapter interface to abstract platform-specific content types.
    ///     Provides a platform-agnostic way to work with UI elements.
    /// </summary>
    public interface IContentAdapter
    {
        /// <summary>
        ///     Gets the underlying platform-specific content object.
        /// </summary>
        object Content { get; }

        /// <summary>
        ///     Gets or sets the DataContext of the content.
        /// </summary>
        object? DataContext { get; set; }

        /// <summary>
        ///     Gets the parent DataContext (inherited from parent control).
        /// </summary>
        object? ParentDataContext { get; }
    }
}
