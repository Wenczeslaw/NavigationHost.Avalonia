using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NavigationHost.WPF.Abstractions;
using NavigationHost.WPF.Extensions;
using NavigationHost.WPF.Services;
using NavigationHost.WPF.Tests.Infrastructure;
using Xunit;

namespace NavigationHost.WPF.Tests.Extensions
{
    /// <summary>
    ///     Tests for NavigationServiceCollectionExtensions.
    /// </summary>
    public class NavigationServiceCollectionExtensionsTests
    {
        [WpfFact]
        public void AddNavigationHost_ShouldRegisterRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddNavigationHost();
            var provider = services.BuildServiceProvider();

            // Assert
            provider.GetService<IHostManager>().Should().NotBeNull();
            provider.GetService<IHostRegistry>().Should().NotBeNull();
            provider.GetService<IViewModelConventionResolver>().Should().NotBeNull();
        }

        [WpfFact]
        public void AddNavigationHost_ShouldRegisterHostManagerAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNavigationHost();
            var provider = services.BuildServiceProvider();

            // Act
            var instance1 = provider.GetService<IHostManager>();
            var instance2 = provider.GetService<IHostManager>();

            // Assert
            instance1.Should().BeSameAs(instance2);
        }

        [WpfFact]
        public void AddNavigationHost_ShouldSetHostManagerLocator()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNavigationHost();
            var provider = services.BuildServiceProvider();

            // Act
            var hostManager = provider.GetService<IHostManager>();
            var locatorInstance = HostManagerLocator.Current;

            // Assert
            locatorInstance.Should().BeSameAs(hostManager);
        }


        [WpfFact]
        public void AddView_ShouldRegisterViewAsTransient()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNavigationHost();

            // Act
            services.AddView<TestView>();
            var provider = services.BuildServiceProvider();

            // Assert
            var instance1 = provider.GetService<TestView>();
            var instance2 = provider.GetService<TestView>();

            instance1.Should().NotBeNull();
            instance2.Should().NotBeNull();
            instance1.Should().NotBeSameAs(instance2);
        }

        [WpfFact]
        public void AddView_WithViewModel_ShouldRegisterBothAsTransient()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNavigationHost();

            // Act
            services.AddView<TestView, TestViewModel>();
            var provider = services.BuildServiceProvider();

            // Assert
            var view = provider.GetService<TestView>();
            var viewModel = provider.GetService<TestViewModel>();

            view.Should().NotBeNull();
            viewModel.Should().NotBeNull();
        }

        [WpfFact]
        public void AddNavigationHost_MultipleCalls_ShouldNotDuplicateRegistrations()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddNavigationHost();
            services.AddNavigationHost();
            var provider = services.BuildServiceProvider();

            // Assert - Should not throw
            var hostManager = provider.GetService<IHostManager>();
            hostManager.Should().NotBeNull();
        }
    }
}

