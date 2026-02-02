using System;
using System.Collections.Generic;
using System.Windows;
using NavigationHost.Abstractions;
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
        public void RegisterHost(string hostName, INavigationHost host)
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
        public INavigationHost? GetHost(string hostName)
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
        ///     Checks if a host with the specified name exists.
        /// </summary>
        /// <param name="hostName">The name of the host to check.</param>
        /// <returns>True if the host exists; otherwise, false.</returns>
        public bool HostExists(string hostName)
        {
            return GetHost(hostName) != null;
        }

        /// <summary>
        ///     Navigates to the specified content in a host.
        /// </summary>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="content">The content to navigate to.</param>
        public void Navigate(string hostName, object content)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host name cannot be null or whitespace.", nameof(hostName));

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            if (!(content is FrameworkElement frameworkElement))
                throw new ArgumentException("Content must be a FrameworkElement for WPF platform.", nameof(content));

            var host = GetHost(hostName);
            if (host == null)
                throw new InvalidOperationException($"No host registered with name '{hostName}'.");

            // Get current content to check navigation lifecycle
            var currentContent = (host as NavigationHost)?.CurrentContent;

            // Navigation lifecycle: Check if current ViewModel allows navigation away (sync only)
            if (currentContent?.DataContext != null && currentContent.DataContext is INavigationAware currentNavigationAware)
            {
                if (!currentNavigationAware.CanNavigateFrom())
                    return; // Navigation cancelled by current ViewModel

                currentNavigationAware.OnNavigatedFrom();
            }

            host.SetContent(frameworkElement);
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
            var view = _instanceFactory.CreateInstance(contentType);

            // Try to resolve and set ViewModel using convention
            if (view is FrameworkElement frameworkElement)
            {
                TrySetViewModelByConvention(frameworkElement, parameter);
            }

            // Navigate to the view
            Navigate(hostName, view);
        }

        /// <summary>
        ///     Navigates to the specified content type in a host.
        /// </summary>
        /// <typeparam name="T">The type of content to navigate to.</typeparam>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="parameter">Optional parameter to pass to the view model or content.</param>
        public void Navigate<T>(string hostName, object? parameter = null)
        {
            Navigate(hostName, typeof(T), parameter);
        }

        /// <summary>
        ///     Asynchronously navigates to the specified content in a host.
        /// </summary>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="content">The content to navigate to.</param>
        /// <returns>A task representing the asynchronous navigation operation.</returns>
        public async System.Threading.Tasks.Task NavigateAsync(string hostName, object content)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host name cannot be null or whitespace.", nameof(hostName));

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            if (!(content is FrameworkElement frameworkElement))
                throw new ArgumentException("Content must be a FrameworkElement for WPF platform.", nameof(content));

            var host = GetHost(hostName);
            if (host == null)
                throw new InvalidOperationException($"No host registered with name '{hostName}'.");

            // Get current content to check navigation lifecycle
            var currentContent = (host as NavigationHost)?.CurrentContent;

            // Navigation lifecycle: Check if current ViewModel allows navigation away
            if (currentContent?.DataContext != null)
            {
                // Check async interface first
                if (currentContent.DataContext is IAsyncNavigationAware currentAsyncNavigationAware)
                {
                    if (!await currentAsyncNavigationAware.CanNavigateFromAsync())
                        return; // Navigation cancelled by current ViewModel

                    await currentAsyncNavigationAware.OnNavigatedFromAsync();
                }
                else if (currentContent.DataContext is INavigationAware currentNavigationAware)
                {
                    if (!currentNavigationAware.CanNavigateFrom())
                        return; // Navigation cancelled by current ViewModel

                    currentNavigationAware.OnNavigatedFrom();
                }
            }

            host.SetContent(frameworkElement);
        }

        /// <summary>
        ///     Asynchronously navigates to the specified content type with optional parameters in a host.
        /// </summary>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="contentType">The type of content to navigate to.</param>
        /// <param name="parameter">Optional parameter to pass to the view model or content.</param>
        /// <returns>A task representing the asynchronous navigation operation.</returns>
        public async System.Threading.Tasks.Task NavigateAsync(string hostName, Type contentType, object? parameter = null)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host name cannot be null or whitespace.", nameof(hostName));

            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            if (!typeof(FrameworkElement).IsAssignableFrom(contentType))
                throw new ArgumentException($"Content type must derive from FrameworkElement.", nameof(contentType));

            // Create the view instance
            var view = _instanceFactory.CreateInstance(contentType);

            // Try to resolve and set ViewModel using convention
            if (view is FrameworkElement frameworkElement)
            {
                var shouldNavigate = await TrySetViewModelByConventionAsync(frameworkElement, parameter);
                if (!shouldNavigate)
                    return; // Navigation was cancelled
            }

            // Navigate to the view
            await NavigateAsync(hostName, view);
        }

        /// <summary>
        ///     Asynchronously navigates to the specified content type in a host.
        /// </summary>
        /// <typeparam name="T">The type of content to navigate to.</typeparam>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="parameter">Optional parameter to pass to the view model or content.</param>
        /// <returns>A task representing the asynchronous navigation operation.</returns>
        public System.Threading.Tasks.Task NavigateAsync<T>(string hostName, object? parameter = null)
        {
            return NavigateAsync(hostName, typeof(T), parameter);
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
                // First try to get HostManager from attached property
                var hostManager = GetHostManager(d);
                
                if (hostManager != null)
                {
                    hostManager.RegisterHost(hostName, host);
                }
                else
                {
                    // If not set via attached property, try to get from HostManagerLocator
                    // Register on Loaded event to ensure the host is fully initialized
                    host.Loaded += (s, args) =>
                    {
                        var manager = GetHostManager(host);
                        if (manager == null)
                        {
                            // Try to get from HostManagerLocator as fallback
                            try
                            {
                                var locatorManager = HostManagerLocator.Current;
                                if (locatorManager is HostManager platformManager)
                                {
                                    platformManager.RegisterHost(hostName, host);
                                }
                            }
                            catch
                            {
                                // HostManagerLocator.Current may throw if not initialized
                                // This is expected in some scenarios, so we silently ignore
                            }
                        }
                        else
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
            // Check if view already has a DataContext (e.g., set in XAML or constructor)
            if (view.DataContext != null)
            {
                // Even if DataContext exists, call navigation lifecycle methods for re-navigation
                if (view.DataContext is INavigationAware existingNavigationAware)
                {
                    if (existingNavigationAware.CanNavigateTo(parameter))
                    {
                        existingNavigationAware.OnNavigatedTo(parameter);
                    }
                }
                return; // Keep existing DataContext
            }

            var viewType = view.GetType();
            var viewModelType = _conventionResolver.ResolveViewModelType(viewType);

            if (viewModelType == null)
                return; // No ViewModel found by convention

            try
            {
                var viewModel = _instanceFactory.CreateViewModel(viewModelType);

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

        private async System.Threading.Tasks.Task<bool> TrySetViewModelByConventionAsync(FrameworkElement view, object? parameter)
        {
            // Check if view already has a DataContext (e.g., set in XAML or constructor)
            if (view.DataContext != null)
            {
                // Even if DataContext exists, call navigation lifecycle methods for re-navigation
                if (view.DataContext is IAsyncNavigationAware existingAsyncNavigationAware)
                {
                    if (!await existingAsyncNavigationAware.CanNavigateToAsync(parameter))
                        return false; // Navigation cancelled
                    
                    await existingAsyncNavigationAware.OnNavigatedToAsync(parameter);
                }
                else if (view.DataContext is INavigationAware existingNavigationAware)
                {
                    if (!existingNavigationAware.CanNavigateTo(parameter))
                        return false; // Navigation cancelled
                    
                    existingNavigationAware.OnNavigatedTo(parameter);
                }
                return true; // Keep existing DataContext, continue with navigation
            }

            var viewType = view.GetType();
            var viewModelType = _conventionResolver.ResolveViewModelType(viewType);

            if (viewModelType == null)
                return true; // No ViewModel found by convention, continue with navigation

            var viewModel = _instanceFactory.CreateViewModel(viewModelType);

            // If ViewModel implements IAsyncNavigationAware or INavigationAware, call navigation methods
            if (viewModel is IAsyncNavigationAware asyncNavigationAware)
            {
                if (!await asyncNavigationAware.CanNavigateToAsync(parameter))
                {
                    return false; // Navigation cancelled
                }

                await asyncNavigationAware.OnNavigatedToAsync(parameter);
            }
            else if (viewModel is INavigationAware navigationAware)
            {
                if (!navigationAware.CanNavigateTo(parameter))
                {
                    return false; // Navigation cancelled
                }

                navigationAware.OnNavigatedTo(parameter);
            }

            view.DataContext = viewModel;
            return true; // Navigation should proceed
        }
    }
}

