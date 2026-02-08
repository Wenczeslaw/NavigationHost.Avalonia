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
            
            // Register mapping registry with a factory that applies all pending mappings
            services.TryAddSingleton<IViewModelMappingRegistry>(provider =>
            {
                var registry = new Services.Internal.ViewModelMappingRegistry();
                
                // Apply all registered mappings when the registry is created
                var mappingRegistrations = provider.GetServices<IViewModelMappingRegistration>();
                foreach (var registration in mappingRegistrations)
                {
                    registration.Register(registry);
                }
                
                return registry;
            });
            
            // Register convention resolver that uses the already-initialized registry
            services.TryAddSingleton<IViewModelConventionResolver>(provider =>
            {
                var mappingRegistry = provider.GetRequiredService<IViewModelMappingRegistry>();
                return new Services.Internal.ViewModelConventionResolver(mappingRegistry);
            });
            
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
        ///     Automatically creates an explicit View-ViewModel mapping.
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
            
            // Automatically register the View-ViewModel mapping
            services.AddSingleton<IViewModelMappingRegistration>(
                new ViewModelMappingRegistration(typeof(TView), typeof(TViewModel)));
            
            return services;
        }
    }

    /// <summary>
    ///     Interface to mark services that need to register mappings.
    /// </summary>
    internal interface IViewModelMappingRegistration
    {
        void Register(IViewModelMappingRegistry registry);
    }

    /// <summary>
    ///     Internal class to hold mapping configuration that will be applied when accessed.
    /// </summary>
    internal class ViewModelMappingRegistration : IViewModelMappingRegistration
    {
        private readonly Type _viewType;
        private readonly Type _viewModelType;

        public ViewModelMappingRegistration(Type viewType, Type viewModelType)
        {
            _viewType = viewType;
            _viewModelType = viewModelType;
        }

        public void Register(IViewModelMappingRegistry registry)
        {
            registry.RegisterMapping(_viewType, _viewModelType);
        }
    }
}
