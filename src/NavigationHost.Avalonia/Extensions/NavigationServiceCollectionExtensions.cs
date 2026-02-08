using System;
using Microsoft.Extensions.DependencyInjection;
using NavigationHost.Abstractions;
using NavigationHost.Avalonia.Services;
using NavigationHost.Avalonia.Services.Internal;

namespace NavigationHost.Avalonia.Extensions
{
    /// <summary>
    ///     Extension methods for registering navigation services in dependency injection container.
    /// </summary>
    public static class NavigationServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds HostManager as a singleton service to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddHostManager(this IServiceCollection services)
        {
            // Register internal services
            services.AddSingleton<IHostRegistry, HostRegistry>();
            
            // Register mapping registry with a factory that applies all pending mappings
            services.AddSingleton<IViewModelMappingRegistry>(provider =>
            {
                var registry = new ViewModelMappingRegistry();
                
                // Apply all registered mappings when the registry is created
                var mappingRegistrations = provider.GetServices<IViewModelMappingRegistration>();
                foreach (var registration in mappingRegistrations)
                {
                    registration.Register(registry);
                }
                
                return registry;
            });
            
            // Register convention resolver that uses the already-initialized registry
            services.AddSingleton<IViewModelConventionResolver>(provider =>
            {
                var mappingRegistry = provider.GetRequiredService<IViewModelMappingRegistry>();
                return new ViewModelConventionResolver(mappingRegistry);
            });
            
            services.AddSingleton<InstanceFactory>();
            services.AddSingleton<IInstanceFactory>(provider => provider.GetRequiredService<InstanceFactory>());

            services.AddSingleton<IHostManager, HostManager>(provider =>
                {
                    var hostRegistry = provider.GetRequiredService<IHostRegistry>();
                    var conventionResolver = provider.GetRequiredService<IViewModelConventionResolver>();
                    var instanceFactory = provider.GetRequiredService<InstanceFactory>();

                    var hostManager = new HostManager(hostRegistry, conventionResolver, instanceFactory);
                    
                    // Configure service provider for instance factory
                    if (instanceFactory is InstanceFactory concreteFactory)
                    {
                        concreteFactory.ConfigureServiceProvider(provider);
                    }

                    // ✅ 自动设置到定位器，供 NavigationHost 自动获取
                    HostManagerLocator.Current = hostManager;

                    return hostManager;
                }
            );

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
            where TView : class
        {
            services.AddTransient<TView>();
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
            where TView : class
            where TViewModel : class
        {
            services.AddTransient<TView>();
            services.AddTransient<TViewModel>();
            
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