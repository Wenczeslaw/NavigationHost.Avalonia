using System;
using Avalonia.Controls;

namespace NavigationHost.Avalonia.Services.Internal
{
    /// <summary>
    /// Internal implementation of instance factory.
    /// Uses DI container (IServiceProvider) to resolve views and viewmodels.
    /// </summary>
    internal sealed class InstanceFactory
    {
        private IServiceProvider? _serviceProvider;

        /// <summary>
        /// Configures the service provider for dependency injection.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use for resolving instances.</param>
        /// <exception cref="ArgumentNullException">Thrown when serviceProvider is null.</exception>
        public void ConfigureServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Creates a view instance from the DI container.
        /// </summary>
        /// <param name="viewType">The type of view to create.</param>
        /// <returns>A view instance resolved from the DI container.</returns>
        /// <exception cref="InvalidOperationException">Thrown when service provider is not configured.</exception>
        /// <exception cref="InvalidOperationException">Thrown when view cannot be resolved from DI container.</exception>
        public Control CreateView(Type viewType)
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException(
                    "Service provider is not configured. Call ConfigureServiceProvider before creating views.");

            var view = _serviceProvider.GetService(viewType) as Control;
            if (view == null)
                throw new InvalidOperationException(
                    $"Unable to resolve view of type '{viewType.FullName}' from the DI container. " +
                    "Ensure the view is registered in the service collection.");

            return view;
        }

        /// <summary>
        /// Creates a view model instance from the DI container.
        /// </summary>
        /// <param name="viewModelType">The type of view model to create.</param>
        /// <returns>A view model instance resolved from the DI container.</returns>
        /// <exception cref="InvalidOperationException">Thrown when service provider is not configured.</exception>
        /// <exception cref="InvalidOperationException">Thrown when view model cannot be resolved from DI container.</exception>
        public object CreateViewModel(Type viewModelType)
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException(
                    "Service provider is not configured. Call ConfigureServiceProvider before creating view models.");

            var viewModel = _serviceProvider.GetService(viewModelType);
            if (viewModel == null)
                throw new InvalidOperationException(
                    $"Unable to resolve view model of type '{viewModelType.FullName}' from the DI container. " +
                    "Ensure the view model is registered in the service collection.");

            return viewModel;
        }
    }
}

