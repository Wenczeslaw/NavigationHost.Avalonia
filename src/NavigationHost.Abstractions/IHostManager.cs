using System;
using System.Collections.Generic;

namespace NavigationHost.Abstractions
{
    /// <summary>
    ///     Defines the host manager service interface.
    ///     Manages multiple navigation hosts and provides centralized navigation control.
    ///     Note: Stack functionality has been removed.
    /// </summary>
    /// <typeparam name="TContent">The type of content that can be displayed (e.g., Control, UserControl).</typeparam>
    /// <typeparam name="THost">The type of navigation host.</typeparam>
    public interface IHostManager<TContent, THost> 
        where TContent : class
        where THost : class
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

        /// <summary>
        ///     Navigates to the specified content in a host.
        /// </summary>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="content">The content to navigate to.</param>
        void Navigate(string hostName, TContent content);

        /// <summary>
        ///     Navigates to the specified content type with optional parameters in a host.
        /// </summary>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="contentType">The type of content to navigate to.</param>
        /// <param name="parameter">Optional parameter to pass to the view model or content.</param>
        void Navigate(
            string hostName,
            Type contentType,
            object? parameter = null
        );

        /// <summary>
        ///     Navigates to the specified content type in a host.
        /// </summary>
        /// <typeparam name="T">The type of content to navigate to.</typeparam>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="parameter">Optional parameter to pass to the view model or content.</param>
        void Navigate<T>(string hostName, object? parameter = null)
            where T : TContent;
    }
}

