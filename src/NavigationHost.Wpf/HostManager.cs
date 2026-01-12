using System;
using System.Collections.Generic;
using System.Windows;
using NavigationHost.WPF.Abstractions;
using NavigationHost.WPF.Services;

namespace NavigationHost.WPF
{
    /// <summary>
    ///     Manages navigation hosts in the application, similar to Prism's RegionManager.
    ///     Provides attached properties to register NavigationHost controls with names in XAML.
    ///     Supports view-viewmodel registration with DI container integration.
    ///     Uses convention-based resolution to automatically map views to viewmodels.
    ///     Must be configured with a DI container via dependency injection.
    ///     Acts as a facade coordinating specialized internal services.
    /// </summary>
    public class HostManager : IHostManager
    {
        /// <summary>
        ///     Defines the HostName attached property.
        /// </summary>
        public static readonly DependencyProperty HostNameProperty =
            DependencyProperty.RegisterAttached(
                "HostName",
                typeof(string),
                typeof(HostManager),
                new PropertyMetadata(null, OnHostNameChanged));

        /// <summary>
        ///     Defines the HostManager attached property.
        /// </summary>
        public static readonly DependencyProperty HostManagerProperty =
            DependencyProperty.RegisterAttached(
                "HostManager",
                typeof(HostManager),
                typeof(HostManager),
                new PropertyMetadata(null, OnHostManagerChanged));

        private readonly IHostRegistry _hostRegistry;
        private readonly IViewModelConventionResolver _conventionResolver;
        private readonly Services.Internal.InstanceFactory _instanceFactory;

        /// <summary>
        ///     Initializes a new instance of the HostManager with custom service implementations.
        ///     Used for dependency injection scenarios.
        /// </summary>
        internal HostManager(
            IHostRegistry hostRegistry,
            IViewModelConventionResolver conventionResolver,
            Services.Internal.InstanceFactory instanceFactory)
        {
            _hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
            _conventionResolver = conventionResolver ?? throw new ArgumentNullException(nameof(conventionResolver));
            _instanceFactory = instanceFactory ?? throw new ArgumentNullException(nameof(instanceFactory));
        }

        /// <summary>
        ///     Registers a navigation host with the specified host name.
        /// </summary>
        /// <param name="hostName">The unique name for the host.</param>
        /// <param name="host">The navigation host to register.</param>
        public void RegisterHost(string hostName, NavigationHost host)
        {
            _hostRegistry.RegisterHost(hostName, host);
        }

        /// <summary>
        ///     Unregisters a host with the specified name.
        /// </summary>
        /// <param name="hostName">The name of the host to unregister.</param>
        /// <returns>True if the host was successfully unregistered; otherwise, false.</returns>
        public bool UnregisterHost(string hostName)
        {
            return _hostRegistry.UnregisterHost(hostName);
        }

        /// <summary>
        ///     Gets a navigation host by host name.
        /// </summary>
        /// <param name="hostName">The name of the host.</param>
        /// <returns>The navigation host, or null if not found.</returns>
        public NavigationHost? GetHost(string hostName)
        {
            return _hostRegistry.GetHost(hostName);
        }

        /// <summary>
        ///     Gets all registered host names.
        /// </summary>
        /// <returns>A collection of registered host names.</returns>
        public IEnumerable<string> GetHostNames()
        {
            return _hostRegistry.GetHostNames();
        }

        /// <summary>
        ///     Navigates to the specified content in a host.
        /// </summary>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="content">The content to navigate to.</param>
        public void Navigate(string hostName, FrameworkElement content)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host name cannot be null or whitespace.", nameof(hostName));

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var host = GetHost(hostName);
            if (host == null)
                throw new InvalidOperationException($"No host registered with name '{hostName}'.");

            host.Navigate(content);
        }

        /// <summary>
        ///     Navigates to the specified content type with optional parameters in a host.
        /// </summary>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="contentType">The type of content to navigate to.</param>
        /// <param name="parameter">Optional parameter to pass to the view model or content.</param>
        public void Navigate(string hostName, Type contentType, object? parameter = null)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host name cannot be null or whitespace.", nameof(hostName));

            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            if (!typeof(FrameworkElement).IsAssignableFrom(contentType))
                throw new ArgumentException($"Content type must derive from FrameworkElement.", nameof(contentType));

            // Create the view instance
            var view = (FrameworkElement)_instanceFactory.CreateInstance(contentType);

            // Try to resolve and set ViewModel using convention
            TrySetViewModelByConvention(view, parameter);

            // Navigate to the view
            Navigate(hostName, view);
        }

        /// <summary>
        ///     Navigates to the specified content type in a host.
        /// </summary>
        /// <typeparam name="T">The type of content to navigate to.</typeparam>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="parameter">Optional parameter to pass to the view model or content.</param>
        public void Navigate<T>(string hostName, object? parameter = null) where T : FrameworkElement
        {
            Navigate(hostName, typeof(T), parameter);
        }

        /// <summary>
        ///     Gets the HostName attached property value.
        /// </summary>
        public static string? GetHostName(DependencyObject obj)
        {
            return (string?)obj.GetValue(HostNameProperty);
        }

        /// <summary>
        ///     Sets the HostName attached property value.
        /// </summary>
        public static void SetHostName(DependencyObject obj, string? value)
        {
            obj.SetValue(HostNameProperty, value);
        }

        /// <summary>
        ///     Gets the HostManager attached property value.
        /// </summary>
        public static HostManager? GetHostManager(DependencyObject obj)
        {
            return (HostManager?)obj.GetValue(HostManagerProperty);
        }

        /// <summary>
        ///     Sets the HostManager attached property value.
        /// </summary>
        public static void SetHostManager(DependencyObject obj, HostManager? value)
        {
            obj.SetValue(HostManagerProperty, value);
        }

        private static void OnHostNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NavigationHost host && e.NewValue is string hostName && !string.IsNullOrWhiteSpace(hostName))
            {
                var hostManager = GetHostManager(d);
                if (hostManager != null)
                {
                    hostManager.RegisterHost(hostName, host);
                }
                else
                {
                    // Store for later registration when HostManager is set
                    host.Loaded += (s, args) =>
                    {
                        var manager = GetHostManager(host);
                        if (manager != null)
                        {
                            manager.RegisterHost(hostName, host);
                        }
                    };
                }
            }
        }

        private static void OnHostManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NavigationHost host && e.NewValue is HostManager hostManager)
            {
                var hostName = GetHostName(host);
                if (!string.IsNullOrWhiteSpace(hostName))
                {
                    hostManager.RegisterHost(hostName, host);
                }
            }
        }

        private void TrySetViewModelByConvention(FrameworkElement view, object? parameter)
        {
            if (view.DataContext != null)
                return; // Already has a DataContext

            var viewType = view.GetType();
            var viewModelType = _conventionResolver.ResolveViewModelType(viewType);

            if (viewModelType == null)
                return; // No ViewModel found by convention

            try
            {
                var viewModel = _instanceFactory.CreateInstance(viewModelType);

                // If ViewModel implements INavigationAware, call navigation methods
                if (viewModel is INavigationAware navigationAware)
                {
                    if (navigationAware.CanNavigateTo(parameter))
                    {
                        navigationAware.OnNavigatedTo(parameter);
                    }
                    else
                    {
                        return; // Navigation cancelled
                    }
                }

                view.DataContext = viewModel;
            }
            catch
            {
                // If ViewModel creation fails, continue without it
            }
        }
    }
}

