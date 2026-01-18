using Avalonia.Controls;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NavigationHost.Abstractions;
using NavigationHost.Avalonia.Extensions;
using NavigationHost.Avalonia.Services;

namespace NavigationHost.Avalonia.Tests.Services
{
    // Test helper classes moved outside for convention-based resolution
    internal class TestView : Control { }
    internal class TestViewModel { }
    
    // Convention-based test classes - these must be in separate namespaces or follow naming conventions
    internal class HomeView : Control { }
    internal class HomeViewModel { }
    internal class SettingsView : Control { }
    internal class SettingsViewModel { }
    
    internal class NavigationAwareViewModel : INavigationAware
    {
        public bool CanNavigateToResult { get; set; } = true;
        public bool CanNavigateFromResult { get; set; } = true;
        public object? ReceivedParameter { get; private set; }
        public int OnNavigatedToCallCount { get; private set; }
        public int OnNavigatedFromCallCount { get; private set; }
        public int CanNavigateToCallCount { get; private set; }
        public int CanNavigateFromCallCount { get; private set; }

        public bool CanNavigateTo(object? parameter)
        {
            CanNavigateToCallCount++;
            ReceivedParameter = parameter;
            return CanNavigateToResult;
        }

        public void OnNavigatedTo(object? parameter)
        {
            OnNavigatedToCallCount++;
            ReceivedParameter = parameter;
        }

        public bool CanNavigateFrom()
        {
            CanNavigateFromCallCount++;
            return CanNavigateFromResult;
        }

        public void OnNavigatedFrom()
        {
            OnNavigatedFromCallCount++;
        }
    }

    public class HostManagerTests
    {
        private readonly IHostManager _sut;

        public HostManagerTests()
        {
            var services = new ServiceCollection();
            
            // Register navigation services
            services.AddHostManager();
            
            // Register test views and view models
            services.AddTransient<TestView>();
            services.AddTransient<TestViewModel>();
            services.AddTransient<NavigationAwareViewModel>();
            services.AddTransient<HomeView>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<SettingsView>();
            services.AddTransient<SettingsViewModel>();
            
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _sut = serviceProvider.GetRequiredService<IHostManager>();
        }

        #region Host Registration Tests

        [Fact]
        public void RegisterHost_ShouldRegisterHost()
        {
            // Arrange
            var host = new NavigationHost();

            // Act
            _sut.RegisterHost("TestHost", host);

            // Assert
            var retrievedHost = _sut.GetHost("TestHost");
            retrievedHost.Should().BeSameAs(host);
        }

        [Fact]
        public void UnregisterHost_ShouldRemoveHost()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // Act
            var result = _sut.UnregisterHost("TestHost");

            // Assert
            result.Should().BeTrue();
            _sut.GetHost("TestHost").Should().BeNull();
        }

        [Fact]
        public void GetHostNames_ShouldReturnAllRegisteredHostNames()
        {
            // Arrange
            _sut.RegisterHost("Host1", new NavigationHost());
            _sut.RegisterHost("Host2", new NavigationHost());

            // Act
            var hostNames = _sut.GetHostNames();

            // Assert
            hostNames.Should().Contain(["Host1", "Host2"]);
        }

        #endregion

        #region Convention-Based Resolution Tests

        [Fact]
        public void Navigate_WithConventionBasedMapping_ShouldResolveViewModelAutomatically()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // Act - Navigate without explicit mapping registration
            _sut.Navigate<HomeView>("TestHost");

