using System;
using System.Windows;
using AbstractionsNS = NavigationHost.Abstractions;

namespace NavigationHost.WPF.Events
{
    /// <summary>
    ///     Provides data for navigation events in WPF.
    /// </summary>
    public sealed class NavigationEventArgs : AbstractionsNS.NavigationEventArgs<FrameworkElement>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationEventArgs" /> class.
        /// </summary>
        /// <param name="content">The content being navigated to.</param>
        /// <param name="previousContent">The previous content, if any.</param>
        public NavigationEventArgs(FrameworkElement? content, FrameworkElement? previousContent)
            : base(content, previousContent)
        {
        }
    }
}

