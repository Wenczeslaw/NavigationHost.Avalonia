using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NavigationHost.Abstractions;
using NavigationHost.Avalonia.Extensions;
using NavigationHost.Avalonia.Services;

namespace NavigationHost.Avalonia.Tests.Infrastructure
{
    /// <summary>
    /// Base class for test fixtures providing common setup and utilities.
    /// </summary>
    public abstract class TestFixtureBase : IDisposable
    {
        protected IServiceProvider ServiceProvider { get; private set; } = null!;
        protected IHostManager HostManager { get; private set; } = null!;
        
        private bool _disposed;

        protected TestFixtureBase()
        {
            SetupServices();
        }

        /// <summary>
        /// Sets up the DI container with navigation services.
        /// Override to customize service registration.
        /// </summary>
        protected virtual void SetupServices()
        {
            // Reset locator for test isolation
            HostManagerLocator.Reset();
            
            var services = new ServiceCollection();
            
            // Register navigation services
            services.AddHostManager();
            
            // Register common test types
            RegisterCommonTestTypes(services);
            
            // Allow derived classes to register additional services
            ConfigureServices(services);
            
            ServiceProvider = services.BuildServiceProvider();
            HostManager = ServiceProvider.GetRequiredService<IHostManager>();
        }

        /// <summary>
        /// Override to register additional services.
        /// </summary>
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Override in derived classes
        }

        /// <summary>
        /// Registers common test view and viewmodel types.
        /// </summary>
        private void RegisterCommonTestTypes(IServiceCollection services)
        {
            services.AddTransient<TestView>();
            services.AddTransient<TestViewModel>();
            services.AddTransient<NavigationAwareTestViewModel>();
            services.AddTransient<HomeView>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<SettingsView>();
            services.AddTransient<SettingsViewModel>();
        }

        /// <summary>
        /// Creates a new NavigationHost instance.
        /// </summary>
        protected NavigationHost CreateNavigationHost()
        {
            return new NavigationHost();
        }

        /// <summary>
        /// Registers and returns a new NavigationHost with the given name.
        /// </summary>
        protected NavigationHost CreateAndRegisterHost(string hostName)
        {
            var host = CreateNavigationHost();
            HostManager.RegisterHost(hostName, host);
            return host;
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                (ServiceProvider as IDisposable)?.Dispose();
                HostManagerLocator.Reset();
            }

            _disposed = true;
        }
    }

    #region Test Helper Classes

    /// <summary>
    /// Simple test view for testing purposes.
    /// </summary>
    public class TestView : Control { }

    /// <summary>
    /// Simple test viewmodel for testing purposes.
    /// </summary>
    public class TestViewModel
    {
        public string Name { get; set; } = "TestViewModel";
    }

    /// <summary>
    /// NavigationAware test viewmodel for lifecycle testing.
    /// </summary>
    public class NavigationAwareTestViewModel : INavigationAware
    {
        public bool CanNavigateToResult { get; set; } = true;
        public bool CanNavigateFromResult { get; set; } = true;
        public object? ReceivedParameter { get; private set; }
        public int OnNavigatedToCallCount { get; private set; }
        public int OnNavigatedFromCallCount { get; private set; }
        public int CanNavigateToCallCount { get; private set; }
        public int CanNavigateFromCallCount { get; private set; }
        
        public List<object?> NavigatedToParameters { get; } = new List<object?>();
        public List<string> LifecycleEvents { get; } = new List<string>();

        public bool CanNavigateTo(object? parameter)
        {
            CanNavigateToCallCount++;
            ReceivedParameter = parameter;
            LifecycleEvents.Add($"CanNavigateTo({parameter})");
            return CanNavigateToResult;
        }

        public void OnNavigatedTo(object? parameter)
        {
            OnNavigatedToCallCount++;
            ReceivedParameter = parameter;
            NavigatedToParameters.Add(parameter);
            LifecycleEvents.Add($"OnNavigatedTo({parameter})");
        }

        public bool CanNavigateFrom()
        {
            CanNavigateFromCallCount++;
            LifecycleEvents.Add("CanNavigateFrom()");
            return CanNavigateFromResult;
        }

        public void OnNavigatedFrom()
        {
            OnNavigatedFromCallCount++;
            LifecycleEvents.Add("OnNavigatedFrom()");
        }

        public void Reset()
        {
            OnNavigatedToCallCount = 0;
            OnNavigatedFromCallCount = 0;
            CanNavigateToCallCount = 0;
            CanNavigateFromCallCount = 0;
            ReceivedParameter = null;
            NavigatedToParameters.Clear();
            LifecycleEvents.Clear();
        }
    }

    /// <summary>
    /// Convention-based test view: HomeView
    /// </summary>
    public class HomeView : Control { }

    /// <summary>
    /// Convention-based test viewmodel: HomeViewModel
    /// </summary>
    public class HomeViewModel
    {
        public string Title { get; set; } = "Home";
    }

    /// <summary>
    /// Convention-based test view: SettingsView
    /// </summary>
    public class SettingsView : Control { }

    /// <summary>
    /// Convention-based test viewmodel: SettingsViewModel
    /// </summary>
    public class SettingsViewModel
    {
        public string Title { get; set; } = "Settings";
    }

    #endregion
}
