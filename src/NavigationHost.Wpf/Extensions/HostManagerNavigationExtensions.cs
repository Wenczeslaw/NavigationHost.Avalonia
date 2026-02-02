using System;
using System.Threading.Tasks;
using System.Windows;
using NavigationHost.Abstractions;

namespace NavigationHost.WPF.Extensions
{
    /// <summary>
    ///     Provides extension methods for safe navigation, similar to Prism's RequestNavigate.
    /// </summary>
    public static class HostManagerNavigationExtensions
    {
        /// <summary>
        ///     Attempts to navigate to the specified content type, with optional retry if host is not ready.
        ///     Similar to Prism's RequestNavigate pattern.
        /// </summary>
        /// <param name="hostManager">The host manager instance.</param>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="contentType">The type of content to navigate to.</param>
        /// <param name="parameter">Optional parameter to pass to the view model.</param>
        /// <param name="onComplete">Optional callback invoked when navigation completes (success or failure).</param>
        /// <param name="retryOnHostNotReady">If true, retries navigation after a short delay when host is not registered.</param>
        public static void RequestNavigate(
            this IHostManager hostManager,
            string hostName,
            Type contentType,
            object? parameter = null,
            Action<NavigationResult>? onComplete = null,
            bool retryOnHostNotReady = true)
        {
            if (hostManager == null)
                throw new ArgumentNullException(nameof(hostManager));

            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host name cannot be null or whitespace.", nameof(hostName));

            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            // Check if host exists
            if (!hostManager.HostExists(hostName))
            {
                if (retryOnHostNotReady)
                {
                    // Retry after ensuring UI is loaded
                    Application.Current.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            if (hostManager.HostExists(hostName))
                            {
                                PerformNavigation(hostManager, hostName, contentType, parameter, onComplete);
                            }
                            else
                            {
                                // Final attempt after another delay
                                Application.Current.Dispatcher.BeginInvoke(
                                    new Action(() => PerformNavigation(hostManager, hostName, contentType, parameter, onComplete)),
                                    System.Windows.Threading.DispatcherPriority.Loaded
                                );
                            }
                        }),
                        System.Windows.Threading.DispatcherPriority.Loaded
                    );
                }
                else
                {
                    onComplete?.Invoke(new NavigationResult
                    {
                        Success = false,
                        Error = new InvalidOperationException($"No host registered with name '{hostName}'.")
                    });
                }
                return;
            }

            // Host exists, navigate immediately
            PerformNavigation(hostManager, hostName, contentType, parameter, onComplete);
        }

        /// <summary>
        ///     Attempts to navigate to the specified content type, with optional retry if host is not ready.
        /// </summary>
        /// <typeparam name="T">The type of content to navigate to.</typeparam>
        /// <param name="hostManager">The host manager instance.</param>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="parameter">Optional parameter to pass to the view model.</param>
        /// <param name="onComplete">Optional callback invoked when navigation completes.</param>
        /// <param name="retryOnHostNotReady">If true, retries navigation after a short delay when host is not registered.</param>
        public static void RequestNavigate<T>(
            this IHostManager hostManager,
            string hostName,
            object? parameter = null,
            Action<NavigationResult>? onComplete = null,
            bool retryOnHostNotReady = true)
        {
            RequestNavigate(hostManager, hostName, typeof(T), parameter, onComplete, retryOnHostNotReady);
        }

        /// <summary>
        ///     Asynchronously attempts to navigate to the specified content type.
        /// </summary>
        /// <param name="hostManager">The host manager instance.</param>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="contentType">The type of content to navigate to.</param>
        /// <param name="parameter">Optional parameter to pass to the view model.</param>
        /// <param name="retryOnHostNotReady">If true, retries navigation after a short delay when host is not registered.</param>
        /// <returns>A task that represents the navigation result.</returns>
        public static async Task<NavigationResult> RequestNavigateAsync(
            this IHostManager hostManager,
            string hostName,
            Type contentType,
            object? parameter = null,
            bool retryOnHostNotReady = true)
        {
            if (hostManager == null)
                throw new ArgumentNullException(nameof(hostManager));

            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host name cannot be null or whitespace.", nameof(hostName));

            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            // Check if host exists
            if (!hostManager.HostExists(hostName))
            {
                if (retryOnHostNotReady)
                {
                    // Wait for UI thread and retry
                    await Application.Current.Dispatcher.InvokeAsync(
                        async () => { },
                        System.Windows.Threading.DispatcherPriority.Loaded
                    );

                    // Check again after waiting
                    if (!hostManager.HostExists(hostName))
                    {
                        return new NavigationResult
                        {
                            Success = false,
                            Error = new InvalidOperationException($"No host registered with name '{hostName}' after waiting for host initialization.")
                        };
                    }
                }
                else
                {
                    return new NavigationResult
                    {
                        Success = false,
                        Error = new InvalidOperationException($"No host registered with name '{hostName}'.")
                    };
                }
            }

            return await PerformNavigationAsync(hostManager, hostName, contentType, parameter);
        }

        /// <summary>
        ///     Asynchronously attempts to navigate to the specified content type.
        /// </summary>
        /// <typeparam name="T">The type of content to navigate to.</typeparam>
        /// <param name="hostManager">The host manager instance.</param>
        /// <param name="hostName">The name of the host to navigate in.</param>
        /// <param name="parameter">Optional parameter to pass to the view model.</param>
        /// <param name="retryOnHostNotReady">If true, retries navigation after a short delay when host is not registered.</param>
        /// <returns>A task that represents the navigation result.</returns>
        public static Task<NavigationResult> RequestNavigateAsync<T>(
            this IHostManager hostManager,
            string hostName,
            object? parameter = null,
            bool retryOnHostNotReady = true)
        {
            return RequestNavigateAsync(hostManager, hostName, typeof(T), parameter, retryOnHostNotReady);
        }

        private static void PerformNavigation(
            IHostManager hostManager,
            string hostName,
            Type contentType,
            object? parameter,
            Action<NavigationResult>? onComplete)
        {
            try
            {
                hostManager.Navigate(hostName, contentType, parameter);
                onComplete?.Invoke(new NavigationResult { Success = true });
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(new NavigationResult
                {
                    Success = false,
                    Error = ex
                });
            }
        }

        private static async Task<NavigationResult> PerformNavigationAsync(
            IHostManager hostManager,
            string hostName,
            Type contentType,
            object? parameter)
        {
            try
            {
                await hostManager.NavigateAsync(hostName, contentType, parameter);
                return new NavigationResult { Success = true };
            }
            catch (Exception ex)
            {
                return new NavigationResult
                {
                    Success = false,
                    Error = ex
                };
            }
        }
    }

    /// <summary>
    ///     Represents the result of a navigation operation.
    /// </summary>
    public class NavigationResult
    {
        /// <summary>
        ///     Gets or sets whether the navigation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Gets or sets the error that occurred during navigation, if any.
        /// </summary>
        public Exception? Error { get; set; }
    }
}
