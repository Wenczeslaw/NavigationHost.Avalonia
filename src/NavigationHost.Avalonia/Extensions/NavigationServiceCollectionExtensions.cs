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
            services.AddSingleton<IViewModelConventionResolver, ViewModelConventionResolver>();
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
    }
}