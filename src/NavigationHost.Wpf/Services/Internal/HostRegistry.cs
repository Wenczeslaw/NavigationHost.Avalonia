using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NavigationHost.WPF.Abstractions;

namespace NavigationHost.WPF.Services.Internal
{
    /// <summary>
    ///     Internal service responsible for managing navigation host registration and retrieval.
    ///     Thread-safe implementation using ConcurrentDictionary.
    /// </summary>
    internal sealed class HostRegistry : IHostRegistry
    {
        private readonly ConcurrentDictionary<string, NavigationHost> _hosts = new ConcurrentDictionary<string, NavigationHost>();

        /// <summary>
        ///     Registers a navigation host with the specified host name.
        /// </summary>
        /// <param name="hostName">The unique name for the host.</param>
        /// <param name="host">The navigation host to register.</param>
        public void RegisterHost(string hostName, NavigationHost host)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host name cannot be null or whitespace.", nameof(hostName));

            if (host == null)
                throw new ArgumentNullException(nameof(host));

            if (!_hosts.TryAdd(hostName, host))
                throw new InvalidOperationException($"A host with name '{hostName}' is already registered.");
        }

        /// <summary>
        ///     Unregisters a host with the specified name.
        /// </summary>
        /// <param name="hostName">The name of the host to unregister.</param>
        /// <returns>True if the host was successfully unregistered; otherwise, false.</returns>
        public bool UnregisterHost(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                return false;

            return _hosts.TryRemove(hostName, out _);
        }

        /// <summary>
        ///     Gets a navigation host by host name.
        /// </summary>
        /// <param name="hostName">The name of the host.</param>
        /// <returns>The navigation host, or null if not found.</returns>
        public NavigationHost? GetHost(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                return null;

            _hosts.TryGetValue(hostName, out var host);
            return host;
        }

        /// <summary>
        ///     Gets all registered host names.
        /// </summary>
        /// <returns>A collection of registered host names.</returns>
        public IEnumerable<string> GetHostNames()
        {
            return _hosts.Keys;
        }
    }
}
