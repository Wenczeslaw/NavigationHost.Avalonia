using System;
using System.Collections.Generic;
using NavigationHost.Avalonia.Abstractions;

namespace NavigationHost.Avalonia.Services.Internal
{
    /// <summary>
    /// Internal implementation of host registry service.
    /// </summary>
    internal sealed class HostRegistry : IHostRegistry
    {
        private readonly Dictionary<string, NavigationHost> _hosts = new Dictionary<string, NavigationHost>();

        public void RegisterHost(string hostName, NavigationHost host)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host name cannot be null or whitespace.", nameof(hostName));

            if (host == null)
                throw new ArgumentNullException(nameof(host));

            if (_hosts.ContainsKey(hostName))
                UnregisterHost(hostName);

            _hosts[hostName] = host;
        }

        public bool UnregisterHost(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                return false;

            return _hosts.Remove(hostName);
        }

        public NavigationHost? GetHost(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                return null;

            return _hosts.TryGetValue(hostName, out var host) ? host : null;
        }

        public IEnumerable<string> GetHostNames()
        {
            return _hosts.Keys;
        }
    }
}

