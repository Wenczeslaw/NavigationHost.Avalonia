using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NavigationHost.Abstractions;
using NavigationHost.WPF.Services;

namespace NavigationHost.WPF.Extensions
{
    /// <summary>
    ///     Extension methods for configuring navigation services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class NavigationServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds navigation host services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddNavigationHost(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Register internal services
            services.TryAddSingleton<IHostRegistry, Services.Internal.HostRegistry>();
            services.TryAddSingleton<IViewModelConventionResolver, Services.Internal.ViewModelConventionResolver>();
            services.TryAddSingleton<Services.Internal.InstanceFactory>();

            // Register HostManager as singleton
            services.TryAddSingleton<IHostManager>(provider =>
            {
                var hostRegistry = provider.GetRequiredService<IHostRegistry>();
                var conventionResolver = provider.GetRequiredService<IViewModelConventionResolver>();
                var instanceFactory = provider.GetRequiredService<Services.Internal.InstanceFactory>();

                var hostManager = new HostManager(hostRegistry, conventionResolver, instanceFactory);

                // Set the static locator for non-DI scenarios
                HostManagerLocator.SetCurrent(hostManager);

                return hostManager;
            });

            // Also register the concrete type for scenarios where it's needed
            services.TryAddSingleton(provider => (HostManager)provider.GetRequiredService<IHostManager>());

            return services;
        }

        /// <summary>
        ///     Registers a view type in the service collection.
        ///     This allows the view to be resolved from the DI container.
        /// </summary>
        /// <typeparam name="TView">The view type to register.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddView<TView>(this IServiceCollection services)
            where TView : System.Windows.FrameworkElement
        {
            services.TryAddTransient<TView>();
            return services;
        }

        /// <summary>
        ///     Registers a view and its view model in the service collection.
        /// </summary>
        /// <typeparam name="TView">The view type to register.</typeparam>
        /// <typeparam name="TViewModel">The view model type to register.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddView<TView, TViewModel>(this IServiceCollection services)
            where TView : System.Windows.FrameworkElement
            where TViewModel : class
        {
            services.TryAddTransient<TView>();
            services.TryAddTransient<TViewModel>();
            return services;
        }
    }
}
