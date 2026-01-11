using System.Collections.Generic;
using NavigationHost.Avalonia.Abstractions;

namespace NavigationHost.Avalonia.Services
{
    /// <summary>
    ///     Service locator for HostManager. Provides global access to the HostManager instance.
    ///     This is used internally by NavigationHost to automatically resolve the HostManager.
    /// </summary>
    internal static class HostManagerLocator
    {
        private static IHostManager? _instance;
        private static readonly List<PendingRegistration> _pendingRegistrations = new List<PendingRegistration>();

        /// <summary>
        ///     Gets or sets the current HostManager instance.
        /// </summary>
        public static IHostManager? Current
        {
            get => _instance;
            set
            {
                _instance = value;
                
                // Process any pending registrations when HostManager becomes available
                if (_instance != null && _instance is HostManager hostManager)
                {
                    ProcessPendingRegistrations(hostManager);
                }
            }
        }

        /// <summary>
        ///     Checks if a HostManager instance is available.
        /// </summary>
        public static bool IsInitialized => _instance != null;

        /// <summary>
        ///     Registers a pending host registration that will be processed when HostManager becomes available.
        /// </summary>
        /// <param name="host">The NavigationHost to register.</param>
        /// <param name="hostName">The name for the host.</param>
        internal static void RegisterPending(NavigationHost host, string hostName)
        {
            // If HostManager is already available, register immediately
            if (_instance != null && _instance is HostManager hostManager)
            {
                hostManager.RegisterHost(hostName, host);
                HostManager.SetHostManager(host, hostManager);
                return;
            }

            // Otherwise, add to pending list
            _pendingRegistrations.Add(new PendingRegistration(host, hostName));
        }

        /// <summary>
        ///     Processes all pending host registrations.
        /// </summary>
        /// <param name="hostManager">The HostManager to register hosts with.</param>
        private static void ProcessPendingRegistrations(HostManager hostManager)
        {
            if (_pendingRegistrations.Count == 0)
                return;

            foreach (var pending in _pendingRegistrations)
            {
                hostManager.RegisterHost(pending.HostName, pending.Host);
                HostManager.SetHostManager(pending.Host, hostManager);
            }

            _pendingRegistrations.Clear();
        }

        /// <summary>
        ///     Resets the current HostManager instance (useful for testing).
        /// </summary>
        internal static void Reset()
        {
            _instance = null;
            _pendingRegistrations.Clear();
        }

        /// <summary>
        ///     Represents a pending host registration.
        /// </summary>
        private class PendingRegistration
        {
            public NavigationHost Host { get; }
            public string HostName { get; }

            public PendingRegistration(NavigationHost host, string hostName)
            {
                Host = host;
                HostName = hostName;
            }
        }
    }
}
