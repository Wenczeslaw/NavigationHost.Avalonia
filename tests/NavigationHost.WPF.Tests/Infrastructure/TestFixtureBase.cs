using System;
using Microsoft.Extensions.DependencyInjection;
using NavigationHost.WPF.Abstractions;
using NavigationHost.WPF.Extensions;

namespace NavigationHost.WPF.Tests.Infrastructure
{
    /// <summary>
    ///     Base class for test fixtures that provides common test infrastructure.
    /// </summary>
    public abstract class TestFixtureBase : IDisposable
    {
        protected IServiceProvider ServiceProvider { get; private set; }
        protected IHostManager HostManager { get; private set; }

        protected TestFixtureBase()
        {
            // Don't create Application in tests - will use STAFact attribute for WPF threading
            ServiceProvider = BuildServiceProvider();
            HostManager = ServiceProvider.GetRequiredService<IHostManager>();
        }

        /// <summary>
        ///     Builds and configures the service provider for tests.
        /// </summary>
        protected virtual IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            return services.BuildServiceProvider();
        }

        /// <summary>
        ///     Override this method to configure additional services for specific tests.
        /// </summary>
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddNavigationHost();
        }

        public virtual void Dispose()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