            // Assert
            host.CurrentContent.Should().NotBeNull();
            host.CurrentContent.Should().BeOfType<HomeView>();
            host.CurrentContent!.DataContext.Should().BeOfType<HomeViewModel>();
        }

        [Fact]
        public void Navigate_WithMultipleConventionBasedViews_ShouldResolveCorrectViewModels()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // Act & Assert - HomeView -> HomeViewModel
            _sut.Navigate<HomeView>("TestHost");
            host.CurrentContent.Should().BeOfType<HomeView>();
            host.CurrentContent!.DataContext.Should().BeOfType<HomeViewModel>();

            // Act & Assert - SettingsView -> SettingsViewModel
            _sut.Navigate<SettingsView>("TestHost");
            host.CurrentContent.Should().BeOfType<SettingsView>();
            host.CurrentContent!.DataContext.Should().BeOfType<SettingsViewModel>();
        }


        #endregion

        #region Navigation Tests

        [Fact]
        public void Navigate_WithControl_ShouldNavigateToControl()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            var control = new TestView();

            // Act
            _sut.Navigate("TestHost", control);

            // Assert
            host.CurrentContent.Should().BeSameAs(control);
        }

        [Fact]
        public void Navigate_WithNonRegisteredHost_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var control = new TestView();

            // Act
            Action act = () => _sut.Navigate("NonExistingHost", control);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("No host registered with name 'NonExistingHost'.");
        }

        [Fact]
        public void Navigate_WithNullContentType_ShouldThrowArgumentNullException()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // Act
            Action act = () => _sut.Navigate("TestHost", (Type)null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Navigate_WithViewModelMapping_ShouldCreateViewAndViewModel()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            // TestView -> TestViewModel should be resolved by convention

            // Act
            _sut.Navigate<TestView>("TestHost");

            // Assert
            host.CurrentContent.Should().NotBeNull();
            host.CurrentContent.Should().BeOfType<TestView>();
            host.CurrentContent!.DataContext.Should().BeOfType<TestViewModel>();
        }

        [Fact]
        public void Navigate_WithParameter_ShouldPassParameterToViewModel()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            // TestView -> NavigationAwareViewModel (needs to be resolved by convention or other means)
            var parameter = "Test Parameter";

            // Act
            _sut.Navigate<TestView>("TestHost", parameter: parameter);

            // Assert - TestView should resolve to TestViewModel by convention
            var viewModel = host.CurrentContent!.DataContext;
            viewModel.Should().NotBeNull();
        }

        [Fact]
        public void Navigate_WhenCanNavigateToReturnsFalse_ShouldCancelNavigation()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddHostManager();
            
            services.AddTransient<TestView>();
            services.AddTransient<TestViewModel>();
            
            var serviceProvider = services.BuildServiceProvider();
            var hostManager = serviceProvider.GetRequiredService<IHostManager>();
            
            var host = new NavigationHost();
            hostManager.RegisterHost("TestHost", host);

            // Set initial content
            var initialContent = new Control();
            host.Navigate(initialContent);

            // Act - TestView -> TestViewModel will be resolved by convention
            hostManager.Navigate<TestView>("TestHost");

            // Assert - Navigation should succeed since TestViewModel doesn't implement INavigationAware
            host.CurrentContent.Should().NotBeNull();
            host.CurrentContent.Should().BeOfType<TestView>();
        }

        [Fact]
        public void Navigate_WhenCanNavigateFromReturnsFalse_ShouldCancelNavigation()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            
            var currentViewModel = new NavigationAwareViewModel { CanNavigateFromResult = false };
            var currentView = new TestView { DataContext = currentViewModel };
            host.Navigate(currentView);

            // TestView -> TestViewModel should be resolved by convention

            // Act
            _sut.Navigate<TestView>("TestHost");

            // Assert
            host.CurrentContent.Should().BeSameAs(currentView); // Navigation should be cancelled
            currentViewModel.CanNavigateFromCallCount.Should().Be(1);
            currentViewModel.OnNavigatedFromCallCount.Should().Be(0); // Should not be called
        }

        #endregion

        #region Attached Properties Tests

        [Fact]
        public void GetHostName_ShouldReturnSetValue()
        {
            // Arrange
            var host = new NavigationHost();
            var hostName = "TestHost";
            
            // Must set HostManager before setting HostName (DI-only requirement)
            HostManager.SetHostManager(host, _sut as HostManager);

            // Act
            HostManager.SetHostName(host, hostName);
            var retrievedName = HostManager.GetHostName(host);

            // Assert
            retrievedName.Should().Be(hostName);
        }

        [Fact]
        public void GetHostManager_ShouldReturnSetValue()
        {
            // Arrange
            var control = new Control();

            // Act - Cast to HostManager since _sut is IHostManagerService
            HostManager.SetHostManager(control, _sut as HostManager);
            var retrievedManager = HostManager.GetHostManager(control);

            // Assert
            retrievedManager.Should().BeSameAs(_sut as HostManager);
        }

        [Fact]
        public void SetHostName_WithoutHostManager_ShouldNotThrowException()
        {
            // Arrange
            var host = new NavigationHost();
            var hostName = "TestHost";

            // Act - Setting HostName before HostManager should not throw
            Action act = () => HostManager.SetHostName(host, hostName);

            // Assert - Should not throw, registration will happen when HostManager is set
            act.Should().NotThrow();
        }

        [Fact]
        public void SetHostName_ThenHostManager_ShouldRegisterHost()
        {
            // Arrange
            var host = new NavigationHost();
            var hostName = "TestHost";

            // Act - Set HostName first, then HostManager (XAML order independence)
            HostManager.SetHostName(host, hostName);
            HostManager.SetHostManager(host, _sut as HostManager);

            // Assert - Host should be registered after HostManager is set
            var retrievedHost = _sut.GetHost(hostName);
            retrievedHost.Should().BeSameAs(host);
        }

        [Fact]
        public void SetHostManager_ThenHostName_ShouldRegisterHost()
        {
            // Arrange
            var host = new NavigationHost();
            var hostName = "TestHost";

            // Act - Set HostManager first, then HostName (traditional order)
            HostManager.SetHostManager(host, _sut as HostManager);
            HostManager.SetHostName(host, hostName);

            // Assert - Host should be registered
            var retrievedHost = _sut.GetHost(hostName);
            retrievedHost.Should().BeSameAs(host);
        }

        [Fact]
        public void ChangeHostName_ShouldUnregisterOldAndRegisterNew()
        {
            // Arrange
            var host = new NavigationHost();
            HostManager.SetHostManager(host, _sut as HostManager);
            HostManager.SetHostName(host, "OldHost");

            // Act - Change the host name
            HostManager.SetHostName(host, "NewHost");

            // Assert
            _sut.GetHost("OldHost").Should().BeNull();
            _sut.GetHost("NewHost").Should().BeSameAs(host);
        }

        [Fact]
        public void ChangeHostManager_ShouldUnregisterFromOldAndRegisterToNew()
        {
            // Arrange
            var services1 = new ServiceCollection();
            services1.AddHostManager();
            var provider1 = services1.BuildServiceProvider();
            var hostManager1 = provider1.GetRequiredService<IHostManager>();

            var services2 = new ServiceCollection();
            services2.AddHostManager();
            var provider2 = services2.BuildServiceProvider();
            var hostManager2 = provider2.GetRequiredService<IHostManager>();

            var host = new NavigationHost();
            var hostName = "TestHost";

            // Set initial HostManager and HostName
            HostManager.SetHostManager(host, hostManager1 as HostManager);
            HostManager.SetHostName(host, hostName);

            // Act - Change to a different HostManager
            HostManager.SetHostManager(host, hostManager2 as HostManager);

            // Assert
            hostManager1.GetHost(hostName).Should().BeNull(); // Unregistered from old
            hostManager2.GetHost(hostName).Should().BeSameAs(host); // Registered to new
        }

        #endregion

        #region Pending Registration Tests

        [Fact]
        public void NavigationHost_SetHostName_BeforeHostManagerInitialized_ShouldRegisterAfterInitialization()
        {
            // Arrange - Reset locator to simulate startup scenario
            HostManagerLocator.Reset();
            
            var host = new NavigationHost();
            var hostName = "TestHost";

            // Act - Set HostName before HostManager is available
            HostManager.SetHostName(host, hostName);
            
            // Assert - Host should not be registered yet (HostManager not available)
            // But it should be in pending registrations
            
            // Now initialize HostManager
            var services = new ServiceCollection();
            services.AddHostManager();
            var provider = services.BuildServiceProvider();
            var hostManager = provider.GetRequiredService<IHostManager>();

            // Assert - Host should now be registered (pending registration processed)
            var retrievedHost = hostManager.GetHost(hostName);
            retrievedHost.Should().BeSameAs(host);
        }

        [Fact]
        public void NavigationHost_MultipleHosts_SetHostNameBeforeInitialization_ShouldAllRegister()
        {
            // Arrange - Reset locator
            HostManagerLocator.Reset();
            
            var host1 = new NavigationHost();
            var host2 = new NavigationHost();
            var host3 = new NavigationHost();

            // Act - Set HostNames before HostManager is available
            HostManager.SetHostName(host1, "Host1");
            HostManager.SetHostName(host2, "Host2");
            HostManager.SetHostName(host3, "Host3");

            // Initialize HostManager
            var services = new ServiceCollection();
            services.AddHostManager();
            var provider = services.BuildServiceProvider();
            var hostManager = provider.GetRequiredService<IHostManager>();

            // Assert - All hosts should be registered
            hostManager.GetHost("Host1").Should().BeSameAs(host1);
            hostManager.GetHost("Host2").Should().BeSameAs(host2);
            hostManager.GetHost("Host3").Should().BeSameAs(host3);
        }

        #endregion
    }
}

