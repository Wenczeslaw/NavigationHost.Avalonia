using System;
using NavigationHost.Abstractions;

namespace NavigationHost.WPF.Services
{
    /// <summary>
    ///     Provides a service locator pattern for accessing the HostManager instance.
    ///     This is useful for scenarios where dependency injection is not available.
    ///     Prefer using dependency injection when possible.
    /// </summary>
    public static class HostManagerLocator
    {
        private static IHostManager? _current;

        /// <summary>
        ///     Gets or sets the current HostManager instance.
        /// </summary>
        public static IHostManager Current
        {
            get => _current ?? throw new InvalidOperationException(
                "HostManager has not been initialized. " +
                "Call HostManagerLocator.SetCurrent() or use dependency injection.");
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        ///     Sets the current HostManager instance.
        /// </summary>
        /// <param name="hostManager">The HostManager instance to set.</param>
        public static void SetCurrent(IHostManager hostManager)
        {
            Current = hostManager;
        }

        /// <summary>
        ///     Clears the current HostManager instance.
        /// </summary>
        public static void Clear()
        {
            _current = null;
        }
    }
}

