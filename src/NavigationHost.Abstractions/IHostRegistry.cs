using System.Collections.Generic;

namespace NavigationHost.Abstractions
{
    /// <summary>
    ///     Service responsible for managing navigation host registration and retrieval.
    /// </summary>
    /// <typeparam name="THost">The type of navigation host.</typeparam>
    public interface IHostRegistry<THost> where THost : class
    {
        /// <summary>
        ///     Registers a navigation host with the specified host name.
        /// </summary>
        /// <param name="hostName">The unique name for the host.</param>
        /// <param name="host">The navigation host to register.</param>
        void RegisterHost(string hostName, THost host);

        /// <summary>
        ///     Unregisters a host with the specified name.
        /// </summary>
        /// <param name="hostName">The name of the host to unregister.</param>
        /// <returns>True if the host was successfully unregistered; otherwise, false.</returns>
        bool UnregisterHost(string hostName);

        /// <summary>
        ///     Gets a navigation host by host name.
        /// </summary>
        /// <param name="hostName">The name of the host.</param>
        /// <returns>The navigation host, or null if not found.</returns>
        THost? GetHost(string hostName);

        /// <summary>
        ///     Gets all registered host names.
        /// </summary>
        /// <returns>A collection of registered host names.</returns>
        IEnumerable<string> GetHostNames();
    }
}

